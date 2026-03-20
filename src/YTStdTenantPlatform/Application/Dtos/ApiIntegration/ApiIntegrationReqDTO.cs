using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>创建 API 密钥请求</summary>
    public sealed class CreateApiKeyReqDTO
    {
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>密钥名称</summary>
        public string KeyName { get; set; } = "";
        /// <summary>过期时间</summary>
        public DateTime? ExpiresAt { get; set; }
    }

    /// <summary>创建 Webhook 请求</summary>
    public sealed class CreateWebhookReqDTO
    {
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>Webhook 名称</summary>
        public string WebhookName { get; set; } = "";
        /// <summary>目标地址</summary>
        public string TargetUrl { get; set; } = "";
    }

    /// <summary>更新 Webhook 请求</summary>
    public sealed class UpdateWebhookReqDTO
    {
        /// <summary>Webhook 名称</summary>
        public string? WebhookName { get; set; }
        /// <summary>目标地址</summary>
        public string? TargetUrl { get; set; }
    }
}
