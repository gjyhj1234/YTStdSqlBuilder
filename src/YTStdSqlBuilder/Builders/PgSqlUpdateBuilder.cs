using YTStdSqlBuilder.Conditions;
using YTStdSqlBuilder.Expressions;
using YTStdSqlBuilder.Internal;

namespace YTStdSqlBuilder;

public sealed class PgSqlUpdateBuilder
{
    private readonly SqlTableSource _table;
    private readonly List<SqlAssignment> _assignments = new();
    private readonly ConditionGroupBuilder _whereBuilder = new();
    private readonly List<SqlSelectItem> _returning = new();
    private bool _allowUnsafe;
    private bool _built;

    internal SqlTableSource Table => _table;
    internal IReadOnlyList<SqlAssignment> Assignments => _assignments;
    internal ConditionGroupBuilder WhereBuilder => _whereBuilder;
    internal IReadOnlyList<SqlSelectItem> ReturningItems => _returning;

    internal PgSqlUpdateBuilder(SqlTableSource table)
    {
        _table = Guard.NotNull(table);
    }

    public PgSqlUpdateBuilder Set(ColumnExpr column, SqlExpr value)
    {
        _assignments.Add(new SqlAssignment(column, value));
        return this;
    }

    public PgSqlUpdateBuilder SetIf(bool condition, ColumnExpr column, SqlExpr value)
    {
        if (condition)
            _assignments.Add(new SqlAssignment(column, value));
        return this;
    }

    // WHERE
    public PgSqlUpdateBuilder Where(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _whereBuilder.Where(left, op, right);
        return this;
    }

    public PgSqlUpdateBuilder Where(SqlExpr expr)
    {
        _whereBuilder.Where(expr);
        return this;
    }

    public PgSqlUpdateBuilder WhereIf(bool condition, SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _whereBuilder.WhereIf(condition, left, op, right);
        return this;
    }

    public PgSqlUpdateBuilder And(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _whereBuilder.And(left, op, right);
        return this;
    }

    public PgSqlUpdateBuilder AndIf(bool condition, SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _whereBuilder.AndIf(condition, left, op, right);
        return this;
    }

    public PgSqlUpdateBuilder Or(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _whereBuilder.Or(left, op, right);
        return this;
    }

    public PgSqlUpdateBuilder OrIf(bool condition, SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _whereBuilder.OrIf(condition, left, op, right);
        return this;
    }

    public PgSqlUpdateBuilder WhereGroup(Action<ConditionGroupBuilder> group)
    {
        _whereBuilder.WhereGroup(group);
        return this;
    }

    public PgSqlUpdateBuilder AndGroup(Action<ConditionGroupBuilder> group)
    {
        _whereBuilder.AndGroup(group);
        return this;
    }

    public PgSqlUpdateBuilder OrGroup(Action<ConditionGroupBuilder> group)
    {
        _whereBuilder.OrGroup(group);
        return this;
    }

    // RETURNING
    public PgSqlUpdateBuilder Returning(params SqlSelectItem[] items)
    {
        _returning.AddRange(items);
        return this;
    }

    public PgSqlUpdateBuilder Returning(params SqlExpr[] exprs)
    {
        foreach (var expr in exprs)
            _returning.Add(new SqlSelectItem(expr));
        return this;
    }

    public PgSqlUpdateBuilder AllowUnsafeUpdate()
    {
        _allowUnsafe = true;
        return this;
    }

    public PgSqlRenderResult Build()
    {
        if (_built)
            ThrowHelper.ThrowInvalidOperation("This update builder has already been built.");
        _built = true;

        if (_assignments.Count == 0)
            ThrowHelper.ThrowInvalidOperation("UPDATE requires at least one column assignment.");

        if (!_allowUnsafe && _whereBuilder.IsEmpty)
            ThrowHelper.ThrowInvalidOperation("UPDATE without WHERE is unsafe. Call AllowUnsafeUpdate() to allow it.");

        return PgSqlRenderer.RenderUpdate(this, PgSqlRenderMode.Parameterized);
    }

    public string BuildDebugSql()
    {
        if (_built)
            ThrowHelper.ThrowInvalidOperation("This update builder has already been built.");
        _built = true;

        if (_assignments.Count == 0)
            ThrowHelper.ThrowInvalidOperation("UPDATE requires at least one column assignment.");

        if (!_allowUnsafe && _whereBuilder.IsEmpty)
            ThrowHelper.ThrowInvalidOperation("UPDATE without WHERE is unsafe. Call AllowUnsafeUpdate() to allow it.");

        return PgSqlRenderer.RenderUpdate(this, PgSqlRenderMode.DebugSql).Sql;
    }
}
