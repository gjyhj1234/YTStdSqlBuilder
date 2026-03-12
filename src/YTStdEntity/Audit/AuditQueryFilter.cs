using System;

namespace YTStdEntity.Audit;

/// <summary>
/// 审计查询过滤条件。
/// 所有条件为 AND 逻辑，null 表示不过滤。
/// </summary>
public sealed class AuditQueryFilter
{
    /// <summary>按原表主键筛选</summary>
    public long? Id { get; set; }

    /// <summary>按操作类型筛选</summary>
    public AuditOpt? Opt { get; set; }

    /// <summary>按操作人 userId 筛选</summary>
    public long? OperatorId { get; set; }

    /// <summary>时间范围起（含），UTC</summary>
    public DateTime? StartTime { get; set; }

    /// <summary>时间范围止（含），UTC</summary>
    public DateTime? EndTime { get; set; }

    /// <summary>分页：页码（从 0 开始）</summary>
    public int PageIndex { get; set; } = 0;

    /// <summary>分页：每页大小（默认 50，最大 500）</summary>
    public int PageSize { get; set; } = 50;
}
