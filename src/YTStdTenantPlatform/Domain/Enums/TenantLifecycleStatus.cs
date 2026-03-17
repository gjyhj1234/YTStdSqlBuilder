namespace YTStdTenantPlatform.Domain.Enums;

/// <summary>租户生命周期状态</summary>
public enum TenantLifecycleStatus
{
    /// <summary>试用中</summary>
    Trial,

    /// <summary>正常</summary>
    Active,

    /// <summary>即将到期</summary>
    Expiring,

    /// <summary>已过期</summary>
    Expired,

    /// <summary>已暂停</summary>
    Suspended,

    /// <summary>已关闭</summary>
    Closed,

    /// <summary>已删除</summary>
    Deleted
}
