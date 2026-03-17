using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>SaaS 套餐版本</summary>
[Entity(TableName = "saas_package_versions", NeedAuditTable = true)]
[DetailOf(typeof(SaasPackage), ForeignKey = "PackageId")]
[Index("uq_saas_package_versions_package_version", "package_id", "version_code", Kind = IndexKind.Unique)]
public class SaasPackageVersion
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>套餐 ID</summary>
    public long PackageId { get; set; }

    /// <summary>版本编码</summary>
    [Column(Length = 64, IsRequired = true)]
    public string VersionCode { get; set; } = "";

    /// <summary>版本名称</summary>
    [Column(Length = 128, IsRequired = true)]
    public string VersionName { get; set; } = "";

    /// <summary>版本类型</summary>
    [Column(Length = 32, IsRequired = true)]
    public string EditionType { get; set; } = "";

    /// <summary>计费周期</summary>
    [Column(Length = 32, IsRequired = true)]
    public string BillingCycle { get; set; } = "";

    /// <summary>价格</summary>
    [Column(Length = 18, IsRequired = true)]
    public decimal Price { get; set; }

    /// <summary>货币编码</summary>
    [Column(Length = 16, IsRequired = true)]
    public string CurrencyCode { get; set; } = "";

    /// <summary>试用天数</summary>
    public int TrialDays { get; set; }

    /// <summary>是否默认</summary>
    public bool IsDefault { get; set; }

    /// <summary>是否启用</summary>
    public bool Enabled { get; set; }

    /// <summary>生效开始时间</summary>
    public DateTime? EffectiveFrom { get; set; }

    /// <summary>生效结束时间</summary>
    public DateTime? EffectiveTo { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; }
}
