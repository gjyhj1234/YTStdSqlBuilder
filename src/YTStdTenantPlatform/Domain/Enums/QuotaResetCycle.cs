namespace YTStdTenantPlatform.Domain.Enums;

/// <summary>配额重置周期</summary>
public enum QuotaResetCycle
{
    /// <summary>不重置</summary>
    None,

    /// <summary>每小时</summary>
    Hourly,

    /// <summary>每天</summary>
    Daily,

    /// <summary>每周</summary>
    Weekly,

    /// <summary>每月</summary>
    Monthly,

    /// <summary>每年</summary>
    Yearly
}
