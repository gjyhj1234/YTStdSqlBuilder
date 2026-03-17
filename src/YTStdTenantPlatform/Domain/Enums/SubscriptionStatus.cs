namespace YTStdTenantPlatform.Domain.Enums;

/// <summary>订阅状态</summary>
public enum SubscriptionStatus
{
    /// <summary>正常</summary>
    Active,

    /// <summary>即将到期</summary>
    Expiring,

    /// <summary>已过期</summary>
    Expired,

    /// <summary>已暂停</summary>
    Suspended,

    /// <summary>已取消</summary>
    Cancelled
}
