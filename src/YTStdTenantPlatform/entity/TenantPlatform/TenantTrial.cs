using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>租户试用</summary>
[Entity(TableName = "tenant_trials", NeedAuditTable = true)]
[Index("uq_tenant_trials_tenant_started", "tenant_ref_id", "started_at", Kind = IndexKind.Unique)]
public class TenantTrial
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>关联租户 ID</summary>
    public long TenantRefId { get; set; }

    /// <summary>套餐版本 ID</summary>
    public long? PackageVersionId { get; set; }

    /// <summary>开始时间</summary>
    public DateTime StartedAt { get; set; }

    /// <summary>到期时间</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>转化订阅 ID</summary>
    public long? ConvertedSubscriptionId { get; set; }

    /// <summary>状态</summary>
    [Column(Length = 32, IsRequired = true)]
    public string Status { get; set; } = "";

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; }
}
