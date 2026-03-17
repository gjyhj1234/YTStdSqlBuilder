using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>租户 API 用量统计</summary>
[Entity(TableName = "tenant_api_usage_stats")]
[Index("idx_api_usage_tenant_date", "tenant_ref_id", "stat_date")]
public class TenantApiUsageStat
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>关联租户 ID</summary>
    public long TenantRefId { get; set; }

    /// <summary>API 密钥 ID</summary>
    public long? ApiKeyId { get; set; }

    /// <summary>统计日期</summary>
    [Column(DbType = "date")]
    public DateTime StatDate { get; set; }

    /// <summary>API 路径</summary>
    [Column(Length = 255, IsRequired = true)]
    public string ApiPath { get; set; } = "";

    /// <summary>请求次数</summary>
    public long RequestCount { get; set; }

    /// <summary>成功次数</summary>
    public long SuccessCount { get; set; }

    /// <summary>错误次数</summary>
    public long ErrorCount { get; set; }

    /// <summary>平均延迟（毫秒）</summary>
    public int AverageLatencyMs { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }
}
