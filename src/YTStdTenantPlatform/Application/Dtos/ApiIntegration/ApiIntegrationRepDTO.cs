using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>租户 API 密钥列表项</summary>
    public sealed class TenantApiKeyRepDTO
    {
        /// <summary>密钥 ID</summary>
        public long Id { get; set; }
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>密钥名称</summary>
        public string KeyName { get; set; } = "";
        /// <summary>访问密钥</summary>
        public string AccessKey { get; set; } = "";
        /// <summary>状态</summary>
        public string Status { get; set; } = "";
        /// <summary>配额上限</summary>
        public long? QuotaLimit { get; set; }
        /// <summary>速率限制</summary>
        public int? RateLimit { get; set; }
        /// <summary>最后使用时间</summary>
        public DateTime? LastUsedAt { get; set; }
        /// <summary>过期时间</summary>
        public DateTime? ExpiresAt { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>API 密钥创建结果（包含仅在创建时返回的密钥明文）</summary>
    public sealed class ApiKeyCreatedRepDTO
    {
        /// <summary>密钥 ID</summary>
        public long Id { get; set; }
        /// <summary>Access Key</summary>
        public string AccessKey { get; set; } = "";
        /// <summary>Secret Key（仅创建时返回一次，后续不可查看）</summary>
        public string SecretKey { get; set; } = "";
    }

    /// <summary>租户 API 用量统计列表项</summary>
    public sealed class TenantApiUsageStatRepDTO
    {
        /// <summary>统计 ID</summary>
        public long Id { get; set; }
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>API 密钥 ID</summary>
        public long? ApiKeyId { get; set; }
        /// <summary>统计日期</summary>
        public DateTime StatDate { get; set; }
        /// <summary>API 路径</summary>
        public string ApiPath { get; set; } = "";
        /// <summary>请求次数</summary>
        public long RequestCount { get; set; }
        /// <summary>成功次数</summary>
        public long SuccessCount { get; set; }
        /// <summary>错误次数</summary>
        public long ErrorCount { get; set; }
        /// <summary>平均延迟（毫秒）</summary>
        public int AverageLatencyMs { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>Webhook 事件列表项</summary>
    public sealed class WebhookEventRepDTO
    {
        /// <summary>事件 ID</summary>
        public long Id { get; set; }
        /// <summary>事件编码</summary>
        public string EventCode { get; set; } = "";
        /// <summary>事件名称</summary>
        public string EventName { get; set; } = "";
        /// <summary>描述</summary>
        public string? Description { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>租户 Webhook 列表项</summary>
    public sealed class TenantWebhookRepDTO
    {
        /// <summary>Webhook ID</summary>
        public long Id { get; set; }
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>Webhook 名称</summary>
        public string WebhookName { get; set; } = "";
        /// <summary>目标地址</summary>
        public string TargetUrl { get; set; } = "";
        /// <summary>状态</summary>
        public string Status { get; set; } = "";
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>Webhook 投递日志列表项</summary>
    public sealed class WebhookDeliveryLogRepDTO
    {
        /// <summary>日志 ID</summary>
        public long Id { get; set; }
        /// <summary>Webhook ID</summary>
        public long WebhookId { get; set; }
        /// <summary>事件 ID</summary>
        public long? EventId { get; set; }
        /// <summary>投递状态</summary>
        public string DeliveryStatus { get; set; } = "";
        /// <summary>响应状态码</summary>
        public int? ResponseStatusCode { get; set; }
        /// <summary>重试次数</summary>
        public int RetryCount { get; set; }
        /// <summary>投递时间</summary>
        public DateTime? DeliveredAt { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }
}
