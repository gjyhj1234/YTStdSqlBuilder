using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace YTStdSqlBuilder.Generator.TemplateAnalysis
{
    /// <summary>
    /// Analyzes Define_ method syntax tree to extract SQL structure.
    /// </summary>
    internal static class TemplateAnalyzer
    {
        public static TemplateQueryInfo? Analyze(
            MethodDeclarationSyntax defineMethod,
            MethodDeclarationSyntax queryMethod,
            SemanticModel semanticModel)
        {
            string methodName = queryMethod.Identifier.Text;
            var body = defineMethod.Body;
            if (body == null) return null;

            var tables = new Dictionary<string, TemplateTableRef>();
            var selectColumns = new List<TemplateSelectColumn>();
            var parameters = new List<TemplateParam>();
            var whereClauses = new List<TemplateWhereClause>();
            var joins = new List<TemplateJoinClause>();
            var groupByColumns = new List<string>();
            var havingClauses = new List<TemplateWhereClause>();
            var orderByItems = new List<TemplateOrderByItem>();
            int? limit = null;
            int? offset = null;
            bool isDynamic = false;
            TemplateTableRef? fromTable = null;

            // Extract query method parameters
            var methodParams = ExtractMethodParameters(queryMethod);

            // Walk statements to find b.Table(...) variable declarations
            foreach (var statement in body.Statements)
            {
                if (statement is LocalDeclarationStatementSyntax localDecl)
                {
                    foreach (var variable in localDecl.Declaration.Variables)
                    {
                        if (variable.Initializer?.Value is InvocationExpressionSyntax invocation)
                        {
                            var tableRef = TryExtractTableRef(invocation, variable.Identifier.Text);
                            if (tableRef != null)
                            {
                                tables[variable.Identifier.Text] = tableRef;
                            }
                        }
                    }
                }

                // Find the chained builder invocation (b.Select(...).From(...).Where(...) etc.)
                if (statement is ExpressionStatementSyntax exprStmt)
                {
                    AnalyzeChainedExpression(
                        exprStmt.Expression, tables, selectColumns, parameters,
                        whereClauses, joins, groupByColumns, havingClauses,
                        orderByItems, ref limit, ref offset, ref isDynamic, ref fromTable);
                }
                // Also handle: var x = b.Select(...).From(...)...;
                if (statement is LocalDeclarationStatementSyntax localDecl2)
                {
                    foreach (var variable in localDecl2.Declaration.Variables)
                    {
                        if (variable.Initializer?.Value != null)
                        {
                            AnalyzeChainedExpression(
                                variable.Initializer.Value, tables, selectColumns, parameters,
                                whereClauses, joins, groupByColumns, havingClauses,
                                orderByItems, ref limit, ref offset, ref isDynamic, ref fromTable);
                        }
                    }
                }
            }

            // Determine fallback
            bool fallbackToInterpreter = HasFallbackAttribute(queryMethod);

            // Build param list from method signature, assigning ordinals based on usage in Where
            var queryParams = BuildParamList(methodParams, parameters);

            var sqlStructure = new TemplateSqlStructure(
                fromTable,
                whereClauses.ToImmutableArray(),
                joins.ToImmutableArray(),
                groupByColumns.ToImmutableArray(),
                havingClauses.ToImmutableArray(),
                orderByItems.ToImmutableArray(),
                limit,
                offset);

            string? defineBody = isDynamic ? defineMethod.Body?.ToFullString() : null;

            return new TemplateQueryInfo(
                methodName,
                queryParams,
                selectColumns.ToImmutableArray(),
                tables.Values.ToImmutableArray(),
                sqlStructure,
                isDynamic,
                fallbackToInterpreter,
                defineBody);
        }

        private static List<(string Name, string Type, bool IsNullable)> ExtractMethodParameters(
            MethodDeclarationSyntax method)
        {
            var result = new List<(string, string, bool)>();
            foreach (var param in method.ParameterList.Parameters)
            {
                string name = param.Identifier.Text;
                string typeName = param.Type?.ToString() ?? "object";
                bool isNullable = typeName.EndsWith("?");
                result.Add((name, typeName, isNullable));
            }
            return result;
        }

        private static bool HasFallbackAttribute(MethodDeclarationSyntax method)
        {
            foreach (var attrList in method.AttributeLists)
            {
                foreach (var attr in attrList.Attributes)
                {
                    string attrName = attr.Name.ToString();
                    if (attrName == "PgSqlQuery" || attrName == "PgSqlQueryAttribute")
                    {
                        if (attr.ArgumentList != null)
                        {
                            foreach (var arg in attr.ArgumentList.Arguments)
                            {
                                if (arg.NameEquals?.Name.ToString() == "FallbackToInterpreter")
                                {
                                    string value = arg.Expression.ToString();
                                    if (value == "true") return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        private static TemplateTableRef? TryExtractTableRef(InvocationExpressionSyntax invocation, string variableName)
        {
            // Match: b.Table("tableName", "alias")
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name.Identifier.Text == "Table" &&
                invocation.ArgumentList.Arguments.Count == 2)
            {
                string? tableName = ExtractStringLiteral(invocation.ArgumentList.Arguments[0].Expression);
                string? alias = ExtractStringLiteral(invocation.ArgumentList.Arguments[1].Expression);
                if (tableName != null && alias != null)
                {
                    return new TemplateTableRef(tableName, alias, variableName);
                }
            }
            return null;
        }

        private static void AnalyzeChainedExpression(
            ExpressionSyntax expression,
            Dictionary<string, TemplateTableRef> tables,
            List<TemplateSelectColumn> selectColumns,
            List<TemplateParam> parameters,
            List<TemplateWhereClause> whereClauses,
            List<TemplateJoinClause> joins,
            List<string> groupByColumns,
            List<TemplateWhereClause> havingClauses,
            List<TemplateOrderByItem> orderByItems,
            ref int? limit,
            ref int? offset,
            ref bool isDynamic,
            ref TemplateTableRef? fromTable)
        {
            // Flatten the chain of method invocations
            var chain = FlattenMethodChain(expression);

            foreach (var invocation in chain)
            {
                if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
                    continue;

                string methodName = memberAccess.Name.Identifier.Text;
                var args = invocation.ArgumentList.Arguments;

                switch (methodName)
                {
                    case "Select":
                        ExtractSelectColumns(args, tables, selectColumns, parameters);
                        break;

                    case "From":
                        if (args.Count == 1)
                        {
                            string? varName = args[0].Expression.ToString();
                            if (tables.TryGetValue(varName, out var tRef))
                            {
                                fromTable = tRef;
                            }
                        }
                        break;

                    case "Where":
                        ExtractWhereClause(args, tables, parameters, whereClauses, "WHERE", false, null);
                        break;

                    case "WhereIf":
                        isDynamic = true;
                        ExtractConditionalWhereClause(args, tables, parameters, whereClauses, "WHERE");
                        break;

                    case "And":
                        ExtractWhereClause(args, tables, parameters, whereClauses, "AND", false, null);
                        break;

                    case "AndIf":
                        isDynamic = true;
                        ExtractConditionalWhereClause(args, tables, parameters, whereClauses, "AND");
                        break;

                    case "Or":
                        ExtractWhereClause(args, tables, parameters, whereClauses, "OR", false, null);
                        break;

                    case "OrIf":
                        isDynamic = true;
                        ExtractConditionalWhereClause(args, tables, parameters, whereClauses, "OR");
                        break;

                    case "LeftJoin":
                        ExtractJoin(args, tables, joins, "LEFT JOIN");
                        break;

                    case "InnerJoin":
                        ExtractJoin(args, tables, joins, "INNER JOIN");
                        break;

                    case "GroupBy":
                        ExtractGroupBy(args, tables, groupByColumns);
                        break;

                    case "Having":
                        ExtractWhereClause(args, tables, parameters, havingClauses, "HAVING", false, null);
                        break;

                    case "OrderBy":
                        ExtractOrderBy(args, tables, orderByItems, false);
                        break;

                    case "OrderByDesc":
                        ExtractOrderBy(args, tables, orderByItems, true);
                        break;

                    case "Limit":
                        if (args.Count == 1)
                            limit = ExtractIntLiteral(args[0].Expression);
                        break;

                    case "Offset":
                        if (args.Count == 1)
                            offset = ExtractIntLiteral(args[0].Expression);
                        break;
                }
            }
        }

        private static List<InvocationExpressionSyntax> FlattenMethodChain(ExpressionSyntax expression)
        {
            var chain = new List<InvocationExpressionSyntax>();
            var current = expression;
            while (current is InvocationExpressionSyntax invocation)
            {
                chain.Add(invocation);
                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    current = memberAccess.Expression;
                }
                else
                {
                    break;
                }
            }
            chain.Reverse();
            return chain;
        }

        private static void ExtractSelectColumns(
            SeparatedSyntaxList<ArgumentSyntax> args,
            Dictionary<string, TemplateTableRef> tables,
            List<TemplateSelectColumn> selectColumns,
            List<TemplateParam> parameters)
        {
            int ordinal = selectColumns.Count;
            foreach (var arg in args)
            {
                var colInfo = TryExtractColumnExpr(arg.Expression, tables);
                if (colInfo != null)
                {
                    selectColumns.Add(new TemplateSelectColumn(
                        colInfo.Value.ColumnName,
                        colInfo.Value.Alias,
                        colInfo.Value.ClrType,
                        ordinal++,
                        colInfo.Value.TableAlias,
                        colInfo.Value.IsNullable));
                }
            }
        }

        private static void ExtractWhereClause(
            SeparatedSyntaxList<ArgumentSyntax> args,
            Dictionary<string, TemplateTableRef> tables,
            List<TemplateParam> parameters,
            List<TemplateWhereClause> clauses,
            string logicalOp,
            bool isConditional,
            string? conditionParamName)
        {
            // Where(left, op, right) - 3 args
            if (args.Count >= 3)
            {
                var leftCol = TryExtractColumnRef(args[0].Expression, tables);
                string? opStr = ExtractOperator(args[1].Expression);
                var rightInfo = TryExtractRightSide(args[2].Expression, tables, parameters);

                if (leftCol != null && opStr != null)
                {
                    clauses.Add(new TemplateWhereClause(
                        leftCol.Value.ColumnName,
                        leftCol.Value.TableAlias,
                        opStr,
                        rightInfo?.ParamName,
                        rightInfo?.ColumnName,
                        rightInfo?.TableAlias,
                        logicalOp,
                        isConditional,
                        conditionParamName));
                }
            }
        }

        private static void ExtractConditionalWhereClause(
            SeparatedSyntaxList<ArgumentSyntax> args,
            Dictionary<string, TemplateTableRef> tables,
            List<TemplateParam> parameters,
            List<TemplateWhereClause> clauses,
            string logicalOp)
        {
            // WhereIf(left, op, right) - 3 args (same as Where)
            // The condition bool parameter is auto-derived from the right-side b.Param() name
            if (args.Count >= 3)
            {
                var leftCol = TryExtractColumnRef(args[0].Expression, tables);
                string? opStr = ExtractOperator(args[1].Expression);
                var rightInfo = TryExtractRightSide(args[2].Expression, tables, parameters);

                if (leftCol != null && opStr != null)
                {
                    // Use the right-side param name as the condition param reference
                    string? condParamName = rightInfo?.ParamName;
                    clauses.Add(new TemplateWhereClause(
                        leftCol.Value.ColumnName,
                        leftCol.Value.TableAlias,
                        opStr,
                        rightInfo?.ParamName,
                        rightInfo?.ColumnName,
                        rightInfo?.TableAlias,
                        logicalOp,
                        true,
                        condParamName));
                }
            }
        }

        private static void ExtractJoin(
            SeparatedSyntaxList<ArgumentSyntax> args,
            Dictionary<string, TemplateTableRef> tables,
            List<TemplateJoinClause> joins,
            string joinType)
        {
            if (args.Count < 2) return;

            string? varName = args[0].Expression.ToString();
            TemplateTableRef? joinTable = null;
            if (tables.TryGetValue(varName, out var tRef))
            {
                joinTable = tRef;
            }
            if (joinTable == null) return;

            // Parse the lambda in the second argument for ON clauses
            var onClauses = new List<TemplateJoinOnClause>();
            if (args[1].Expression is LambdaExpressionSyntax lambda)
            {
                ExtractJoinOnClauses(lambda, tables, onClauses);
            }

            joins.Add(new TemplateJoinClause(joinType, joinTable, onClauses.ToImmutableArray()));
        }

        private static void ExtractJoinOnClauses(
            LambdaExpressionSyntax lambda,
            Dictionary<string, TemplateTableRef> tables,
            List<TemplateJoinOnClause> onClauses)
        {
            // Lambda body contains chained On/And calls
            ExpressionSyntax? body = null;
            if (lambda is SimpleLambdaExpressionSyntax simpleLambda)
            {
                body = simpleLambda.ExpressionBody;
                if (body == null && simpleLambda.Body is BlockSyntax block)
                {
                    foreach (var stmt in block.Statements)
                    {
                        if (stmt is ExpressionStatementSyntax exprStmt)
                        {
                            ExtractJoinOnFromExpression(exprStmt.Expression, tables, onClauses);
                        }
                    }
                    return;
                }
            }
            else if (lambda is ParenthesizedLambdaExpressionSyntax parenLambda)
            {
                body = parenLambda.ExpressionBody;
                if (body == null && parenLambda.Body is BlockSyntax block)
                {
                    foreach (var stmt in block.Statements)
                    {
                        if (stmt is ExpressionStatementSyntax exprStmt)
                        {
                            ExtractJoinOnFromExpression(exprStmt.Expression, tables, onClauses);
                        }
                    }
                    return;
                }
            }

            if (body != null)
            {
                ExtractJoinOnFromExpression(body, tables, onClauses);
            }
        }

        private static void ExtractJoinOnFromExpression(
            ExpressionSyntax expression,
            Dictionary<string, TemplateTableRef> tables,
            List<TemplateJoinOnClause> onClauses)
        {
            var chain = FlattenMethodChain(expression);
            foreach (var invocation in chain)
            {
                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    string name = memberAccess.Name.Identifier.Text;
                    if (name == "On" || name == "And")
                    {
                        var args = invocation.ArgumentList.Arguments;
                        if (args.Count >= 3)
                        {
                            var left = TryExtractColumnRef(args[0].Expression, tables);
                            string? op = ExtractOperator(args[1].Expression);
                            var right = TryExtractColumnRef(args[2].Expression, tables);

                            if (left != null && op != null && right != null)
                            {
                                onClauses.Add(new TemplateJoinOnClause(
                                    left.Value.ColumnName, left.Value.TableAlias,
                                    op,
                                    right.Value.ColumnName, right.Value.TableAlias));
                            }
                        }
                    }
                }
            }
        }

        private static void ExtractGroupBy(
            SeparatedSyntaxList<ArgumentSyntax> args,
            Dictionary<string, TemplateTableRef> tables,
            List<string> groupByColumns)
        {
            foreach (var arg in args)
            {
                var colRef = TryExtractColumnRef(arg.Expression, tables);
                if (colRef != null)
                {
                    groupByColumns.Add($"\"{colRef.Value.TableAlias}\".\"{colRef.Value.ColumnName}\"");
                }
            }
        }

        private static void ExtractOrderBy(
            SeparatedSyntaxList<ArgumentSyntax> args,
            Dictionary<string, TemplateTableRef> tables,
            List<TemplateOrderByItem> orderByItems,
            bool descending)
        {
            foreach (var arg in args)
            {
                var colRef = TryExtractColumnRef(arg.Expression, tables);
                if (colRef != null)
                {
                    orderByItems.Add(new TemplateOrderByItem(
                        colRef.Value.ColumnName, colRef.Value.TableAlias, descending));
                }
            }
        }

        private struct ColumnInfo
        {
            public string ColumnName;
            public string? Alias;
            public string ClrType;
            public string TableAlias;
            public bool IsNullable;
        }

        private struct ColumnRef
        {
            public string ColumnName;
            public string TableAlias;
        }

        private struct RightSideInfo
        {
            public string? ParamName;
            public string? ColumnName;
            public string? TableAlias;
        }

        private static ColumnInfo? TryExtractColumnExpr(
            ExpressionSyntax expression,
            Dictionary<string, TemplateTableRef> tables)
        {
            // Match: table.Col<Type>("name") or table.Col("name")
            if (expression is InvocationExpressionSyntax invocation &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                string methodName = memberAccess.Name.Identifier.Text;
                if (methodName == "Col" && invocation.ArgumentList.Arguments.Count == 1)
                {
                    string? colName = ExtractStringLiteral(invocation.ArgumentList.Arguments[0].Expression);
                    string? varName = GetReceiverName(memberAccess.Expression);

                    if (colName != null && varName != null && tables.TryGetValue(varName, out var tableRef))
                    {
                        // Check for generic type argument: Col<int>
                        string clrType = "string";
                        bool isNullable = false;
                        if (memberAccess.Name is GenericNameSyntax genericName &&
                            genericName.TypeArgumentList.Arguments.Count == 1)
                        {
                            clrType = genericName.TypeArgumentList.Arguments[0].ToString();
                            isNullable = clrType.EndsWith("?");
                        }

                        return new ColumnInfo
                        {
                            ColumnName = colName,
                            Alias = null,
                            ClrType = clrType,
                            TableAlias = tableRef.Alias,
                            IsNullable = isNullable
                        };
                    }
                }
            }

            // Match: table.Col<Type>("name").As("alias") or .As<Type>("alias")
            if (expression is InvocationExpressionSyntax asInvocation &&
                asInvocation.Expression is MemberAccessExpressionSyntax asMemberAccess &&
                asMemberAccess.Name.Identifier.Text == "As")
            {
                string? alias = null;
                string? asType = null;

                if (asMemberAccess.Name is GenericNameSyntax asGenericName &&
                    asGenericName.TypeArgumentList.Arguments.Count == 1)
                {
                    asType = asGenericName.TypeArgumentList.Arguments[0].ToString();
                }

                if (asInvocation.ArgumentList.Arguments.Count >= 1)
                {
                    alias = ExtractStringLiteral(asInvocation.ArgumentList.Arguments[0].Expression);
                }

                // Recursively extract the inner column
                var inner = TryExtractColumnExpr(asMemberAccess.Expression, tables);
                if (inner != null)
                {
                    var result = inner.Value;
                    if (alias != null) result.Alias = alias;
                    if (asType != null)
                    {
                        result.ClrType = asType;
                        result.IsNullable = asType.EndsWith("?");
                    }
                    return result;
                }
            }

            return null;
        }

        private static ColumnRef? TryExtractColumnRef(
            ExpressionSyntax expression,
            Dictionary<string, TemplateTableRef> tables)
        {
            if (expression is InvocationExpressionSyntax invocation &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name.Identifier.Text == "Col" &&
                invocation.ArgumentList.Arguments.Count == 1)
            {
                string? colName = ExtractStringLiteral(invocation.ArgumentList.Arguments[0].Expression);
                string? varName = GetReceiverName(memberAccess.Expression);

                if (colName != null && varName != null && tables.TryGetValue(varName, out var tableRef))
                {
                    return new ColumnRef { ColumnName = colName, TableAlias = tableRef.Alias };
                }
            }

            // Also handle generic Col<T>
            if (expression is InvocationExpressionSyntax genInvocation &&
                genInvocation.Expression is MemberAccessExpressionSyntax genMemberAccess &&
                genMemberAccess.Name is GenericNameSyntax genName &&
                genName.Identifier.Text == "Col" &&
                genInvocation.ArgumentList.Arguments.Count == 1)
            {
                string? colName = ExtractStringLiteral(genInvocation.ArgumentList.Arguments[0].Expression);
                string? varName = GetReceiverName(genMemberAccess.Expression);

                if (colName != null && varName != null && tables.TryGetValue(varName, out var tableRef))
                {
                    return new ColumnRef { ColumnName = colName, TableAlias = tableRef.Alias };
                }
            }

            return null;
        }

        private static RightSideInfo? TryExtractRightSide(
            ExpressionSyntax expression,
            Dictionary<string, TemplateTableRef> tables,
            List<TemplateParam> parameters)
        {
            // Match: b.Param<Type>("paramName")
            if (expression is InvocationExpressionSyntax invocation &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                string methodName = memberAccess.Name is GenericNameSyntax gn
                    ? gn.Identifier.Text
                    : memberAccess.Name.Identifier.Text;

                if (methodName == "Param" && invocation.ArgumentList.Arguments.Count == 1)
                {
                    string? paramName = ExtractStringLiteral(invocation.ArgumentList.Arguments[0].Expression);
                    if (paramName != null)
                    {
                        string paramType = "object";
                        bool isNullable = false;
                        if (memberAccess.Name is GenericNameSyntax genericName &&
                            genericName.TypeArgumentList.Arguments.Count == 1)
                        {
                            paramType = genericName.TypeArgumentList.Arguments[0].ToString();
                            isNullable = paramType.EndsWith("?");
                        }

                        // Add to param list if not already there
                        int existing = parameters.FindIndex(p => p.ParamName == paramName);
                        if (existing < 0)
                        {
                            parameters.Add(new TemplateParam(paramName, paramType, isNullable, parameters.Count));
                        }

                        return new RightSideInfo { ParamName = paramName };
                    }
                }
            }

            // Match: column reference
            var colRef = TryExtractColumnRef(expression, tables);
            if (colRef != null)
            {
                return new RightSideInfo
                {
                    ColumnName = colRef.Value.ColumnName,
                    TableAlias = colRef.Value.TableAlias
                };
            }

            return null;
        }

        private static string? ExtractOperator(ExpressionSyntax expression)
        {
            // Match: Op.Eq, SqlComparisonOperator.Eq, etc.
            if (expression is MemberAccessExpressionSyntax memberAccess)
            {
                return memberAccess.Name.Identifier.Text switch
                {
                    "Eq" => "=",
                    "NotEq" => "!=",
                    "Gt" => ">",
                    "Gte" => ">=",
                    "Lt" => "<",
                    "Lte" => "<=",
                    "Like" => "LIKE",
                    "ILike" => "ILIKE",
                    "NotLike" => "NOT LIKE",
                    "NotILike" => "NOT ILIKE",
                    "In" => "IN",
                    "NotIn" => "NOT IN",
                    "IsNull" => "IS NULL",
                    "IsNotNull" => "IS NOT NULL",
                    "Between" => "BETWEEN",
                    "NotBetween" => "NOT BETWEEN",
                    "ArrayContains" => "@>",
                    "ArrayContainedBy" => "<@",
                    "ArrayOverlaps" => "&&",
                    _ => null
                };
            }
            return null;
        }

        private static string? GetReceiverName(ExpressionSyntax expression)
        {
            if (expression is IdentifierNameSyntax identifier)
                return identifier.Identifier.Text;
            return null;
        }

        private static string? ExtractStringLiteral(ExpressionSyntax expression)
        {
            if (expression is LiteralExpressionSyntax literal &&
                literal.IsKind(SyntaxKind.StringLiteralExpression))
            {
                return literal.Token.ValueText;
            }
            return null;
        }

        private static int? ExtractIntLiteral(ExpressionSyntax expression)
        {
            if (expression is LiteralExpressionSyntax literal &&
                literal.IsKind(SyntaxKind.NumericLiteralExpression) &&
                literal.Token.Value is int intValue)
            {
                return intValue;
            }
            return null;
        }

        private static ImmutableArray<TemplateParam> BuildParamList(
            List<(string Name, string Type, bool IsNullable)> methodParams,
            List<TemplateParam> extractedParams)
        {
            // Always use method signature types as the source of truth.
            // The extracted type from b.Param<T>() is only used if no method param exists,
            // or if the non-generic b.Param() was used (which defaults to "object").
            var result = new List<TemplateParam>();
            int ordinal = 0;
            foreach (var mp in methodParams)
            {
                result.Add(new TemplateParam(mp.Name, mp.Type, mp.IsNullable, ordinal++));
            }
            return result.ToImmutableArray();
        }
    }
}
