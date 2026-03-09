using System.Text.RegularExpressions;
using YTStdSqlBuilder.Conditions;
using YTStdSqlBuilder.Expressions;

namespace YTStdSqlBuilder;

internal static partial class PgSqlRenderer
{
    internal static PgSqlRenderResult RenderQuery(PgSqlQueryBuilder builder, PgSqlRenderMode mode)
    {
        var ctx = new RenderContext(mode);
        RenderQueryCore(builder, ctx);
        return ctx.ToResult();
    }

    internal static PgSqlRenderResult RenderInsert(PgSqlInsertBuilder builder, PgSqlRenderMode mode)
    {
        var ctx = new RenderContext(mode);
        RenderInsertCore(builder, ctx);
        return ctx.ToResult();
    }

    internal static PgSqlRenderResult RenderUpdate(PgSqlUpdateBuilder builder, PgSqlRenderMode mode)
    {
        var ctx = new RenderContext(mode);
        RenderUpdateCore(builder, ctx);
        return ctx.ToResult();
    }

    internal static PgSqlRenderResult RenderDelete(PgSqlDeleteBuilder builder, PgSqlRenderMode mode)
    {
        var ctx = new RenderContext(mode);
        RenderDeleteCore(builder, ctx);
        return ctx.ToResult();
    }

    private static void RenderQueryCore(PgSqlQueryBuilder builder, RenderContext ctx)
    {
        // SELECT
        ctx.Append(Q.Select);
        ctx.Append(' ');

        if (builder.IsDistinct)
        {
            ctx.Append(Q.Distinct);
            ctx.Append(' ');
        }

        RenderSelectItems(builder.SelectItems, ctx);

        // FROM
        if (builder.FromTable is not null)
        {
            ctx.Append(' ');
            ctx.Append(Q.From);
            ctx.Append(' ');
            RenderTableSource(builder.FromTable, ctx);
        }
        else if (builder.FromSubQuery is not null)
        {
            ctx.Append(' ');
            ctx.Append(Q.From);
            ctx.Append(" (");
            RenderQueryCore(builder.FromSubQuery, ctx);
            ctx.Append(") ");
            ctx.Append(Q.As);
            ctx.Append(' ');
            ctx.Append(SqlIdentifier.Escape(builder.FromSubQueryAlias!));
        }

        // JOINs
        foreach (var join in builder.Joins)
        {
            ctx.Append(' ');
            ctx.Append(join.JoinType switch
            {
                SqlJoinType.Inner => Q.InnerJoin,
                SqlJoinType.Left => Q.LeftJoin,
                SqlJoinType.Right => Q.RightJoin,
                _ => Q.InnerJoin
            });
            ctx.Append(' ');
            RenderTableSource(join.Table, ctx);
            ctx.Append(' ');
            ctx.Append(Q.On);
            ctx.Append(' ');
            RenderConditionGroup(join.OnCondition, ctx);
        }

        // WHERE
        var whereGroup = builder.WhereBuilder.Build();
        if (!whereGroup.IsEmpty)
        {
            ctx.Append(' ');
            ctx.Append(Q.Where);
            ctx.Append(' ');
            RenderConditionNodes(whereGroup.Nodes, ctx);
        }

        // GROUP BY
        if (builder.GroupByItems.Count > 0)
        {
            ctx.Append(' ');
            ctx.Append(Q.GroupBy);
            ctx.Append(' ');
            for (int i = 0; i < builder.GroupByItems.Count; i++)
            {
                if (i > 0) ctx.Append(", ");
                RenderExpr(builder.GroupByItems[i], ctx);
            }
        }

        // HAVING
        var havingGroup = builder.HavingBuilder.Build();
        if (!havingGroup.IsEmpty)
        {
            ctx.Append(' ');
            ctx.Append(Q.Having);
            ctx.Append(' ');
            RenderConditionNodes(havingGroup.Nodes, ctx);
        }

        // ORDER BY
        if (builder.OrderByItems.Count > 0)
        {
            ctx.Append(' ');
            ctx.Append(Q.OrderBy);
            ctx.Append(' ');
            for (int i = 0; i < builder.OrderByItems.Count; i++)
            {
                if (i > 0) ctx.Append(", ");
                RenderExpr(builder.OrderByItems[i].Expression, ctx);
                ctx.Append(builder.OrderByItems[i].Descending ? " DESC" : " ASC");
            }
        }

        // LIMIT
        if (builder.LimitValue.HasValue)
        {
            ctx.Append(' ');
            ctx.Append(Q.Limit);
            ctx.Append(' ');
            ctx.Append(builder.LimitValue.Value.ToString());
        }

        // OFFSET
        if (builder.OffsetValue.HasValue)
        {
            ctx.Append(' ');
            ctx.Append(Q.Offset);
            ctx.Append(' ');
            ctx.Append(builder.OffsetValue.Value.ToString());
        }
    }

    private static void RenderInsertCore(PgSqlInsertBuilder builder, RenderContext ctx)
    {
        ctx.Append(Q.Insert);
        ctx.Append(' ');
        ctx.Append(Q.Into);
        ctx.Append(' ');
        RenderTableName(builder.Table, ctx);

        // Columns
        ctx.Append(" (");
        for (int i = 0; i < builder.Assignments.Count; i++)
        {
            if (i > 0) ctx.Append(", ");
            ctx.Append(SqlIdentifier.Escape(builder.Assignments[i].Column.Column.Name));
        }
        ctx.Append(')');

        // Values
        ctx.Append(' ');
        ctx.Append(Q.Values);
        ctx.Append(" (");
        for (int i = 0; i < builder.Assignments.Count; i++)
        {
            if (i > 0) ctx.Append(", ");
            RenderExpr(builder.Assignments[i].Value, ctx);
        }
        ctx.Append(')');

        // RETURNING
        RenderReturning(builder.ReturningItems, ctx);
    }

    private static void RenderUpdateCore(PgSqlUpdateBuilder builder, RenderContext ctx)
    {
        ctx.Append(Q.Update);
        ctx.Append(' ');
        RenderTableSourceForUpdate(builder.Table, ctx);

        // SET
        ctx.Append(' ');
        ctx.Append(Q.Set);
        ctx.Append(' ');
        for (int i = 0; i < builder.Assignments.Count; i++)
        {
            if (i > 0) ctx.Append(", ");
            ctx.Append(SqlIdentifier.Escape(builder.Assignments[i].Column.Column.Name));
            ctx.Append(" = ");
            RenderExpr(builder.Assignments[i].Value, ctx);
        }

        // WHERE
        var whereGroup = builder.WhereBuilder.Build();
        if (!whereGroup.IsEmpty)
        {
            ctx.Append(' ');
            ctx.Append(Q.Where);
            ctx.Append(' ');
            RenderConditionNodes(whereGroup.Nodes, ctx);
        }

        // RETURNING
        RenderReturning(builder.ReturningItems, ctx);
    }

    private static void RenderDeleteCore(PgSqlDeleteBuilder builder, RenderContext ctx)
    {
        ctx.Append(Q.Delete);
        ctx.Append(' ');
        ctx.Append(Q.From);
        ctx.Append(' ');
        RenderTableSourceForUpdate(builder.Table, ctx);

        // WHERE
        var whereGroup = builder.WhereBuilder.Build();
        if (!whereGroup.IsEmpty)
        {
            ctx.Append(' ');
            ctx.Append(Q.Where);
            ctx.Append(' ');
            RenderConditionNodes(whereGroup.Nodes, ctx);
        }

        // RETURNING
        RenderReturning(builder.ReturningItems, ctx);
    }

    private static void RenderSelectItems(IReadOnlyList<SqlSelectItem> items, RenderContext ctx)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (i > 0) ctx.Append(", ");
            RenderExpr(items[i].Expression, ctx);
            if (items[i].Alias is not null)
            {
                ctx.Append(' ');
                ctx.Append(Q.As);
                ctx.Append(' ');
                ctx.Append(SqlIdentifier.Escape(items[i].Alias!));
            }
        }
    }

    private static void RenderReturning(IReadOnlyList<SqlSelectItem> items, RenderContext ctx)
    {
        if (items.Count == 0) return;
        ctx.Append(' ');
        ctx.Append(Q.Returning);
        ctx.Append(' ');
        RenderSelectItems(items, ctx);
    }

    private static void RenderTableSource(SqlTableSource source, RenderContext ctx)
    {
        ctx.Append(SqlIdentifier.EscapeQualified(source.Table.Schema, source.Table.Name));
        ctx.Append(' ');
        ctx.Append(Q.As);
        ctx.Append(' ');
        ctx.Append(SqlIdentifier.Escape(source.Alias));
    }

    private static void RenderTableName(SqlTableSource source, RenderContext ctx)
    {
        ctx.Append(SqlIdentifier.EscapeQualified(source.Table.Schema, source.Table.Name));
    }

    private static void RenderTableSourceForUpdate(SqlTableSource source, RenderContext ctx)
    {
        ctx.Append(SqlIdentifier.EscapeQualified(source.Table.Schema, source.Table.Name));
        ctx.Append(' ');
        ctx.Append(Q.As);
        ctx.Append(' ');
        ctx.Append(SqlIdentifier.Escape(source.Alias));
    }

    private static void RenderExpr(SqlExpr expr, RenderContext ctx)
    {
        switch (expr)
        {
            case ColumnExpr col:
                ctx.Append(SqlIdentifier.EscapeColumn(col.Column.TableSource, col.Column.Name));
                break;

            case ParamExpr param:
                if (ctx.Mode == PgSqlRenderMode.DebugSql)
                {
                    if (param.Value is System.Collections.IEnumerable enumerable and not string)
                    {
                        ctx.Append(string.Join(", ", PgSqlLiteralFormatter.FormatEnumerable(enumerable)));
                    }
                    else
                    {
                        ctx.Append(PgSqlLiteralFormatter.Format(param.Value));
                    }
                }
                else
                {
                    var name = ctx.AddParam(param.Value, param.DbType);
                    ctx.Append(name);
                }
                break;

            case LiteralExpr lit:
                ctx.Append(lit.SqlText);
                break;

            case FuncExpr func:
                ctx.Append(func.FunctionName);
                ctx.Append('(');
                for (int i = 0; i < func.Arguments.Length; i++)
                {
                    if (i > 0) ctx.Append(", ");
                    RenderExpr(func.Arguments[i], ctx);
                }
                ctx.Append(')');
                break;

            case AllExpr all:
                if (all.TableSource is not null)
                {
                    ctx.Append(SqlIdentifier.Escape(all.TableSource.Alias));
                    ctx.Append(".*");
                }
                else
                {
                    ctx.Append('*');
                }
                break;

            case SubQueryExpr sub:
                ctx.Append('(');
                RenderSubQuery(sub, ctx);
                ctx.Append(')');
                break;

            case CaseExpr caseExpr:
                RenderCaseExpr(caseExpr, ctx);
                break;

            case RawExpr raw:
                ctx.Append(raw.SqlText);
                break;
        }
    }

    private static void RenderSubQuery(SubQueryExpr sub, RenderContext ctx)
    {
        if (ctx.Mode == PgSqlRenderMode.DebugSql)
        {
            ctx.Append(sub.Sql);
        }
        else
        {
            // Renumber subquery parameters
            string sql = sub.Sql;
            int offset = ctx.ParamCount;

            if (sub.Params.Length > 0)
            {
                // Replace @pN with @p(N+offset) - process in reverse order to avoid conflicts
                for (int i = sub.Params.Length - 1; i >= 0; i--)
                {
                    string oldName = $"@p{i}";
                    string newName = $"@p{i + offset}";
                    sql = sql.Replace(oldName, newName);
                }

                foreach (var p in sub.Params)
                {
                    ctx.AddParamDirect($"@p{offset}", p.Value, p.DbType);
                    offset++;
                }
            }

            ctx.Append(sql);
        }
    }

    private static void RenderCaseExpr(CaseExpr caseExpr, RenderContext ctx)
    {
        ctx.Append(Q.Case);
        foreach (var (condition, result) in caseExpr.WhenClauses)
        {
            ctx.Append(' ');
            ctx.Append(Q.When);
            ctx.Append(' ');
            RenderConditionGroup(condition, ctx);
            ctx.Append(' ');
            ctx.Append(Q.Then);
            ctx.Append(' ');
            RenderExpr(result, ctx);
        }
        if (caseExpr.ElseResult is not null)
        {
            ctx.Append(' ');
            ctx.Append(Q.Else);
            ctx.Append(' ');
            RenderExpr(caseExpr.ElseResult, ctx);
        }
        ctx.Append(' ');
        ctx.Append(Q.End);
    }

    private static void RenderConditionGroup(SqlConditionGroup group, RenderContext ctx)
    {
        RenderConditionNodes(group.Nodes, ctx);
    }

    private static void RenderConditionNodes(List<SqlConditionNode> nodes, RenderContext ctx)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            if (i > 0)
            {
                ctx.Append(nodes[i].LogicalOperator == SqlLogicalOperator.Or ? " OR " : " AND ");
            }
            RenderConditionNode(nodes[i], ctx);
        }
    }

    private static void RenderConditionNode(SqlConditionNode node, RenderContext ctx)
    {
        switch (node.NodeKind)
        {
            case SqlConditionNodeKind.SimpleCondition:
                RenderCondition(node.Condition!, ctx);
                break;

            case SqlConditionNodeKind.AndGroup:
            case SqlConditionNodeKind.OrGroup:
                ctx.Append('(');
                for (int i = 0; i < node.Children!.Count; i++)
                {
                    if (i > 0)
                    {
                        ctx.Append(node.Children[i].LogicalOperator == SqlLogicalOperator.Or ? " OR " : " AND ");
                    }
                    RenderConditionNode(node.Children[i], ctx);
                }
                ctx.Append(')');
                break;
        }
    }

    private static void RenderCondition(SqlCondition condition, RenderContext ctx)
    {
        var op = condition.Operator;

        // EXISTS / NOT EXISTS
        if (op == SqlComparisonOperator.Exists)
        {
            ctx.Append(Q.Exists);
            ctx.Append(' ');
            if (condition.Left is SubQueryExpr sub)
            {
                ctx.Append('(');
                RenderSubQuery(sub, ctx);
                ctx.Append(')');
            }
            else
            {
                RenderExpr(condition.Left, ctx);
            }
            return;
        }
        if (op == SqlComparisonOperator.NotExists)
        {
            ctx.Append(Q.Not);
            ctx.Append(' ');
            ctx.Append(Q.Exists);
            ctx.Append(' ');
            if (condition.Left is SubQueryExpr sub)
            {
                ctx.Append('(');
                RenderSubQuery(sub, ctx);
                ctx.Append(')');
            }
            else
            {
                RenderExpr(condition.Left, ctx);
            }
            return;
        }

        // IS NULL / IS NOT NULL operators
        if (op == SqlComparisonOperator.IsNull)
        {
            RenderExpr(condition.Left, ctx);
            ctx.Append(' ');
            ctx.Append(Q.IsNull);
            return;
        }
        if (op == SqlComparisonOperator.IsNotNull)
        {
            RenderExpr(condition.Left, ctx);
            ctx.Append(' ');
            ctx.Append(Q.IsNotNull);
            return;
        }

        // Null semantics for Eq/NotEq
        if (condition.Right is ParamExpr rightParam && rightParam.Value is null)
        {
            if (op == SqlComparisonOperator.Eq)
            {
                RenderExpr(condition.Left, ctx);
                ctx.Append(' ');
                ctx.Append(Q.IsNull);
                return;
            }
            if (op == SqlComparisonOperator.NotEq)
            {
                RenderExpr(condition.Left, ctx);
                ctx.Append(' ');
                ctx.Append(Q.IsNotNull);
                return;
            }
        }

        // IN / NOT IN handling
        if (op is SqlComparisonOperator.In or SqlComparisonOperator.NotIn)
        {
            RenderInCondition(condition, op, ctx);
            return;
        }

        // Standard binary condition
        RenderExpr(condition.Left, ctx);
        ctx.Append(' ');
        ctx.Append(GetOperatorSql(op));
        ctx.Append(' ');
        if (condition.Right is not null)
            RenderExpr(condition.Right, ctx);
    }

    private static void RenderInCondition(SqlCondition condition, SqlComparisonOperator op, RenderContext ctx)
    {
        bool isNotIn = op == SqlComparisonOperator.NotIn;

        if (condition.Right is ParamExpr paramExpr &&
            paramExpr.Value is System.Collections.IEnumerable enumerable and not string)
        {
            var items = new List<object?>();
            foreach (var item in enumerable)
                items.Add(item);

            if (items.Count == 0)
            {
                ctx.Append(isNotIn ? Q.True : Q.False);
                return;
            }

            RenderExpr(condition.Left, ctx);
            ctx.Append(isNotIn ? " NOT IN (" : " IN (");

            if (ctx.Mode == PgSqlRenderMode.DebugSql)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (i > 0) ctx.Append(", ");
                    ctx.Append(PgSqlLiteralFormatter.Format(items[i]));
                }
            }
            else
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (i > 0) ctx.Append(", ");
                    var name = ctx.AddParam(items[i], paramExpr.DbType);
                    ctx.Append(name);
                }
            }

            ctx.Append(')');
        }
        else
        {
            // Subquery IN or single-value IN
            RenderExpr(condition.Left, ctx);
            ctx.Append(isNotIn ? " NOT IN " : " IN ");
            if (condition.Right is not null)
                RenderExpr(condition.Right, ctx);
        }
    }

    private static string GetOperatorSql(SqlComparisonOperator op) => op switch
    {
        SqlComparisonOperator.Eq => "=",
        SqlComparisonOperator.NotEq => "!=",
        SqlComparisonOperator.Gt => ">",
        SqlComparisonOperator.Gte => ">=",
        SqlComparisonOperator.Lt => "<",
        SqlComparisonOperator.Lte => "<=",
        SqlComparisonOperator.Like => "LIKE",
        SqlComparisonOperator.ILike => "ILIKE",
        SqlComparisonOperator.NotLike => "NOT LIKE",
        SqlComparisonOperator.NotILike => "NOT ILIKE",
        SqlComparisonOperator.Between => "BETWEEN",
        SqlComparisonOperator.NotBetween => "NOT BETWEEN",
        _ => "="
    };

    private sealed class RenderContext
    {
        private readonly System.Text.StringBuilder _sb;
        private readonly List<PgSqlParam> _params = new();

        public PgSqlRenderMode Mode { get; }
        public int ParamCount => _params.Count;

        public RenderContext(PgSqlRenderMode mode)
        {
            Mode = mode;
            _sb = new System.Text.StringBuilder(256);
        }

        public void Append(char c) => _sb.Append(c);
        public void Append(string s) => _sb.Append(s);

        public string AddParam(object? value, NpgsqlTypes.NpgsqlDbType? dbType)
        {
            var name = $"@p{_params.Count}";
            _params.Add(new PgSqlParam(name, value, dbType));
            return name;
        }

        public void AddParamDirect(string name, object? value, NpgsqlTypes.NpgsqlDbType? dbType)
        {
            _params.Add(new PgSqlParam(name, value, dbType));
        }

        public PgSqlRenderResult ToResult()
        {
            return new PgSqlRenderResult(_sb.ToString(), _params.ToArray());
        }
    }
}
