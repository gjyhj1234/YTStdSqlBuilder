using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>通知模板列表项</summary>
    public sealed class NotificationTemplateRepDTO
    {
        /// <summary>模板 ID</summary>
        public long Id { get; set; }
        /// <summary>模板编码</summary>
        public string TemplateCode { get; set; } = "";
        /// <summary>模板名称</summary>
        public string TemplateName { get; set; } = "";
        /// <summary>通知渠道</summary>
        public string Channel { get; set; } = "";
        /// <summary>主题模板</summary>
        public string? SubjectTemplate { get; set; }
        /// <summary>正文模板</summary>
        public string BodyTemplate { get; set; } = "";
        /// <summary>状态</summary>
        public string Status { get; set; } = "";
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>通知列表项</summary>
    public sealed class NotificationRepDTO
    {
        /// <summary>通知 ID</summary>
        public long Id { get; set; }
        /// <summary>关联租户 ID</summary>
        public long? TenantRefId { get; set; }
        /// <summary>模板 ID</summary>
        public long? TemplateId { get; set; }
        /// <summary>通知渠道</summary>
        public string Channel { get; set; } = "";
        /// <summary>接收人</summary>
        public string Recipient { get; set; } = "";
        /// <summary>主题</summary>
        public string? Subject { get; set; }
        /// <summary>正文</summary>
        public string Body { get; set; } = "";
        /// <summary>发送状态</summary>
        public string SendStatus { get; set; } = "";
        /// <summary>发送时间</summary>
        public DateTime? SentAt { get; set; }
        /// <summary>阅读时间</summary>
        public DateTime? ReadAt { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }
}
