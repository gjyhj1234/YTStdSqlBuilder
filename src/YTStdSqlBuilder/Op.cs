using YTStdSqlBuilder.Conditions;

namespace YTStdSqlBuilder;

public static class Op
{
    public static readonly SqlComparisonOperator Eq = SqlComparisonOperator.Eq;
    public static readonly SqlComparisonOperator NotEq = SqlComparisonOperator.NotEq;
    public static readonly SqlComparisonOperator Gt = SqlComparisonOperator.Gt;
    public static readonly SqlComparisonOperator Gte = SqlComparisonOperator.Gte;
    public static readonly SqlComparisonOperator Lt = SqlComparisonOperator.Lt;
    public static readonly SqlComparisonOperator Lte = SqlComparisonOperator.Lte;
    public static readonly SqlComparisonOperator Like = SqlComparisonOperator.Like;
    public static readonly SqlComparisonOperator ILike = SqlComparisonOperator.ILike;
    public static readonly SqlComparisonOperator NotLike = SqlComparisonOperator.NotLike;
    public static readonly SqlComparisonOperator NotILike = SqlComparisonOperator.NotILike;
    public static readonly SqlComparisonOperator In = SqlComparisonOperator.In;
    public static readonly SqlComparisonOperator NotIn = SqlComparisonOperator.NotIn;
    public static readonly SqlComparisonOperator IsNull = SqlComparisonOperator.IsNull;
    public static readonly SqlComparisonOperator IsNotNull = SqlComparisonOperator.IsNotNull;
    public static readonly SqlComparisonOperator Between = SqlComparisonOperator.Between;
    public static readonly SqlComparisonOperator NotBetween = SqlComparisonOperator.NotBetween;
    public static readonly SqlComparisonOperator Exists = SqlComparisonOperator.Exists;
    public static readonly SqlComparisonOperator NotExists = SqlComparisonOperator.NotExists;
    public static readonly SqlComparisonOperator ArrayContains = SqlComparisonOperator.ArrayContains;
    public static readonly SqlComparisonOperator ArrayContainedBy = SqlComparisonOperator.ArrayContainedBy;
    public static readonly SqlComparisonOperator ArrayOverlaps = SqlComparisonOperator.ArrayOverlaps;
}
