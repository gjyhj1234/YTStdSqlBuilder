using System;
using System.Threading;

namespace YTStdLogger.Protection;

/// <summary>
/// 日志限速器，按秒限制进入系统的日志总数，并统计丢弃数量。
/// </summary>
public sealed class LogRateLimiter
{
    private readonly int _maxPerSecond;
    private readonly int _summaryIntervalSeconds;
    private long _windowSecond;
    private int _windowCount;
    private long _droppedTotal;
    private long _lastSummaryTicks;

    /// <summary>
    /// 初始化限速器。
    /// </summary>
    public LogRateLimiter(int maxPerSecond, int summaryIntervalSeconds)
    {
        _maxPerSecond = maxPerSecond;
        _summaryIntervalSeconds = summaryIntervalSeconds;
        _windowSecond = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        _lastSummaryTicks = Environment.TickCount64;
    }

    /// <summary>
    /// 尝试放行一条日志。
    /// </summary>
    public bool TryAcquire()
    {
        long sec = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long old = Volatile.Read(ref _windowSecond);
        if (old != sec)
        {
            if (Interlocked.CompareExchange(ref _windowSecond, sec, old) == old)
            {
                Volatile.Write(ref _windowCount, 0);
            }
        }

        int value = Interlocked.Increment(ref _windowCount);
        if (value > _maxPerSecond)
        {
            Interlocked.Increment(ref _droppedTotal);
            return false;
        }

        return true;
    }

    /// <summary>
    /// 记录队列满导致的丢弃。
    /// </summary>
    public void MarkDroppedByQueue()
    {
        Interlocked.Increment(ref _droppedTotal);
    }

    /// <summary>
    /// 尝试生成丢弃摘要。
    /// </summary>
    public bool TryBuildSummary(out long dropped)
    {
        long now = Environment.TickCount64;
        long last = Volatile.Read(ref _lastSummaryTicks);
        if (now - last < _summaryIntervalSeconds * 1000L)
        {
            dropped = 0;
            return false;
        }

        if (Interlocked.CompareExchange(ref _lastSummaryTicks, now, last) != last)
        {
            dropped = 0;
            return false;
        }

        dropped = Interlocked.Exchange(ref _droppedTotal, 0);
        return dropped > 0;
    }
}
