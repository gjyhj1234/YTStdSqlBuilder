using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>租户 UI 品牌</summary>
[Entity(TableName = "tenant_ui_brandings", NeedAuditTable = true)]
[Index("uq_tenant_ui_brandings_tenant_ref_id", "tenant_ref_id", Kind = IndexKind.Unique)]
public class TenantUiBranding
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>关联租户 ID</summary>
    public long TenantRefId { get; set; }

    /// <summary>品牌名称</summary>
    [Column(Length = 128)]
    public string? BrandName { get; set; }

    /// <summary>登录页配置</summary>
    [Column(DbType = "jsonb")]
    public string? LoginPageConfig { get; set; }

    /// <summary>UI 主题</summary>
    [Column(DbType = "jsonb")]
    public string? UiTheme { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; }
}
