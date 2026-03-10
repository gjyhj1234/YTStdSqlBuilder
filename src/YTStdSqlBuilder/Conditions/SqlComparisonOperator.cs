namespace YTStdSqlBuilder.Conditions;

public enum SqlComparisonOperator
{
    Eq,
    NotEq,
    Gt,
    Gte,
    Lt,
    Lte,
    Like,
    ILike,
    NotLike,
    NotILike,
    In,
    NotIn,
    IsNull,
    IsNotNull,
    Between,
    NotBetween,
    Exists,
    NotExists,
    ArrayContains,
    ArrayContainedBy,
    ArrayOverlaps
}
