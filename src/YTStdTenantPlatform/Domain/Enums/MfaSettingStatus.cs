namespace YTStdTenantPlatform.Domain.Enums;

/// <summary>多因素认证设置状态</summary>
public enum MfaSettingStatus
{
    /// <summary>待激活</summary>
    Pending,

    /// <summary>已激活</summary>
    Active,

    /// <summary>已禁用</summary>
    Disabled
}
