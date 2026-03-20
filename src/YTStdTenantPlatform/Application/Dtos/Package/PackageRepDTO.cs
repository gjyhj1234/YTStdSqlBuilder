using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>SaaS 套餐列表项</summary>
    public sealed class SaasPackageRepDTO
    {
        /// <summary>套餐 ID</summary>
        public long Id { get; set; }
        /// <summary>套餐编码</summary>
        public string PackageCode { get; set; } = "";
        /// <summary>套餐名称</summary>
        public string PackageName { get; set; } = "";
        /// <summary>描述</summary>
        public string? Description { get; set; }
        /// <summary>状态</summary>
        public string Status { get; set; } = "";
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>SaaS 套餐版本列表项</summary>
    public sealed class SaasPackageVersionRepDTO
    {
        /// <summary>版本 ID</summary>
        public long Id { get; set; }
        /// <summary>套餐 ID</summary>
        public long PackageId { get; set; }
        /// <summary>版本编码</summary>
        public string VersionCode { get; set; } = "";
        /// <summary>版本名称</summary>
        public string VersionName { get; set; } = "";
        /// <summary>版本类型</summary>
        public string EditionType { get; set; } = "";
        /// <summary>计费周期</summary>
        public string BillingCycle { get; set; } = "";
        /// <summary>价格</summary>
        public decimal Price { get; set; }
        /// <summary>货币编码</summary>
        public string CurrencyCode { get; set; } = "";
        /// <summary>试用天数</summary>
        public int TrialDays { get; set; }
        /// <summary>是否默认</summary>
        public bool IsDefault { get; set; }
        /// <summary>是否启用</summary>
        public bool Enabled { get; set; }
        /// <summary>生效开始时间</summary>
        public DateTime? EffectiveFrom { get; set; }
        /// <summary>生效结束时间</summary>
        public DateTime? EffectiveTo { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>SaaS 套餐能力列表项</summary>
    public sealed class SaasPackageCapabilityRepDTO
    {
        /// <summary>能力 ID</summary>
        public long Id { get; set; }
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
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }
}
