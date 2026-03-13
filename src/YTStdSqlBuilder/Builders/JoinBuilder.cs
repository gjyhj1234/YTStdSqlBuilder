using YTStdSqlBuilder.Conditions;
using YTStdSqlBuilder.Expressions;

namespace YTStdSqlBuilder;

/// <summary>SQL JOIN 条件构建器</summary>
public sealed class JoinBuilder
{
    private readonly ConditionGroupBuilder _conditions = new();

    /// <summary>添加 ON 条件</summary>
    /// <param name="left">左侧表达式</param>
    /// <param name="op">比较运算符</param>
    /// <param name="right">右侧表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public JoinBuilder On(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _conditions.Where(left, op, right);
        return this;
    }

    /// <summary>添加 AND 条件</summary>
    /// <param name="left">左侧表达式</param>
    /// <param name="op">比较运算符</param>
    /// <param name="right">右侧表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public JoinBuilder And(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _conditions.And(left, op, right);
        return this;
    }

    /// <summary>添加 OR 条件</summary>
    /// <param name="left">左侧表达式</param>
    /// <param name="op">比较运算符</param>
    /// <param name="right">右侧表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public JoinBuilder Or(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _conditions.Or(left, op, right);
        return this;
    }

    /// <summary>添加 ON 条件子组</summary>
    /// <param name="group">条件组配置委托</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public JoinBuilder OnGroup(Action<ConditionGroupBuilder> group)
    {
        _conditions.AndGroup(group);
        return this;
    }

    internal SqlConditionGroup Build() => _conditions.Build();
}
