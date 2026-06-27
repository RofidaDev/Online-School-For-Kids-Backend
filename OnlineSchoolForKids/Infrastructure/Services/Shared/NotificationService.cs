using Domain.Entities.Notification;
using Domain.Enums.Content;
using Domain.Interfaces.Repositories.Users;
using Domain.Interfaces.Services.Shared;
using Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Shared
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repo;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            INotificationRepository repo,
            IHubContext<NotificationHub> hub,
            ILogger<NotificationService> logger)
        {
            _repo = repo;
            _hub = hub;
            _logger = logger;
        }

        public async Task SendAsync(
            string userId,
            string title,
            string message,
            NotificationType type,
            string? actionUrl = null,
            CancellationToken ct = default)
        {
            // ── Step 1: Save to MongoDB ───────────────────────────────────────────
            // Always persisted first. If the user is offline they will get it
            // via the REST API when they next open the app.
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                ActionUrl = actionUrl
            };

            try
            {
                await _repo.CreateAsync(notification, ct);
                _logger.LogInformation(
                    "Notification [{Type}] saved for user {UserId} — \"{Title}\"",
                    type, userId, title);
            }
            catch (Exception ex)
            {
                // If DB save fails, do NOT send a ghost real-time notification.
                _logger.LogError(ex,
                    "Failed to save notification [{Type}] for user {UserId}", type, userId);
                return;
            }

            // ── Step 2: Push via SignalR (fire-and-forget) ────────────────────────
            // If the user is offline, SignalR finds no connections in their group
            // and silently does nothing. The notification is already in MongoDB.
            _ = PushToUserAsync(userId, notification);
        }

        private async Task PushToUserAsync(string userId, Notification n)
        {
            try
            {
                await _hub.Clients
                    .Group(userId)               // all active tabs / devices for this user
                    .SendAsync("ReceiveNotification", new
                    {
                        id = n.Id,
                        title = n.Title,
                        message = n.Message,
                        type = n.Type.ToString(),
                        actionUrl = n.ActionUrl,
                        isRead = false,
                        createdAt = n.CreatedAt
                    });

                _logger.LogInformation(
                    "SignalR push sent to user {UserId} — [{Type}]", userId, n.Type);
            }
            catch (Exception ex)
            {
                // Real-time push is best-effort. Notification is already in MongoDB.
                _logger.LogWarning(ex,
                    "SignalR push failed for user {UserId} — notification {Id} is still in DB",
                    userId, n.Id);
            }
        }
    }
}