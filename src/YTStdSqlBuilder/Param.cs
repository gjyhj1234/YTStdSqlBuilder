using NpgsqlTypes;
using YTStdSqlBuilder.Expressions;

namespace YTStdSqlBuilder;

/// <summary>提供 SQL 参数表达式的工厂方法</summary>
public static class Param
{
    /// <summary>创建参数表达式</summary>
    /// <param name="value">参数值</param>
    /// <returns>参数表达式</returns>
    public static ParamExpr Value(object? value) => new(value);

    /// <summary>创建带数据库类型的参数表达式</summary>
    /// <param name="value">参数值</param>
    /// <param name="dbType">NpgsqlDbType 数据库类型</param>
    /// <returns>参数表达式</returns>
    public static ParamExpr Value(object? value, NpgsqlDbType dbType) => new(value, dbType);
}
