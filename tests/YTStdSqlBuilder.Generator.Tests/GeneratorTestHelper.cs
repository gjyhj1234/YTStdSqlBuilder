using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using YTStdSqlBuilder.Generator;

namespace YTStdSqlBuilder.Generator.Tests;

internal static class GeneratorTestHelper
{
    public static CSharpGeneratorDriver CreateDriver()
    {
        var generator = new PgSqlTemplateGenerator();
        return (CSharpGeneratorDriver)CSharpGeneratorDriver.Create(generator);
    }

    public static CSharpCompilation CreateCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Runtime.dll")),
            MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Collections.dll")),
            MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "netstandard.dll")),
            MetadataReference.CreateFromFile(typeof(PgSql).Assembly.Location),
        };

        // Add Npgsql reference (needed for PgSqlParam)
        var npgsqlAssembly = typeof(NpgsqlTypes.NpgsqlDbType).Assembly;
        references.Add(MetadataReference.CreateFromFile(npgsqlAssembly.Location));

        return CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    public static (GeneratorDriver Driver, Compilation OutputCompilation, ImmutableArray<Diagnostic> Diagnostics)
        RunGenerator(string source)
    {
        var compilation = CreateCompilation(source);
        var driver = CreateDriver();
        var updatedDriver = driver.RunGeneratorsAndUpdateCompilation(
            compilation, out var outputCompilation, out var diagnostics);
        return (updatedDriver, outputCompilation, diagnostics);
    }

    public static ImmutableArray<Diagnostic> GetErrors(ImmutableArray<Diagnostic> diagnostics)
    {
        return diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToImmutableArray();
    }

    public static ImmutableArray<Diagnostic> GetCompilationErrors(Compilation compilation)
    {
        return compilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToImmutableArray();
    }

    public static string? GetGeneratedSource(GeneratorDriver driver, string hintNameContains)
    {
        var result = driver.GetRunResult();
        foreach (var generatorResult in result.Results)
        {
            foreach (var source in generatorResult.GeneratedSources)
            {
                if (source.HintName.Contains(hintNameContains))
                    return source.SourceText.ToString();
            }
        }
        return null;
    }

    public const string SimpleSelectSource = @"
using YTStdSqlBuilder;
using YTStdSqlBuilder.Conditions;
using YTStdSqlBuilder.Expressions;

[PgSqlTemplate]
public static partial class TestQueries
{
    [PgSqlQuery]
    public static partial PgSqlRenderResult GetUserById(int userId);

    private static void Define_GetUserById(PgSqlTemplateBuilder b)
    {
        var user = b.Table(""users"", ""u"");
        b.Select(
            user.Col<int>(""id""),
            user.Col<string>(""name""))
         .From(user)
         .Where(user.Col(""id""), Op.Eq, b.Param<int>(""userId""));
    }
}";

    public const string MultiColumnSource = @"
using YTStdSqlBuilder;
using YTStdSqlBuilder.Conditions;
using YTStdSqlBuilder.Expressions;

[PgSqlTemplate]
public static partial class TestQueries
{
    [PgSqlQuery]
    public static partial PgSqlRenderResult GetUserDetails(int userId);

    private static void Define_GetUserDetails(PgSqlTemplateBuilder b)
    {
        var user = b.Table(""users"", ""u"");
        b.Select(
            user.Col<int>(""id""),
            user.Col<string>(""name""),
            user.Col<string>(""email""),
            user.Col<bool>(""is_active""))
         .From(user)
         .Where(user.Col(""id""), Op.Eq, b.Param<int>(""userId""));
    }
}";

    public const string DynamicWhereIfSource = @"
using YTStdSqlBuilder;
using YTStdSqlBuilder.Conditions;
using YTStdSqlBuilder.Expressions;

[PgSqlTemplate]
public static partial class TestQueries
{
    [PgSqlQuery]
    public static partial PgSqlRenderResult SearchUsers(bool name_condition, string name, bool minAge_condition, int minAge);

    private static void Define_SearchUsers(PgSqlTemplateBuilder b)
    {
        var user = b.Table(""users"", ""u"");
        b.Select(
            user.Col<int>(""id""),
            user.Col<string>(""name""))
         .From(user)
         .WhereIf(user.Col(""name""), Op.ILike, b.Param<string>(""name""))
         .AndIf(user.Col(""age""), Op.Gte, b.Param<int>(""minAge""));
    }
}";

    public const string NoDefineMethodSource = @"
using YTStdSqlBuilder;
using YTStdSqlBuilder.Conditions;
using YTStdSqlBuilder.Expressions;

[PgSqlTemplate]
public static partial class TestQueries
{
    [PgSqlQuery]
    public static partial PgSqlRenderResult GetUserById(int userId);
}";

    public const string NoParametersSource = @"
using YTStdSqlBuilder;
using YTStdSqlBuilder.Conditions;
using YTStdSqlBuilder.Expressions;

[PgSqlTemplate]
public static partial class TestQueries
{
    [PgSqlQuery]
    public static partial PgSqlRenderResult GetAllUsers();

    private static void Define_GetAllUsers(PgSqlTemplateBuilder b)
    {
        var user = b.Table(""users"", ""u"");
        b.Select(
            user.Col<int>(""id""),
            user.Col<string>(""name""))
         .From(user);
    }
}";

    public const string NonGenericParamSource = @"
using YTStdSqlBuilder;
using YTStdSqlBuilder.Conditions;
using YTStdSqlBuilder.Expressions;

[PgSqlTemplate]
public static partial class TestQueries
{
    [PgSqlQuery]
    public static partial PgSqlRenderResult GetUserById(int userId);

    private static void Define_GetUserById(PgSqlTemplateBuilder b)
    {
        var user = b.Table(""users"", ""u"");
        b.Select(
            user.Col(""id""),
            user.Col(""name""))
         .From(user)
         .Where(user.Col(""id""), Op.Eq, b.Param(""userId""));
    }
}";
}
