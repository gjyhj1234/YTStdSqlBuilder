namespace YTStdSqlBuilder.Expressions;

/// <summary>原始 SQL 文本表达式</summary>
public sealed class RawExpr : SqlExpr
{
    /// <summary>表达式类型</summary>
    public override SqlExprKind Kind => SqlExprKind.Raw;

    /// <summary>原始 SQL 文本</summary>
    public string SqlText { get; }

    /// <summary>创建原始 SQL 文本表达式</summary>
    /// <param name="sqlText">SQL 文本</param>
    public RawExpr(string sqlText)
    {
        SqlText = sqlText ?? string.Empty;
    }
}
