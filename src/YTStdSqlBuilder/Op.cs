using YTStdSqlBuilder.Conditions;

namespace YTStdSqlBuilder;

/// <summary>提供 SQL 比较运算符常量的快捷访问</summary>
public static class Op
{
    /// <summary>等于（=）</summary>
    public static readonly SqlComparisonOperator Eq = SqlComparisonOperator.Eq;
    /// <summary>不等于（!=）</summary>
    public static readonly SqlComparisonOperator NotEq = SqlComparisonOperator.NotEq;
    /// <summary>大于（&gt;）</summary>
    public static readonly SqlComparisonOperator Gt = SqlComparisonOperator.Gt;
    /// <summary>大于等于（&gt;=）</summary>
    public static readonly SqlComparisonOperator Gte = SqlComparisonOperator.Gte;
    /// <summary>小于（&lt;）</summary>
    public static readonly SqlComparisonOperator Lt = SqlComparisonOperator.Lt;
    /// <summary>小于等于（&lt;=）</summary>
    public static readonly SqlComparisonOperator Lte = SqlComparisonOperator.Lte;
    /// <summary>LIKE 模式匹配</summary>
    public static readonly SqlComparisonOperator Like = SqlComparisonOperator.Like;
    /// <summary>ILIKE 不区分大小写模式匹配</summary>
    public static readonly SqlComparisonOperator ILike = SqlComparisonOperator.ILike;
    /// <summary>NOT LIKE 模式不匹配</summary>
    public static readonly SqlComparisonOperator NotLike = SqlComparisonOperator.NotLike;
    /// <summary>NOT ILIKE 不区分大小写模式不匹配</summary>
    public static readonly SqlComparisonOperator NotILike = SqlComparisonOperator.NotILike;
    /// <summary>IN 包含于集合</summary>
    public static readonly SqlComparisonOperator In = SqlComparisonOperator.In;
    /// <summary>NOT IN 不包含于集合</summary>
    public static readonly SqlComparisonOperator NotIn = SqlComparisonOperator.NotIn;
    /// <summary>IS NULL 为空</summary>
    public static readonly SqlComparisonOperator IsNull = SqlComparisonOperator.IsNull;
    /// <summary>IS NOT NULL 不为空</summary>
    public static readonly SqlComparisonOperator IsNotNull = SqlComparisonOperator.IsNotNull;
    /// <summary>BETWEEN 区间范围</summary>
    public static readonly SqlComparisonOperator Between = SqlComparisonOperator.Between;
    /// <summary>NOT BETWEEN 不在区间范围</summary>
    public static readonly SqlComparisonOperator NotBetween = SqlComparisonOperator.NotBetween;
    /// <summary>EXISTS 子查询存在</summary>
    public static readonly SqlComparisonOperator Exists = SqlComparisonOperator.Exists;
    /// <summary>NOT EXISTS 子查询不存在</summary>
    public static readonly SqlComparisonOperator NotExists = SqlComparisonOperator.NotExists;
    /// <summary>PostgreSQL 数组包含（@&gt;）</summary>
    public static readonly SqlComparisonOperator ArrayContains = SqlComparisonOperator.ArrayContains;
    /// <summary>PostgreSQL 数组被包含（&lt;@）</summary>
    public static readonly SqlComparisonOperator ArrayContainedBy = SqlComparisonOperator.ArrayContainedBy;
    /// <summary>PostgreSQL 数组重叠（&amp;&amp;）</summary>
    public static readonly SqlComparisonOperator ArrayOverlaps = SqlComparisonOperator.ArrayOverlaps;
}
