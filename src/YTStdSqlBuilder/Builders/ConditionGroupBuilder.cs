using YTStdSqlBuilder.Conditions;
using YTStdSqlBuilder.Expressions;

namespace YTStdSqlBuilder;

public sealed class ConditionGroupBuilder
{
    private readonly List<SqlConditionNode> _nodes = new();

    public ConditionGroupBuilder Where(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _nodes.Add(SqlConditionNode.Simple(new SqlCondition(left, op, right), SqlLogicalOperator.And));
        return this;
    }

    public ConditionGroupBuilder Where(SqlExpr expr)
    {
        _nodes.Add(SqlConditionNode.Simple(
            new SqlCondition(expr, SqlComparisonOperator.Exists), SqlLogicalOperator.And));
        return this;
    }

    public ConditionGroupBuilder WhereIf(bool condition, SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        if (condition) Where(left, op, right);
        return this;
    }

    public ConditionGroupBuilder And(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _nodes.Add(SqlConditionNode.Simple(new SqlCondition(left, op, right), SqlLogicalOperator.And));
        return this;
    }

    public ConditionGroupBuilder And(SqlExpr expr)
    {
        _nodes.Add(SqlConditionNode.Simple(
            new SqlCondition(expr, SqlComparisonOperator.Exists), SqlLogicalOperator.And));
        return this;
    }

    public ConditionGroupBuilder AndIf(bool condition, SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        if (condition) And(left, op, right);
        return this;
    }

    public ConditionGroupBuilder Or(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _nodes.Add(SqlConditionNode.Simple(new SqlCondition(left, op, right), SqlLogicalOperator.Or));
        return this;
    }

    public ConditionGroupBuilder Or(SqlExpr expr)
    {
        _nodes.Add(SqlConditionNode.Simple(
            new SqlCondition(expr, SqlComparisonOperator.Exists), SqlLogicalOperator.Or));
        return this;
    }

    public ConditionGroupBuilder OrIf(bool condition, SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        if (condition) Or(left, op, right);
        return this;
    }

    public ConditionGroupBuilder WhereGroup(Action<ConditionGroupBuilder> group)
    {
        var inner = new ConditionGroupBuilder();
        group(inner);
        if (!inner.IsEmpty)
            _nodes.Add(SqlConditionNode.AndGroup(inner.BuildNodes(), SqlLogicalOperator.And));
        return this;
    }

    public ConditionGroupBuilder AndGroup(Action<ConditionGroupBuilder> group)
    {
        var inner = new ConditionGroupBuilder();
        group(inner);
        if (!inner.IsEmpty)
            _nodes.Add(SqlConditionNode.AndGroup(inner.BuildNodes(), SqlLogicalOperator.And));
        return this;
    }

    public ConditionGroupBuilder OrGroup(Action<ConditionGroupBuilder> group)
    {
        var inner = new ConditionGroupBuilder();
        group(inner);
        if (!inner.IsEmpty)
            _nodes.Add(SqlConditionNode.OrGroup(inner.BuildNodes(), SqlLogicalOperator.Or));
        return this;
    }

    public bool IsEmpty => _nodes.Count == 0;

    internal List<SqlConditionNode> BuildNodes() => new(_nodes);

    public SqlConditionGroup Build()
    {
        return new SqlConditionGroup(BuildNodes());
    }
}
