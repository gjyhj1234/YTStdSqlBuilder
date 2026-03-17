using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>审计日志</summary>
[Entity(TableName = "audit_logs")]
[Index("idx_audit_logs_tenant_time", "tenant_ref_id", "created_at")]
public class AuditLog
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>关联租户 ID</summary>
    public long? TenantRefId { get; set; }

    /// <summary>审计类型</summary>
    [Column(Length = 64, IsRequired = true)]
    public string AuditType { get; set; } = "";

    /// <summary>严重级别</summary>
    [Column(Length = 32, IsRequired = true)]
    public string Severity { get; set; } = "";

    /// <summary>主体类型</summary>
    [Column(Length = 64)]
    public string? SubjectType { get; set; }

    /// <summary>主体 ID</summary>
    [Column(Length = 128)]
    public string? SubjectId { get; set; }

    /// <summary>变更摘要</summary>
    [Column(DbType = "jsonb")]
    public string? ChangeSummary { get; set; }

    /// <summary>合规标签</summary>
    [Column(Length = 64)]
    public string? ComplianceTag { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }
}
