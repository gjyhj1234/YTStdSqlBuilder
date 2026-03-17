using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>租户分组</summary>
[Entity(TableName = "tenant_groups", NeedAuditTable = true)]
[Index("uq_tenant_groups_group_code", "group_code", Kind = IndexKind.Unique)]
public class TenantGroup
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>分组编码</summary>
    [Column(Length = 64, IsRequired = true)]
    public string GroupCode { get; set; } = "";

    /// <summary>分组名称</summary>
    [Column(Length = 128, IsRequired = true)]
    public string GroupName { get; set; } = "";

    /// <summary>描述</summary>
    public string? Description { get; set; }

    /// <summary>父级分组 ID</summary>
    public long? ParentId { get; set; }

    /// <summary>创建人</summary>
    public long? CreatedBy { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; }
}
