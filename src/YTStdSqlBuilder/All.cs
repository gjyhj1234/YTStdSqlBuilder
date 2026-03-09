using YTStdSqlBuilder.Expressions;

namespace YTStdSqlBuilder;

public static class All
{
    public static readonly AllExpr Value = AllExpr.Star;
    public static AllExpr Of(SqlTableSource table) => new(table);
}
