using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using YTStdLogger.Logging;

namespace YTStdLogger.Core;

/// <summary>
/// 全局静态日志门面。
/// 用于业务代码以单例方式调用日志能力，避免在业务层重复管理引擎实例。
/// <para>
/// 提供两类重载：
/// <list type="bullet">
/// <item><c>string message</c> — 直接传入已构建好的消息字符串。</item>
/// <item><c>Func&lt;string&gt; messageFactory</c> — 延迟求值：仅在该日志等级实际启用时才调用工厂方法构建字符串，
/// 避免在日志未启用时产生不必要的字符串分配与 GC 压力。推荐在 Debug 等可能被全局禁用的等级使用。</item>
/// </list>
/// </para>
/// </summary>
public static class Logger
{
    private static readonly object Gate = new object();
    private static LoggerEngine? _engine;

    /// <summary>
    /// 初始化全局日志引擎。
    /// 若已初始化则复用已存在单例；配置不一致时由 <see cref="LoggerFactory"/> 抛出异常。
    /// </summary>
    public static void Init(LogOptions options)
    {
        LoggerEngine engine = LoggerFactory.Create(options);
        lock (Gate)
        {
            _engine = engine;
        }
    }

    /// <summary>
    /// 写入任意等级日志。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Log(int tenantId, long userId, LogLevel level, string message)
    {
        _engine?.Log(tenantId, userId, level, message);
    }

    /// <summary>
    /// 写入任意等级日志（延迟求值）。仅在该等级启用时才调用 <paramref name="messageFactory"/> 构建消息字符串。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Log(int tenantId, long userId, LogLevel level, Func<string> messageFactory)
    {
        _engine?.Log(tenantId, userId, level, messageFactory);
    }

    /// <summary>
    /// 写入致命日志。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Fatal(int tenantId, long userId, string message)
    {
        _engine?.Fatal(tenantId, userId, message);
    }

    /// <summary>
    /// 写入致命日志（延迟求值）。仅在该等级启用时才调用 <paramref name="messageFactory"/> 构建消息字符串。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Fatal(int tenantId, long userId, Func<string> messageFactory)
    {
        _engine?.Fatal(tenantId, userId, messageFactory);
    }

    /// <summary>
    /// 写入错误日志。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Error(int tenantId, long userId, string message)
    {
        _engine?.Error(tenantId, userId, message);
    }

    /// <summary>
    /// 写入错误日志（延迟求值）。仅在该等级启用时才调用 <paramref name="messageFactory"/> 构建消息字符串。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Error(int tenantId, long userId, Func<string> messageFactory)
    {
        _engine?.Error(tenantId, userId, messageFactory);
    }

    /// <summary>
    /// 写入警告日志。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Warn(int tenantId, long userId, string message)
    {
        _engine?.Warn(tenantId, userId, message);
    }

    /// <summary>
    /// 写入警告日志（延迟求值）。仅在该等级启用时才调用 <paramref name="messageFactory"/> 构建消息字符串。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Warn(int tenantId, long userId, Func<string> messageFactory)
    {
        _engine?.Warn(tenantId, userId, messageFactory);
    }

    /// <summary>
    /// 写入信息日志。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Info(int tenantId, long userId, string message)
    {
        _engine?.Info(tenantId, userId, message);
    }

    /// <summary>
    /// 写入信息日志（延迟求值）。仅在该等级启用时才调用 <paramref name="messageFactory"/> 构建消息字符串。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Info(int tenantId, long userId, Func<string> messageFactory)
    {
        _engine?.Info(tenantId, userId, messageFactory);
    }

    /// <summary>
    /// 写入调试日志。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Debug(int tenantId, long userId, string message)
    {
        _engine?.Debug(tenantId, userId, message);
    }

    /// <summary>
    /// 写入调试日志（延迟求值）。仅在该等级启用时才调用 <paramref name="messageFactory"/> 构建消息字符串，
    /// 避免在 Debug 未启用时产生不必要的字符串分配。推荐在高频调用路径中使用此重载。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Debug(int tenantId, long userId, Func<string> messageFactory)
    {
        _engine?.Debug(tenantId, userId, messageFactory);
    }

    /// <summary>
    /// 为指定租户开启运行时 Debug 覆盖。
    /// </summary>
    public static void EnableTenantDebug(int tenantId)
    {
        _engine?.EnableTenantDebug(tenantId);
    }

    /// <summary>
    /// 为指定租户关闭运行时 Debug 覆盖。
    /// </summary>
    /// <returns>若原本已开启则返回 <c>true</c>。</returns>
    public static bool DisableTenantDebug(int tenantId)
    {
        LoggerEngine? engine = _engine;
        if (engine is null)
        {
            return false;
        }

        return engine.DisableTenantDebug(tenantId);
    }

    /// <summary>
    /// 判断指定租户是否已开启运行时 Debug 覆盖。
    /// </summary>
    public static bool IsTenantDebugEnabled(int tenantId)
    {
        LoggerEngine? engine = _engine;
        if (engine is null)
        {
            return false;
        }

        return engine.IsTenantDebugEnabled(tenantId);
    }

    /// <summary>
    /// 全局累计已写入条数。
    /// </summary>
    public static long WrittenCount => _engine?.WrittenCount ?? 0;

    /// <summary>
    /// 全局累计丢弃条数。
    /// </summary>
    public static long DroppedCount => _engine?.DroppedCount ?? 0;

    /// <summary>
    /// 优雅关闭日志引擎。
    /// </summary>
    public static async ValueTask ShutdownAsync()
    {
        LoggerEngine? engine;
        lock (Gate)
        {
            engine = _engine;
            _engine = null;
        }

        if (engine is null)
        {
            return;
        }

        await engine.DisposeAsync().ConfigureAwait(false);
        LoggerFactory.Reset();
    }
}
