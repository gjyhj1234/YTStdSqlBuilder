using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>通知模板响应数据</summary>
    public sealed class NotificationTemplateRepDTO
    {
        /// <summary>模板ID</summary>
        public long Id { get; set; }
        /// <summary>模板编码</summary>
        public string TemplateCode { get; set; } = "";
        /// <summary>模板名称</summary>
        public string TemplateName { get; set; } = "";
        /// <summary>通知类型</summary>
        public string NotificationType { get; set; } = "";
        /// <summary>模板内容</summary>
        public string? Content { get; set; }
        /// <summary>状态</summary>
        public string Status { get; set; } = "";
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>通知响应数据</summary>
    public sealed class NotificationRepDTO
    {
        /// <summary>通知ID</summary>
        public long Id { get; set; }
        /// <summary>租户ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>通知类型</summary>
        public string NotificationType { get; set; } = "";
        /// <summary>标题</summary>
        public string Title { get; set; } = "";
        /// <summary>内容</summary>
        public string? Content { get; set; }
        /// <summary>是否已读</summary>
        public bool IsRead { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }
}
