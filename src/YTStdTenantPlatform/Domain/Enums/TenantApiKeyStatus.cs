namespace YTStdTenantPlatform.Domain.Enums;

/// <summary>租户API密钥状态</summary>
public enum TenantApiKeyStatus
{
    /// <summary>正常</summary>
    Active,

    /// <summary>已禁用</summary>
    Disabled,

    /// <summary>已删除</summary>
    Deleted
}
