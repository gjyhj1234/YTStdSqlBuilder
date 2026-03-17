using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>租户订阅变更</summary>
[Entity(TableName = "tenant_subscription_changes")]
public class TenantSubscriptionChange
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>关联租户 ID</summary>
    public long TenantRefId { get; set; }

    /// <summary>订阅 ID</summary>
    public long? SubscriptionId { get; set; }

    /// <summary>变更类型</summary>
    [Column(Length = 32, IsRequired = true)]
    public string ChangeType { get; set; } = "";

    /// <summary>原套餐版本 ID</summary>
    public long? FromPackageVersionId { get; set; }

    /// <summary>目标套餐版本 ID</summary>
    public long? ToPackageVersionId { get; set; }

    /// <summary>生效时间</summary>
    public DateTime EffectiveAt { get; set; }

    /// <summary>备注</summary>
    public string? Remark { get; set; }

    /// <summary>创建人</summary>
    public long? CreatedBy { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }
}
