namespace YTStdTenantPlatform.Domain.Enums;

/// <summary>数据作业状态</summary>
public enum DataJobStatus
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
