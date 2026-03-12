using System;

namespace YTStdEntity.Backup;

/// <summary>增量备份配置</summary>
public sealed class IncrementalBackupOptions
{
    /// <summary>目标库连接地址列表（支持多目标库）</summary>
    public string[] TargetConnectionStrings { get; set; } = Array.Empty<string>();

    /// <summary>备份间隔（默认 30 秒）</summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(30);
}
