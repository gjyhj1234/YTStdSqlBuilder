namespace YTStdSqlBuilder.Conditions;

/// <summary>SQL 条件节点，可以是简单条件或条件组</summary>
public sealed class SqlConditionNode
{
    /// <summary>节点类型</summary>
    public SqlConditionNodeKind NodeKind { get; }
    /// <summary>简单条件（当 NodeKind 为 SimpleCondition 时有值）</summary>
    public SqlCondition? Condition { get; }
    /// <summary>子节点列表（当 NodeKind 为 AndGroup 或 OrGroup 时有值）</summary>
    public List<SqlConditionNode>? Children { get; }
    /// <summary>逻辑运算符</summary>
    public SqlLogicalOperator LogicalOperator { get; }

    private SqlConditionNode(SqlConditionNodeKind nodeKind, SqlCondition? condition,
        List<SqlConditionNode>? children, SqlLogicalOperator logicalOp = SqlLogicalOperator.And)
    {
        NodeKind = nodeKind;
        Condition = condition;
        Children = children;
        LogicalOperator = logicalOp;
    }

    /// <summary>创建简单条件节点</summary>
    /// <param name="condition">条件表达式</param>
    /// <param name="logicalOp">逻辑运算符</param>
    /// <returns>简单条件节点</returns>
    public static SqlConditionNode Simple(SqlCondition condition, SqlLogicalOperator logicalOp = SqlLogicalOperator.And) =>
        new(SqlConditionNodeKind.SimpleCondition, condition ?? throw new ArgumentNullException(nameof(condition)), null, logicalOp);

    /// <summary>创建 AND 条件组节点</summary>
    /// <param name="children">子条件节点列表</param>
    /// <param name="logicalOp">逻辑运算符</param>
    /// <returns>AND 条件组节点</returns>
    public static SqlConditionNode AndGroup(List<SqlConditionNode> children, SqlLogicalOperator logicalOp = SqlLogicalOperator.And) =>
        new(SqlConditionNodeKind.AndGroup, null, children ?? throw new ArgumentNullException(nameof(children)), logicalOp);

    /// <summary>创建 OR 条件组节点</summary>
    /// <param name="children">子条件节点列表</param>
    /// <param name="logicalOp">逻辑运算符</param>
    /// <returns>OR 条件组节点</returns>
    public static SqlConditionNode OrGroup(List<SqlConditionNode> children, SqlLogicalOperator logicalOp = SqlLogicalOperator.And) =>
        new(SqlConditionNodeKind.OrGroup, null, children ?? throw new ArgumentNullException(nameof(children)), logicalOp);
}
