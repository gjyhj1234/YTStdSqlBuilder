using NpgsqlTypes;
using YTStdSqlBuilder.Expressions;

namespace YTStdSqlBuilder;

public static class Param
{
    public static ParamExpr Value(object? value) => new(value);
    public static ParamExpr Value(object? value, NpgsqlDbType dbType) => new(value, dbType);
}
