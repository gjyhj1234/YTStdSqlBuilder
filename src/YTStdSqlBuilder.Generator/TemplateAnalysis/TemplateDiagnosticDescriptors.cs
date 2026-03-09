using Microsoft.CodeAnalysis;

namespace YTStdSqlBuilder.Generator.TemplateAnalysis
{
    internal static class TemplateDiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor NoDefineMethod = new(
            id: "YTSQL001",
            title: "Missing Define_ method",
            messageFormat: "No Define_{0} method found for [PgSqlQuery] method '{0}'",
            category: "YTStdSqlBuilder",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor DefineMethodParseError = new(
            id: "YTSQL002",
            title: "Define_ method parse error",
            messageFormat: "Could not parse Define_{0} method body: {1}",
            category: "YTStdSqlBuilder",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor DynamicQueryNoFallback = new(
            id: "YTSQL003",
            title: "Dynamic query without fallback",
            messageFormat: "Query '{0}' uses dynamic conditions (WhereIf/AndIf/OrIf) but FallbackToInterpreter is not set",
            category: "YTStdSqlBuilder",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor NoSelectColumns = new(
            id: "YTSQL004",
            title: "No SELECT columns found",
            messageFormat: "No SELECT columns found in Define_{0}",
            category: "YTStdSqlBuilder",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor ParameterMismatch = new(
            id: "YTSQL005",
            title: "Parameter mismatch",
            messageFormat: "Parameter '{0}' in Define_{1} does not match method signature parameter",
            category: "YTStdSqlBuilder",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);
    }
}
