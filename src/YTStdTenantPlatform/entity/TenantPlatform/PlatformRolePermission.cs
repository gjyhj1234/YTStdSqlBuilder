using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>平台角色权限关联</summary>
[Entity(TableName = "platform_role_permissions")]
[Index("uq_platform_role_permissions_role_permission", "role_id", "permission_id", Kind = IndexKind.Unique)]
public class PlatformRolePermission
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>角色 ID</summary>
    public long RoleId { get; set; }

    /// <summary>权限 ID</summary>
    public long PermissionId { get; set; }

    /// <summary>授权人</summary>
    public long? GrantedBy { get; set; }

    /// <summary>授权时间</summary>
    public DateTime GrantedAt { get; set; }
}
