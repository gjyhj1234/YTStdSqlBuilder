using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>更新租户系统配置请求</summary>
    public sealed class UpdateTenantSystemConfigReqDTO
    {
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
    }

    /// <summary>创建/更新租户功能开关请求</summary>
    public sealed class SaveTenantFeatureFlagReqDTO
    {
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>功能键</summary>
        public string FeatureKey { get; set; } = "";
        /// <summary>功能名称</summary>
        public string FeatureName { get; set; } = "";
        /// <summary>是否启用</summary>
        public bool Enabled { get; set; }
        /// <summary>灰度类型（all/percentage/whitelist）</summary>
        public string RolloutType { get; set; } = "all";
    }

    /// <summary>创建/更新租户参数请求</summary>
    public sealed class SaveTenantParameterReqDTO
    {
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>参数键</summary>
        public string ParamKey { get; set; } = "";
        /// <summary>参数名称</summary>
        public string ParamName { get; set; } = "";
        /// <summary>参数类型（string/number/boolean/json）</summary>
        public string ParamType { get; set; } = "string";
        /// <summary>参数值</summary>
        public string ParamValue { get; set; } = "";
    }
}
