using YTStdSqlBuilder.Conditions;
using YTStdSqlBuilder.Expressions;

namespace YTStdSqlBuilder;

public static class Case
{
    public static CaseWhenBuilder When(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
        => new CaseWhenBuilder().AddWhen(left, op, right);
}

public sealed class CaseWhenBuilder
{
    private readonly List<(SqlConditionGroup Condition, SqlExpr? Result)> _clauses = new();

    internal CaseWhenBuilder AddWhen(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        var group = new SqlConditionGroup(
            new List<SqlConditionNode> { SqlConditionNode.Simple(new SqlCondition(left, op, right)) });
        _clauses.Add((group, null));
        return this;
    }

    public CaseWhenBuilder Then(SqlExpr result)
    {
        var last = _clauses[^1];
        _clauses[^1] = (last.Condition, result);
        return this;
    }

    public CaseWhenBuilder When(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
        => AddWhen(left, op, right);

    public CaseExpr Else(SqlExpr result)
    {
        var clauses = _clauses.Select(c => (c.Condition, c.Result!)).ToList();
        return new CaseExpr(clauses, result);
    }

    public CaseExpr End()
    {
        var clauses = _clauses.Select(c => (c.Condition, c.Result!)).ToList();
        return new CaseExpr(clauses);
    }
}
