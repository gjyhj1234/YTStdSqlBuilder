using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using YTStdLogger.Buffer;
using YTStdLogger.IO;
using YTStdLogger.Logging;
using YTStdLogger.Protection;
using YTStdLogger.Retention;

namespace YTStdLogger.Core;

/// <summary>
/// 日志核心引擎。
/// 线程模型：业务线程仅执行轻量入队；独立消费者线程负责批量格式化与落盘。
/// 异常策略：业务线程不抛出日志异常，消费者线程内部吞吐并输出 stderr。
/// </summary>
public sealed class LoggerEngine : IDisposable, IAsyncDisposable
{
    private readonly LogOptions _options;
    private readonly MpmcRingBuffer<LogMessage> _queue;
    private readonly LogMessagePool _pool;
    private readonly LogRateLimiter _rateLimiter;
    private readonly BatchedFileWriter _writer;
    private readonly LogRetentionCleaner _cleaner;
    private readonly ConcurrentDictionary<int, byte> _tenantDebugOverrides;
    private readonly Thread _consumerThread;
    private readonly CancellationTokenSource _cts;
    private volatile bool _accepting;
    private long _writtenCount;
    private long _dropCount;
    private int _stopState;
    private int _disposeState;

    /// <summary>
    /// 消费线程未处理异常通知。
    /// </summary>
    public event Action<Exception>? ConsumerFault;

    /// <summary>
    /// 已写入日志计数。
    /// </summary>
    public long WrittenCount => Interlocked.Read(ref _writtenCount);

    /// <summary>
    /// 丢弃日志计数。
    /// </summary>
    public long DroppedCount => Interlocked.Read(ref _dropCount);

    /// <summary>
    /// 初始化日志引擎并启动后台线程。
    /// </summary>
    public LoggerEngine(LogOptions options)
    {
        _options = options;
        _options.Validate();

        _queue = new MpmcRingBuffer<LogMessage>(_options.QueueCapacity);
        _pool = new LogMessagePool();
        _rateLimiter = new LogRateLimiter(_options.MaxLogsPerSecond, _options.DropSummaryIntervalSeconds);
        _writer = new BatchedFileWriter(new TenantDatePathResolver(_options.RootPath), new DefaultLogFormatter(), _options.FlushEveryBatch);
        _cleaner = new LogRetentionCleaner(_options.RootPath, _options.RetentionMonths);
        _tenantDebugOverrides = new ConcurrentDictionary<int, byte>();
        _cts = new CancellationTokenSource();
        _accepting = true;
        _consumerThread = new Thread(ConsumeLoop)
        {
            IsBackground = true,
            Name = "YTStdLogger.Consumer"
        };
        _consumerThread.Start();
    }

    /// <summary>
    /// 判断指定等级是否可记录。
    /// </summary>
    public bool IsEnabled(LogLevel level)
    {
        return level <= _options.MinimumLevel;
    }

    /// <summary>
    /// 判断指定租户与等级是否可记录。
    /// 当租户被显式开启 Debug 时，将忽略全局等级限制。
    /// </summary>
    public bool IsEnabled(int tenantId, LogLevel level)
    {
        if (IsEnabled(level))
        {
            return true;
        }

        return IsTenantDebugEnabled(tenantId);
    }

    /// <summary>
    /// 运行时开启指定租户 Debug。
    /// 开启后该租户可输出 Debug 及以下全部等级日志，不影响其他租户。
    /// </summary>
    public void EnableTenantDebug(int tenantId)
    {
        _tenantDebugOverrides[tenantId] = 1;
    }

    /// <summary>
    /// 运行时关闭指定租户 Debug。
    /// </summary>
    /// <returns>若原本存在开关则返回 <c>true</c>。</returns>
    public bool DisableTenantDebug(int tenantId)
    {
        return _tenantDebugOverrides.TryRemove(tenantId, out _);
    }

    /// <summary>
    /// 判断指定租户是否已开启 Debug 覆盖。
    /// </summary>
    public bool IsTenantDebugEnabled(int tenantId)
    {
        return _tenantDebugOverrides.ContainsKey(tenantId);
    }

    /// <summary>
    /// 写入日志。
    /// </summary>
    public void Log(int tenantId, long userId, LogLevel level, string message)
    {
        if (!_accepting)
        {
            return;
        }

        if (!IsEnabled(tenantId, level))
        {
            return;
        }

        if (!_rateLimiter.TryAcquire())
        {
            Interlocked.Increment(ref _dropCount);
            return;
        }

        LogMessage entry = _pool.Rent(message);
        entry.TenantId = tenantId;
        entry.UserId = userId;
        entry.Level = level;
        entry.Timestamp = _options.UseUtcTimestamp ? DateTime.UtcNow : DateTime.Now;
        entry.ThreadId = Environment.CurrentManagedThreadId;

        if (!_queue.TryEnqueue(entry))
        {
            _pool.Return(entry);
            _rateLimiter.MarkDroppedByQueue();
            Interlocked.Increment(ref _dropCount);
        }
    }

    /// <summary>
    /// 写入致命日志。
    /// </summary>
    public void Fatal(int tenantId, long userId, string message)
    {
        Log(tenantId, userId, LogLevel.Fatal, message);
    }

    /// <summary>
    /// 写入错误日志。
    /// </summary>
    public void Error(int tenantId, long userId, string message)
    {
        Log(tenantId, userId, LogLevel.Error, message);
    }

    /// <summary>
    /// 写入警告日志。
    /// </summary>
    public void Warn(int tenantId, long userId, string message)
    {
        Log(tenantId, userId, LogLevel.Warn, message);
    }

    /// <summary>
    /// 写入信息日志。
    /// </summary>
    public void Infor(int tenantId, long userId, string message)
    {
        Log(tenantId, userId, LogLevel.Infor, message);
    }

    /// <summary>
    /// 写入信息日志。
    /// 与 <see cref="Infor(int,long,string)"/> 语义一致，提供更常见命名。
    /// </summary>
    public void Info(int tenantId, long userId, string message)
    {
        Log(tenantId, userId, LogLevel.Infor, message);
    }

    /// <summary>
    /// 写入调试日志。
    /// </summary>
    public void Debug(int tenantId, long userId, string message)
    {
        Log(tenantId, userId, LogLevel.Debug, message);
    }

    /// <summary>
    /// 停止接收并排空队列。
    /// 幂等：重复调用会直接返回。
    /// </summary>
    public ValueTask StopAsync()
    {
        if (Interlocked.Exchange(ref _stopState, 1) != 0)
        {
            return ValueTask.CompletedTask;
        }

        _accepting = false;
        _cts.Cancel();
        if (!_consumerThread.Join(TimeSpan.FromSeconds(10)))
        {
            Console.Error.WriteLine("[YTStdLogger] 消费线程未在预期时间结束。");
        }

        _writer.FlushAll();
        return ValueTask.CompletedTask;
    }

    private void ConsumeLoop()
    {
        LogMessage[] batch = new LogMessage[_options.BatchSize];
        DateTime lastFlushTime = DateTime.UtcNow;
        int idleSpin = 0;

        try
        {
            while (true)
            {
                int count = _queue.TryDequeueBatch(batch);
                if (count > 0)
                {
                    _writer.WriteBatch(new ReadOnlySpan<LogMessage>(batch, 0, count));
                    Interlocked.Add(ref _writtenCount, count);
                    for (int i = 0; i < count; i++)
                    {
                        _pool.Return(batch[i]);
                        batch[i] = null!;
                    }

                    lastFlushTime = DateTime.UtcNow;
                    idleSpin = 0;
                }
                else
                {
                    idleSpin++;
                    if (idleSpin < 20)
                    {
                        Thread.SpinWait(64);
                    }
                    else
                    {
                        Thread.Sleep(1);
                    }
                }

                if ((DateTime.UtcNow - lastFlushTime).TotalMilliseconds >= _options.BatchMaxDelayMs)
                {
                    _writer.FlushAll();
                    lastFlushTime = DateTime.UtcNow;
                }

                if (_rateLimiter.TryBuildSummary(out long dropped) && dropped > 0)
                {
                    string summary = "过去 " + _options.DropSummaryIntervalSeconds.ToString() + " 秒丢弃 " + dropped.ToString() + " 条日志。";
                    _writer.WriteInternalWarning(_options.UseUtcTimestamp ? DateTime.UtcNow : DateTime.Now, summary);
                }

                if (_cts.IsCancellationRequested && !_accepting)
                {
                    while (_queue.TryDequeue(out LogMessage? item))
                    {
                        if (item is not null)
                        {
                            batch[0] = item;
                            _writer.WriteBatch(new ReadOnlySpan<LogMessage>(batch, 0, 1));
                            Interlocked.Increment(ref _writtenCount);
                            _pool.Return(item);
                        }
                    }

                    break;
                }
            }
        }
        catch (Exception ex)
        {
            ConsumerFault?.Invoke(ex);
            Console.Error.WriteLine("[YTStdLogger][Consumer] " + ex.Message);
        }
        finally
        {
            _writer.FlushAll();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposeState, 1) != 0)
        {
            return;
        }

        StopAsync().GetAwaiter().GetResult();
        DisposeResources();
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposeState, 1) != 0)
        {
            return;
        }

        await StopAsync().ConfigureAwait(false);
        DisposeResources();
    }

    private void DisposeResources()
    {
        _writer.Dispose();
        _cleaner.Dispose();
        _cts.Dispose();
    }
}
