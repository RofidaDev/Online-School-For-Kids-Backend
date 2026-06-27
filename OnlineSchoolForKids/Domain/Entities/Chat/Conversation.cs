using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Chat;

public class Conversation : BaseEntity
{
    /// <summary>Always sorted so participantA &lt; participantB — prevents duplicates.</summary>
    public string ParticipantAId { get; set; } = string.Empty;
    public string ParticipantBId { get; set; } = string.Empty;

    public string? LastMessageId { get; set; }
    public string? LastMessageContent { get; set; }
    public DateTime? LastMessageAt { get; set; }

    // Per-participant unread counts (stored as a dict for simplicity)
    public Dictionary<string, int> UnreadCounts { get; set; } = new();
}