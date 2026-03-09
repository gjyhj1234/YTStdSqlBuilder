using YTStdSqlBuilder.Expressions;
using YTStdSqlBuilder.Internal;

namespace YTStdSqlBuilder;

public sealed class PgSqlInsertBuilder
{
    private readonly SqlTableSource _table;
    private readonly List<SqlAssignment> _assignments = new();
    private readonly List<SqlSelectItem> _returning = new();
    private ColumnExpr[]? _columns;
    private bool _built;

    internal SqlTableSource Table => _table;
    internal IReadOnlyList<SqlAssignment> Assignments => _assignments;
    internal IReadOnlyList<SqlSelectItem> ReturningItems => _returning;

    internal PgSqlInsertBuilder(SqlTableSource table)
    {
        _table = Guard.NotNull(table);
    }

    public PgSqlInsertBuilder Set(ColumnExpr column, SqlExpr value)
    {
        _assignments.Add(new SqlAssignment(column, value));
        return this;
    }

    public PgSqlInsertBuilder Columns(params ColumnExpr[] columns)
    {
        _columns = columns;
        return this;
    }

    public PgSqlInsertBuilder Values(params SqlExpr[] values)
    {
        if (_columns == null || _columns.Length == 0)
            ThrowHelper.ThrowInvalidOperation("Columns must be specified before Values.");

        if (_columns.Length != values.Length)
            ThrowHelper.ThrowInvalidOperation("Column count must match value count.");

        for (int i = 0; i < _columns.Length; i++)
            _assignments.Add(new SqlAssignment(_columns[i], values[i]));

        return this;
    }

    public PgSqlInsertBuilder Returning(params SqlSelectItem[] items)
    {
        _returning.AddRange(items);
        return this;
    }

    public PgSqlInsertBuilder Returning(params SqlExpr[] exprs)
    {
        foreach (var expr in exprs)
            _returning.Add(new SqlSelectItem(expr));
        return this;
    }

    public PgSqlRenderResult Build()
    {
        if (_built)
            ThrowHelper.ThrowInvalidOperation("This insert builder has already been built.");
        _built = true;

        if (_assignments.Count == 0)
            ThrowHelper.ThrowInvalidOperation("INSERT requires at least one column assignment.");

        return PgSqlRenderer.RenderInsert(this, PgSqlRenderMode.Parameterized);
    }

    public string BuildDebugSql()
    {
        if (_built)
            ThrowHelper.ThrowInvalidOperation("This insert builder has already been built.");
        _built = true;

        if (_assignments.Count == 0)
            ThrowHelper.ThrowInvalidOperation("INSERT requires at least one column assignment.");

        return PgSqlRenderer.RenderInsert(this, PgSqlRenderMode.DebugSql).Sql;
    }
}
