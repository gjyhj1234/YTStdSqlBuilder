using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>通知</summary>
[Entity(TableName = "notifications")]
[Index("idx_notifications_tenant_status", "tenant_ref_id", "send_status")]
public class Notification
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>关联租户 ID</summary>
    public long? TenantRefId { get; set; }

    /// <summary>模板 ID</summary>
    public long? TemplateId { get; set; }

    /// <summary>通知渠道</summary>
    [Column(Length = 32, IsRequired = true)]
    public string Channel { get; set; } = "";

    /// <summary>接收人</summary>
    [Column(Length = 255, IsRequired = true)]
    public string Recipient { get; set; } = "";

    /// <summary>主题</summary>
    [Column(Length = 255)]
    public string? Subject { get; set; }

    /// <summary>正文</summary>
    [Column(IsRequired = true)]
    public string Body { get; set; } = "";

    /// <summary>发送状态</summary>
    [Column(Length = 32, IsRequired = true)]
    public string SendStatus { get; set; } = "";

    /// <summary>发送时间</summary>
    public DateTime? SentAt { get; set; }

    /// <summary>阅读时间</summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }
}
