using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>平台监控指标</summary>
[Entity(TableName = "platform_monitor_metrics")]
[Index("idx_platform_monitor_metrics_type_time", "metric_type", "collected_at")]
public class PlatformMonitorMetric
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>组件名称</summary>
    [Column(Length = 128, IsRequired = true)]
    public string ComponentName { get; set; } = "";

    /// <summary>指标类型</summary>
    [Column(Length = 32, IsRequired = true)]
    public string MetricType { get; set; } = "";

    /// <summary>指标键</summary>
    [Column(Length = 128, IsRequired = true)]
    public string MetricKey { get; set; } = "";

    /// <summary>指标值</summary>
    [Column(Length = 18, Precision = 4, IsRequired = true)]
    public decimal MetricValue { get; set; }

    /// <summary>指标单位</summary>
    [Column(Length = 32)]
    public string? MetricUnit { get; set; }

    /// <summary>采集时间</summary>
    public DateTime CollectedAt { get; set; }
}
