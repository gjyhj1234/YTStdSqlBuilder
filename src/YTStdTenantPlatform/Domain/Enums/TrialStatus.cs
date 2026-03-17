namespace YTStdTenantPlatform.Domain.Enums;

/// <summary>试用状态</summary>
public enum TrialStatus
{
    /// <summary>试用中</summary>
    Active,

    /// <summary>已过期</summary>
    Expired,

    /// <summary>已转正</summary>
    Converted,

    /// <summary>已取消</summary>
    Cancelled
}
