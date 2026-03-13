using NpgsqlTypes;

namespace YTStdSqlBuilder;

/// <summary>PostgreSQL 查询参数</summary>
public readonly struct PgSqlParam
{
    /// <summary>参数名称</summary>
    public readonly string Name;
    /// <summary>NpgsqlDbType 数据库类型（可选）</summary>
    public readonly NpgsqlDbType? DbType;
    /// <summary>参数值</summary>
    public readonly object? Value;

    /// <summary>创建 PostgreSQL 查询参数</summary>
    /// <param name="name">参数名称</param>
    /// <param name="value">参数值</param>
    /// <param name="dbType">数据库类型（可选）</param>
    public PgSqlParam(string name, object? value, NpgsqlDbType? dbType = null)
    {
        Name = name;
        DbType = dbType;
        Value = value;
    }
}
