using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>租户日统计响应数据</summary>
    public sealed class TenantDailyStatRepDTO
    {
        /// <summary>统计ID</summary>
        public long Id { get; set; }
        /// <summary>统计日期</summary>
        public DateTime StatDate { get; set; }
        /// <summary>新增租户数</summary>
        public int NewTenantCount { get; set; }
        /// <summary>活跃租户数</summary>
        public int ActiveTenantCount { get; set; }
        /// <summary>总租户数</summary>
        public int TotalTenantCount { get; set; }
    }

    /// <summary>平台监控指标响应数据</summary>
    public sealed class PlatformMonitorMetricRepDTO
    {
        /// <summary>指标ID</summary>
        public long Id { get; set; }
        /// <summary>指标名称</summary>
        public string MetricName { get; set; } = "";
        /// <summary>指标值</summary>
        public string MetricValue { get; set; } = "";
        /// <summary>采集时间</summary>
        public DateTime CollectedAt { get; set; }
    }
}
