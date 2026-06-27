using Domain.Enums.Content;

namespace Domain.Interfaces.Services.Shared
{

    public interface INotificationService
    {
        Task SendAsync(
            string userId,
            string title,
            string message,
            NotificationType type,
            string? actionUrl = null,
            CancellationToken ct = default);
    }
}

