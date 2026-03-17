using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>租户 Webhook</summary>
[Entity(TableName = "tenant_webhooks", NeedAuditTable = true)]
public class TenantWebhook
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>关联租户 ID</summary>
    public long TenantRefId { get; set; }

    /// <summary>Webhook 名称</summary>
    [Column(Length = 128, IsRequired = true)]
    public string WebhookName { get; set; } = "";

    /// <summary>目标地址</summary>
    [Column(Length = 500, IsRequired = true)]
    public string TargetUrl { get; set; } = "";

    /// <summary>密钥令牌散列</summary>
    [Column(Length = 255)]
    public string? SecretTokenHash { get; set; }

    /// <summary>状态</summary>
    [Column(Length = 32, IsRequired = true)]
    public string Status { get; set; } = "";

    /// <summary>重试策略</summary>
    [Column(DbType = "jsonb")]
    public string? RetryPolicy { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; }
}
