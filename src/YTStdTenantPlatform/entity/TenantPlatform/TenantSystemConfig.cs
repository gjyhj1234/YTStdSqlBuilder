using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>租户系统配置</summary>
[Entity(TableName = "tenant_system_configs", NeedAuditTable = true)]
[Index("uq_tenant_system_configs_tenant_ref_id", "tenant_ref_id", Kind = IndexKind.Unique)]
public class TenantSystemConfig
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>关联租户 ID</summary>
    public long TenantRefId { get; set; }

    /// <summary>系统名称</summary>
    [Column(Length = 128)]
    public string? SystemName { get; set; }

    /// <summary>Logo 地址</summary>
    [Column(Length = 255)]
    public string? LogoUrl { get; set; }

    /// <summary>系统主题</summary>
    [Column(Length = 64)]
    public string? SystemTheme { get; set; }

    /// <summary>默认语言</summary>
    [Column(Length = 32)]
    public string? DefaultLanguage { get; set; }

    /// <summary>默认时区</summary>
    [Column(Length = 64)]
    public string? DefaultTimezone { get; set; }

    /// <summary>扩展配置</summary>
    [Column(DbType = "jsonb")]
    public string? ExtraConfig { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; }
}
