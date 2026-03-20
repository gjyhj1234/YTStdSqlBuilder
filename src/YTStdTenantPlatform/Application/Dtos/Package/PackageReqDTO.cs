using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>创建 SaaS 套餐请求</summary>
    public sealed class CreateSaasPackageReqDTO
    {
        /// <summary>套餐编码</summary>
        public string PackageCode { get; set; } = "";
        /// <summary>套餐名称</summary>
        public string PackageName { get; set; } = "";
        /// <summary>描述</summary>
        public string? Description { get; set; }
    }

    /// <summary>更新 SaaS 套餐请求</summary>
    public sealed class UpdateSaasPackageReqDTO
    {
        /// <summary>套餐名称</summary>
        public string? PackageName { get; set; }
        /// <summary>描述</summary>
        public string? Description { get; set; }
    }

    /// <summary>创建 SaaS 套餐版本请求</summary>
    public sealed class CreateSaasPackageVersionReqDTO
    {
        /// <summary>套餐 ID</summary>
        public long PackageId { get; set; }
        /// <summary>版本编码</summary>
        public string VersionCode { get; set; } = "";
        /// <summary>版本名称</summary>
        public string VersionName { get; set; } = "";
        /// <summary>版本类型</summary>
        public string EditionType { get; set; } = "";
        /// <summary>计费周期（monthly/quarterly/yearly）</summary>
        public string BillingCycle { get; set; } = "monthly";
        /// <summary>价格</summary>
        public decimal Price { get; set; }
        /// <summary>货币编码</summary>
        public string CurrencyCode { get; set; } = "CNY";
        /// <summary>试用天数</summary>
        public int TrialDays { get; set; } = 0;
        /// <summary>是否默认</summary>
        public bool IsDefault { get; set; } = false;
    }

    /// <summary>创建/更新 SaaS 套餐能力请求</summary>
    public sealed class SaveSaasPackageCapabilityReqDTO
    {
        /// <summary>套餐版本 ID</summary>
        public long PackageVersionId { get; set; }
        /// <summary>能力键</summary>
        public string CapabilityKey { get; set; } = "";
        /// <summary>能力名称</summary>
        public string CapabilityName { get; set; } = "";
        /// <summary>能力类型</summary>
        public string CapabilityType { get; set; } = "";
        /// <summary>能力值</summary>
        public string CapabilityValue { get; set; } = "";
    }
}
