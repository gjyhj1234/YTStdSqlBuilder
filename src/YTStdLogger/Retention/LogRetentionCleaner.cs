using System;
using System.IO;
using System.Threading;

namespace YTStdLogger.Retention;

/// <summary>
/// 日志保留清理器。
/// 在后台周期性扫描并删除超出保留月数的月份目录。
/// 清理异常会被吞吐并通过标准错误输出，不影响主日志流程。
/// </summary>
public sealed class LogRetentionCleaner : IDisposable
{
    private readonly string _rootPath;
    private readonly int _retentionMonths;
    private readonly Timer _timer;

    /// <summary>
    /// 初始化清理器。
    /// </summary>
    public LogRetentionCleaner(string rootPath, int retentionMonths)
    {
        _rootPath = rootPath;
        _retentionMonths = retentionMonths;
        _timer = new Timer(CleanCallback, null, TimeSpan.FromMinutes(1), TimeSpan.FromHours(1));
    }

    private void CleanCallback(object? state)
    {
        try
        {
            CleanOnce();
        }
        catch (Exception ex)
        {
            var vsb = new ValueStringBuilder(128);
            vsb.Append("[YTStdLogger][Retention] ");
            vsb.Append(ex.Message);
            Console.Error.WriteLine(vsb.ToString());
        }
    }

    /// <summary>
    /// 执行一次清理。
    /// </summary>
    public void CleanOnce()
    {
        if (!Directory.Exists(_rootPath))
        {
            return;
        }

        DateTime cutoffMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-_retentionMonths);
        string[] monthDirs = Directory.GetDirectories(_rootPath);
        for (int i = 0; i < monthDirs.Length; i++)
        {
            string monthName = Path.GetFileName(monthDirs[i]);
            if (monthName.Length != 6)
            {
                continue;
            }

            if (!int.TryParse(monthName.AsSpan(0, 4), out int year) || !int.TryParse(monthName.AsSpan(4, 2), out int month))
            {
                continue;
            }

            if (month is < 1 or > 12)
            {
                continue;
            }

            DateTime dirMonth = new DateTime(year, month, 1);
            if (dirMonth < cutoffMonth)
            {
                TryDelete(monthDirs[i]);
            }
            else
            {
                CleanDayDirectories(monthDirs[i], cutoffMonth);
            }
        }
    }

    private static void CleanDayDirectories(string monthDir, DateTime cutoffMonth)
    {
        string[] dayDirs = Directory.GetDirectories(monthDir);
        for (int i = 0; i < dayDirs.Length; i++)
        {
            string dayName = Path.GetFileName(dayDirs[i]);
            if (dayName.Length != 8)
            {
                continue;
            }

            if (!int.TryParse(dayName.AsSpan(0, 4), out int y) ||
                !int.TryParse(dayName.AsSpan(4, 2), out int m) ||
                !int.TryParse(dayName.AsSpan(6, 2), out int d))
            {
                continue;
            }

            DateTime day;
            try
            {
                day = new DateTime(y, m, d);
            }
            catch
            {
                continue;
            }

            if (day < cutoffMonth)
            {
                TryDelete(dayDirs[i]);
            }
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            Directory.Delete(path, true);
        }
        catch (Exception ex)
        {
            var vsb = new ValueStringBuilder(128);
            vsb.Append("[YTStdLogger][Retention] 删除失败: ");
            vsb.Append(ex.Message);
            Console.Error.WriteLine(vsb.ToString());
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _timer.Dispose();
    }
}
