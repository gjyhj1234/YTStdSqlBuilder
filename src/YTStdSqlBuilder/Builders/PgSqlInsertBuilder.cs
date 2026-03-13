using YTStdSqlBuilder.Expressions;
using YTStdSqlBuilder.Internal;

namespace YTStdSqlBuilder;

/// <summary>PostgreSQL INSERT 语句构建器</summary>
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

    /// <summary>设置列值（逐列添加）</summary>
    /// <param name="column">目标列</param>
    /// <param name="value">值表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlInsertBuilder Set(ColumnExpr column, SqlExpr value)
    {
        _assignments.Add(new SqlAssignment(column, value));
        return this;
    }

    /// <summary>指定插入的列</summary>
    /// <param name="columns">列表达式数组</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlInsertBuilder Columns(params ColumnExpr[] columns)
    {
        _columns = columns;
        return this;
    }

    /// <summary>指定插入的值（需先调用 Columns）</summary>
    /// <param name="values">值表达式数组</param>
    /// <returns>当前构建器实例（链式调用）</returns>
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

    /// <summary>添加 RETURNING 子句（选择项）</summary>
    /// <param name="items">要返回的选择项</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlInsertBuilder Returning(params SqlSelectItem[] items)
    {
        _returning.AddRange(items);
        return this;
    }

    /// <summary>添加 RETURNING 子句（表达式）</summary>
    /// <param name="exprs">要返回的表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlInsertBuilder Returning(params SqlExpr[] exprs)
    {
        foreach (var expr in exprs)
            _returning.Add(new SqlSelectItem(expr));
        return this;
    }

    /// <summary>构建并渲染 INSERT 语句</summary>
    /// <returns>包含 SQL 字符串和参数数组的渲染结果</returns>
    public PgSqlRenderResult Build()
    {
        if (_built)
            ThrowHelper.ThrowInvalidOperation("This insert builder has already been built.");
        _built = true;

        if (_assignments.Count == 0)
            ThrowHelper.ThrowInvalidOperation("INSERT requires at least one column assignment.");

        return PgSqlRenderer.RenderInsert(this, PgSqlRenderMode.Parameterized);
    }

    /// <summary>构建调试用 SQL 字符串（参数值内嵌）</summary>
    /// <returns>调试用 SQL 字符串</returns>
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
