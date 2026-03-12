namespace YTStdEntity.Audit;

/// <summary>
/// 表示两个审计快照之间单个字段的差异。
/// </summary>
public sealed class AuditDiffField
{
    /// <summary>字段名称</summary>
    public string FieldName { get; init; } = "";

    /// <summary>变更前的值（字符串形式）</summary>
    public string? OldValue { get; init; }

    /// <summary>变更后的值（字符串形式）</summary>
    public string? NewValue { get; init; }
}
