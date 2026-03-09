using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace YTStdSqlBuilder.Generator.TemplateAnalysis
{
    /// <summary>
    /// Information about a class annotated with [PgSqlTemplate].
    /// </summary>
    internal sealed class TemplateClassInfo : IEquatable<TemplateClassInfo>
    {
        public string ClassName { get; }
        public string? Namespace { get; }
        public ImmutableArray<TemplateQueryInfo> QueryMethods { get; }
        public bool IsStatic { get; }

        public TemplateClassInfo(
            string className,
            string? ns,
            ImmutableArray<TemplateQueryInfo> queryMethods,
            bool isStatic)
        {
            ClassName = className;
            Namespace = ns;
            QueryMethods = queryMethods;
            IsStatic = isStatic;
        }

        public bool Equals(TemplateClassInfo? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (ClassName != other.ClassName || Namespace != other.Namespace ||
                IsStatic != other.IsStatic ||
                QueryMethods.Length != other.QueryMethods.Length)
                return false;
            for (int i = 0; i < QueryMethods.Length; i++)
            {
                if (!QueryMethods[i].Equals(other.QueryMethods[i]))
                    return false;
            }
            return true;
        }

        public override bool Equals(object? obj) => Equals(obj as TemplateClassInfo);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + (ClassName?.GetHashCode() ?? 0);
                hash = hash * 31 + (Namespace?.GetHashCode() ?? 0);
                hash = hash * 31 + IsStatic.GetHashCode();
                hash = hash * 31 + QueryMethods.Length;
                return hash;
            }
        }
    }

    /// <summary>
    /// Information about a method annotated with [PgSqlQuery].
    /// </summary>
    internal sealed class TemplateQueryInfo : IEquatable<TemplateQueryInfo>
    {
        public string MethodName { get; }
        public ImmutableArray<TemplateParam> Parameters { get; }
        public ImmutableArray<TemplateSelectColumn> SelectColumns { get; }
        public ImmutableArray<TemplateTableRef> Tables { get; }
        public TemplateSqlStructure SqlStructure { get; }
        public bool IsDynamic { get; }
        public bool FallbackToInterpreter { get; }
        public string? DefineMethodBody { get; }

        public TemplateQueryInfo(
            string methodName,
            ImmutableArray<TemplateParam> parameters,
            ImmutableArray<TemplateSelectColumn> selectColumns,
            ImmutableArray<TemplateTableRef> tables,
            TemplateSqlStructure sqlStructure,
            bool isDynamic,
            bool fallbackToInterpreter,
            string? defineMethodBody)
        {
            MethodName = methodName;
            Parameters = parameters;
            SelectColumns = selectColumns;
            Tables = tables;
            SqlStructure = sqlStructure;
            IsDynamic = isDynamic;
            FallbackToInterpreter = fallbackToInterpreter;
            DefineMethodBody = defineMethodBody;
        }

        public bool Equals(TemplateQueryInfo? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return MethodName == other.MethodName &&
                   IsDynamic == other.IsDynamic &&
                   FallbackToInterpreter == other.FallbackToInterpreter &&
                   DefineMethodBody == other.DefineMethodBody;
        }

        public override bool Equals(object? obj) => Equals(obj as TemplateQueryInfo);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + (MethodName?.GetHashCode() ?? 0);
                hash = hash * 31 + IsDynamic.GetHashCode();
                hash = hash * 31 + FallbackToInterpreter.GetHashCode();
                return hash;
            }
        }
    }

    /// <summary>
    /// A column in the SELECT list.
    /// </summary>
    internal sealed class TemplateSelectColumn : IEquatable<TemplateSelectColumn>
    {
        public string ColumnName { get; }
        public string? Alias { get; }
        public string ClrTypeName { get; }
        public int Ordinal { get; }
        public string TableAlias { get; }
        public bool IsNullable { get; }

        public TemplateSelectColumn(
            string columnName,
            string? alias,
            string clrTypeName,
            int ordinal,
            string tableAlias,
            bool isNullable)
        {
            ColumnName = columnName;
            Alias = alias;
            ClrTypeName = clrTypeName;
            Ordinal = ordinal;
            TableAlias = tableAlias;
            IsNullable = isNullable;
        }

        public bool Equals(TemplateSelectColumn? other)
        {
            if (other is null) return false;
            return ColumnName == other.ColumnName &&
                   Alias == other.Alias &&
                   ClrTypeName == other.ClrTypeName &&
                   Ordinal == other.Ordinal &&
                   TableAlias == other.TableAlias &&
                   IsNullable == other.IsNullable;
        }

        public override bool Equals(object? obj) => Equals(obj as TemplateSelectColumn);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + (ColumnName?.GetHashCode() ?? 0);
                hash = hash * 31 + Ordinal;
                return hash;
            }
        }
    }

    /// <summary>
    /// A parameter reference in the query.
    /// </summary>
    internal sealed class TemplateParam : IEquatable<TemplateParam>
    {
        public string ParamName { get; }
        public string TypeName { get; }
        public bool IsNullable { get; }
        public int Ordinal { get; }

        public TemplateParam(string paramName, string typeName, bool isNullable, int ordinal)
        {
            ParamName = paramName;
            TypeName = typeName;
            IsNullable = isNullable;
            Ordinal = ordinal;
        }

        public bool Equals(TemplateParam? other)
        {
            if (other is null) return false;
            return ParamName == other.ParamName &&
                   TypeName == other.TypeName &&
                   IsNullable == other.IsNullable &&
                   Ordinal == other.Ordinal;
        }

        public override bool Equals(object? obj) => Equals(obj as TemplateParam);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + (ParamName?.GetHashCode() ?? 0);
                hash = hash * 31 + Ordinal;
                return hash;
            }
        }
    }

    /// <summary>
    /// A table reference extracted from a Define_ method.
    /// </summary>
    internal sealed class TemplateTableRef : IEquatable<TemplateTableRef>
    {
        public string TableName { get; }
        public string Alias { get; }
        public string VariableName { get; }

        public TemplateTableRef(string tableName, string alias, string variableName)
        {
            TableName = tableName;
            Alias = alias;
            VariableName = variableName;
        }

        public bool Equals(TemplateTableRef? other)
        {
            if (other is null) return false;
            return TableName == other.TableName &&
                   Alias == other.Alias &&
                   VariableName == other.VariableName;
        }

        public override bool Equals(object? obj) => Equals(obj as TemplateTableRef);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + (TableName?.GetHashCode() ?? 0);
                hash = hash * 31 + (Alias?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }

    /// <summary>
    /// The extracted SQL structure from a Define_ method.
    /// </summary>
    internal sealed class TemplateSqlStructure : IEquatable<TemplateSqlStructure>
    {
        public TemplateTableRef? FromTable { get; }
        public ImmutableArray<TemplateWhereClause> WhereClauses { get; }
        public ImmutableArray<TemplateJoinClause> Joins { get; }
        public ImmutableArray<string> GroupByColumns { get; }
        public ImmutableArray<TemplateWhereClause> HavingClauses { get; }
        public ImmutableArray<TemplateOrderByItem> OrderByColumns { get; }
        public int? Limit { get; }
        public int? Offset { get; }

        public TemplateSqlStructure(
            TemplateTableRef? fromTable,
            ImmutableArray<TemplateWhereClause> whereClauses,
            ImmutableArray<TemplateJoinClause> joins,
            ImmutableArray<string> groupByColumns,
            ImmutableArray<TemplateWhereClause> havingClauses,
            ImmutableArray<TemplateOrderByItem> orderByColumns,
            int? limit,
            int? offset)
        {
            FromTable = fromTable;
            WhereClauses = whereClauses;
            Joins = joins;
            GroupByColumns = groupByColumns;
            HavingClauses = havingClauses;
            OrderByColumns = orderByColumns;
            Limit = limit;
            Offset = offset;
        }

        public bool Equals(TemplateSqlStructure? other)
        {
            if (other is null) return false;
            return Limit == other.Limit && Offset == other.Offset;
        }

        public override bool Equals(object? obj) => Equals(obj as TemplateSqlStructure);
        public override int GetHashCode() => (Limit ?? 0) * 31 + (Offset ?? 0);
    }

    /// <summary>
    /// A WHERE/AND/OR clause.
    /// </summary>
    internal sealed class TemplateWhereClause
    {
        public string LeftColumn { get; }
        public string LeftTableAlias { get; }
        public string Operator { get; }
        public string? RightParamName { get; }
        public string? RightColumn { get; }
        public string? RightTableAlias { get; }
        public string LogicalOp { get; }
        public bool IsConditional { get; }
        public string? ConditionParamName { get; }

        public TemplateWhereClause(
            string leftColumn,
            string leftTableAlias,
            string @operator,
            string? rightParamName,
            string? rightColumn,
            string? rightTableAlias,
            string logicalOp,
            bool isConditional,
            string? conditionParamName)
        {
            LeftColumn = leftColumn;
            LeftTableAlias = leftTableAlias;
            Operator = @operator;
            RightParamName = rightParamName;
            RightColumn = rightColumn;
            RightTableAlias = rightTableAlias;
            LogicalOp = logicalOp;
            IsConditional = isConditional;
            ConditionParamName = conditionParamName;
        }
    }

    /// <summary>
    /// A JOIN clause.
    /// </summary>
    internal sealed class TemplateJoinClause
    {
        public string JoinType { get; }
        public TemplateTableRef Table { get; }
        public ImmutableArray<TemplateJoinOnClause> OnClauses { get; }

        public TemplateJoinClause(
            string joinType,
            TemplateTableRef table,
            ImmutableArray<TemplateJoinOnClause> onClauses)
        {
            JoinType = joinType;
            Table = table;
            OnClauses = onClauses;
        }
    }

    /// <summary>
    /// A JOIN ON condition.
    /// </summary>
    internal sealed class TemplateJoinOnClause
    {
        public string LeftColumn { get; }
        public string LeftTableAlias { get; }
        public string Operator { get; }
        public string RightColumn { get; }
        public string RightTableAlias { get; }

        public TemplateJoinOnClause(
            string leftColumn,
            string leftTableAlias,
            string @operator,
            string rightColumn,
            string rightTableAlias)
        {
            LeftColumn = leftColumn;
            LeftTableAlias = leftTableAlias;
            Operator = @operator;
            RightColumn = rightColumn;
            RightTableAlias = rightTableAlias;
        }
    }

    /// <summary>
    /// An ORDER BY item.
    /// </summary>
    internal sealed class TemplateOrderByItem
    {
        public string Column { get; }
        public string TableAlias { get; }
        public bool Descending { get; }

        public TemplateOrderByItem(string column, string tableAlias, bool descending)
        {
            Column = column;
            TableAlias = tableAlias;
            Descending = descending;
        }
    }
}
