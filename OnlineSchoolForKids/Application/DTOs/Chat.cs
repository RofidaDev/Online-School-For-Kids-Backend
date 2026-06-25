using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs;


// ── Shared ────────────────────────────────────────────────────────────────────

public record MessageReactionDto(string Emoji, string UserId);

public record ReplyContextDto(
    string MessageId,
    string SenderName,
    string Content);

public record ChatMessageDto
{
    public string Id { get; init; } = string.Empty;
    public string ContextId { get; init; } = string.Empty;
    public string SenderId { get; init; } = string.Empty;
    public string SenderName { get; init; } = string.Empty;
    public string? SenderAvatar { get; init; }
    public string Content { get; init; } = string.Empty;
    public string Type { get; init; } = "text";
    public string? FileUrl { get; init; }
    public string? FileName { get; init; }
    public DateTime Timestamp { get; init; }
    public bool IsEdited { get; init; }
    public bool IsDeleted { get; init; }
    public List<MessageReactionDto> Reactions { get; init; } = new();
    public ReplyContextDto? ReplyTo { get; init; }
    // DM only
    public List<string> ReadBy { get; init; } = new();
}

// ── Direct Messages ───────────────────────────────────────────────────────────

public record ConversationDto
{
    public string Id { get; init; } = string.Empty;
    public string ParticipantId { get; init; } = string.Empty;
    public string ParticipantName { get; init; } = string.Empty;
    public string? ParticipantAvatar { get; init; }
    public string LastMessage { get; init; } = string.Empty;
    public DateTime? LastMessageAt { get; init; }
    public int UnreadCount { get; init; }
    public bool IsOnline { get; init; }
}

public record StartConversationRequest(string OtherUserId);

public record SendMessageRequest(
    string Content,
    string Type = "text",           // text | image | file
    string? FileUrl = null,
    string? FileName = null,
    string? ReplyToMessageId = null);

public record EditMessageRequest(string NewContent);

// ── Groups ────────────────────────────────────────────────────────────────────

public record GroupMemberDto(
    string UserId,
    string UserName,
    string? UserAvatar,
    string Role,
    bool IsOnline);

public record GroupDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? AvatarUrl { get; init; }
    public string? CourseId { get; init; }
    public int MembersCount { get; init; }
    public string LastMessage { get; init; } = string.Empty;
    public DateTime? LastMessageAt { get; init; }
    public int UnreadCount { get; init; }
    public string MyRole { get; init; } = "member";
}

public record CreateGroupRequest(
    string Name,
    string? Description,
    string? CourseId,
    List<string> MemberIds);

public record UpdateGroupRequest(
    string? Name,
    string? Description);

public record AddGroupMemberRequest(string UserId);

// ── Pagination ─────────────────────────────────────────────────────────────────
public record PagedResult<T>(List<T> Items, bool HasMore, string? NextCursor);

public record GroupSettingsDto(
    string Name,
    string? Description,
    string? AvatarUrl,
    string WhoCanInvite,        // "owner" | "admins" | "all"
    string WhoCanAddMembers,    // "owner" | "admins" | "all"
    bool ShowOldChatToNewMembers,
    bool InviteLinkEnabled
);

public record UpdateGroupSettingsRequest(
    string? Name,
    string? Description,
    string? AvatarUrl,
    string? WhoCanInvite,
    string? WhoCanAddMembers,
    bool? ShowOldChatToNewMembers,
    bool? InviteLinkEnabled
);

public record InviteLinkDto(string Code, string Url, bool Enabled);

public record GroupPreviewDto(string GroupId, string Name, string? Description, string? AvatarUrl, int MembersCount);

public record UserSearchResultDto(string Id, string FullName, string? Email, string? ProfilePictureUrl);

public record AddMemberByEmailRequest(string Email, string Role = "member");

public record AddMemberByEmailResponse(bool UserFound, bool Added, string? InviteShareUrl, string Message);