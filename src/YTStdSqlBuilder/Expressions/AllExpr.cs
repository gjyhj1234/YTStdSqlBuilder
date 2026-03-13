namespace YTStdSqlBuilder.Expressions;

/// <summary>通配符表达式，表示 SELECT * 或 表.* </summary>
public sealed class AllExpr : SqlExpr
{
    /// <summary>表达式类型</summary>
    public override SqlExprKind Kind => SqlExprKind.All;

    /// <summary>关联的表源（为 null 表示全局 *）</summary>
    public SqlTableSource? TableSource { get; }

    /// <summary>创建通配符表达式</summary>
    /// <param name="tableSource">关联的表源（可选）</param>
    public AllExpr(SqlTableSource? tableSource = null)
    {
        TableSource = tableSource;
    }

    /// <summary>全局通配符（*）实例</summary>
    public static readonly AllExpr Star = new();
}
