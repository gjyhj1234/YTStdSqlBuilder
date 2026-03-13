using YTStdSqlBuilder.Internal;

namespace YTStdSqlBuilder;

/// <summary>SQL 表定义</summary>
public sealed class SqlTable
{
    /// <summary>表名</summary>
    public string Name { get; }
    /// <summary>Schema 名称（可选）</summary>
    public string? Schema { get; }

    /// <summary>创建 SQL 表定义</summary>
    /// <param name="name">表名</param>
    /// <param name="schema">Schema 名称（可选）</param>
    public SqlTable(string name, string? schema = null)
    {
        Name = Guard.NotNullOrEmpty(name);
        Schema = schema;
    }

    /// <summary>为表设置别名，创建表源</summary>
    /// <param name="alias">表别名</param>
    /// <returns>带别名的表源</returns>
    public SqlTableSource As(string alias) => new(this, alias);
}
