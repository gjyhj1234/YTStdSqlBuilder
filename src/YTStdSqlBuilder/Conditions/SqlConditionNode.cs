namespace YTStdSqlBuilder.Conditions;

public sealed class SqlConditionNode
{
    public SqlConditionNodeKind NodeKind { get; }
    public SqlCondition? Condition { get; }
    public List<SqlConditionNode>? Children { get; }
    public SqlLogicalOperator LogicalOperator { get; }

    private SqlConditionNode(SqlConditionNodeKind nodeKind, SqlCondition? condition,
        List<SqlConditionNode>? children, SqlLogicalOperator logicalOp = SqlLogicalOperator.And)
    {
        NodeKind = nodeKind;
        Condition = condition;
        Children = children;
        LogicalOperator = logicalOp;
    }

    public static SqlConditionNode Simple(SqlCondition condition, SqlLogicalOperator logicalOp = SqlLogicalOperator.And) =>
        new(SqlConditionNodeKind.SimpleCondition, condition ?? throw new ArgumentNullException(nameof(condition)), null, logicalOp);

    public static SqlConditionNode AndGroup(List<SqlConditionNode> children, SqlLogicalOperator logicalOp = SqlLogicalOperator.And) =>
        new(SqlConditionNodeKind.AndGroup, null, children ?? throw new ArgumentNullException(nameof(children)), logicalOp);

    public static SqlConditionNode OrGroup(List<SqlConditionNode> children, SqlLogicalOperator logicalOp = SqlLogicalOperator.And) =>
        new(SqlConditionNodeKind.OrGroup, null, children ?? throw new ArgumentNullException(nameof(children)), logicalOp);
}
