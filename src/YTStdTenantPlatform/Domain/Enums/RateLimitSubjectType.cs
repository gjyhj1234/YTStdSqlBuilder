namespace YTStdTenantPlatform.Domain.Enums;

/// <summary>限流主体类型</summary>
public enum RateLimitSubjectType
{
    /// <summary>API</summary>
    Api,

    /// <summary>租户</summary>
    Tenant,

    /// <summary>IP</summary>
    Ip
}
