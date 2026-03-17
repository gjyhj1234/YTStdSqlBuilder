using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>租户功能开关</summary>
[Entity(TableName = "tenant_feature_flags", NeedAuditTable = true)]
[Index("uq_tenant_feature_flags_tenant_feature", "tenant_ref_id", "feature_key", Kind = IndexKind.Unique)]
[Index("idx_feature_flags_tenant_enabled", "tenant_ref_id", "enabled")]
public class TenantFeatureFlag
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>关联租户 ID</summary>
    public long TenantRefId { get; set; }

    /// <summary>功能键</summary>
    [Column(Length = 128, IsRequired = true)]
    public string FeatureKey { get; set; } = "";

    /// <summary>功能名称</summary>
    [Column(Length = 128, IsRequired = true)]
    public string FeatureName { get; set; } = "";

    /// <summary>是否启用</summary>
    public bool Enabled { get; set; }

    /// <summary>灰度类型</summary>
    [Column(Length = 32, IsRequired = true)]
    public string RolloutType { get; set; } = "";

    /// <summary>灰度配置</summary>
    [Column(DbType = "jsonb")]
    public string? RolloutConfig { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; }
}
