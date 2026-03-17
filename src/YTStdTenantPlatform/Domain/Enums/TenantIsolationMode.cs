namespace YTStdTenantPlatform.Domain.Enums;

/// <summary>租户隔离模式</summary>
public enum TenantIsolationMode
{
    /// <summary>共享数据库</summary>
    SharedDatabase,

    /// <summary>Schema隔离</summary>
    SchemaIsolated,

    /// <summary>数据库隔离</summary>
    DatabaseIsolated,

    /// <summary>混合模式</summary>
    Hybrid
}
