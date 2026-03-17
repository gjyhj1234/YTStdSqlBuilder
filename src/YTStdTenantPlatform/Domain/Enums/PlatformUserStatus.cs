namespace YTStdTenantPlatform.Domain.Enums;

/// <summary>平台用户状态</summary>
public enum PlatformUserStatus
{
    /// <summary>正常</summary>
    Active,

    /// <summary>已禁用</summary>
    Disabled,

    /// <summary>已删除</summary>
    Deleted,

    /// <summary>已锁定</summary>
    Locked
}
