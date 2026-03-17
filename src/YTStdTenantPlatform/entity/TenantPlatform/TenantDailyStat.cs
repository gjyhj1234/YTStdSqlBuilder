using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>租户每日统计</summary>
[Entity(TableName = "tenant_daily_stats")]
[Index("uq_tenant_daily_stats_tenant_date", "tenant_ref_id", "stat_date", Kind = IndexKind.Unique)]
public class TenantDailyStat
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>关联租户 ID</summary>
    public long TenantRefId { get; set; }

    /// <summary>统计日期</summary>
    [Column(DbType = "date")]
    public DateTime StatDate { get; set; }

    /// <summary>活跃用户数</summary>
    public int ActiveUserCount { get; set; }

    /// <summary>新增用户数</summary>
    public int NewUserCount { get; set; }

    /// <summary>API 调用次数</summary>
    public long ApiCallCount { get; set; }

    /// <summary>存储字节数</summary>
    public long StorageBytes { get; set; }

    /// <summary>资源评分</summary>
    [Column(Length = 10, IsRequired = true)]
    public decimal ResourceScore { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }
}
