using YTStdSqlBuilder.Conditions;
using YTStdSqlBuilder.Internal;

namespace YTStdSqlBuilder.Expressions;

/// <summary>CASE WHEN 表达式</summary>
public sealed class CaseExpr : SqlExpr
{
    /// <summary>表达式类型</summary>
    public override SqlExprKind Kind => SqlExprKind.Case;

    /// <summary>WHEN 子句列表（条件和结果对）</summary>
    public List<(SqlConditionGroup Condition, SqlExpr Result)> WhenClauses { get; }
    /// <summary>ELSE 结果表达式（可选）</summary>
    public SqlExpr? ElseResult { get; }

    /// <summary>创建 CASE WHEN 表达式</summary>
    /// <param name="whenClauses">WHEN 子句列表</param>
    /// <param name="elseResult">ELSE 结果表达式（可选）</param>
    public CaseExpr(List<(SqlConditionGroup Condition, SqlExpr Result)> whenClauses, SqlExpr? elseResult = null)
    {
        WhenClauses = Guard.NotNull(whenClauses);
        ElseResult = elseResult;
    }
}
