using YTStdSqlBuilder.Conditions;
using YTStdSqlBuilder.Expressions;

namespace YTStdSqlBuilder;

/// <summary>SQL 条件组构建器</summary>
public sealed class ConditionGroupBuilder
{
    private readonly List<SqlConditionNode> _nodes = new();

    /// <summary>添加 WHERE 条件</summary>
    /// <param name="left">左侧表达式</param>
    /// <param name="op">比较运算符</param>
    /// <param name="right">右侧表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public ConditionGroupBuilder Where(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _nodes.Add(SqlConditionNode.Simple(new SqlCondition(left, op, right), SqlLogicalOperator.And));
        return this;
    }

    /// <summary>添加 WHERE 表达式条件</summary>
    /// <param name="expr">条件表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public ConditionGroupBuilder Where(SqlExpr expr)
    {
        _nodes.Add(SqlConditionNode.Simple(
            new SqlCondition(expr, SqlComparisonOperator.Exists), SqlLogicalOperator.And));
        return this;
    }

    /// <summary>条件为真时添加 WHERE 条件</summary>
    /// <param name="condition">是否添加条件</param>
    /// <param name="left">左侧表达式</param>
    /// <param name="op">比较运算符</param>
    /// <param name="right">右侧表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public ConditionGroupBuilder WhereIf(bool condition, SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        if (condition) Where(left, op, right);
        return this;
    }

    /// <summary>添加 AND 条件</summary>
    /// <param name="left">左侧表达式</param>
    /// <param name="op">比较运算符</param>
    /// <param name="right">右侧表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public ConditionGroupBuilder And(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _nodes.Add(SqlConditionNode.Simple(new SqlCondition(left, op, right), SqlLogicalOperator.And));
        return this;
    }

    /// <summary>添加 AND 表达式条件</summary>
    /// <param name="expr">条件表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public ConditionGroupBuilder And(SqlExpr expr)
    {
        _nodes.Add(SqlConditionNode.Simple(
            new SqlCondition(expr, SqlComparisonOperator.Exists), SqlLogicalOperator.And));
        return this;
    }

    /// <summary>条件为真时添加 AND 条件</summary>
    /// <param name="condition">是否添加条件</param>
    /// <param name="left">左侧表达式</param>
    /// <param name="op">比较运算符</param>
    /// <param name="right">右侧表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public ConditionGroupBuilder AndIf(bool condition, SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        if (condition) And(left, op, right);
        return this;
    }

    /// <summary>添加 OR 条件</summary>
    /// <param name="left">左侧表达式</param>
    /// <param name="op">比较运算符</param>
    /// <param name="right">右侧表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public ConditionGroupBuilder Or(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _nodes.Add(SqlConditionNode.Simple(new SqlCondition(left, op, right), SqlLogicalOperator.Or));
        return this;
    }

    /// <summary>添加 OR 表达式条件</summary>
    /// <param name="expr">条件表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public ConditionGroupBuilder Or(SqlExpr expr)
    {
        _nodes.Add(SqlConditionNode.Simple(
            new SqlCondition(expr, SqlComparisonOperator.Exists), SqlLogicalOperator.Or));
        return this;
    }

    /// <summary>条件为真时添加 OR 条件</summary>
    /// <param name="condition">是否添加条件</param>
    /// <param name="left">左侧表达式</param>
    /// <param name="op">比较运算符</param>
    /// <param name="right">右侧表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public ConditionGroupBuilder OrIf(bool condition, SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        if (condition) Or(left, op, right);
        return this;
    }

    /// <summary>添加 WHERE 条件子组（括号包裹的 AND 组）</summary>
    /// <param name="group">条件组配置委托</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public ConditionGroupBuilder WhereGroup(Action<ConditionGroupBuilder> group)
    {
        var inner = new ConditionGroupBuilder();
        group(inner);
        if (!inner.IsEmpty)
            _nodes.Add(SqlConditionNode.AndGroup(inner.BuildNodes(), SqlLogicalOperator.And));
        return this;
    }

    /// <summary>添加 AND 条件子组（括号包裹的 AND 组）</summary>
    /// <param name="group">条件组配置委托</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public ConditionGroupBuilder AndGroup(Action<ConditionGroupBuilder> group)
    {
        var inner = new ConditionGroupBuilder();
        group(inner);
        if (!inner.IsEmpty)
            _nodes.Add(SqlConditionNode.AndGroup(inner.BuildNodes(), SqlLogicalOperator.And));
        return this;
    }

    /// <summary>添加 OR 条件子组（括号包裹的 OR 组）</summary>
    /// <param name="group">条件组配置委托</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public ConditionGroupBuilder OrGroup(Action<ConditionGroupBuilder> group)
    {
        var inner = new ConditionGroupBuilder();
        group(inner);
        if (!inner.IsEmpty)
            _nodes.Add(SqlConditionNode.OrGroup(inner.BuildNodes(), SqlLogicalOperator.Or));
        return this;
    }

    /// <summary>是否无条件</summary>
    public bool IsEmpty => _nodes.Count == 0;

    internal List<SqlConditionNode> BuildNodes() => new(_nodes);

    /// <summary>构建条件组</summary>
    /// <returns>条件组对象</returns>
    public SqlConditionGroup Build()
    {
        return new SqlConditionGroup(BuildNodes());
    }
}
