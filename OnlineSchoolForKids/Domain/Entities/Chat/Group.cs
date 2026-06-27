using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Chat;

[BsonIgnoreExtraElements]
public class Group : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AvatarUrl { get; set; }

    /// <summary>null  → free-form group; set → tied to a course/cohort</summary>
    public string? CourseId { get; set; }

    public string CreatedByUserId { get; set; } = string.Empty;

    public List<GroupMember> Members { get; set; } = new();

    public string? LastMessageId { get; set; }
    public string? LastMessageContent { get; set; }
    public DateTime? LastMessageAt { get; set; }

    public string InviteCode { get; set; } = Guid.NewGuid().ToString("N")[..10];
    public bool InviteLinkEnabled { get; set; } = true;

    // Permission settings
    public GroupPermission WhoCanInvite { get; set; } = GroupPermission.AdminsAndOwner;
    public GroupPermission WhoCanAddMembers { get; set; } = GroupPermission.AdminsAndOwner;
    public bool ShowOldChatToNewMembers { get; set; } = true;

}

public enum GroupPermission
{
    OwnerOnly = 0,
    AdminsAndOwner = 1,
    AllMembers = 2
}