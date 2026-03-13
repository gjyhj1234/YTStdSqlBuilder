using YTStdSqlBuilder.Internal;

namespace YTStdSqlBuilder.Expressions;

/// <summary>列引用表达式</summary>
public sealed class ColumnExpr : SqlExpr
{
    /// <summary>表达式类型</summary>
    public override SqlExprKind Kind => SqlExprKind.Column;

    /// <summary>引用的列定义</summary>
    public SqlColumn Column { get; }

    /// <summary>创建列引用表达式</summary>
    /// <param name="column">列定义</param>
    public ColumnExpr(SqlColumn column)
    {
        Column = Guard.NotNull(column);
    }
}
