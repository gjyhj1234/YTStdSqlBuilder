using System;
using System.IO;
using YTStdLogger.Logging;

namespace YTStdLogger.IO;

/// <summary>
/// 负责将租户、日期、等级映射到目标目录与文件路径。
/// </summary>
public sealed class TenantDatePathResolver
{
    private readonly string _rootPath;

    /// <summary>
    /// 初始化路径解析器。
    /// </summary>
    public TenantDatePathResolver(string rootPath)
    {
        _rootPath = rootPath;
    }

    /// <summary>
    /// 获取租户日期目录。
    /// </summary>
    public string GetTenantDirectory(DateTime ts, int tenantId)
    {
        string month = ts.Year.ToString("0000") + ts.Month.ToString("00");
        string day = month + ts.Day.ToString("00");
        return Path.Combine(_rootPath, month, day, tenantId.ToString());
    }

    /// <summary>
    /// 获取日志文件路径。
    /// </summary>
    public string GetLogFilePath(DateTime ts, int tenantId, LogLevel level)
    {
        string dir = GetTenantDirectory(ts, tenantId);
        return Path.Combine(dir, GetFileName(level));
    }

    /// <summary>
    /// 获取等级文件名。
    /// </summary>
    public static string GetFileName(LogLevel level)
    {
        return level switch
        {
            LogLevel.Fatal => "fatal.txt",
            LogLevel.Error => "error.txt",
            LogLevel.Warn => "warn.txt",
            LogLevel.Infor => "infor.txt",
            _ => "debug.txt"
        };
    }
}
