using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>租户订阅列表项</summary>
    public sealed class TenantSubscriptionRepDTO
    {
        /// <summary>订阅 ID</summary>
        public long Id { get; set; }
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>套餐版本 ID</summary>
        public long PackageVersionId { get; set; }
        /// <summary>订阅状态</summary>
        public string SubscriptionStatus { get; set; } = "";
        /// <summary>订阅类型</summary>
        public string SubscriptionType { get; set; } = "";
        /// <summary>开始时间</summary>
        public DateTime StartedAt { get; set; }
        /// <summary>到期时间</summary>
        public DateTime ExpiresAt { get; set; }
        /// <summary>是否自动续费</summary>
        public bool AutoRenew { get; set; }
        /// <summary>取消时间</summary>
        public DateTime? CancelledAt { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>租户试用列表项</summary>
    public sealed class TenantTrialRepDTO
    {
        /// <summary>试用 ID</summary>
        public long Id { get; set; }
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>套餐版本 ID</summary>
        public long? PackageVersionId { get; set; }
        /// <summary>状态</summary>
        public string Status { get; set; } = "";
        /// <summary>开始时间</summary>
        public DateTime StartedAt { get; set; }
        /// <summary>到期时间</summary>
        public DateTime ExpiresAt { get; set; }
        /// <summary>转化订阅 ID</summary>
        public long? ConvertedSubscriptionId { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>租户订阅变更列表项</summary>
    public sealed class TenantSubscriptionChangeRepDTO
    {
        /// <summary>变更 ID</summary>
        public long Id { get; set; }
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>订阅 ID</summary>
        public long? SubscriptionId { get; set; }
        /// <summary>变更类型</summary>
        public string ChangeType { get; set; } = "";
        /// <summary>原套餐版本 ID</summary>
        public long? FromPackageVersionId { get; set; }
        /// <summary>目标套餐版本 ID</summary>
        public long? ToPackageVersionId { get; set; }
        /// <summary>生效时间</summary>
        public DateTime EffectiveAt { get; set; }
        /// <summary>备注</summary>
        public string? Remark { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }
}
