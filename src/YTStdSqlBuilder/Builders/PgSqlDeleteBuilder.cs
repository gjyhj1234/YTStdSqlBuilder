using YTStdSqlBuilder.Conditions;
using YTStdSqlBuilder.Expressions;
using YTStdSqlBuilder.Internal;

namespace YTStdSqlBuilder;

/// <summary>PostgreSQL DELETE 语句构建器</summary>
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
    /// <summary>添加 WHERE 条件</summary>
    /// <param name="left">左侧表达式</param>
    /// <param name="op">比较运算符</param>
    /// <param name="right">右侧表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlDeleteBuilder Where(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _whereBuilder.Where(left, op, right);
        return this;
    }

    /// <summary>添加 WHERE 表达式条件</summary>
    /// <param name="expr">条件表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlDeleteBuilder Where(SqlExpr expr)
    {
        _whereBuilder.Where(expr);
        return this;
    }

    /// <summary>条件为真时添加 WHERE 条件</summary>
    /// <param name="condition">是否添加条件</param>
    /// <param name="left">左侧表达式</param>
    /// <param name="op">比较运算符</param>
    /// <param name="right">右侧表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlDeleteBuilder WhereIf(bool condition, SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _whereBuilder.WhereIf(condition, left, op, right);
        return this;
    }

    /// <summary>添加 AND 条件</summary>
    /// <param name="left">左侧表达式</param>
    /// <param name="op">比较运算符</param>
    /// <param name="right">右侧表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlDeleteBuilder And(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _whereBuilder.And(left, op, right);
        return this;
    }

    /// <summary>条件为真时添加 AND 条件</summary>
    /// <param name="condition">是否添加条件</param>
    /// <param name="left">左侧表达式</param>
    /// <param name="op">比较运算符</param>
    /// <param name="right">右侧表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlDeleteBuilder AndIf(bool condition, SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _whereBuilder.AndIf(condition, left, op, right);
        return this;
    }

    /// <summary>添加 OR 条件</summary>
    /// <param name="left">左侧表达式</param>
    /// <param name="op">比较运算符</param>
    /// <param name="right">右侧表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlDeleteBuilder Or(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _whereBuilder.Or(left, op, right);
        return this;
    }

    /// <summary>条件为真时添加 OR 条件</summary>
    /// <param name="condition">是否添加条件</param>
    /// <param name="left">左侧表达式</param>
    /// <param name="op">比较运算符</param>
    /// <param name="right">右侧表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlDeleteBuilder OrIf(bool condition, SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _whereBuilder.OrIf(condition, left, op, right);
        return this;
    }

    /// <summary>添加 WHERE 条件子组</summary>
    /// <param name="group">条件组配置委托</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlDeleteBuilder WhereGroup(Action<ConditionGroupBuilder> group)
    {
        _whereBuilder.WhereGroup(group);
        return this;
    }

    /// <summary>添加 AND 条件子组</summary>
    /// <param name="group">条件组配置委托</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlDeleteBuilder AndGroup(Action<ConditionGroupBuilder> group)
    {
        _whereBuilder.AndGroup(group);
        return this;
    }

    /// <summary>添加 OR 条件子组</summary>
    /// <param name="group">条件组配置委托</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlDeleteBuilder OrGroup(Action<ConditionGroupBuilder> group)
    {
        _whereBuilder.OrGroup(group);
        return this;
    }

    // RETURNING
    /// <summary>添加 RETURNING 子句（选择项）</summary>
    /// <param name="items">要返回的选择项</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlDeleteBuilder Returning(params SqlSelectItem[] items)
    {
        _returning.AddRange(items);
        return this;
    }

    /// <summary>添加 RETURNING 子句（表达式）</summary>
    /// <param name="exprs">要返回的表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlDeleteBuilder Returning(params SqlExpr[] exprs)
    {
        foreach (var expr in exprs)
            _returning.Add(new SqlSelectItem(expr));
        return this;
    }

    /// <summary>允许无 WHERE 条件的不安全删除</summary>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlDeleteBuilder AllowUnsafeDelete()
    {
        _allowUnsafe = true;
        return this;
    }

    /// <summary>构建并渲染 DELETE 语句</summary>
    /// <returns>包含 SQL 字符串和参数数组的渲染结果</returns>
    public PgSqlRenderResult Build()
    {
        if (_built)
            ThrowHelper.ThrowInvalidOperation("This delete builder has already been built.");
        _built = true;

        if (!_allowUnsafe && _whereBuilder.IsEmpty)
            ThrowHelper.ThrowInvalidOperation("DELETE without WHERE is unsafe. Call AllowUnsafeDelete() to allow it.");

        return PgSqlRenderer.RenderDelete(this, PgSqlRenderMode.Parameterized);
    }

    /// <summary>构建调试用 SQL 字符串（参数值内嵌）</summary>
    /// <returns>调试用 SQL 字符串</returns>
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
