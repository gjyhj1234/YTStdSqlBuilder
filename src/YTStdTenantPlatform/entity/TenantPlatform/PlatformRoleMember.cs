using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>平台角色成员</summary>
[Entity(TableName = "platform_role_members")]
[Index("uq_platform_role_members_role_user", "role_id", "user_id", Kind = IndexKind.Unique)]
public class PlatformRoleMember
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>角色 ID</summary>
    public long RoleId { get; set; }

    /// <summary>用户 ID</summary>
    public long UserId { get; set; }

    /// <summary>分配人</summary>
    public long? AssignedBy { get; set; }

    /// <summary>分配时间</summary>
    public DateTime AssignedAt { get; set; }
}
