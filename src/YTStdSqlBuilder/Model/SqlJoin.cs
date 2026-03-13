using YTStdSqlBuilder.Conditions;
using YTStdSqlBuilder.Internal;

namespace YTStdSqlBuilder;

/// <summary>SQL JOIN 定义</summary>
public sealed class SqlJoin
{
    /// <summary>要连接的表</summary>
    public SqlTableSource Table { get; }
    /// <summary>连接类型</summary>
    public SqlJoinType JoinType { get; }
    /// <summary>ON 条件组</summary>
    public SqlConditionGroup OnCondition { get; }

    /// <summary>创建 SQL JOIN 定义</summary>
    /// <param name="table">要连接的表</param>
    /// <param name="joinType">连接类型</param>
    /// <param name="onCondition">ON 条件组</param>
    public SqlJoin(SqlTableSource table, SqlJoinType joinType, SqlConditionGroup onCondition)
    {
        Table = Guard.NotNull(table);
        JoinType = joinType;
        OnCondition = Guard.NotNull(onCondition);
    }
}
