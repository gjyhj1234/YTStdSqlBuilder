namespace YTStdTenantPlatform.Domain.Enums;

/// <summary>数据作业类型</summary>
public enum DataJobType
{
    /// <summary>归档</summary>
    Archive,

    /// <summary>备份</summary>
    Backup,

    /// <summary>迁移</summary>
    Migration,

    /// <summary>清理</summary>
    Cleanup
}
