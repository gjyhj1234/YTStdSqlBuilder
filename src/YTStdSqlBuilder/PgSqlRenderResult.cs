namespace YTStdSqlBuilder;

/// <summary>SQL 渲染结果，包含 SQL 字符串和参数数组</summary>
public readonly struct PgSqlRenderResult
{
    /// <summary>渲染后的 SQL 字符串</summary>
    public readonly string Sql;
    /// <summary>SQL 参数数组</summary>
    public readonly PgSqlParam[] Params;

    /// <summary>创建 SQL 渲染结果</summary>
    /// <param name="sql">SQL 字符串</param>
    /// <param name="params">参数数组</param>
    public PgSqlRenderResult(string sql, PgSqlParam[] @params)
    {
        Sql = sql;
        Params = @params;
    }
}
