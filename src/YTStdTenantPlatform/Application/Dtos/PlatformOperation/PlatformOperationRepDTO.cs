using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>租户每日统计列表项</summary>
    public sealed class TenantDailyStatRepDTO
    {
        /// <summary>统计 ID</summary>
        public long Id { get; set; }
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>统计日期</summary>
        public DateTime StatDate { get; set; }
        /// <summary>活跃用户数</summary>
        public int ActiveUserCount { get; set; }
        /// <summary>新增用户数</summary>
        public int NewUserCount { get; set; }
        /// <summary>API 调用次数</summary>
        public long ApiCallCount { get; set; }
        /// <summary>存储字节数</summary>
        public long StorageBytes { get; set; }
        /// <summary>资源评分</summary>
        public decimal ResourceScore { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>平台监控指标列表项</summary>
    public sealed class PlatformMonitorMetricRepDTO
    {
        /// <summary>指标 ID</summary>
        public long Id { get; set; }
        /// <summary>组件名称</summary>
        public string ComponentName { get; set; } = "";
        /// <summary>指标类型</summary>
        public string MetricType { get; set; } = "";
        /// <summary>指标键</summary>
        public string MetricKey { get; set; } = "";
        /// <summary>指标值</summary>
        public decimal MetricValue { get; set; }
        /// <summary>指标单位</summary>
        public string? MetricUnit { get; set; }
        /// <summary>采集时间</summary>
        public DateTime CollectedAt { get; set; }
    }
}
