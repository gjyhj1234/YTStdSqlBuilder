using System;
using System.Runtime.CompilerServices;

namespace YTStdLogger.Logging;

/// <summary>
/// 可复用日志消息对象，避免频繁分配。
/// </summary>
public sealed class LogMessage
{
    /// <summary>
    /// 租户编号。
    /// </summary>
    public int TenantId;

    /// <summary>
    /// 用户编号。
    /// </summary>
    public long UserId;

    /// <summary>
    /// 日志等级。
    /// </summary>
    public LogLevel Level;

    /// <summary>
    /// 日志时间戳。
    /// </summary>
    public DateTime Timestamp;

    /// <summary>
    /// 线程编号。
    /// </summary>
    public int ThreadId;

    /// <summary>
    /// 消息字符缓冲。
    /// </summary>
    public char[]? MessageBuffer;

    /// <summary>
    /// 消息长度。
    /// </summary>
    public int MessageLength;

    /// <summary>
    /// 重置对象字段。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset()
    {
        TenantId = 0;
        UserId = 0;
        Level = LogLevel.Infor;
        Timestamp = default;
        ThreadId = 0;
        MessageLength = 0;
    }
}
