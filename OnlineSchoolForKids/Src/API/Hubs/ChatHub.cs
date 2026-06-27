using Domain.Entities.Chat;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace API.Hubs;


[Authorize]
public class ChatHub : Hub
{
    private readonly ChatRepository _repo;

    // Track online users: userId → connectionId (use IConnectionMultiplexer / Redis
    // in production for multi-server deployments)
    private static readonly Dictionary<string, HashSet<string>> _online = new();
    private static readonly object _lock = new();

    public ChatHub(ChatRepository repo) => _repo = repo;

    // ── Connection lifecycle ──────────────────────────────────────────────────

    public override async Task OnConnectedAsync()
    {
        var userId = UserId();

        lock (_lock)
        {
            if (!_online.TryGetValue(userId, out var conns))
                _online[userId] = conns = new HashSet<string>();
            conns.Add(Context.ConnectionId);
        }

        // Re-join all group SignalR rooms this user belongs to
        var groups = await _repo.GetUserGroupsAsync(userId);
        foreach (var g in groups)
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupRoom(g.Id));

        // Tell the user's conversations that they're online
        await Clients.Others.SendAsync("UserOnline", userId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? ex)
    {
        var userId = UserId();
        bool stillOnline;

        lock (_lock)
        {
            if (_online.TryGetValue(userId, out var conns))
            {
                conns.Remove(Context.ConnectionId);
                if (conns.Count == 0) _online.Remove(userId);
            }
            stillOnline = _online.ContainsKey(userId);
        }

        if (!stillOnline)
            await Clients.Others.SendAsync("UserOffline", userId);

        await base.OnDisconnectedAsync(ex);
    }

    // ── DM methods ────────────────────────────────────────────────────────────

    /// <summary>Client calls this after opening a conversation to mark messages read.</summary>
    public async Task MarkConversationRead(string conversationId)
    {
        var userId = UserId();
        await _repo.MarkDmReadAsync(conversationId, userId);
        await _repo.ResetDmUnreadAsync(conversationId, userId);

        // Notify the other participant that their messages were read
        var conv = await _repo.GetUserConversationsAsync(userId);
        var target = conv.FirstOrDefault(c => c.Id == conversationId);
        if (target is null) return;

        var otherId = target.ParticipantAId == userId
            ? target.ParticipantBId : target.ParticipantAId;

        await NotifyUser(otherId, "MessagesRead", new { conversationId, readBy = userId });
    }

    public async Task SendTyping(string conversationId, bool isTyping)
    {
        var userId = UserId();
        var convs = await _repo.GetUserConversationsAsync(userId);
        var conv = convs.FirstOrDefault(c => c.Id == conversationId);
        if (conv is null) return;

        var otherId = conv.ParticipantAId == userId
            ? conv.ParticipantBId : conv.ParticipantAId;

        await NotifyUser(otherId, "TypingIndicator",
            new { conversationId, userId, isTyping });
    }

    // ── Group methods ─────────────────────────────────────────────────────────

    public async Task JoinGroup(string groupId)
    {
        var group = await _repo.GetGroupAsync(groupId);
        if (group is null || !IsMember(group, UserId())) return;

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupRoom(groupId));
        await _repo.ResetGroupUnreadAsync(groupId, UserId());
    }

    public async Task LeaveGroup(string groupId) =>
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupRoom(groupId));

    public async Task SendGroupTyping(string groupId, bool isTyping) =>
        await Clients.OthersInGroup(GroupRoom(groupId))
            .SendAsync("GroupTyping", new { groupId, userId = UserId(), isTyping });

    // ── Helpers ───────────────────────────────────────────────────────────────

    public static bool IsOnline(string userId)
    {
        lock (_lock) return _online.ContainsKey(userId);
    }

    private string UserId() =>
        Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!;

    private static string GroupRoom(string groupId) => $"group:{groupId}";

    private bool IsMember(Group group, string userId) =>
        group.Members.Any(m => m.UserId == userId);

    private async Task NotifyUser(string userId, string method, object payload)
    {
        HashSet<string>? conns;
        lock (_lock) _online.TryGetValue(userId, out conns);
        if (conns is null) return;

        foreach (var connId in conns)
            await Clients.Client(connId).SendAsync(method, payload);
    }
   
}