using YTStdSqlBuilder.Expressions;

namespace YTStdSqlBuilder;

public static class Exists
{
    public static SubQueryExpr Of(PgSqlQueryBuilder subquery)
    {
        var result = subquery.Build();
        return new SubQueryExpr(result);
    }
}
