using YTStdSqlBuilder.Conditions;
using YTStdSqlBuilder.Expressions;
using YTStdSqlBuilder.Internal;

namespace YTStdSqlBuilder;

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
    public PgSqlQueryBuilder Distinct()
    {
        _distinct = true;
        return this;
    }

    // FROM
    public PgSqlQueryBuilder From(SqlTableSource table)
    {
        _from = Guard.NotNull(table);
        return this;
    }

    public PgSqlQueryBuilder From(PgSqlQueryBuilder subQuery, string alias)
    {
        _fromSubQuery = Guard.NotNull(subQuery);
        _fromSubQueryAlias = Guard.NotNullOrEmpty(alias);
        return this;
    }

    // JOIN
    public PgSqlQueryBuilder Join(SqlTableSource table, Action<JoinBuilder> configure)
    {
        return AddJoin(table, SqlJoinType.Inner, configure);
    }

    public PgSqlQueryBuilder LeftJoin(SqlTableSource table, Action<JoinBuilder> configure)
    {
        return AddJoin(table, SqlJoinType.Left, configure);
    }

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
    public PgSqlQueryBuilder Where(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _whereBuilder.Where(left, op, right);
        return this;
    }

    public PgSqlQueryBuilder Where(SqlExpr expr)
    {
        _whereBuilder.Where(expr);
        return this;
    }

    public PgSqlQueryBuilder WhereIf(bool condition, SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _whereBuilder.WhereIf(condition, left, op, right);
        return this;
    }

    public PgSqlQueryBuilder And(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _whereBuilder.And(left, op, right);
        return this;
    }

    public PgSqlQueryBuilder And(SqlExpr expr)
    {
        _whereBuilder.And(expr);
        return this;
    }

    public PgSqlQueryBuilder AndIf(bool condition, SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _whereBuilder.AndIf(condition, left, op, right);
        return this;
    }

    public PgSqlQueryBuilder Or(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _whereBuilder.Or(left, op, right);
        return this;
    }

    public PgSqlQueryBuilder Or(SqlExpr expr)
    {
        _whereBuilder.Or(expr);
        return this;
    }

    public PgSqlQueryBuilder OrIf(bool condition, SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _whereBuilder.OrIf(condition, left, op, right);
        return this;
    }

    public PgSqlQueryBuilder WhereGroup(Action<ConditionGroupBuilder> group)
    {
        _whereBuilder.WhereGroup(group);
        return this;
    }

    public PgSqlQueryBuilder AndGroup(Action<ConditionGroupBuilder> group)
    {
        _whereBuilder.AndGroup(group);
        return this;
    }

    public PgSqlQueryBuilder OrGroup(Action<ConditionGroupBuilder> group)
    {
        _whereBuilder.OrGroup(group);
        return this;
    }

    // GROUP BY
    public PgSqlQueryBuilder GroupBy(params SqlExpr[] exprs)
    {
        _groupBy.AddRange(exprs);
        return this;
    }

    // HAVING
    public PgSqlQueryBuilder Having(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _havingBuilder.Where(left, op, right);
        return this;
    }

    public PgSqlQueryBuilder HavingAnd(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _havingBuilder.And(left, op, right);
        return this;
    }

    public PgSqlQueryBuilder HavingOr(SqlExpr left, SqlComparisonOperator op, SqlExpr right)
    {
        _havingBuilder.Or(left, op, right);
        return this;
    }

    // ORDER BY
    public PgSqlQueryBuilder OrderBy(params SqlOrderItem[] items)
    {
        _orderBy.AddRange(items);
        return this;
    }

    public PgSqlQueryBuilder OrderBy(params SqlExpr[] exprs)
    {
        foreach (var expr in exprs)
            _orderBy.Add(new SqlOrderItem(expr, descending: false));
        return this;
    }

    public PgSqlQueryBuilder OrderByDesc(params SqlExpr[] exprs)
    {
        foreach (var expr in exprs)
            _orderBy.Add(new SqlOrderItem(expr, descending: true));
        return this;
    }

    // LIMIT / OFFSET
    public PgSqlQueryBuilder Limit(int limit)
    {
        _limit = limit;
        return this;
    }

    public PgSqlQueryBuilder Offset(int offset)
    {
        _offset = offset;
        return this;
    }

    // Build
    public PgSqlRenderResult Build()
    {
        if (_built)
            ThrowHelper.ThrowInvalidOperation("This query builder has already been built.");
        _built = true;
        return PgSqlRenderer.RenderQuery(this, PgSqlRenderMode.Parameterized);
    }

    public string BuildDebugSql()
    {
        if (_built)
            ThrowHelper.ThrowInvalidOperation("This query builder has already been built.");
        _built = true;
        return PgSqlRenderer.RenderQuery(this, PgSqlRenderMode.DebugSql).Sql;
    }
}
