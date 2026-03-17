using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>SaaS 套餐能力</summary>
[Entity(TableName = "saas_package_capabilities", NeedAuditTable = true)]
[DetailOf(typeof(SaasPackageVersion), ForeignKey = "PackageVersionId")]
[Index("uq_saas_package_capabilities_version_capability", "package_version_id", "capability_key", Kind = IndexKind.Unique)]
public class SaasPackageCapability
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>套餐版本 ID</summary>
    public long PackageVersionId { get; set; }

    /// <summary>能力键</summary>
    [Column(Length = 128, IsRequired = true)]
    public string CapabilityKey { get; set; } = "";

    /// <summary>能力名称</summary>
    [Column(Length = 128, IsRequired = true)]
    public string CapabilityName { get; set; } = "";

    /// <summary>能力类型</summary>
    [Column(Length = 32, IsRequired = true)]
    public string CapabilityType { get; set; } = "";

    /// <summary>能力值</summary>
    [Column(DbType = "jsonb", IsRequired = true)]
    public string CapabilityValue { get; set; } = "";

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; }
}
