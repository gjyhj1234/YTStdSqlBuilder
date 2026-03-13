using YTStdSqlBuilder.Expressions;

namespace YTStdSqlBuilder;

/// <summary>PostgreSQL SQL 语句构建器入口</summary>
public static class PgSql
{
    /// <summary>创建 SELECT 查询构建器</summary>
    /// <param name="items">要选择的列项</param>
    /// <returns>SELECT 查询构建器实例</returns>
    public static PgSqlQueryBuilder Select(params SqlSelectItem[] items) => new(items);

    /// <summary>创建 SELECT 查询构建器</summary>
    /// <param name="exprs">要选择的表达式</param>
    /// <returns>SELECT 查询构建器实例</returns>
    public static PgSqlQueryBuilder Select(params SqlExpr[] exprs) => new(exprs);

    /// <summary>创建 INSERT INTO 语句构建器</summary>
    /// <param name="table">目标表</param>
    /// <returns>INSERT 构建器实例</returns>
    public static PgSqlInsertBuilder InsertInto(SqlTableSource table) => new(table);

    /// <summary>创建 UPDATE 语句构建器</summary>
    /// <param name="table">目标表</param>
    /// <returns>UPDATE 构建器实例</returns>
    public static PgSqlUpdateBuilder Update(SqlTableSource table) => new(table);

    /// <summary>创建 DELETE FROM 语句构建器</summary>
    /// <param name="table">目标表</param>
    /// <returns>DELETE 构建器实例</returns>
    public static PgSqlDeleteBuilder DeleteFrom(SqlTableSource table) => new(table);
}
