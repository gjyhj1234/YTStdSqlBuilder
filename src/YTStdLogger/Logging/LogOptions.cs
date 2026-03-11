using System;
using System.IO;

namespace YTStdLogger.Logging;

/// <summary>
/// 日志配置项，默认值偏保守并适合低资源环境。
/// </summary>
public sealed class LogOptions
{
    /// <summary>
    /// 日志根目录。
    /// </summary>
    public string RootPath { get; set; } = Path.Combine(AppContext.BaseDirectory, "logs");

    /// <summary>
    /// 最小等级阈值，等级值小于等于该值的日志会被记录。
    /// </summary>
    public LogLevel MinimumLevel { get; set; } = LogLevel.Debug;

    /// <summary>
    /// 队列容量，必须为 2 的幂。
    /// </summary>
    public int QueueCapacity { get; set; } = 8192;

    /// <summary>
    /// 批量写入条数阈值。
    /// </summary>
    public int BatchSize { get; set; } = 128;

    /// <summary>
    /// 批量写入最大等待毫秒。
    /// </summary>
    public int BatchMaxDelayMs { get; set; } = 20;

    /// <summary>
    /// 日志保留月数。
    /// </summary>
    public int RetentionMonths { get; set; } = 3;

    /// <summary>
    /// 每秒最大日志条数。
    /// </summary>
    public int MaxLogsPerSecond { get; set; } = 20000;

    /// <summary>
    /// 丢弃摘要输出周期（秒）。
    /// </summary>
    public int DropSummaryIntervalSeconds { get; set; } = 10;

    /// <summary>
    /// 是否每批次强制 Flush。
    /// </summary>
    public bool FlushEveryBatch { get; set; }

    /// <summary>
    /// 是否使用 UTC 时间戳。
    /// </summary>
    public bool UseUtcTimestamp { get; set; }

    /// <summary>
    /// 校验配置合法性。
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(RootPath))
        {
            throw new ArgumentException("RootPath 不能为空。", nameof(RootPath));
        }

        if (QueueCapacity < 2 || (QueueCapacity & (QueueCapacity - 1)) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(QueueCapacity), "QueueCapacity 必须是 >=2 的 2 次幂。\n");
        }

        if (BatchSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(BatchSize));
        }

        if (BatchMaxDelayMs <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(BatchMaxDelayMs));
        }

        if (RetentionMonths <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(RetentionMonths));
        }

        if (MaxLogsPerSecond <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxLogsPerSecond));
        }

        if (DropSummaryIntervalSeconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(DropSummaryIntervalSeconds));
        }
    }
}
