namespace YTStdSqlBuilder.Conditions;

/// <summary>SQL 条件节点类型枚举</summary>
public enum SqlConditionNodeKind
{
    /// <summary>简单条件</summary>
    SimpleCondition,
    /// <summary>AND 条件组</summary>
    AndGroup,
    /// <summary>OR 条件组</summary>
    OrGroup
}
