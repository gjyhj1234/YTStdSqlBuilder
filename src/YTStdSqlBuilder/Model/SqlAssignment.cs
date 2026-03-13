using YTStdSqlBuilder.Expressions;
using YTStdSqlBuilder.Internal;

namespace YTStdSqlBuilder;

/// <summary>SQL 赋值操作，用于 INSERT/UPDATE 语句</summary>
public sealed class SqlAssignment
{
    /// <summary>目标列</summary>
    public ColumnExpr Column { get; }
    /// <summary>赋值表达式</summary>
    public SqlExpr Value { get; }

    /// <summary>创建 SQL 赋值操作</summary>
    /// <param name="column">目标列</param>
    /// <param name="value">赋值表达式</param>
    public SqlAssignment(ColumnExpr column, SqlExpr value)
    {
        Column = Guard.NotNull(column);
        Value = Guard.NotNull(value);
    }
}
