using System;
using YTStdLogger.Logging;

namespace YTStdLogger.Core;

/// <summary>
/// 日志工厂。
/// </summary>
public static class LoggerFactory
{
    private static readonly object Gate = new object();
    private static LoggerEngine? _singleton;
    private static LogOptions? _configuredOptions;

    /// <summary>
    /// 创建或获取全局唯一日志引擎。
    /// </summary>
    public static LoggerEngine Create(LogOptions options)
    {
        LoggerEngine? existing = _singleton;
        if (existing is not null)
        {
            EnsureSameConfiguration(options);
            return existing;
        }

        lock (Gate)
        {
            if (_singleton is null)
            {
                _configuredOptions = CloneOptions(options);
                _singleton = new LoggerEngine(options);
            }
            else
            {
                EnsureSameConfiguration(options);
            }

            return _singleton;
        }
    }

    /// <summary>
    /// 重置全局单例引用。
    /// 仅在应用生命周期内确需重建引擎时调用。
    /// </summary>
    public static void Reset()
    {
        lock (Gate)
        {
            _singleton = null;
            _configuredOptions = null;
        }
    }

    private static void EnsureSameConfiguration(LogOptions options)
    {
        LogOptions? current = _configuredOptions;
        if (current is null)
        {
            return;
        }

        if (!Same(current.RootPath, options.RootPath) ||
            current.MinimumLevel != options.MinimumLevel ||
            current.QueueCapacity != options.QueueCapacity ||
            current.BatchSize != options.BatchSize ||
            current.BatchMaxDelayMs != options.BatchMaxDelayMs ||
            current.RetentionMonths != options.RetentionMonths ||
            current.MaxLogsPerSecond != options.MaxLogsPerSecond ||
            current.DropSummaryIntervalSeconds != options.DropSummaryIntervalSeconds ||
            current.FlushEveryBatch != options.FlushEveryBatch ||
            current.UseUtcTimestamp != options.UseUtcTimestamp)
        {
            throw new InvalidOperationException("LoggerFactory 已初始化，禁止使用不同配置重复创建全局单例。请先调用 Reset() 后重建。");
        }
    }

    private static LogOptions CloneOptions(LogOptions options)
    {
        return new LogOptions
        {
            RootPath = options.RootPath,
            MinimumLevel = options.MinimumLevel,
            QueueCapacity = options.QueueCapacity,
            BatchSize = options.BatchSize,
            BatchMaxDelayMs = options.BatchMaxDelayMs,
            RetentionMonths = options.RetentionMonths,
            MaxLogsPerSecond = options.MaxLogsPerSecond,
            DropSummaryIntervalSeconds = options.DropSummaryIntervalSeconds,
            FlushEveryBatch = options.FlushEveryBatch,
            UseUtcTimestamp = options.UseUtcTimestamp
        };
    }

    private static bool Same(string left, string right)
    {
        return string.Equals(left, right, StringComparison.Ordinal);
    }
}
