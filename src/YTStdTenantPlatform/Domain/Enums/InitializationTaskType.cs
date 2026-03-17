namespace YTStdTenantPlatform.Domain.Enums;

/// <summary>初始化任务类型</summary>
public enum InitializationTaskType
{
    /// <summary>数据库初始化</summary>
    Database,

    /// <summary>配置初始化</summary>
    Config,

    /// <summary>套餐初始化</summary>
    Plan,

    /// <summary>资源初始化</summary>
    Resource
}
