namespace YTStdSqlBuilder.Conditions;

/// <summary>SQL 比较运算符枚举</summary>
public enum SqlComparisonOperator
{
    /// <summary>等于（=）</summary>
    Eq,
    /// <summary>不等于（!=）</summary>
    NotEq,
    /// <summary>大于（&gt;）</summary>
    Gt,
    /// <summary>大于等于（&gt;=）</summary>
    Gte,
    /// <summary>小于（&lt;）</summary>
    Lt,
    /// <summary>小于等于（&lt;=）</summary>
    Lte,
    /// <summary>LIKE 模式匹配</summary>
    Like,
    /// <summary>ILIKE 不区分大小写模式匹配</summary>
    ILike,
    /// <summary>NOT LIKE 模式不匹配</summary>
    NotLike,
    /// <summary>NOT ILIKE 不区分大小写模式不匹配</summary>
    NotILike,
    /// <summary>IN 包含于集合</summary>
    In,
    /// <summary>NOT IN 不包含于集合</summary>
    NotIn,
    /// <summary>IS NULL 为空</summary>
    IsNull,
    /// <summary>IS NOT NULL 不为空</summary>
    IsNotNull,
    /// <summary>BETWEEN 区间范围</summary>
    Between,
    /// <summary>NOT BETWEEN 不在区间范围</summary>
    NotBetween,
    /// <summary>EXISTS 子查询存在</summary>
    Exists,
    /// <summary>NOT EXISTS 子查询不存在</summary>
    NotExists,
    /// <summary>PostgreSQL 数组包含（@&gt;）</summary>
    ArrayContains,
    /// <summary>PostgreSQL 数组被包含（&lt;@）</summary>
    ArrayContainedBy,
    /// <summary>PostgreSQL 数组重叠（&amp;&amp;）</summary>
    ArrayOverlaps
}
