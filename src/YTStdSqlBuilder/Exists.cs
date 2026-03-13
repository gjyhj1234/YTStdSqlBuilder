using YTStdSqlBuilder.Expressions;

namespace YTStdSqlBuilder;

/// <summary>提供 SQL EXISTS 子查询的工厂方法</summary>
public static class Exists
{
    /// <summary>创建 EXISTS 子查询表达式</summary>
    /// <param name="subquery">子查询构建器</param>
    /// <returns>子查询表达式</returns>
    public static SubQueryExpr Of(PgSqlQueryBuilder subquery)
    {
        var result = subquery.Build();
        return new SubQueryExpr(result);
    }
}
