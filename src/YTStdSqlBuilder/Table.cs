namespace YTStdSqlBuilder;

/// <summary>提供 SQL 表定义的工厂方法</summary>
public static class Table
{
    /// <summary>定义表（使用默认 schema）</summary>
    /// <param name="name">表名</param>
    /// <returns>SQL 表定义</returns>
    public static SqlTable Def(string name) => new(name);

    /// <summary>定义带 schema 的表</summary>
    /// <param name="schema">schema 名称</param>
    /// <param name="name">表名</param>
    /// <returns>SQL 表定义</returns>
    public static SqlTable Def(string schema, string name) => new(name, schema);
}
