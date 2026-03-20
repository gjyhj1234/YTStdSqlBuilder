using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>创建API密钥请求参数</summary>
    public sealed class CreateApiKeyReqDTO
    {
        /// <summary>租户ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>密钥名称</summary>
        public string KeyName { get; set; } = "";
        /// <summary>描述</summary>
        public string? Description { get; set; }
        /// <summary>过期时间</summary>
        public DateTime? ExpiresAt { get; set; }
    }

    /// <summary>创建Webhook请求参数</summary>
    public sealed class CreateWebhookReqDTO
    {
        /// <summary>租户ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>Webhook名称</summary>
        public string WebhookName { get; set; } = "";
        /// <summary>目标URL</summary>
        public string TargetUrl { get; set; } = "";
        /// <summary>秘钥</summary>
        public string? Secret { get; set; }
    }

    /// <summary>更新Webhook请求参数</summary>
    public sealed class UpdateWebhookReqDTO
    {
        /// <summary>Webhook名称</summary>
        public string? WebhookName { get; set; }
        /// <summary>目标URL</summary>
        public string? TargetUrl { get; set; }
        /// <summary>秘钥</summary>
        public string? Secret { get; set; }
    }
}
