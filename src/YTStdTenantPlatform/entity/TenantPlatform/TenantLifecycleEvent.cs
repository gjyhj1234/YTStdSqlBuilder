using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>租户生命周期事件</summary>
[Entity(TableName = "tenant_lifecycle_events")]
[Index("idx_tenant_lifecycle_events_tenant_time", "tenant_ref_id", "occurred_at")]
public class TenantLifecycleEvent
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>关联租户 ID</summary>
    public long TenantRefId { get; set; }

    /// <summary>事件类型</summary>
    [Column(Length = 64, IsRequired = true)]
    public string EventType { get; set; } = "";

    /// <summary>原状态</summary>
    [Column(Length = 32)]
    public string? FromStatus { get; set; }

    /// <summary>目标状态</summary>
    [Column(Length = 32)]
    public string? ToStatus { get; set; }

    /// <summary>原因</summary>
    public string? Reason { get; set; }

    /// <summary>操作人 ID</summary>
    public long? OperatorId { get; set; }

    /// <summary>元数据</summary>
    [Column(DbType = "jsonb")]
    public string? Metadata { get; set; }

    /// <summary>发生时间</summary>
    public DateTime OccurredAt { get; set; }
}
