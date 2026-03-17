using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>Webhook 投递日志</summary>
[Entity(TableName = "webhook_delivery_logs")]
[Index("idx_webhook_delivery_status_time", "delivery_status", "created_at")]
public class WebhookDeliveryLog
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>Webhook ID</summary>
    public long WebhookId { get; set; }

    /// <summary>事件 ID</summary>
    public long? EventId { get; set; }

    /// <summary>投递状态</summary>
    [Column(Length = 32, IsRequired = true)]
    public string DeliveryStatus { get; set; } = "";

    /// <summary>请求头</summary>
    [Column(DbType = "jsonb")]
    public string? RequestHeaders { get; set; }

    /// <summary>请求体</summary>
    [Column(DbType = "jsonb")]
    public string? RequestBody { get; set; }

    /// <summary>响应状态码</summary>
    public int? ResponseStatusCode { get; set; }

    /// <summary>响应体</summary>
    public string? ResponseBody { get; set; }

    /// <summary>重试次数</summary>
    public int RetryCount { get; set; }

    /// <summary>投递时间</summary>
    public DateTime? DeliveredAt { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }
}
