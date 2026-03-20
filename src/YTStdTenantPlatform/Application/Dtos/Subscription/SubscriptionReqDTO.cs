using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>创建订阅请求参数</summary>
    public sealed class CreateSubscriptionReqDTO
    {
        /// <summary>租户ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>套餐版本ID</summary>
        public long PackageVersionId { get; set; }
        /// <summary>订阅来源</summary>
        public string SubscriptionSource { get; set; } = "manual";
    }

    /// <summary>创建试用请求参数</summary>
    public sealed class CreateTrialReqDTO
    {
        /// <summary>租户ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>套餐版本ID</summary>
        public long PackageVersionId { get; set; }
        /// <summary>试用天数</summary>
        public int TrialDays { get; set; } = 14;
    }
}
