using System;
using System.Buffers;
using System.Collections.Concurrent;
using YTStdLogger.Logging;

namespace YTStdLogger.Buffer;

/// <summary>
/// 日志消息对象池，负责复用 <see cref="LogMessage"/> 与消息字符缓冲。
/// </summary>
public sealed class LogMessagePool
{
    private readonly ConcurrentBag<LogMessage> _messages = new ConcurrentBag<LogMessage>();

    /// <summary>
    /// 租借消息对象并复制文本内容。
    /// </summary>
    public LogMessage Rent(string text)
    {
        if (!_messages.TryTake(out LogMessage? message))
        {
            message = new LogMessage();
        }

        int len = text.Length;
        char[] buffer = ArrayPool<char>.Shared.Rent(len == 0 ? 1 : len);
        text.AsSpan().CopyTo(buffer);
        message.MessageBuffer = buffer;
        message.MessageLength = len;
        return message;
    }

    /// <summary>
    /// 归还消息对象。
    /// </summary>
    public void Return(LogMessage message)
    {
        char[]? buffer = message.MessageBuffer;
        if (buffer is not null)
        {
            ArrayPool<char>.Shared.Return(buffer, clearArray: false);
            message.MessageBuffer = null;
        }

        message.Reset();
        _messages.Add(message);
    }
}
