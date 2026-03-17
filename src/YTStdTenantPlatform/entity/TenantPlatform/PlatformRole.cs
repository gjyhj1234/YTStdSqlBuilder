using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>平台角色</summary>
[Entity(TableName = "platform_roles", NeedAuditTable = true)]
[Index("uq_platform_roles_code", "code", Kind = IndexKind.Unique)]
public class PlatformRole
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>角色编码</summary>
    [Column(Length = 64, IsRequired = true)]
    public string Code { get; set; } = "";

    /// <summary>角色名称</summary>
    [Column(Length = 128, IsRequired = true)]
    public string Name { get; set; } = "";

    /// <summary>描述</summary>
    public string? Description { get; set; }

    /// <summary>角色状态</summary>
    [Column(Length = 32, IsRequired = true)]
    public string Status { get; set; } = "";

    /// <summary>创建人</summary>
    public long? CreatedBy { get; set; }

    /// <summary>更新人</summary>
    public long? UpdatedBy { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; }
}
