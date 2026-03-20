using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>创建通知模板请求参数</summary>
    public sealed class CreateNotificationTemplateReqDTO
    {
        /// <summary>模板编码</summary>
        public string TemplateCode { get; set; } = "";
        /// <summary>模板名称</summary>
        public string TemplateName { get; set; } = "";
        /// <summary>通知类型</summary>
        public string NotificationType { get; set; } = "";
        /// <summary>模板内容</summary>
        public string? Content { get; set; }
    }

    /// <summary>更新通知模板请求参数</summary>
    public sealed class UpdateNotificationTemplateReqDTO
    {
        /// <summary>模板名称</summary>
        public string? TemplateName { get; set; }
        /// <summary>模板内容</summary>
        public string? Content { get; set; }
    }

    /// <summary>创建通知请求参数</summary>
    public sealed class CreateNotificationReqDTO
    {
        /// <summary>租户ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>通知类型</summary>
        public string NotificationType { get; set; } = "";
        /// <summary>标题</summary>
        public string Title { get; set; } = "";
        /// <summary>内容</summary>
        public string? Content { get; set; }
    }
}
