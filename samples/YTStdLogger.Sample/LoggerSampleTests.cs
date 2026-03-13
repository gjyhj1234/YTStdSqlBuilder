using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using YTStdLogger.Buffer;
using YTStdLogger.Core;
using YTStdLogger.IO;
using YTStdLogger.Logging;
using YTStdLogger.Protection;
using YTStdLogger.Retention;

namespace YTStdLogger.Sample;

/// <summary>
/// 日志系统示例测试。
/// 演示 LoggerEngine、Logger 静态门面、BatchedFileWriter、LogRateLimiter 等核心组件的用法。
/// </summary>
public class LoggerSampleTests
{
    /// <summary>
    /// 演示 Logger 初始化与基本日志写入。
    /// </summary>
    [Fact]
    public async Task Sample_Logger_Init_And_Write()
    {
        string root = Path.Combine(Path.GetTempPath(), "ytlog_sample_" + Guid.NewGuid().ToString("N"));
        try
        {
            var options = new LogOptions
            {
                RootPath = root,
                MinimumLevel = LogLevel.Debug,
                QueueCapacity = 1024,
                BatchSize = 64,
                BatchMaxDelayMs = 100,
                RetentionMonths = 1,
                MaxLogsPerSecond = 10000,
                FlushEveryBatch = true,
                UseUtcTimestamp = false
            };

            Logger.Init(options);

            // 写入各等级日志
            Logger.Fatal(1, 100, "致命错误示例");
            Logger.Error(1, 100, "错误日志示例");
            Logger.Warn(1, 100, "警告日志示例");
            Logger.Info(1, 100, "信息日志示例");
            Logger.Debug(1, 100, "调试日志示例");

            // 延迟求值重载（仅在等级启用时才执行工厂方法）
            Logger.Debug(1, 100, () => "延迟求值日志: " + DateTime.Now.ToString());

            // 等待消费线程处理
            Thread.Sleep(500);

            Assert.True(Logger.WrittenCount > 0, "应有日志被写入");

            await Logger.ShutdownAsync();

            // 验证日志文件已创建
            Assert.True(Directory.Exists(root), "日志根目录应已创建");
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, true);
        }
    }

    /// <summary>
    /// 演示租户级别 Debug 开关。
    /// </summary>
    [Fact]
    public async Task Sample_TenantDebug_Toggle()
    {
        string root = Path.Combine(Path.GetTempPath(), "ytlog_tenant_" + Guid.NewGuid().ToString("N"));
        try
        {
            var options = new LogOptions
            {
                RootPath = root,
                MinimumLevel = LogLevel.Error, // 全局仅允许 Error 及以上
                QueueCapacity = 1024,
                BatchSize = 64,
                BatchMaxDelayMs = 100,
                RetentionMonths = 1,
                MaxLogsPerSecond = 10000,
                FlushEveryBatch = true
            };

            Logger.Init(options);

            // 全局等级为 Error，Debug 日志不会记录
            Logger.Debug(1, 100, "此 Debug 不会被记录");

            // 开启租户 1 的 Debug 覆盖
            Logger.EnableTenantDebug(1);
            Assert.True(Logger.IsTenantDebugEnabled(1));

            // 现在租户 1 的 Debug 日志会被记录
            Logger.Debug(1, 100, "租户1的 Debug 日志已开启");

            // 关闭租户 Debug
            bool wasEnabled = Logger.DisableTenantDebug(1);
            Assert.True(wasEnabled);
            Assert.False(Logger.IsTenantDebugEnabled(1));

            Thread.Sleep(500);
            await Logger.ShutdownAsync();
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, true);
        }
    }

    /// <summary>
    /// 演示 LoggerEngine 直接使用。
    /// </summary>
    [Fact]
    public async Task Sample_LoggerEngine_Direct()
    {
        string root = Path.Combine(Path.GetTempPath(), "ytlog_engine_" + Guid.NewGuid().ToString("N"));
        try
        {
            var options = new LogOptions
            {
                RootPath = root,
                MinimumLevel = LogLevel.Debug,
                QueueCapacity = 256,
                BatchSize = 32,
                BatchMaxDelayMs = 50,
                RetentionMonths = 1,
                MaxLogsPerSecond = 5000,
                FlushEveryBatch = true
            };

            await using var engine = new LoggerEngine(options);

            engine.Info(1, 1, "引擎直接写入信息日志");
            engine.Error(2, 2, "引擎直接写入错误日志");

            Thread.Sleep(300);
            Assert.True(engine.WrittenCount > 0);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, true);
        }
    }

    /// <summary>
    /// 演示 LogRateLimiter 限速器。
    /// </summary>
    [Fact]
    public void Sample_RateLimiter()
    {
        var limiter = new LogRateLimiter(maxPerSecond: 10, summaryIntervalSeconds: 5);

        int acquired = 0;
        for (int i = 0; i < 20; i++)
        {
            if (limiter.TryAcquire())
                acquired++;
        }

        // 前 10 条应放行，后 10 条应被限速
        Assert.Equal(10, acquired);
    }

    /// <summary>
    /// 演示 MpmcRingBuffer 无锁队列。
    /// </summary>
    [Fact]
    public void Sample_MpmcRingBuffer()
    {
        var buffer = new MpmcRingBuffer<string>(8);

        Assert.True(buffer.TryEnqueue("a"));
        Assert.True(buffer.TryEnqueue("b"));
        Assert.True(buffer.TryEnqueue("c"));

        Assert.True(buffer.TryDequeue(out string? v1));
        Assert.Equal("a", v1);

        Assert.True(buffer.TryDequeue(out string? v2));
        Assert.Equal("b", v2);
    }

    /// <summary>
    /// 演示 LogMessagePool 对象池。
    /// </summary>
    [Fact]
    public void Sample_LogMessagePool()
    {
        var pool = new LogMessagePool();

        LogMessage msg = pool.Rent("测试消息");
        Assert.Equal(4, msg.MessageLength); // "测试消息" 4个字符
        Assert.NotNull(msg.MessageBuffer);

        pool.Return(msg);

        // 复用
        LogMessage msg2 = pool.Rent("另一条");
        Assert.Equal(3, msg2.MessageLength);
        pool.Return(msg2);
    }

    /// <summary>
    /// 演示 TenantDatePathResolver 路径解析。
    /// </summary>
    [Fact]
    public void Sample_PathResolver()
    {
        var resolver = new TenantDatePathResolver("/var/logs");

        DateTime ts = new DateTime(2026, 3, 15, 10, 30, 0);
        string dir = resolver.GetTenantDirectory(ts, 42);
        Assert.Contains("202603", dir);
        Assert.Contains("20260315", dir);
        Assert.Contains("42", dir);

        string filePath = resolver.GetLogFilePath(ts, 42, LogLevel.Error);
        Assert.EndsWith("error.txt", filePath);
    }

    /// <summary>
    /// 演示 DefaultLogFormatter 格式化。
    /// </summary>
    [Fact]
    public void Sample_LogFormatter()
    {
        var formatter = new DefaultLogFormatter();

        var msg = new LogMessage
        {
            TenantId = 1,
            UserId = 100,
            Level = LogLevel.Error,
            Timestamp = new DateTime(2026, 1, 1, 12, 0, 0, 500),
            ThreadId = 7
        };
        string text = "测试格式化";
        msg.MessageBuffer = new char[text.Length];
        text.AsSpan().CopyTo(msg.MessageBuffer);
        msg.MessageLength = text.Length;

        char[] dest = new char[256];
        int written = formatter.Format(msg, dest);

        string result = new string(dest, 0, written);
        Assert.Contains("[ERROR]", result);
        Assert.Contains("[T7]", result);
        Assert.Contains("测试格式化", result);
    }

    /// <summary>
    /// 演示日志文件等级名称映射。
    /// </summary>
    [Fact]
    public void Sample_LogLevel_FileNames()
    {
        Assert.Equal("fatal.txt", TenantDatePathResolver.GetFileName(LogLevel.Fatal));
        Assert.Equal("error.txt", TenantDatePathResolver.GetFileName(LogLevel.Error));
        Assert.Equal("warn.txt", TenantDatePathResolver.GetFileName(LogLevel.Warn));
        Assert.Equal("infor.txt", TenantDatePathResolver.GetFileName(LogLevel.Infor));
        Assert.Equal("debug.txt", TenantDatePathResolver.GetFileName(LogLevel.Debug));
    }
}
