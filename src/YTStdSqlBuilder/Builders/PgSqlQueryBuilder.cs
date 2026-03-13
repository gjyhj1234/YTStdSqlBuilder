using YTStdSqlBuilder.Conditions;
using YTStdSqlBuilder.Expressions;
using YTStdSqlBuilder.Internal;

namespace YTStdSqlBuilder;

/// <summary>PostgreSQL SELECT 查询构建器</summary>
public sealed class PgSqlQueryBuilder
{
    private readonly List<SqlSelectItem> _selectItems = new();
    private bool _distinct;
    private SqlTableSource? _from;
    private PgSqlQueryBuilder? _fromSubQuery;
    private string? _fromSubQueryAlias;
    private readonly List<SqlJoin> _joins = new();
    private readonly ConditionGroupBuilder _whereBuilder = new();
    private readonly List<SqlExpr> _groupBy = new();
    private readonly ConditionGroupBuilder _havingBuilder = new();
    private readonly List<SqlOrderItem> _orderBy = new();
    private int? _limit;
    private int? _offset;
    private bool _built;

    internal IReadOnlyList<SqlSelectItem> SelectItems => _selectItems;
    internal bool IsDistinct => _distinct;
    internal SqlTableSource? FromTable => _from;
    internal PgSqlQueryBuilder? FromSubQuery => _fromSubQuery;
    internal string? FromSubQueryAlias => _fromSubQueryAlias;
    internal IReadOnlyList<SqlJoin> Joins => _joins;
    internal ConditionGroupBuilder WhereBuilder => _whereBuilder;
    internal IReadOnlyList<SqlExpr> GroupByItems => _groupBy;
    internal ConditionGroupBuilder HavingBuilder => _havingBuilder;
    internal IReadOnlyList<SqlOrderItem> OrderByItems => _orderBy;
    internal int? LimitValue => _limit;
    internal int? OffsetValue => _offset;

    internal PgSqlQueryBuilder() { }

    internal PgSqlQueryBuilder(SqlSelectItem[] items)
    {
        _selectItems.AddRange(items);
    }

    internal PgSqlQueryBuilder(SqlExpr[] exprs)
    {
        foreach (var expr in exprs)
            _selectItems.Add(new SqlSelectItem(expr));
    }

    // SELECT
    /// <summary>启用 DISTINCT 去重</summary>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlQueryBuilder Distinct()
    {
        _distinct = true;
        return this;
    }

    // FROM
    /// <summary>设置 FROM 表源</summary>
    /// <param name="table">查询的目标表</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlQueryBuilder From(SqlTableSource table)
    {
        _from = Guard.NotNull(table);
        return this;
    }

    /// <summary>设置 FROM 子查询</summary>
    /// <param name="subQuery">子查询构建器</param>
    /// <param name="alias">子查询别名</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlQueryBuilder From(PgSqlQueryBuilder subQuery, string alias)
    {
        _fromSubQuery = Guard.NotNull(subQuery);
        _fromSubQueryAlias = Guard.NotNullOrEmpty(alias);
        return this;
    }

    // JOIN
    /// <summary>添加 INNER JOIN</summary>
    /// <param name="table">要连接的表</param>
    /// <param name="configure">JOIN 条件配置委托</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlQueryBuilder Join(SqlTableSource table, Action<JoinBuilder> configure)
    {
        return AddJoin(table, SqlJoinType.Inner, configure);
    }

    /// <summary>添加 LEFT JOIN</summary>
    /// <param name="table">要连接的表</param>
    /// <param name="configure">JOIN 条件配置委托</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlQueryBuilder LeftJoin(SqlTableSource table, Action<JoinBuilder> configure)
    {
        return AddJoin(table, SqlJoinType.Left, configure);
    }

    /// <summary>添加 RIGHT JOIN</summary>
    /// <param name="table">要连接的表</param>
    /// <param name="configure">JOIN 条件配置委托</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlQueryBuilder RightJoin(SqlTableSource table, Action<JoinBuilder> configure)
    {
        return AddJoin(table, SqlJoinType.Right, configure);
    }

    private PgSqlQueryBuilder AddJoin(SqlTableSource table, SqlJoinType joinType, Action<JoinBuilder> configure)
    {
        var builder = new JoinBuilder();
        configure(builder);
        _joins.Add(new SqlJoin(table, joinType, builder.Build()));
        return this;
    }

    // WHERE
    /// <summary>添加 WHERE 条件</summary>
    /// <param name="left">左侧表达式</param>
    /// <param name="op">比较运算符</param>
    /// <param name="right">右侧表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlQueryBuilder Where(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _whereBuilder.Where(left, op, right);
        return this;
    }

    /// <summary>添加 WHERE 表达式条件</summary>
    /// <param name="expr">条件表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlQueryBuilder Where(SqlExpr expr)
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
    public PgSqlQueryBuilder WhereIf(bool condition, SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _whereBuilder.WhereIf(condition, left, op, right);
        return this;
    }

    /// <summary>添加 AND 条件</summary>
    /// <param name="left">左侧表达式</param>
    /// <param name="op">比较运算符</param>
    /// <param name="right">右侧表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlQueryBuilder And(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _whereBuilder.And(left, op, right);
        return this;
    }

    /// <summary>添加 AND 表达式条件</summary>
    /// <param name="expr">条件表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlQueryBuilder And(SqlExpr expr)
    {
        _whereBuilder.And(expr);
        return this;
    }

    /// <summary>条件为真时添加 AND 条件</summary>
    /// <param name="condition">是否添加条件</param>
    /// <param name="left">左侧表达式</param>
    /// <param name="op">比较运算符</param>
    /// <param name="right">右侧表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlQueryBuilder AndIf(bool condition, SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _whereBuilder.AndIf(condition, left, op, right);
        return this;
    }

    /// <summary>添加 OR 条件</summary>
    /// <param name="left">左侧表达式</param>
    /// <param name="op">比较运算符</param>
    /// <param name="right">右侧表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlQueryBuilder Or(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _whereBuilder.Or(left, op, right);
        return this;
    }

    /// <summary>添加 OR 表达式条件</summary>
    /// <param name="expr">条件表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlQueryBuilder Or(SqlExpr expr)
    {
        _whereBuilder.Or(expr);
        return this;
    }

    /// <summary>条件为真时添加 OR 条件</summary>
    /// <param name="condition">是否添加条件</param>
    /// <param name="left">左侧表达式</param>
    /// <param name="op">比较运算符</param>
    /// <param name="right">右侧表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlQueryBuilder OrIf(bool condition, SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _whereBuilder.OrIf(condition, left, op, right);
        return this;
    }

    /// <summary>添加 WHERE 条件子组</summary>
    /// <param name="group">条件组配置委托</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlQueryBuilder WhereGroup(Action<ConditionGroupBuilder> group)
    {
        _whereBuilder.WhereGroup(group);
        return this;
    }

    /// <summary>添加 AND 条件子组</summary>
    /// <param name="group">条件组配置委托</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlQueryBuilder AndGroup(Action<ConditionGroupBuilder> group)
    {
        _whereBuilder.AndGroup(group);
        return this;
    }

    /// <summary>添加 OR 条件子组</summary>
    /// <param name="group">条件组配置委托</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlQueryBuilder OrGroup(Action<ConditionGroupBuilder> group)
    {
        _whereBuilder.OrGroup(group);
        return this;
    }

    // GROUP BY
    /// <summary>添加 GROUP BY 子句</summary>
    /// <param name="exprs">分组表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlQueryBuilder GroupBy(params SqlExpr[] exprs)
    {
        _groupBy.AddRange(exprs);
        return this;
    }

    // HAVING
    /// <summary>添加 HAVING 条件</summary>
    /// <param name="left">左侧表达式</param>
    /// <param name="op">比较运算符</param>
    /// <param name="right">右侧表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlQueryBuilder Having(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _havingBuilder.Where(left, op, right);
        return this;
    }

    /// <summary>添加 HAVING AND 条件</summary>
    /// <param name="left">左侧表达式</param>
    /// <param name="op">比较运算符</param>
    /// <param name="right">右侧表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlQueryBuilder HavingAnd(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _havingBuilder.And(left, op, right);
        return this;
    }

    /// <summary>添加 HAVING OR 条件</summary>
    /// <param name="left">左侧表达式</param>
    /// <param name="op">比较运算符</param>
    /// <param name="right">右侧表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlQueryBuilder HavingOr(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _havingBuilder.Or(left, op, right);
        return this;
    }

    // ORDER BY
    /// <summary>添加 ORDER BY 子句（排序项）</summary>
    /// <param name="items">排序项数组</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlQueryBuilder OrderBy(params SqlOrderItem[] items)
    {
        _orderBy.AddRange(items);
        return this;
    }

    /// <summary>添加 ORDER BY 子句（升序）</summary>
    /// <param name="exprs">排序表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlQueryBuilder OrderBy(params SqlExpr[] exprs)
    {
        foreach (var expr in exprs)
            _orderBy.Add(new SqlOrderItem(expr, descending: false));
        return this;
    }

    /// <summary>添加 ORDER BY 子句（降序）</summary>
    /// <param name="exprs">排序表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlQueryBuilder OrderByDesc(params SqlExpr[] exprs)
    {
        foreach (var expr in exprs)
            _orderBy.Add(new SqlOrderItem(expr, descending: true));
        return this;
    }

    // LIMIT / OFFSET
    /// <summary>设置 LIMIT（最大返回行数）</summary>
    /// <param name="limit">最大行数</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlQueryBuilder Limit(int limit)
    {
        _limit = limit;
        return this;
    }

    /// <summary>设置 OFFSET（跳过行数）</summary>
    /// <param name="offset">跳过行数</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public PgSqlQueryBuilder Offset(int offset)
    {
        _offset = offset;
        return this;
    }

    // Build
    /// <summary>构建并渲染 SELECT 查询</summary>
    /// <returns>包含 SQL 字符串和参数数组的渲染结果</returns>
    public PgSqlRenderResult Build()
    {
        if (_built)
            ThrowHelper.ThrowInvalidOperation("This query builder has already been built.");
        _built = true;
        return PgSqlRenderer.RenderQuery(this, PgSqlRenderMode.Parameterized);
    }

    /// <summary>构建调试用 SQL 字符串（参数值内嵌）</summary>
    /// <returns>调试用 SQL 字符串</returns>
    public string BuildDebugSql()
    {
        if (_built)
            ThrowHelper.ThrowInvalidOperation("This query builder has already been built.");
        _built = true;
        return PgSqlRenderer.RenderQuery(this, PgSqlRenderMode.DebugSql).Sql;
    }
}
