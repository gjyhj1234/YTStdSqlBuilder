using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>操作日志</summary>
[Entity(TableName = "operation_logs")]
[Index("idx_operation_logs_tenant_time", "tenant_ref_id", "created_at")]
public class OperationLog
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>关联租户 ID</summary>
    public long? TenantRefId { get; set; }

    /// <summary>操作者类型</summary>
    [Column(Length = 32, IsRequired = true)]
    public string OperatorType { get; set; } = "";

    /// <summary>操作者 ID</summary>
    public long? OperatorId { get; set; }

    /// <summary>操作动作</summary>
    [Column(Length = 128, IsRequired = true)]
    public string Action { get; set; } = "";

    /// <summary>资源类型</summary>
    [Column(Length = 64)]
    public string? ResourceType { get; set; }

    /// <summary>资源 ID</summary>
    [Column(Length = 128)]
    public string? ResourceId { get; set; }

    /// <summary>请求 ID</summary>
    [Column(Length = 128)]
    public string? RequestId { get; set; }

    /// <summary>IP 地址</summary>
    [Column(Length = 64)]
    public string? IpAddress { get; set; }

    /// <summary>用户代理</summary>
    public string? UserAgent { get; set; }

    /// <summary>操作结果</summary>
    [Column(Length = 32, IsRequired = true)]
    public string OperationResult { get; set; } = "";

    /// <summary>详情</summary>
    [Column(DbType = "jsonb")]
    public string? Details { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }
}
