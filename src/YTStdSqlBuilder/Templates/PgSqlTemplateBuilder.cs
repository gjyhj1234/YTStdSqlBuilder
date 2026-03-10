using YTStdSqlBuilder.Conditions;
using YTStdSqlBuilder.Expressions;

namespace YTStdSqlBuilder;

/// <summary>
/// Builder used in Define_ methods for Source Generator analysis.
/// This class is only used to make Define_ methods compile.
/// The Source Generator analyzes the syntax tree of calls to this builder.
/// This class is NOT called at runtime.
/// </summary>
public sealed class PgSqlTemplateBuilder
{
    /// <summary>Table reference.</summary>
    public TemplateTableSource Table(string tableName, string alias)
        => new(tableName, alias);

    /// <summary>Parameter reference (maps to method parameter).</summary>
    public ParamExpr Param<T>(string paramName) => new(default(T));

    /// <summary>Parameter reference without explicit type (type inferred from method signature).</summary>
    public ParamExpr Param(string paramName) => new(null);

    /// <summary>Condition reference for WhereIf etc. (Deprecated: WhereIf now uses 3-arg form with auto-generated bool parameter)</summary>
    [System.Obsolete("ConditionRef is no longer needed. WhereIf/AndIf/OrIf now use the same 3-arg form as Where/And/Or. A bool parameter with '_condition' suffix is auto-generated.")]
    public bool ConditionRef(string paramName, string conditionTemplate) => false;

    /// <summary>Query building.</summary>
    public TemplateQueryBuilder Select(params SqlSelectItem[] items) => new();
}

public sealed class TemplateTableSource
{
    public string TableName { get; }
    public string Alias { get; }

    internal TemplateTableSource(string tableName, string alias)
    {
        TableName = tableName;
        Alias = alias;
    }

    public ColumnExpr Col(string name)
        => new(new SqlColumn(new SqlTableSource(new SqlTable(TableName), Alias), name));

    public ColumnExpr Col<T>(string name)
        => new(new SqlColumn(new SqlTableSource(new SqlTable(TableName), Alias), name, typeof(T)));
}

public sealed class TemplateQueryBuilder
{
    public TemplateQueryBuilder From(TemplateTableSource table) => this;
    public TemplateQueryBuilder Where(SqlExpr left, SqlComparisonOperator op, SqlExpr right) => this;
    public TemplateQueryBuilder WhereIf(SqlExpr left, SqlComparisonOperator op, SqlExpr right) => this;
    public TemplateQueryBuilder And(SqlExpr left, SqlComparisonOperator op, SqlExpr right) => this;
    public TemplateQueryBuilder AndIf(SqlExpr left, SqlComparisonOperator op, SqlExpr right) => this;
    public TemplateQueryBuilder Or(SqlExpr left, SqlComparisonOperator op, SqlExpr right) => this;
    public TemplateQueryBuilder OrIf(SqlExpr left, SqlComparisonOperator op, SqlExpr right) => this;
    public TemplateQueryBuilder LeftJoin(TemplateTableSource table, Action<object> onBuilder) => this;
    public TemplateQueryBuilder InnerJoin(TemplateTableSource table, Action<object> onBuilder) => this;
    public TemplateQueryBuilder GroupBy(params SqlExpr[] fields) => this;
    public TemplateQueryBuilder Having(SqlExpr left, SqlComparisonOperator op, SqlExpr right) => this;
    public TemplateQueryBuilder OrderBy(params SqlExpr[] fields) => this;
    public TemplateQueryBuilder OrderByDesc(params SqlExpr[] fields) => this;
    public TemplateQueryBuilder Limit(int count) => this;
    public TemplateQueryBuilder Offset(int count) => this;
}
