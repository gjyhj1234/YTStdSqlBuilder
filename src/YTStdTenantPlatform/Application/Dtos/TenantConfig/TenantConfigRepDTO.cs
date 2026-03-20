using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>租户系统配置</summary>
    public sealed class TenantSystemConfigRepDTO
    {
        /// <summary>配置 ID</summary>
        public long Id { get; set; }
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>系统名称</summary>
        public string? SystemName { get; set; }
        /// <summary>Logo 地址</summary>
        public string? LogoUrl { get; set; }
        /// <summary>系统主题</summary>
        public string? SystemTheme { get; set; }
        /// <summary>默认语言</summary>
        public string? DefaultLanguage { get; set; }
        /// <summary>默认时区</summary>
        public string? DefaultTimezone { get; set; }
        /// <summary>更新时间</summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>租户功能开关列表项</summary>
    public sealed class TenantFeatureFlagRepDTO
    {
        /// <summary>功能开关 ID</summary>
        public long Id { get; set; }
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>功能键</summary>
        public string FeatureKey { get; set; } = "";
        /// <summary>功能名称</summary>
        public string FeatureName { get; set; } = "";
        /// <summary>是否启用</summary>
        public bool Enabled { get; set; }
        /// <summary>灰度类型</summary>
        public string RolloutType { get; set; } = "";
        /// <summary>更新时间</summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>租户参数列表项</summary>
    public sealed class TenantParameterRepDTO
    {
        /// <summary>参数 ID</summary>
        public long Id { get; set; }
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>参数键</summary>
        public string ParamKey { get; set; } = "";
        /// <summary>参数名称</summary>
        public string ParamName { get; set; } = "";
        /// <summary>参数类型</summary>
        public string ParamType { get; set; } = "";
        /// <summary>参数值</summary>
        public string ParamValue { get; set; } = "";
        /// <summary>更新时间</summary>
        public DateTime UpdatedAt { get; set; }
    }
}
