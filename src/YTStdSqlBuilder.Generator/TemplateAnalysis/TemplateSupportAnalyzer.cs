using System.Collections.Immutable;

namespace YTStdSqlBuilder.Generator.TemplateAnalysis
{
    /// <summary>
    /// Determines whether a query can be statically generated or needs runtime fallback.
    /// </summary>
    internal static class TemplateSupportAnalyzer
    {
        /// <summary>
        /// Returns true if the query can be fully statically generated (no WhereIf/AndIf/OrIf).
        /// </summary>
        public static bool CanGenerateStaticSql(TemplateQueryInfo query)
        {
            return !query.IsDynamic;
        }

        /// <summary>
        /// Returns true if the result shape (SELECT columns) can be generated.
        /// This is true if SELECT columns are fixed (even for dynamic queries).
        /// </summary>
        public static bool CanGenerateResultShape(TemplateQueryInfo query)
        {
            return query.SelectColumns.Length > 0;
        }

        /// <summary>
        /// Determines the fallback policy for a query.
        /// </summary>
        public static string DetermineFallbackPolicy(TemplateQueryInfo query)
        {
            if (!query.IsDynamic)
                return "Static";

            if (query.FallbackToInterpreter)
                return "RuntimeInterpreter";

            return "None";
        }
    }
}
