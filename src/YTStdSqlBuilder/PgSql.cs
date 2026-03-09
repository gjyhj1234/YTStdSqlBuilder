using YTStdSqlBuilder.Expressions;

namespace YTStdSqlBuilder;

public static class PgSql
{
    public static PgSqlQueryBuilder Select(params SqlSelectItem[] items) => new(items);
    public static PgSqlQueryBuilder Select(params SqlExpr[] exprs) => new(exprs);

    public static PgSqlInsertBuilder InsertInto(SqlTableSource table) => new(table);
    public static PgSqlUpdateBuilder Update(SqlTableSource table) => new(table);
    public static PgSqlDeleteBuilder DeleteFrom(SqlTableSource table) => new(table);
}
