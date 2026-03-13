using YTStdSqlBuilder.Expressions;

namespace YTStdSqlBuilder;

/// <summary>提供 SQL 聚合函数和常用函数的工厂方法</summary>
public static class Func
{
    /// <summary>创建 COUNT 聚合函数表达式</summary>
    /// <param name="expr">要计数的表达式</param>
    /// <returns>COUNT 函数表达式</returns>
    public static FuncExpr Count(SqlExpr expr) => new("COUNT", expr);

    /// <summary>创建 SUM 聚合函数表达式</summary>
    /// <param name="expr">要求和的表达式</param>
    /// <returns>SUM 函数表达式</returns>
    public static FuncExpr Sum(SqlExpr expr) => new("SUM", expr);

    /// <summary>创建 AVG 聚合函数表达式</summary>
    /// <param name="expr">要求平均值的表达式</param>
    /// <returns>AVG 函数表达式</returns>
    public static FuncExpr Avg(SqlExpr expr) => new("AVG", expr);

    /// <summary>创建 MIN 聚合函数表达式</summary>
    /// <param name="expr">要求最小值的表达式</param>
    /// <returns>MIN 函数表达式</returns>
    public static FuncExpr Min(SqlExpr expr) => new("MIN", expr);

    /// <summary>创建 MAX 聚合函数表达式</summary>
    /// <param name="expr">要求最大值的表达式</param>
    /// <returns>MAX 函数表达式</returns>
    public static FuncExpr Max(SqlExpr expr) => new("MAX", expr);

    /// <summary>创建 COALESCE 函数表达式，返回第一个非 NULL 值</summary>
    /// <param name="exprs">候选表达式列表</param>
    /// <returns>COALESCE 函数表达式</returns>
    public static FuncExpr Coalesce(params SqlExpr[] exprs) => new("COALESCE", exprs);

    /// <summary>创建 NULLIF 函数表达式，两值相等时返回 NULL</summary>
    /// <param name="a">第一个表达式</param>
    /// <param name="b">第二个表达式</param>
    /// <returns>NULLIF 函数表达式</returns>
    public static FuncExpr NullIf(SqlExpr a, SqlExpr b) => new("NULLIF", a, b);
}
