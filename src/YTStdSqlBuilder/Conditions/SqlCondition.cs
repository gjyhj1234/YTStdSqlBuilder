using YTStdSqlBuilder.Expressions;
using YTStdSqlBuilder.Internal;

namespace YTStdSqlBuilder.Conditions;

/// <summary>SQL 条件表达式，表示一个比较操作</summary>
public sealed class SqlCondition
{
    /// <summary>左侧表达式</summary>
    public SqlExpr Left { get; }
    /// <summary>比较运算符</summary>
    public SqlComparisonOperator Operator { get; }
    /// <summary>右侧表达式（某些运算符如 IS NULL 可为空）</summary>
    public SqlExpr? Right { get; }

    /// <summary>创建 SQL 条件表达式</summary>
    /// <param name="left">左侧表达式</param>
    /// <param name="operator">比较运算符</param>
    /// <param name="right">右侧表达式（可选）</param>
    public SqlCondition(SqlExpr left, SqlComparisonOperator @operator, SqlExpr? right = null)
    {
        Left = Guard.NotNull(left);
        Operator = @operator;
        Right = right;
    }
}
