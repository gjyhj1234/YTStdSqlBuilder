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

/// <summary>模板表源，用于源代码生成器分析</summary>
public sealed class TemplateTableSource
{
    /// <summary>表名</summary>
    public string TableName { get; }
    /// <summary>表别名</summary>
    public string Alias { get; }

    internal TemplateTableSource(string tableName, string alias)
    {
        TableName = tableName;
        Alias = alias;
    }

    /// <summary>创建列引用表达式</summary>
    /// <param name="name">列名</param>
    /// <returns>列表达式</returns>
    public ColumnExpr Col(string name)
        => new(new SqlColumn(new SqlTableSource(new SqlTable(TableName), Alias), name));

    /// <summary>创建带 CLR 类型的列引用表达式</summary>
    /// <typeparam name="T">CLR 类型</typeparam>
    /// <param name="name">列名</param>
    /// <returns>列表达式</returns>
    public ColumnExpr Col<T>(string name)
        => new(new SqlColumn(new SqlTableSource(new SqlTable(TableName), Alias), name, typeof(T)));
}

/// <summary>模板查询构建器，用于源代码生成器分析</summary>
public sealed class TemplateQueryBuilder
{
    /// <summary>设置 FROM 表源</summary>
    /// <param name="table">模板表源</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public TemplateQueryBuilder From(TemplateTableSource table) => this;

    /// <summary>添加 WHERE 条件</summary>
    /// <param name="left">左侧表达式</param>
    /// <param name="op">比较运算符</param>
    /// <param name="right">右侧表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public TemplateQueryBuilder Where(SqlExpr left, SqlComparisonOperator op, SqlExpr right) => this;

    /// <summary>条件为真时添加 WHERE 条件</summary>
    /// <param name="left">左侧表达式</param>
    /// <param name="op">比较运算符</param>
    /// <param name="right">右侧表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public TemplateQueryBuilder WhereIf(SqlExpr left, SqlComparisonOperator op, SqlExpr right) => this;

    /// <summary>添加 AND 条件</summary>
    /// <param name="left">左侧表达式</param>
    /// <param name="op">比较运算符</param>
    /// <param name="right">右侧表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public TemplateQueryBuilder And(SqlExpr left, SqlComparisonOperator op, SqlExpr right) => this;

    /// <summary>条件为真时添加 AND 条件</summary>
    /// <param name="left">左侧表达式</param>
    /// <param name="op">比较运算符</param>
    /// <param name="right">右侧表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public TemplateQueryBuilder AndIf(SqlExpr left, SqlComparisonOperator op, SqlExpr right) => this;

    /// <summary>添加 OR 条件</summary>
    /// <param name="left">左侧表达式</param>
    /// <param name="op">比较运算符</param>
    /// <param name="right">右侧表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public TemplateQueryBuilder Or(SqlExpr left, SqlComparisonOperator op, SqlExpr right) => this;

    /// <summary>条件为真时添加 OR 条件</summary>
    /// <param name="left">左侧表达式</param>
    /// <param name="op">比较运算符</param>
    /// <param name="right">右侧表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public TemplateQueryBuilder OrIf(SqlExpr left, SqlComparisonOperator op, SqlExpr right) => this;

    /// <summary>添加 LEFT JOIN</summary>
    /// <param name="table">要连接的模板表源</param>
    /// <param name="onBuilder">ON 条件配置委托</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public TemplateQueryBuilder LeftJoin(TemplateTableSource table, Action<object> onBuilder) => this;

    /// <summary>添加 INNER JOIN</summary>
    /// <param name="table">要连接的模板表源</param>
    /// <param name="onBuilder">ON 条件配置委托</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public TemplateQueryBuilder InnerJoin(TemplateTableSource table, Action<object> onBuilder) => this;

    /// <summary>添加 GROUP BY 子句</summary>
    /// <param name="fields">分组表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public TemplateQueryBuilder GroupBy(params SqlExpr[] fields) => this;

    /// <summary>添加 HAVING 条件</summary>
    /// <param name="left">左侧表达式</param>
    /// <param name="op">比较运算符</param>
    /// <param name="right">右侧表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public TemplateQueryBuilder Having(SqlExpr left, SqlComparisonOperator op, SqlExpr right) => this;

    /// <summary>添加 ORDER BY 子句（升序）</summary>
    /// <param name="fields">排序表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public TemplateQueryBuilder OrderBy(params SqlExpr[] fields) => this;

    /// <summary>添加 ORDER BY 子句（降序）</summary>
    /// <param name="fields">排序表达式</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public TemplateQueryBuilder OrderByDesc(params SqlExpr[] fields) => this;

    /// <summary>设置 LIMIT</summary>
    /// <param name="count">最大行数</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public TemplateQueryBuilder Limit(int count) => this;

    /// <summary>设置 OFFSET</summary>
    /// <param name="count">跳过行数</param>
    /// <returns>当前构建器实例（链式调用）</returns>
    public TemplateQueryBuilder Offset(int count) => this;
}
