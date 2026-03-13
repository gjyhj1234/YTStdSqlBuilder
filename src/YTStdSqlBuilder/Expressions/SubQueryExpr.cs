using YTStdSqlBuilder.Internal;

namespace YTStdSqlBuilder.Expressions;

/// <summary>子查询表达式</summary>
public sealed class SubQueryExpr : SqlExpr
{
    /// <summary>表达式类型</summary>
    public override SqlExprKind Kind => SqlExprKind.SubQuery;

    /// <summary>子查询 SQL 文本</summary>
    public string Sql { get; }
    /// <summary>子查询参数数组</summary>
    public PgSqlParam[] Params { get; }

    /// <summary>创建子查询表达式</summary>
    /// <param name="sql">SQL 文本</param>
    /// <param name="params">参数数组（可选）</param>
    public SubQueryExpr(string sql, PgSqlParam[]? @params = null)
    {
        Sql = Guard.NotNullOrEmpty(sql);
        Params = @params ?? Array.Empty<PgSqlParam>();
    }

    /// <summary>从渲染结果创建子查询表达式</summary>
    /// <param name="renderResult">SQL 渲染结果</param>
    public SubQueryExpr(PgSqlRenderResult renderResult)
    {
        Sql = Guard.NotNullOrEmpty(renderResult.Sql);
        Params = renderResult.Params;
    }
}
