using YTStdSqlBuilder.Conditions;
using YTStdSqlBuilder.Expressions;

namespace YTStdSqlBuilder;

public sealed class JoinBuilder
{
    private readonly ConditionGroupBuilder _conditions = new();

    public JoinBuilder On(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _conditions.Where(left, op, right);
        return this;
    }

    public JoinBuilder And(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _conditions.And(left, op, right);
        return this;
    }

    public JoinBuilder Or(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _conditions.Or(left, op, right);
        return this;
    }

    public JoinBuilder OnGroup(Action<ConditionGroupBuilder> group)
    {
        _conditions.AndGroup(group);
        return this;
    }

    internal SqlConditionGroup Build() => _conditions.Build();
}
