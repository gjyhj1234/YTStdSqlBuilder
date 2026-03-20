using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>租户订阅响应数据</summary>
    public sealed class TenantSubscriptionRepDTO
    {
        /// <summary>订阅ID</summary>
        public long Id { get; set; }
        /// <summary>租户ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>套餐版本ID</summary>
        public long PackageVersionId { get; set; }
        /// <summary>订阅状态</summary>
        public string SubscriptionStatus { get; set; } = "";
        /// <summary>开始日期</summary>
        public DateTime? StartDate { get; set; }
        /// <summary>结束日期</summary>
        public DateTime? EndDate { get; set; }
        /// <summary>自动续费</summary>
        public bool AutoRenew { get; set; }
        /// <summary>订阅来源</summary>
        public string SubscriptionSource { get; set; } = "";
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>租户试用响应数据</summary>
    public sealed class TenantTrialRepDTO
    {
        /// <summary>试用ID</summary>
        public long Id { get; set; }
        /// <summary>租户ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>套餐版本ID</summary>
        public long PackageVersionId { get; set; }
        /// <summary>试用状态</summary>
        public string TrialStatus { get; set; } = "";
        /// <summary>开始日期</summary>
        public DateTime? StartDate { get; set; }
        /// <summary>结束日期</summary>
        public DateTime? EndDate { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>租户订阅变更记录响应数据</summary>
    public sealed class TenantSubscriptionChangeRepDTO
    {
        /// <summary>变更ID</summary>
        public long Id { get; set; }
        /// <summary>订阅ID</summary>
        public long SubscriptionId { get; set; }
        /// <summary>变更类型</summary>
        public string ChangeType { get; set; } = "";
        /// <summary>原套餐版本ID</summary>
        public long? FromVersionId { get; set; }
        /// <summary>目标套餐版本ID</summary>
        public long? ToVersionId { get; set; }
        /// <summary>变更原因</summary>
        public string? ChangeReason { get; set; }
        /// <summary>变更时间</summary>
        public DateTime ChangedAt { get; set; }
    }
}
