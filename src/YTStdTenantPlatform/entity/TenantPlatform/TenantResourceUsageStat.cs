using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>租户资源用量统计</summary>
[Entity(TableName = "tenant_resource_usage_stats")]
[Index("uq_tenant_resource_usage_stats_tenant_date", "tenant_ref_id", "metric_date", Kind = IndexKind.Unique)]
public class TenantResourceUsageStat
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>关联租户 ID</summary>
    public long TenantRefId { get; set; }

    /// <summary>统计日期</summary>
    [Column(DbType = "date")]
    public DateTime MetricDate { get; set; }

    /// <summary>用户数</summary>
    public int UserCount { get; set; }

    /// <summary>API 调用次数</summary>
    public long ApiCallCount { get; set; }

    /// <summary>并发请求峰值</summary>
    public int ConcurrentRequestPeak { get; set; }

    /// <summary>存储字节数</summary>
    public long StorageBytes { get; set; }

    /// <summary>数据库字节数</summary>
    public long DatabaseBytes { get; set; }

    /// <summary>文件数量</summary>
    public long FileCount { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }
}
