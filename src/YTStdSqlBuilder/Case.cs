using YTStdSqlBuilder.Conditions;
using YTStdSqlBuilder.Expressions;

namespace YTStdSqlBuilder;

/// <summary>提供 SQL CASE 表达式的入口方法</summary>
public static class Case
{
    /// <summary>创建 CASE WHEN 子句</summary>
    /// <param name="left">左侧表达式</param>
    /// <param name="op">比较运算符</param>
    /// <param name="right">右侧表达式</param>
    /// <returns>CASE WHEN 构建器实例</returns>
    public static CaseWhenBuilder When(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
        => new CaseWhenBuilder().AddWhen(left, op, right);
}

/// <summary>CASE WHEN 表达式构建器</summary>
public sealed class CaseWhenBuilder
{
    private readonly List<(SqlConditionGroup Condition, SqlExpr? Result)> _clauses = new();

    internal CaseWhenBuilder AddWhen(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        var group = new SqlConditionGroup(
            new List<SqlConditionNode> { SqlConditionNode.Simple(new SqlCondition(left, op, right)) });
        _clauses.Add((group, null));
        return this;
    }

    /// <summary>设置当前 WHEN 子句的 THEN 结果</summary>
    /// <param name="result">THEN 结果表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public CaseWhenBuilder Then(SqlExpr result)
    {
        var last = _clauses[^1];
        _clauses[^1] = (last.Condition, result);
        return this;
    }

    /// <summary>添加新的 WHEN 子句</summary>
    /// <param name="left">左侧表达式</param>
    /// <param name="op">比较运算符</param>
    /// <param name="right">右侧表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public CaseWhenBuilder When(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
        => AddWhen(left, op, right);

    /// <summary>设置 ELSE 子句并结束 CASE 表达式</summary>
    /// <param name="result">ELSE 结果表达式</param>
    /// <returns>完成的 CASE 表达式</returns>
    public CaseExpr Else(SqlExpr result)
    {
        var clauses = _clauses.Select(c => (c.Condition, c.Result!)).ToList();
        return new CaseExpr(clauses, result);
    }

    /// <summary>结束 CASE 表达式（无 ELSE 子句）</summary>
    /// <returns>完成的 CASE 表达式</returns>
    public CaseExpr End()
    {
        var clauses = _clauses.Select(c => (c.Condition, c.Result!)).ToList();
        return new CaseExpr(clauses);
    }
}
