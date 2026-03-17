using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>Webhook 事件</summary>
[Entity(TableName = "webhook_events", NeedAuditTable = true)]
[Index("uq_webhook_events_event_code", "event_code", Kind = IndexKind.Unique)]
public class WebhookEvent
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>事件编码</summary>
    [Column(Length = 128, IsRequired = true)]
    public string EventCode { get; set; } = "";

    /// <summary>事件名称</summary>
    [Column(Length = 128, IsRequired = true)]
    public string EventName { get; set; } = "";

    /// <summary>描述</summary>
    public string? Description { get; set; }

    /// <summary>负载结构</summary>
    [Column(DbType = "jsonb")]
    public string? PayloadSchema { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }
}
