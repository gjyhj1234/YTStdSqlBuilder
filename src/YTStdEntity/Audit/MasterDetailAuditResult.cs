using System.Collections.Generic;

namespace YTStdEntity.Audit;

/// <summary>
/// 主从表审计查询结果基类。
/// Source Generator 会为每个有 [DetailOf] 关联的主表生成具体子类。
/// </summary>
public class MasterDetailAuditResult
{
    /// <summary>主表审计记录列表</summary>
    public List<AuditRecord> MasterRecords { get; init; } = new();

    /// <summary>查询时间范围内的总记录数（用于分页）</summary>
    public int TotalCount { get; init; }
}
