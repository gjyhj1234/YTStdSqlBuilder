namespace YTStdSqlBuilder;

/// <summary>SQL 渲染模式</summary>
public enum PgSqlRenderMode
{
    /// <summary>参数化模式，使用占位符和参数数组</summary>
    Parameterized,
    /// <summary>调试模式，将参数值直接内嵌到 SQL 中</summary>
    DebugSql
}
