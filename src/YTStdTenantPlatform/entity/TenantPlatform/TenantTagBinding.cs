using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>租户标签绑定</summary>
[Entity(TableName = "tenant_tag_bindings")]
[Index("uq_tenant_tag_bindings_tenant_tag", "tenant_ref_id", "tag_id", Kind = IndexKind.Unique)]
public class TenantTagBinding
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>关联租户 ID</summary>
    public long TenantRefId { get; set; }

    /// <summary>标签 ID</summary>
    public long TagId { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }
}
