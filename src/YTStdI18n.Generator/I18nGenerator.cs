using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace YTStdI18n.Generator;

/// <summary>
/// i18n Source Generator。
/// 扫描标注了 [I18nResource] 的语言资源类，自动生成：
/// 1. K 常量索引类
/// 2. I18nExtensions 分类扩展方法
/// 3. I18n.Register() 注册方法
/// 4. 编译期校验所有语言包一致性
/// </summary>
[Generator]
public sealed class I18nGenerator : IIncrementalGenerator
{
    private const string AttributeName = "YTStdI18n.I18nResourceAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all classes decorated with [I18nResource]
        var classDeclarations = context.SyntaxProvider.ForAttributeWithMetadataName(
            AttributeName,
            predicate: static (node, _) => node is ClassDeclarationSyntax,
            transform: static (ctx, _) => GetLangPackInfo(ctx))
            .Where(static x => x != null)
            .Collect();

        context.RegisterSourceOutput(classDeclarations, static (spc, packs) =>
        {
            Execute(spc, packs!);
        });
    }

    private static LangPackInfo? GetLangPackInfo(GeneratorAttributeSyntaxContext ctx)
    {
        if (ctx.TargetNode is not ClassDeclarationSyntax classDecl)
            return null;

        var classSymbol = ctx.TargetSymbol as INamedTypeSymbol;
        if (classSymbol == null)
            return null;

        // Read IsBase from attribute
        bool isBase = false;
        foreach (var attrData in classSymbol.GetAttributes())
        {
            if (attrData.AttributeClass?.ToDisplayString() == AttributeName)
            {
                foreach (var named in attrData.NamedArguments)
                {
                    if (named.Key == "IsBase" && named.Value.Value is bool b)
                    {
                        isBase = b;
                    }
                }
            }
        }

        string className = classSymbol.Name;
        string? ns = classSymbol.ContainingNamespace?.IsGlobalNamespace == true
            ? null
            : classSymbol.ContainingNamespace?.ToDisplayString();

        // Extract categories (static string[]? fields)
        var categories = new List<CategoryInfo>();
        foreach (var member in classSymbol.GetMembers())
        {
            if (member is IFieldSymbol field
                && field.IsStatic
                && field.Type is IArrayTypeSymbol arrayType
                && arrayType.ElementType.SpecialType == SpecialType.System_String)
            {
                categories.Add(new CategoryInfo { Name = field.Name });
            }
        }

        // For base language: parse Register() method to get array element comments
        if (isBase)
        {
            var registerMethod = classDecl.Members
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(m => m.Identifier.Text == "Register");

            if (registerMethod != null)
            {
                ParseRegisterMethodForComments(registerMethod, categories);
            }
        }
        else
        {
            // For non-base languages: parse array element counts from Register()
            var registerMethod = classDecl.Members
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(m => m.Identifier.Text == "Register");

            if (registerMethod != null)
            {
                ParseRegisterMethodForCounts(registerMethod, categories);
            }
        }

        return new LangPackInfo
        {
            ClassName = className,
            Namespace = ns,
            IsBase = isBase,
            Categories = categories,
            Location = classDecl.GetLocation()
        };
    }

    private static void ParseRegisterMethodForComments(
        MethodDeclarationSyntax registerMethod,
        List<CategoryInfo> categories)
    {
        // Find all assignment expressions: FieldName = new string[] { ... }
        var body = registerMethod.Body;
        if (body == null) return;

        foreach (var statement in body.Statements)
        {
            if (statement is not ExpressionStatementSyntax exprStmt) continue;
            if (exprStmt.Expression is not AssignmentExpressionSyntax assignment) continue;

            string? fieldName = null;
            if (assignment.Left is IdentifierNameSyntax idName)
                fieldName = idName.Identifier.Text;
            else if (assignment.Left is MemberAccessExpressionSyntax memberAccess)
                fieldName = memberAccess.Name.Identifier.Text;

            if (fieldName == null) continue;

            var category = categories.Find(c => c.Name == fieldName);
            if (category == null) continue;

            // Find the array initializer
            InitializerExpressionSyntax? initializer = null;
            if (assignment.Right is ImplicitArrayCreationExpressionSyntax implicitArray)
                initializer = implicitArray.Initializer;
            else if (assignment.Right is ArrayCreationExpressionSyntax arrayCreation)
                initializer = arrayCreation.Initializer;
            else if (assignment.Right is CollectionExpressionSyntax)
                continue; // skip collection expressions

            if (initializer == null) continue;

            int index = 0;
            foreach (var element in initializer.Expressions)
            {
                // Look for leading trivia comments: /* Name */
                string? commentName = ExtractCommentName(element);
                category.Elements.Add(new ElementInfo
                {
                    Index = index,
                    CommentName = commentName
                });
                index++;
            }

            category.ElementCount = index;
        }
    }

    private static void ParseRegisterMethodForCounts(
        MethodDeclarationSyntax registerMethod,
        List<CategoryInfo> categories)
    {
        var body = registerMethod.Body;
        if (body == null) return;

        foreach (var statement in body.Statements)
        {
            if (statement is not ExpressionStatementSyntax exprStmt) continue;
            if (exprStmt.Expression is not AssignmentExpressionSyntax assignment) continue;

            string? fieldName = null;
            if (assignment.Left is IdentifierNameSyntax idName)
                fieldName = idName.Identifier.Text;
            else if (assignment.Left is MemberAccessExpressionSyntax memberAccess)
                fieldName = memberAccess.Name.Identifier.Text;

            if (fieldName == null) continue;

            var category = categories.Find(c => c.Name == fieldName);
            if (category == null) continue;

            InitializerExpressionSyntax? initializer = null;
            if (assignment.Right is ImplicitArrayCreationExpressionSyntax implicitArray)
                initializer = implicitArray.Initializer;
            else if (assignment.Right is ArrayCreationExpressionSyntax arrayCreation)
                initializer = arrayCreation.Initializer;

            if (initializer == null) continue;

            category.ElementCount = initializer.Expressions.Count;
        }
    }

    private static string? ExtractCommentName(ExpressionSyntax element)
    {
        // Look for /* Name */ pattern in leading trivia
        foreach (var trivia in element.GetLeadingTrivia())
        {
            if (trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
            {
                string commentText = trivia.ToString();
                // Strip /* and */
                if (commentText.StartsWith("/*") && commentText.EndsWith("*/") && commentText.Length > 4)
                {
                    string inner = commentText.Substring(2, commentText.Length - 4).Trim();
                    // Handle format like "0 Success" - take the name part after optional index
                    var parts = inner.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        // If first part is a number, take second part as name
                        if (int.TryParse(parts[0], out _))
                        {
                            return parts[1].Trim();
                        }
                    }
                    // Single word - use it directly
                    if (parts.Length == 1 && !int.TryParse(parts[0], out _))
                    {
                        return parts[0].Trim();
                    }
                    // Handle format "Name" without index
                    if (parts.Length >= 1)
                    {
                        // Check if entire string is just a name (no number prefix)
                        string trimmed = inner.Trim();
                        if (!string.IsNullOrEmpty(trimmed) && !char.IsDigit(trimmed[0]))
                        {
                            // Remove any trailing characters like *
                            trimmed = trimmed.TrimEnd('*', ' ');
                            return trimmed;
                        }
                    }
                }
            }
        }
        return null;
    }

    private static void Execute(
        SourceProductionContext context,
        ImmutableArray<LangPackInfo?> packs)
    {
        var langPacks = new List<LangPackInfo>();
        foreach (var p in packs)
        {
            if (p != null) langPacks.Add(p);
        }

        if (langPacks.Count == 0) return;

        // Find base language
        LangPackInfo? baseLang = null;
        var otherLangs = new List<LangPackInfo>();
        foreach (var pack in langPacks)
        {
            if (pack.IsBase)
            {
                if (baseLang != null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.MultipleBaseLangs,
                        pack.Location));
                    return;
                }
                baseLang = pack;
            }
            else
            {
                otherLangs.Add(pack);
            }
        }

        if (baseLang == null)
        {
            // No base language found
            return;
        }

        // Validate base language: no elements missing comments (I18N006)
        foreach (var cat in baseLang.Categories)
        {
            for (int i = 0; i < cat.Elements.Count; i++)
            {
                if (cat.Elements[i].CommentName == null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.MissingCommentName,
                        baseLang.Location,
                        i, cat.Name));
                }
            }
        }

        // Validate other languages
        foreach (var other in otherLangs)
        {
            // I18N002: Check category fields match
            foreach (var baseCat in baseLang.Categories)
            {
                bool found = false;
                foreach (var otherCat in other.Categories)
                {
                    if (otherCat.Name == baseCat.Name)
                    {
                        found = true;
                        // I18N001: Check array length
                        if (otherCat.ElementCount != baseCat.ElementCount && otherCat.ElementCount > 0 && baseCat.ElementCount > 0)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                DiagnosticDescriptors.ArrayLengthMismatch,
                                other.Location,
                                other.ClassName, baseCat.Name, otherCat.ElementCount, baseCat.ElementCount));
                        }
                        break;
                    }
                }
                if (!found)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.MissingCategory,
                        other.Location,
                        other.ClassName, baseCat.Name, baseLang.ClassName));
                }
            }
        }

        // Generate K constant class
        GenerateKClass(context, baseLang);

        // Generate I18nExtensions
        GenerateExtensions(context, baseLang, langPacks);

        // Generate I18n.Register()
        GenerateRegister(context, langPacks);
    }

    private static void GenerateKClass(SourceProductionContext context, LangPackInfo baseLang)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("namespace YTStdI18n;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// 国际化键常量类。");
        sb.AppendLine("/// 每个分类下的常量值为该翻译在对应数组中的索引位置。");
        sb.AppendLine($"/// 由 Source Generator 根据基准语言 {baseLang.ClassName} 自动生成。");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public static class K");
        sb.AppendLine("{");

        foreach (var category in baseLang.Categories)
        {
            sb.AppendLine($"    /// <summary>{category.Name} 分类常量。</summary>");
            sb.AppendLine($"    public static class {category.Name}");
            sb.AppendLine("    {");

            foreach (var element in category.Elements)
            {
                if (element.CommentName != null)
                {
                    sb.AppendLine($"        public const int {element.CommentName} = {element.Index};");
                }
            }

            sb.AppendLine("    }");
            sb.AppendLine();
        }

        sb.AppendLine("}");

        context.AddSource("K.g.cs", sb.ToString());
    }

    private static void GenerateExtensions(
        SourceProductionContext context,
        LangPackInfo baseLang,
        List<LangPackInfo> allPacks)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using System.Runtime.CompilerServices;");
        sb.AppendLine();
        sb.AppendLine("namespace YTStdI18n;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// 由 Source Generator 自动生成的翻译扩展方法。");
        sb.AppendLine("/// 每个分类对应一组扩展方法（默认语言、指定语言、租户语言）。");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public static class I18nExtensions");
        sb.AppendLine("{");

        foreach (var category in baseLang.Categories)
        {
            string catName = category.Name;

            // 1. Extension: .Category() - uses DefaultLang
            sb.AppendLine($"    /// <summary>获取 {catName} 分类的翻译（使用全局默认语言）。</summary>");
            sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine($"    public static string {catName}(this int index)");
            sb.AppendLine("    {");
            sb.AppendLine("        return I18nCore.DefaultLang switch");
            sb.AppendLine("        {");

            foreach (var pack in allPacks)
            {
                if (pack.IsBase) continue;
                string langEnumName = ClassNameToLangEnum(pack.ClassName);
                string fullClassName = pack.Namespace != null ? $"{pack.Namespace}.{pack.ClassName}" : pack.ClassName;
                sb.AppendLine($"            Lang.{langEnumName} => {fullClassName}.{catName}?[index] ?? {GetBaseRef(baseLang)}.{catName}![index],");
            }

            sb.AppendLine($"            _ => {GetBaseRef(baseLang)}.{catName}![index],");
            sb.AppendLine("        };");
            sb.AppendLine("    }");
            sb.AppendLine();

            // 2. Extension: .Category(Lang lang) - specified language
            sb.AppendLine($"    /// <summary>获取 {catName} 分类的翻译（指定语言）。</summary>");
            sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine($"    public static string {catName}(this int index, Lang lang)");
            sb.AppendLine("    {");
            sb.AppendLine("        return lang switch");
            sb.AppendLine("        {");

            foreach (var pack in allPacks)
            {
                if (pack.IsBase) continue;
                string langEnumName = ClassNameToLangEnum(pack.ClassName);
                string fullClassName = pack.Namespace != null ? $"{pack.Namespace}.{pack.ClassName}" : pack.ClassName;
                sb.AppendLine($"            Lang.{langEnumName} => {fullClassName}.{catName}?[index] ?? {GetBaseRef(baseLang)}.{catName}![index],");
            }

            sb.AppendLine($"            _ => {GetBaseRef(baseLang)}.{catName}![index],");
            sb.AppendLine("        };");
            sb.AppendLine("    }");
            sb.AppendLine();

            // 3. Extension: .Category(int tenantId) - tenant language
            sb.AppendLine($"    /// <summary>获取 {catName} 分类的翻译（使用指定租户的语言偏好）。</summary>");
            sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine($"    public static string {catName}(this int index, int tenantId)");
            sb.AppendLine("    {");
            sb.AppendLine($"        return index.{catName}(I18nCore.GetTenantLang(tenantId));");
            sb.AppendLine("    }");
            sb.AppendLine();
        }

        sb.AppendLine("}");

        context.AddSource("I18nExtensions.g.cs", sb.ToString());
    }

    private static void GenerateRegister(
        SourceProductionContext context,
        List<LangPackInfo> allPacks)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using YTStdLogger.Core;");
        sb.AppendLine();
        sb.AppendLine("namespace YTStdI18n;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// 国际化全局入口（由 Source Generator 自动生成）。");
        sb.AppendLine("/// 包装 I18nCore 并添加 Register() 方法。");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public static class I18n");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>获取或设置全局默认语言。线程安全（volatile 读写）。</summary>");
        sb.AppendLine("    public static Lang DefaultLang { get => I18nCore.DefaultLang; set => I18nCore.DefaultLang = value; }");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>初始化国际化系统。应用启动时调用一次。</summary>");
        sb.AppendLine("    public static void Init(Lang defaultLang = Lang.ZhCn) => I18nCore.Init(defaultLang);");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>设置指定租户的语言偏好。</summary>");
        sb.AppendLine("    public static void SetTenantLang(int tenantId, Lang lang) => I18nCore.SetTenantLang(tenantId, lang);");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>获取指定租户的语言偏好，未设置则返回全局默认语言。</summary>");
        sb.AppendLine("    public static Lang GetTenantLang(int tenantId) => I18nCore.GetTenantLang(tenantId);");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// 注册所有语言包。内部调用各语言类的 Register() 方法。");
        sb.AppendLine("    /// 由 Source Generator 自动生成。");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static void Register()");
        sb.AppendLine("    {");

        foreach (var pack in allPacks)
        {
            string fullName = pack.Namespace != null ? $"{pack.Namespace}.{pack.ClassName}" : pack.ClassName;
            sb.AppendLine($"        {fullName}.Register();");
        }

        sb.AppendLine("        Logger.Info(0, 0L, \"[I18n] 语言包注册完成\");");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource("I18n.Register.g.cs", sb.ToString());
    }

    private static string GetBaseRef(LangPackInfo baseLang)
    {
        return baseLang.Namespace != null
            ? $"{baseLang.Namespace}.{baseLang.ClassName}"
            : baseLang.ClassName;
    }

    /// <summary>
    /// Convert class name like "LangEn" to enum name "En", "LangZhCn" to "ZhCn", etc.
    /// </summary>
    private static string ClassNameToLangEnum(string className)
    {
        if (className.StartsWith("Lang") && className.Length > 4)
        {
            return className.Substring(4);
        }
        return className;
    }

    // Internal data models
    private class LangPackInfo
    {
        public string ClassName { get; set; } = "";
        public string? Namespace { get; set; }
        public bool IsBase { get; set; }
        public List<CategoryInfo> Categories { get; set; } = new List<CategoryInfo>();
        public Location? Location { get; set; }
    }

    private class CategoryInfo
    {
        public string Name { get; set; } = "";
        public List<ElementInfo> Elements { get; set; } = new List<ElementInfo>();
        public int ElementCount { get; set; }
    }

    private class ElementInfo
    {
        public int Index { get; set; }
        public string? CommentName { get; set; }
    }
}
