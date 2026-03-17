using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>SaaS 套餐</summary>
[Entity(TableName = "saas_packages", NeedAuditTable = true)]
[Index("uq_saas_packages_package_code", "package_code", Kind = IndexKind.Unique)]
public class SaasPackage
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>套餐编码</summary>
    [Column(Length = 64, IsRequired = true)]
    public string PackageCode { get; set; } = "";

    /// <summary>套餐名称</summary>
    [Column(Length = 128, IsRequired = true)]
    public string PackageName { get; set; } = "";

    /// <summary>描述</summary>
    public string? Description { get; set; }

    /// <summary>状态</summary>
    [Column(Length = 32, IsRequired = true)]
    public string Status { get; set; } = "";

    /// <summary>创建人</summary>
    public long? CreatedBy { get; set; }

    /// <summary>更新人</summary>
    public long? UpdatedBy { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; }
}
