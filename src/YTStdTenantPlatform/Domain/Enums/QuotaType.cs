namespace YTStdTenantPlatform.Domain.Enums;

/// <summary>配额类型</summary>
public enum QuotaType
{
    /// <summary>用户数量</summary>
    UserCount,

    /// <summary>API调用次数</summary>
    ApiCalls,

    /// <summary>并发请求数</summary>
    ConcurrentRequests,

    /// <summary>存储容量</summary>
    StorageSize,

    /// <summary>数据库容量</summary>
    DatabaseSize,

    /// <summary>文件数量</summary>
    FileCount
}
