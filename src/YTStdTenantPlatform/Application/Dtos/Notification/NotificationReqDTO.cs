using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>创建通知模板请求</summary>
    public sealed class CreateNotificationTemplateReqDTO
    {
        /// <summary>模板编码</summary>
        public string TemplateCode { get; set; } = "";
        /// <summary>模板名称</summary>
        public string TemplateName { get; set; } = "";
        /// <summary>通知渠道（email/sms/webhook/in_app）</summary>
        public string Channel { get; set; } = "email";
        /// <summary>主题模板</summary>
        public string? SubjectTemplate { get; set; }
        /// <summary>正文模板</summary>
        public string BodyTemplate { get; set; } = "";
    }

    /// <summary>更新通知模板请求</summary>
    public sealed class UpdateNotificationTemplateReqDTO
    {
        /// <summary>模板名称</summary>
        public string? TemplateName { get; set; }
        /// <summary>主题模板</summary>
        public string? SubjectTemplate { get; set; }
        /// <summary>正文模板</summary>
        public string? BodyTemplate { get; set; }
    }

    /// <summary>创建通知请求</summary>
    public sealed class CreateNotificationReqDTO
    {
        /// <summary>关联租户 ID</summary>
        public long? TenantRefId { get; set; }
        /// <summary>模板 ID</summary>
        public long? TemplateId { get; set; }
        /// <summary>通知渠道（email/sms/webhook/in_app）</summary>
        public string Channel { get; set; } = "email";
        /// <summary>接收人</summary>
        public string Recipient { get; set; } = "";
        /// <summary>主题</summary>
        public string? Subject { get; set; }
        /// <summary>正文</summary>
        public string Body { get; set; } = "";
    }
}
