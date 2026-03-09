using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using YTStdSqlBuilder.Generator.Emission;
using YTStdSqlBuilder.Generator.TemplateAnalysis;

namespace YTStdSqlBuilder.Generator
{
    [Generator]
    public class PgSqlTemplateGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Find classes with [PgSqlTemplate] attribute
            var templateClasses = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    "YTStdSqlBuilder.PgSqlTemplateAttribute",
                    predicate: static (node, _) => node is ClassDeclarationSyntax,
                    transform: static (ctx, _) => GetTemplateClassInfo(ctx))
                .Where(static info => info is not null);

            context.RegisterSourceOutput(templateClasses, static (spc, info) => GenerateCode(spc, info!));
        }

        private static TemplateClassInfo? GetTemplateClassInfo(GeneratorAttributeSyntaxContext context)
        {
            if (context.TargetNode is not ClassDeclarationSyntax classDecl)
                return null;

            var classSymbol = context.TargetSymbol as INamedTypeSymbol;
            if (classSymbol == null)
                return null;

            string className = classSymbol.Name;
            string? ns = classSymbol.ContainingNamespace?.IsGlobalNamespace == true
                ? null
                : classSymbol.ContainingNamespace?.ToDisplayString();

            bool isStatic = classDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));

            // Find [PgSqlQuery] methods
            var queryMethods = new List<TemplateQueryInfo>();

            foreach (var member in classDecl.Members)
            {
                if (member is not MethodDeclarationSyntax methodDecl)
                    continue;

                // Check for [PgSqlQuery] attribute
                bool hasPgSqlQuery = false;
                foreach (var attrList in methodDecl.AttributeLists)
                {
                    foreach (var attr in attrList.Attributes)
                    {
                        string attrName = attr.Name.ToString();
                        if (attrName == "PgSqlQuery" || attrName == "PgSqlQueryAttribute" ||
                            attrName == "YTStdSqlBuilder.PgSqlQuery" || attrName == "YTStdSqlBuilder.PgSqlQueryAttribute")
                        {
                            hasPgSqlQuery = true;
                            break;
                        }
                    }
                    if (hasPgSqlQuery) break;
                }

                if (!hasPgSqlQuery) continue;

                // Must be partial
                bool isPartial = methodDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
                if (!isPartial) continue;

                string methodName = methodDecl.Identifier.Text;

                // Find matching Define_ method
                string defineMethodName = $"Define_{methodName}";
                MethodDeclarationSyntax? defineMethod = null;

                foreach (var otherMember in classDecl.Members)
                {
                    if (otherMember is MethodDeclarationSyntax otherMethod &&
                        otherMethod.Identifier.Text == defineMethodName)
                    {
                        defineMethod = otherMethod;
                        break;
                    }
                }

                if (defineMethod == null) continue;

                // Analyze the Define_ method
                var queryInfo = TemplateAnalyzer.Analyze(
                    defineMethod, methodDecl, context.SemanticModel);

                if (queryInfo != null)
                {
                    queryMethods.Add(queryInfo);
                }
            }

            if (queryMethods.Count == 0)
                return null;

            return new TemplateClassInfo(
                className,
                ns,
                queryMethods.ToImmutableArray(),
                isStatic);
        }

        private static void GenerateCode(SourceProductionContext context, TemplateClassInfo info)
        {
            string source = TemplateEmitter.Emit(info);
            string hintName = info.Namespace != null
                ? $"{info.Namespace}.{info.ClassName}.g.cs"
                : $"{info.ClassName}.g.cs";
            context.AddSource(hintName, source);
        }
    }
}
