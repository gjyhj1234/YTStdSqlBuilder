using System;

namespace YTStdEntity.Audit;

/// <summary>
/// 通用审计记录，对应 {Entity}_Audit 表的一行数据。
/// 适用于所有启用审计的实体。
/// </summary>
public sealed class AuditRecord
{
    /// <summary>审计记录自增主键</summary>
    public long AuditId { get; init; }

    /// <summary>原表主键 id</summary>
    public long Id { get; init; }

    /// <summary>操作类型：I=插入, U=更新, D=删除</summary>
    public AuditOpt Opt { get; init; }

    /// <summary>操作时间（UTC，带时区）</summary>
    public DateTime OperatedAt { get; init; }

    /// <summary>操作人 userId</summary>
    public long OperatorId { get; init; }

    /// <summary>租户 ID（仅租户表包含）</summary>
    public int? TenantId { get; init; }

    /// <summary>操作前/时完整记录快照（JSONB 原始字符串）</summary>
    public string Snapshot { get; init; } = "";
}
