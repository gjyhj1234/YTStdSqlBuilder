using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>租户初始化任务</summary>
[Entity(TableName = "tenant_initialization_tasks")]
public class TenantInitializationTask
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>关联租户 ID</summary>
    public long TenantRefId { get; set; }

    /// <summary>任务类型</summary>
    [Column(Length = 64, IsRequired = true)]
    public string TaskType { get; set; } = "";

    /// <summary>任务状态</summary>
    [Column(Length = 32, IsRequired = true)]
    public string TaskStatus { get; set; } = "";

    /// <summary>详情</summary>
    [Column(DbType = "jsonb")]
    public string? Details { get; set; }

    /// <summary>开始时间</summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>完成时间</summary>
    public DateTime? FinishedAt { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }
}
