using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>API密钥响应数据</summary>
    public sealed class TenantApiKeyRepDTO
    {
        /// <summary>密钥ID</summary>
        public long Id { get; set; }
        /// <summary>租户ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>密钥名称</summary>
        public string KeyName { get; set; } = "";
        /// <summary>密钥前缀</summary>
        public string KeyPrefix { get; set; } = "";
        /// <summary>状态</summary>
        public string Status { get; set; } = "";
        /// <summary>过期时间</summary>
        public DateTime? ExpiresAt { get; set; }
        /// <summary>最后使用时间</summary>
        public DateTime? LastUsedAt { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>API密钥创建结果响应数据（含原始密钥，仅创建时返回一次）</summary>
    public sealed class ApiKeyCreatedRepDTO
    {
        /// <summary>密钥ID</summary>
        public long Id { get; set; }
        /// <summary>原始密钥（仅此一次可见）</summary>
        public string RawSecret { get; set; } = "";
        /// <summary>密钥前缀</summary>
        public string KeyPrefix { get; set; } = "";
    }

    /// <summary>API使用统计响应数据</summary>
    public sealed class TenantApiUsageStatRepDTO
    {
        /// <summary>统计ID</summary>
        public long Id { get; set; }
        /// <summary>租户ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>统计日期</summary>
        public DateTime StatDate { get; set; }
        /// <summary>请求总量</summary>
        public long TotalRequests { get; set; }
        /// <summary>成功数量</summary>
        public long SuccessCount { get; set; }
        /// <summary>失败数量</summary>
        public long FailureCount { get; set; }
    }

    /// <summary>Webhook事件响应数据</summary>
    public sealed class WebhookEventRepDTO
    {
        /// <summary>事件ID</summary>
        public long Id { get; set; }
        /// <summary>事件类型</summary>
        public string EventType { get; set; } = "";
        /// <summary>事件名称</summary>
        public string EventName { get; set; } = "";
        /// <summary>描述</summary>
        public string? Description { get; set; }
    }

    /// <summary>Webhook配置响应数据</summary>
    public sealed class TenantWebhookRepDTO
    {
        /// <summary>Webhook ID</summary>
        public long Id { get; set; }
        /// <summary>租户ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>Webhook名称</summary>
        public string WebhookName { get; set; } = "";
        /// <summary>目标URL</summary>
        public string TargetUrl { get; set; } = "";
        /// <summary>状态</summary>
        public string Status { get; set; } = "";
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>Webhook投递日志响应数据</summary>
    public sealed class WebhookDeliveryLogRepDTO
    {
        /// <summary>日志ID</summary>
        public long Id { get; set; }
        /// <summary>Webhook ID</summary>
        public long WebhookId { get; set; }
        /// <summary>事件类型</summary>
        public string EventType { get; set; } = "";
        /// <summary>HTTP状态码</summary>
        public int? HttpStatusCode { get; set; }
        /// <summary>投递状态</summary>
        public string DeliveryStatus { get; set; } = "";
        /// <summary>投递时间</summary>
        public DateTime DeliveredAt { get; set; }
    }
}
