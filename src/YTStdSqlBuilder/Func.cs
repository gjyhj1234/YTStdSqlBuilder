using YTStdSqlBuilder.Expressions;

namespace YTStdSqlBuilder;

public static class Func
{
    public static FuncExpr Count(SqlExpr expr) => new("COUNT", expr);
    public static FuncExpr Sum(SqlExpr expr) => new("SUM", expr);
    public static FuncExpr Avg(SqlExpr expr) => new("AVG", expr);
    public static FuncExpr Min(SqlExpr expr) => new("MIN", expr);
    public static FuncExpr Max(SqlExpr expr) => new("MAX", expr);
    public static FuncExpr Coalesce(params SqlExpr[] exprs) => new("COALESCE", exprs);
    public static FuncExpr NullIf(SqlExpr a, SqlExpr b) => new("NULLIF", a, b);
}
