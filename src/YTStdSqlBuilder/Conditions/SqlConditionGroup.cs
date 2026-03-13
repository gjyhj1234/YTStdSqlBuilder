namespace YTStdSqlBuilder.Conditions;

/// <summary>SQL 条件组，包含多个条件节点</summary>
public sealed class SqlConditionGroup
{
    /// <summary>条件节点列表</summary>
    public List<SqlConditionNode> Nodes { get; }
    /// <summary>默认逻辑运算符</summary>
    public SqlLogicalOperator DefaultOperator { get; }

    /// <summary>创建空的条件组</summary>
    /// <param name="defaultOperator">默认逻辑运算符</param>
    public SqlConditionGroup(SqlLogicalOperator defaultOperator = SqlLogicalOperator.And)
    {
        Nodes = new List<SqlConditionNode>();
        DefaultOperator = defaultOperator;
    }

    /// <summary>创建包含指定节点的条件组</summary>
    /// <param name="nodes">条件节点列表</param>
    /// <param name="defaultOperator">默认逻辑运算符</param>
    public SqlConditionGroup(List<SqlConditionNode> nodes, SqlLogicalOperator defaultOperator = SqlLogicalOperator.And)
    {
        Nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
        DefaultOperator = defaultOperator;
    }

    /// <summary>是否无条件</summary>
    public bool IsEmpty => Nodes.Count == 0;
}
