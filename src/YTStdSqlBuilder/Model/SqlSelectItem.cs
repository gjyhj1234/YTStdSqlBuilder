using YTStdSqlBuilder.Expressions;
using YTStdSqlBuilder.Internal;

namespace YTStdSqlBuilder;

/// <summary>SQL 选择项（SELECT 子句中的列或表达式）</summary>
public sealed class SqlSelectItem
{
    /// <summary>选择表达式</summary>
    public SqlExpr Expression { get; }
    /// <summary>列别名（可选）</summary>
    public string? Alias { get; }
    /// <summary>CLR 类型（可选）</summary>
    public Type? ClrType { get; }

    /// <summary>创建 SQL 选择项</summary>
    /// <param name="expression">选择表达式</param>
    /// <param name="alias">列别名（可选）</param>
    /// <param name="clrType">CLR 类型（可选）</param>
    public SqlSelectItem(SqlExpr expression, string? alias = null, Type? clrType = null)
    {
        Expression = Guard.NotNull(expression);
        Alias = alias;
        ClrType = clrType;
    }

    /// <summary>从 SqlExpr 隐式转换为 SqlSelectItem</summary>
    /// <param name="expr">SQL 表达式</param>
    public static implicit operator SqlSelectItem(SqlExpr expr) => new(expr);
    /// <summary>从 ColumnExpr 隐式转换为 SqlSelectItem</summary>
    /// <param name="expr">列表达式</param>
    public static implicit operator SqlSelectItem(ColumnExpr expr) => new(expr);
}
