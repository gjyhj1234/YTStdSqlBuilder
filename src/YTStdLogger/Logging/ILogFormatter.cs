using System;

namespace YTStdLogger.Logging;

/// <summary>
/// 日志格式化接口。
/// </summary>
public interface ILogFormatter
{
    /// <summary>
    /// 将日志消息写入目标字符缓冲区。
    /// </summary>
    /// <param name="message">日志消息。</param>
    /// <param name="destination">目标缓冲区。</param>
    /// <returns>写入字符长度；空间不足时返回 0。</returns>
    int Format(in LogMessage message, Span<char> destination);
}
