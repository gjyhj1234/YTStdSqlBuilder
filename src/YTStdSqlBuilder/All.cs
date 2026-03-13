using YTStdSqlBuilder.Expressions;

namespace YTStdSqlBuilder;

/// <summary>提供 SQL 通配符（*）表达式的工厂方法</summary>
public static class All
{
    /// <summary>表示 SELECT * 的通配符表达式</summary>
    public static readonly AllExpr Value = AllExpr.Star;

    /// <summary>创建指定表的通配符表达式（如 t.*）</summary>
    /// <param name="table">目标表源</param>
    /// <returns>指定表的通配符表达式</returns>
    public static AllExpr Of(SqlTableSource table) => new(table);
}
