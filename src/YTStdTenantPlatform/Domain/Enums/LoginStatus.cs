namespace YTStdTenantPlatform.Domain.Enums;

/// <summary>登录状态</summary>
public enum LoginStatus
{
    /// <summary>登录成功</summary>
    Success,

    /// <summary>登录失败</summary>
    Failed,

    /// <summary>账户已锁定</summary>
    Locked,

    /// <summary>账户已禁用</summary>
    Disabled
}
