using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>租户域名</summary>
[Entity(TableName = "tenant_domains", NeedAuditTable = true)]
[Index("uq_tenant_domains_domain", "domain", Kind = IndexKind.Unique)]
[Index("uq_tenant_domains_tenant_domain", "tenant_ref_id", "domain", Kind = IndexKind.Unique)]
public class TenantDomain
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>关联租户 ID</summary>
    public long TenantRefId { get; set; }

    /// <summary>域名</summary>
    [Column(Length = 255, IsRequired = true)]
    public string Domain { get; set; } = "";

    /// <summary>域名类型</summary>
    [Column(Length = 32, IsRequired = true)]
    public string DomainType { get; set; } = "";

    /// <summary>是否为主域名</summary>
    public bool IsPrimary { get; set; }

    /// <summary>验证状态</summary>
    [Column(Length = 32, IsRequired = true)]
    public string VerificationStatus { get; set; } = "";

    /// <summary>验证令牌</summary>
    [Column(Length = 128)]
    public string? VerificationToken { get; set; }

    /// <summary>验证时间</summary>
    public DateTime? VerifiedAt { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }
}
