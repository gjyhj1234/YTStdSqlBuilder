using YTStdSqlBuilder.Expressions;
using YTStdSqlBuilder.Internal;

namespace YTStdSqlBuilder;

public sealed class SqlSelectItem
{
    public SqlExpr Expression { get; }
    public string? Alias { get; }
    public Type? ClrType { get; }

    public SqlSelectItem(SqlExpr expression, string? alias = null, Type? clrType = null)
    {
        Expression = Guard.NotNull(expression);
        Alias = alias;
        ClrType = clrType;
    }

    public static implicit operator SqlSelectItem(SqlExpr expr) => new(expr);
    public static implicit operator SqlSelectItem(ColumnExpr expr) => new(expr);
}
