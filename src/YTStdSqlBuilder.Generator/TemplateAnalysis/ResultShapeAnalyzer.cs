using System.Collections.Immutable;

namespace YTStdSqlBuilder.Generator.TemplateAnalysis
{
    /// <summary>
    /// Extracts SELECT column structure for DTO generation.
    /// </summary>
    internal static class ResultShapeAnalyzer
    {
        /// <summary>
        /// Extracts the result shape from SELECT columns for Row DTO generation.
        /// </summary>
        public static ImmutableArray<ResultColumnInfo> ExtractResultShape(TemplateQueryInfo query)
        {
            var builder = ImmutableArray.CreateBuilder<ResultColumnInfo>(query.SelectColumns.Length);

            foreach (var col in query.SelectColumns)
            {
                string propertyName = ToPascalCase(col.Alias ?? col.ColumnName);
                string clrType = NormalizeClrType(col.ClrTypeName);
                string readerMethod = GetReaderMethod(clrType);
                bool isNullable = col.IsNullable || clrType.EndsWith("?");

                builder.Add(new ResultColumnInfo(
                    col.ColumnName,
                    col.Alias,
                    propertyName,
                    clrType,
                    readerMethod,
                    col.Ordinal,
                    isNullable));
            }

            return builder.MoveToImmutable();
        }

        /// <summary>
        /// Maps CLR type to NpgsqlDataReader method name.
        /// </summary>
        public static string GetReaderMethod(string clrType)
        {
            string baseType = clrType.TrimEnd('?');
            return baseType switch
            {
                "int" => "GetInt32",
                "Int32" => "GetInt32",
                "System.Int32" => "GetInt32",
                "long" => "GetInt64",
                "Int64" => "GetInt64",
                "System.Int64" => "GetInt64",
                "string" => "GetString",
                "String" => "GetString",
                "System.String" => "GetString",
                "bool" => "GetBoolean",
                "Boolean" => "GetBoolean",
                "System.Boolean" => "GetBoolean",
                "decimal" => "GetDecimal",
                "Decimal" => "GetDecimal",
                "System.Decimal" => "GetDecimal",
                "double" => "GetDouble",
                "Double" => "GetDouble",
                "System.Double" => "GetDouble",
                "float" => "GetFloat",
                "Single" => "GetFloat",
                "System.Single" => "GetFloat",
                "DateTime" => "GetDateTime",
                "System.DateTime" => "GetDateTime",
                "Guid" => "GetGuid",
                "System.Guid" => "GetGuid",
                _ => "GetString"
            };
        }

        /// <summary>
        /// Returns the default value expression for a CLR type (used in init properties).
        /// </summary>
        public static string? GetDefaultValue(string clrType)
        {
            string baseType = clrType.TrimEnd('?');
            if (clrType.EndsWith("?")) return null; // nullable types default to null
            return baseType switch
            {
                "string" or "String" or "System.String" => "\"\"",
                _ => null
            };
        }

        private static string NormalizeClrType(string clrType)
        {
            // Keep the type as-is for code generation
            return clrType;
        }

        private static string ToPascalCase(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;

            var chars = new char[name.Length];
            bool capitalizeNext = true;
            int writeIndex = 0;

            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (c == '_' || c == '-')
                {
                    capitalizeNext = true;
                    continue;
                }
                chars[writeIndex++] = capitalizeNext ? char.ToUpperInvariant(c) : c;
                capitalizeNext = false;
            }

            return new string(chars, 0, writeIndex);
        }
    }

    /// <summary>
    /// Information about a result column for code generation.
    /// </summary>
    internal sealed class ResultColumnInfo
    {
        public string ColumnName { get; }
        public string? Alias { get; }
        public string PropertyName { get; }
        public string ClrType { get; }
        public string ReaderMethod { get; }
        public int Ordinal { get; }
        public bool IsNullable { get; }

        public ResultColumnInfo(
            string columnName,
            string? alias,
            string propertyName,
            string clrType,
            string readerMethod,
            int ordinal,
            bool isNullable)
        {
            ColumnName = columnName;
            Alias = alias;
            PropertyName = propertyName;
            ClrType = clrType;
            ReaderMethod = readerMethod;
            Ordinal = ordinal;
            IsNullable = isNullable;
        }
    }
}
