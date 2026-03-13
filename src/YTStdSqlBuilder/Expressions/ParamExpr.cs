using NpgsqlTypes;

namespace YTStdSqlBuilder.Expressions;

/// <summary>SQL 参数表达式</summary>
public sealed class ParamExpr : SqlExpr
{
    /// <summary>表达式类型</summary>
    public override SqlExprKind Kind => SqlExprKind.Param;

    /// <summary>参数值</summary>
    public object? Value { get; }
    /// <summary>NpgsqlDbType 数据库类型（可选）</summary>
    public NpgsqlDbType? DbType { get; }

    /// <summary>创建参数表达式</summary>
    /// <param name="value">参数值</param>
    /// <param name="dbType">数据库类型（可选）</param>
    public ParamExpr(object? value, NpgsqlDbType? dbType = null)
    {
        Value = value;
        DbType = dbType;
    }
}
