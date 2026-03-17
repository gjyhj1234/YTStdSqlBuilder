using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>租户数据作业</summary>
[Entity(TableName = "tenant_data_jobs")]
[Index("idx_tenant_data_jobs_tenant_type", "tenant_ref_id", "job_type")]
public class TenantDataJob
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>关联租户 ID</summary>
    public long TenantRefId { get; set; }

    /// <summary>作业类型</summary>
    [Column(Length = 32, IsRequired = true)]
    public string JobType { get; set; } = "";

    /// <summary>作业状态</summary>
    [Column(Length = 32, IsRequired = true)]
    public string JobStatus { get; set; } = "";

    /// <summary>存储路径</summary>
    [Column(Length = 255)]
    public string? StoragePath { get; set; }

    /// <summary>负载数据</summary>
    [Column(DbType = "jsonb")]
    public string? Payload { get; set; }

    /// <summary>开始时间</summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>完成时间</summary>
    public DateTime? FinishedAt { get; set; }

    /// <summary>创建人</summary>
    public long? CreatedBy { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }
}
