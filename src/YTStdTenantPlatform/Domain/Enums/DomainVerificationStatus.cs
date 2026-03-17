namespace YTStdTenantPlatform.Domain.Enums;

/// <summary>域名验证状态</summary>
public enum DomainVerificationStatus
{
    /// <summary>待验证</summary>
    Pending,

    /// <summary>已验证</summary>
    Verified,

    /// <summary>验证失败</summary>
    Failed
}
