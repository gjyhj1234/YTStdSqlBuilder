using YTStdSqlBuilder.Expressions;
using YTStdSqlBuilder.Internal;

namespace YTStdSqlBuilder;

/// <summary>SQL 表源（带别名的表引用）</summary>
public sealed class SqlTableSource
{
    /// <summary>表定义</summary>
    public SqlTable Table { get; }
    /// <summary>表别名</summary>
    public string Alias { get; }

    /// <summary>创建 SQL 表源</summary>
    /// <param name="table">表定义</param>
    /// <param name="alias">表别名</param>
    public SqlTableSource(SqlTable table, string alias)
    {
        Table = Guard.NotNull(table);
        Alias = Guard.NotNullOrEmpty(alias);
    }

    /// <summary>通过列名获取列表达式</summary>
    /// <param name="columnName">列名</param>
    /// <returns>列表达式</returns>
    public ColumnExpr this[string columnName] => new(new SqlColumn(this, columnName));

    /// <summary>创建列引用表达式</summary>
    /// <param name="columnName">列名</param>
    /// <returns>列表达式</returns>
    public ColumnExpr Col(string columnName) => this[columnName];

    /// <summary>创建带 CLR 类型的列引用表达式</summary>
    /// <typeparam name="T">CLR 类型</typeparam>
    /// <param name="columnName">列名</param>
    /// <returns>列表达式</returns>
    public ColumnExpr Col<T>(string columnName) => new(new SqlColumn(this, columnName, typeof(T)));
}
