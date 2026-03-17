using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>限流策略</summary>
[Entity(TableName = "rate_limit_policies", NeedAuditTable = true)]
[Index("uq_rate_limit_policies_subject_type_key", "subject_type", "subject_key", Kind = IndexKind.Unique)]
public class RateLimitPolicy
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>主体类型</summary>
    [Column(Length = 32, IsRequired = true)]
    public string SubjectType { get; set; } = "";

    /// <summary>主体标识</summary>
    [Column(Length = 255, IsRequired = true)]
    public string SubjectKey { get; set; } = "";

    /// <summary>窗口秒数</summary>
    public int WindowSeconds { get; set; }

    /// <summary>限制次数</summary>
    public int LimitCount { get; set; }

    /// <summary>突发限制</summary>
    public int? BurstLimit { get; set; }

    /// <summary>状态</summary>
    [Column(Length = 32, IsRequired = true)]
    public string Status { get; set; } = "";

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; }
}
