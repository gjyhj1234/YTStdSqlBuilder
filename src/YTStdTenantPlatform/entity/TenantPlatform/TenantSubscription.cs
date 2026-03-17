using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>租户订阅</summary>
[Entity(TableName = "tenant_subscriptions", NeedAuditTable = true)]
[Index("idx_subscriptions_tenant_status", "tenant_ref_id", "subscription_status")]
public class TenantSubscription
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>关联租户 ID</summary>
    public long TenantRefId { get; set; }

    /// <summary>套餐版本 ID</summary>
    public long PackageVersionId { get; set; }

    /// <summary>订阅状态</summary>
    [Column(Length = 32, IsRequired = true)]
    public string SubscriptionStatus { get; set; } = "";

    /// <summary>订阅类型</summary>
    [Column(Length = 32, IsRequired = true)]
    public string SubscriptionType { get; set; } = "";

    /// <summary>开始时间</summary>
    public DateTime StartedAt { get; set; }

    /// <summary>到期时间</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>是否自动续费</summary>
    public bool AutoRenew { get; set; }

    /// <summary>取消时间</summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>创建人</summary>
    public long? CreatedBy { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; }
}
