using YTStdSqlBuilder.Expressions;
using YTStdSqlBuilder.Internal;

namespace YTStdSqlBuilder;

/// <summary>SQL 排序项</summary>
public sealed class SqlOrderItem
{
    /// <summary>排序表达式</summary>
    public SqlExpr Expression { get; }
    /// <summary>是否降序</summary>
    public bool Descending { get; }

    /// <summary>创建排序项</summary>
    /// <param name="expression">排序表达式</param>
    /// <param name="descending">是否降序</param>
    public SqlOrderItem(SqlExpr expression, bool descending = false)
    {
        Expression = Guard.NotNull(expression);
        Descending = descending;
    }
}
