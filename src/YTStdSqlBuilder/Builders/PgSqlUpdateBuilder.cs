using YTStdSqlBuilder.Conditions;
using YTStdSqlBuilder.Expressions;
using YTStdSqlBuilder.Internal;

namespace YTStdSqlBuilder;

/// <summary>PostgreSQL UPDATE 语句构建器</summary>
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

    /// <summary>设置列值</summary>
    /// <param name="column">目标列</param>
    /// <param name="value">值表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlUpdateBuilder Set(ColumnExpr column, SqlExpr value)
    {
        _assignments.Add(new SqlAssignment(column, value));
        return this;
    }

    /// <summary>条件为真时设置列值</summary>
    /// <param name="condition">是否设置</param>
    /// <param name="column">目标列</param>
    /// <param name="value">值表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlUpdateBuilder SetIf(bool condition, ColumnExpr column, SqlExpr value)
    {
        if (condition)
            _assignments.Add(new SqlAssignment(column, value));
        return this;
    }

    // WHERE
    /// <summary>添加 WHERE 条件</summary>
    /// <param name="left">左侧表达式</param>
    /// <param name="op">比较运算符</param>
    /// <param name="right">右侧表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlUpdateBuilder Where(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _whereBuilder.Where(left, op, right);
        return this;
    }

    /// <summary>添加 WHERE 表达式条件</summary>
    /// <param name="expr">条件表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlUpdateBuilder Where(SqlExpr expr)
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
    public PgSqlUpdateBuilder WhereIf(bool condition, SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _whereBuilder.WhereIf(condition, left, op, right);
        return this;
    }

    /// <summary>添加 AND 条件</summary>
    /// <param name="left">左侧表达式</param>
    /// <param name="op">比较运算符</param>
    /// <param name="right">右侧表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlUpdateBuilder And(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
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
    public PgSqlUpdateBuilder AndIf(bool condition, SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _whereBuilder.AndIf(condition, left, op, right);
        return this;
    }

    /// <summary>添加 OR 条件</summary>
    /// <param name="left">左侧表达式</param>
    /// <param name="op">比较运算符</param>
    /// <param name="right">右侧表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlUpdateBuilder Or(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
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
    public PgSqlUpdateBuilder OrIf(bool condition, SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _whereBuilder.OrIf(condition, left, op, right);
        return this;
    }

    /// <summary>添加 WHERE 条件子组</summary>
    /// <param name="group">条件组配置委托</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlUpdateBuilder WhereGroup(Action<ConditionGroupBuilder> group)
    {
        _whereBuilder.WhereGroup(group);
        return this;
    }

    /// <summary>添加 AND 条件子组</summary>
    /// <param name="group">条件组配置委托</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlUpdateBuilder AndGroup(Action<ConditionGroupBuilder> group)
    {
        _whereBuilder.AndGroup(group);
        return this;
    }

    /// <summary>添加 OR 条件子组</summary>
    /// <param name="group">条件组配置委托</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlUpdateBuilder OrGroup(Action<ConditionGroupBuilder> group)
    {
        _whereBuilder.OrGroup(group);
        return this;
    }

    // RETURNING
    /// <summary>添加 RETURNING 子句（选择项）</summary>
    /// <param name="items">要返回的选择项</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlUpdateBuilder Returning(params SqlSelectItem[] items)
    {
        _returning.AddRange(items);
        return this;
    }

    /// <summary>添加 RETURNING 子句（表达式）</summary>
    /// <param name="exprs">要返回的表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlUpdateBuilder Returning(params SqlExpr[] exprs)
    {
        foreach (var expr in exprs)
            _returning.Add(new SqlSelectItem(expr));
        return this;
    }

    /// <summary>允许无 WHERE 条件的不安全更新</summary>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlUpdateBuilder AllowUnsafeUpdate()
    {
        _allowUnsafe = true;
        return this;
    }

    /// <summary>构建并渲染 UPDATE 语句</summary>
    /// <returns>包含 SQL 字符串和参数数组的渲染结果</returns>
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

    /// <summary>构建调试用 SQL 字符串（参数值内嵌）</summary>
    /// <returns>调试用 SQL 字符串</returns>
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
