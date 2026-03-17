namespace YTStdTenantPlatform.Domain.Enums;

/// <summary>初始化任务状态</summary>
public enum InitializationTaskStatus
{
    /// <summary>待处理</summary>
    Pending,

    /// <summary>运行中</summary>
    Running,

    /// <summary>成功</summary>
    Success,

    /// <summary>失败</summary>
    Failed
}
