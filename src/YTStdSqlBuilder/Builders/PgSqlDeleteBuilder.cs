using YTStdSqlBuilder.Conditions;
using YTStdSqlBuilder.Expressions;
using YTStdSqlBuilder.Internal;

namespace YTStdSqlBuilder;

public sealed class PgSqlDeleteBuilder
{
    private readonly SqlTableSource _table;
    private readonly ConditionGroupBuilder _whereBuilder = new();
    private readonly List<SqlSelectItem> _returning = new();
    private bool _allowUnsafe;
    private bool _built;

    internal SqlTableSource Table => _table;
    internal ConditionGroupBuilder WhereBuilder => _whereBuilder;
    internal IReadOnlyList<SqlSelectItem> ReturningItems => _returning;

    internal PgSqlDeleteBuilder(SqlTableSource table)
    {
        _table = Guard.NotNull(table);
    }

    // WHERE
    public PgSqlDeleteBuilder Where(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _whereBuilder.Where(left, op, right);
        return this;
    }

    public PgSqlDeleteBuilder Where(SqlExpr expr)
    {
        _whereBuilder.Where(expr);
        return this;
    }

    public PgSqlDeleteBuilder WhereIf(bool condition, SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _whereBuilder.WhereIf(condition, left, op, right);
        return this;
    }

    public PgSqlDeleteBuilder And(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _whereBuilder.And(left, op, right);
        return this;
    }

    public PgSqlDeleteBuilder AndIf(bool condition, SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _whereBuilder.AndIf(condition, left, op, right);
        return this;
    }

    public PgSqlDeleteBuilder Or(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _whereBuilder.Or(left, op, right);
        return this;
    }

    public PgSqlDeleteBuilder OrIf(bool condition, SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _whereBuilder.OrIf(condition, left, op, right);
        return this;
    }

    public PgSqlDeleteBuilder WhereGroup(Action<ConditionGroupBuilder> group)
    {
        _whereBuilder.WhereGroup(group);
        return this;
    }

    public PgSqlDeleteBuilder AndGroup(Action<ConditionGroupBuilder> group)
    {
        _whereBuilder.AndGroup(group);
        return this;
    }

    public PgSqlDeleteBuilder OrGroup(Action<ConditionGroupBuilder> group)
    {
        _whereBuilder.OrGroup(group);
        return this;
    }

    // RETURNING
    public PgSqlDeleteBuilder Returning(params SqlSelectItem[] items)
    {
        _returning.AddRange(items);
        return this;
    }

    public PgSqlDeleteBuilder Returning(params SqlExpr[] exprs)
    {
        foreach (var expr in exprs)
            _returning.Add(new SqlSelectItem(expr));
        return this;
    }

    public PgSqlDeleteBuilder AllowUnsafeDelete()
    {
        _allowUnsafe = true;
        return this;
    }

    public PgSqlRenderResult Build()
    {
        if (_built)
            ThrowHelper.ThrowInvalidOperation("This delete builder has already been built.");
        _built = true;

        if (!_allowUnsafe && _whereBuilder.IsEmpty)
            ThrowHelper.ThrowInvalidOperation("DELETE without WHERE is unsafe. Call AllowUnsafeDelete() to allow it.");

        return PgSqlRenderer.RenderDelete(this, PgSqlRenderMode.Parameterized);
    }

    public string BuildDebugSql()
    {
        if (_built)
            ThrowHelper.ThrowInvalidOperation("This delete builder has already been built.");
        _built = true;

        if (!_allowUnsafe && _whereBuilder.IsEmpty)
            ThrowHelper.ThrowInvalidOperation("DELETE without WHERE is unsafe. Call AllowUnsafeDelete() to allow it.");

        return PgSqlRenderer.RenderDelete(this, PgSqlRenderMode.DebugSql).Sql;
    }
}
