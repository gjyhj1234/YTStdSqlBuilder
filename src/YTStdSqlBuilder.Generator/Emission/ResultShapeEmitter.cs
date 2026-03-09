using System.Text;
using YTStdSqlBuilder.Generator.TemplateAnalysis;

namespace YTStdSqlBuilder.Generator.Emission
{
    /// <summary>
    /// Generates Row DTO classes and Reader methods from SELECT columns.
    /// </summary>
    internal static class ResultShapeEmitter
    {
        /// <summary>
        /// Generates a sealed Row class with init-only properties.
        /// </summary>
        public static void EmitRowClass(
            StringBuilder sb,
            string methodName,
            TemplateQueryInfo query,
            string indent)
        {
            var columns = ResultShapeAnalyzer.ExtractResultShape(query);
            if (columns.Length == 0) return;

            string className = $"{methodName}Row";

            sb.AppendLine($"{indent}public sealed class {className}");
            sb.AppendLine($"{indent}{{");

            foreach (var col in columns)
            {
                string defaultVal = ResultShapeAnalyzer.GetDefaultValue(col.ClrType);
                string suffix = defaultVal != null ? $" = {defaultVal};" : "";
                sb.AppendLine($"{indent}    public {col.ClrType} {col.PropertyName} {{ get; init; }}{suffix}");
            }

            sb.AppendLine($"{indent}}}");
        }

        /// <summary>
        /// Generates a static Reader method that reads a row from NpgsqlDataReader.
        /// </summary>
        public static void EmitReaderMethod(
            StringBuilder sb,
            string methodName,
            TemplateQueryInfo query,
            string indent)
        {
            var columns = ResultShapeAnalyzer.ExtractResultShape(query);
            if (columns.Length == 0) return;

            string className = $"{methodName}Row";

            sb.AppendLine($"{indent}public static {className} Read{className}(Npgsql.NpgsqlDataReader reader)");
            sb.AppendLine($"{indent}{{");
            sb.AppendLine($"{indent}    return new {className}");
            sb.AppendLine($"{indent}    {{");

            for (int i = 0; i < columns.Length; i++)
            {
                var col = columns[i];
                string comma = i < columns.Length - 1 ? "," : "";

                if (col.IsNullable)
                {
                    sb.AppendLine($"{indent}        {col.PropertyName} = reader.IsDBNull({col.Ordinal}) ? null : reader.{col.ReaderMethod}({col.Ordinal}){comma}");
                }
                else
                {
                    sb.AppendLine($"{indent}        {col.PropertyName} = reader.{col.ReaderMethod}({col.Ordinal}){comma}");
                }
            }

            sb.AppendLine($"{indent}    }};");
            sb.AppendLine($"{indent}}}");
        }
    }
}
