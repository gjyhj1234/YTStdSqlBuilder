using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>通知模板</summary>
[Entity(TableName = "notification_templates", NeedAuditTable = true)]
[Index("uq_notification_templates_template_code", "template_code", Kind = IndexKind.Unique)]
public class NotificationTemplate
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>模板编码</summary>
    [Column(Length = 64, IsRequired = true)]
    public string TemplateCode { get; set; } = "";

    /// <summary>模板名称</summary>
    [Column(Length = 128, IsRequired = true)]
    public string TemplateName { get; set; } = "";

    /// <summary>通知渠道</summary>
    [Column(Length = 32, IsRequired = true)]
    public string Channel { get; set; } = "";

    /// <summary>主题模板</summary>
    [Column(Length = 255)]
    public string? SubjectTemplate { get; set; }

    /// <summary>正文模板</summary>
    [Column(IsRequired = true)]
    public string BodyTemplate { get; set; } = "";

    /// <summary>模板变量</summary>
    [Column(DbType = "jsonb")]
    public string? Variables { get; set; }

    /// <summary>状态</summary>
    [Column(Length = 32, IsRequired = true)]
    public string Status { get; set; } = "";

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; }
}
