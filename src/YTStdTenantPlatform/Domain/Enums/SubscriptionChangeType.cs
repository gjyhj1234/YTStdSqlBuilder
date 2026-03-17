namespace YTStdTenantPlatform.Domain.Enums;

/// <summary>订阅变更类型</summary>
public enum SubscriptionChangeType
{
    /// <summary>订阅</summary>
    Subscribe,

    /// <summary>升级</summary>
    Upgrade,

    /// <summary>降级</summary>
    Downgrade,

    /// <summary>续费</summary>
    Renew,

    /// <summary>取消</summary>
    Cancel,

    /// <summary>试用转正式</summary>
    TrialToFormal
}
