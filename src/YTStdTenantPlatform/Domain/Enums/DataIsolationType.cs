namespace YTStdTenantPlatform.Domain.Enums;

/// <summary>数据隔离类型</summary>
public enum DataIsolationType
{
    /// <summary>租户隔离</summary>
    TenantIsolation,

    /// <summary>访问控制</summary>
    AccessControl,

    /// <summary>安全策略</summary>
    SecurityPolicy
}
