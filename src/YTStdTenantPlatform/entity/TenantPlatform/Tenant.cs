using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>租户</summary>
[Entity(TableName = "tenants", NeedAuditTable = true)]
[Index("uq_tenants_tenant_code", "tenant_code", Kind = IndexKind.Unique)]
public class Tenant
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>租户编码</summary>
    [Column(Length = 64, IsRequired = true)]
    public string TenantCode { get; set; } = "";

    /// <summary>租户名称</summary>
    [Column(Length = 128, IsRequired = true)]
    public string TenantName { get; set; } = "";

    /// <summary>企业名称</summary>
    [Column(Length = 255)]
    public string? EnterpriseName { get; set; }

    /// <summary>联系人姓名</summary>
    [Column(Length = 128)]
    public string? ContactName { get; set; }

    /// <summary>联系人手机</summary>
    [Column(Length = 32)]
    public string? ContactPhone { get; set; }

    /// <summary>联系人邮箱</summary>
    [Column(Length = 128)]
    public string? ContactEmail { get; set; }

    /// <summary>来源类型</summary>
    [Column(Length = 32, IsRequired = true)]
    public string SourceType { get; set; } = "";

    /// <summary>生命周期状态</summary>
    [Column(Length = 32, IsRequired = true)]
    public string LifecycleStatus { get; set; } = "";

    /// <summary>当前套餐 ID</summary>
    public long? CurrentPlanId { get; set; }

    /// <summary>当前订阅 ID</summary>
    public long? CurrentSubscriptionId { get; set; }

    /// <summary>所属分组 ID</summary>
    public long? GroupId { get; set; }

    /// <summary>行业标签</summary>
    [Column(Length = 64)]
    public string? IndustryTag { get; set; }

    /// <summary>客户等级</summary>
    [Column(Length = 64)]
    public string? CustomerLevel { get; set; }

    /// <summary>客户来源</summary>
    [Column(Length = 64)]
    public string? CustomerSource { get; set; }

    /// <summary>默认语言</summary>
    [Column(Length = 32, IsRequired = true)]
    public string DefaultLanguage { get; set; } = "";

    /// <summary>默认时区</summary>
    [Column(Length = 64, IsRequired = true)]
    public string DefaultTimezone { get; set; } = "";

    /// <summary>隔离模式</summary>
    [Column(Length = 32, IsRequired = true)]
    public string IsolationMode { get; set; } = "";

    /// <summary>数据库名称</summary>
    [Column(Length = 128)]
    public string? DatabaseName { get; set; }

    /// <summary>架构名称</summary>
    [Column(Length = 128)]
    public string? SchemaName { get; set; }

    /// <summary>默认域名</summary>
    [Column(Length = 255)]
    public string? DefaultDomain { get; set; }

    /// <summary>是否启用</summary>
    public bool Enabled { get; set; }

    /// <summary>开通时间</summary>
    public DateTime? OpenedAt { get; set; }

    /// <summary>激活时间</summary>
    public DateTime? ActivatedAt { get; set; }

    /// <summary>到期时间</summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>暂停时间</summary>
    public DateTime? SuspendedAt { get; set; }

    /// <summary>关闭时间</summary>
    public DateTime? ClosedAt { get; set; }

    /// <summary>删除时间</summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>创建人</summary>
    public long? CreatedBy { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; }
}
