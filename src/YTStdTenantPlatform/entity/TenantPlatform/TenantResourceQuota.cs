using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>租户资源配额</summary>
[Entity(TableName = "tenant_resource_quotas", NeedAuditTable = true)]
[Index("uq_tenant_resource_quotas_tenant_type", "tenant_ref_id", "quota_type", Kind = IndexKind.Unique)]
public class TenantResourceQuota
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>关联租户 ID</summary>
    public long TenantRefId { get; set; }

    /// <summary>配额类型</summary>
    [Column(Length = 64, IsRequired = true)]
    public string QuotaType { get; set; } = "";

    /// <summary>配额上限</summary>
    public long QuotaLimit { get; set; }

    /// <summary>告警阈值</summary>
    public long? WarningThreshold { get; set; }

    /// <summary>重置周期</summary>
    [Column(Length = 32)]
    public string? ResetCycle { get; set; }

    /// <summary>生效开始时间</summary>
    public DateTime? EffectiveFrom { get; set; }

    /// <summary>生效结束时间</summary>
    public DateTime? EffectiveTo { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; }
}
