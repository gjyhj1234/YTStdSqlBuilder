using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>租户分组成员</summary>
[Entity(TableName = "tenant_group_members")]
[Index("uq_tenant_group_members_group_tenant", "group_id", "tenant_ref_id", Kind = IndexKind.Unique)]
public class TenantGroupMember
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>分组 ID</summary>
    public long GroupId { get; set; }

    /// <summary>关联租户 ID</summary>
    public long TenantRefId { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }
}
