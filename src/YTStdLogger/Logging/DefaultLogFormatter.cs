using System;

namespace YTStdLogger.Logging;

/// <summary>
/// 默认日志格式化器。
/// 核心路径基于 <see cref="Span{T}"/> 直接写入，减少中间字符串分配。
/// </summary>
public sealed class DefaultLogFormatter : ILogFormatter
{
    /// <inheritdoc/>
    public int Format(in LogMessage message, Span<char> destination)
    {
        int p = 0;
        if (destination.Length < 48 + message.MessageLength)
        {
            return 0;
        }

        destination[p++] = '[';
        WriteDateTime(message.Timestamp, destination, ref p);
        destination[p++] = ']';
        destination[p++] = '[';
        p += WriteLevel(message.Level, destination.Slice(p));
        destination[p++] = ']';
        destination[p++] = '[';
        destination[p++] = 'T';
        p += WriteUInt((uint)message.ThreadId, destination.Slice(p));
        destination[p++] = ']';
        destination[p++] = ' ';

        char[]? buf = message.MessageBuffer;
        if (buf is not null && message.MessageLength > 0)
        {
            new ReadOnlySpan<char>(buf, 0, message.MessageLength).CopyTo(destination.Slice(p));
            p += message.MessageLength;
        }

        destination[p++] = '\n';
        return p;
    }

    private static int WriteLevel(LogLevel level, Span<char> destination)
    {
        ReadOnlySpan<char> text = level switch
        {
            LogLevel.Fatal => "FATAL",
            LogLevel.Error => "ERROR",
            LogLevel.Warn => "WARN",
            LogLevel.Infor => "INFOR",
            _ => "DEBUG"
        };

        text.CopyTo(destination);
        return text.Length;
    }

    private static void WriteDateTime(DateTime dt, Span<char> destination, ref int p)
    {
        int year = dt.Year;
        int month = dt.Month;
        int day = dt.Day;
        int hour = dt.Hour;
        int minute = dt.Minute;
        int second = dt.Second;
        int ms = dt.Millisecond;

        Write2(year / 100, destination, ref p);
        Write2(year % 100, destination, ref p);
        destination[p++] = '-';
        Write2(month, destination, ref p);
        destination[p++] = '-';
        Write2(day, destination, ref p);
        destination[p++] = ' ';
        Write2(hour, destination, ref p);
        destination[p++] = ':';
        Write2(minute, destination, ref p);
        destination[p++] = ':';
        Write2(second, destination, ref p);
        destination[p++] = '.';
        Write3(ms, destination, ref p);
    }

    private static void Write2(int value, Span<char> destination, ref int p)
    {
        destination[p++] = (char)('0' + ((value / 10) % 10));
        destination[p++] = (char)('0' + (value % 10));
    }

    private static void Write3(int value, Span<char> destination, ref int p)
    {
        destination[p++] = (char)('0' + ((value / 100) % 10));
        destination[p++] = (char)('0' + ((value / 10) % 10));
        destination[p++] = (char)('0' + (value % 10));
    }

    private static int WriteUInt(uint value, Span<char> destination)
    {
        if (value == 0)
        {
            destination[0] = '0';
            return 1;
        }

        Span<char> temp = stackalloc char[10];
        int count = 0;
        while (value > 0)
        {
            uint digit = value % 10;
            temp[count++] = (char)('0' + digit);
            value /= 10;
        }

        for (int i = 0; i < count; i++)
        {
            destination[i] = temp[count - 1 - i];
        }

        return count;
    }
}
