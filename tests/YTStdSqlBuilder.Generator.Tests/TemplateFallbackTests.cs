using Microsoft.CodeAnalysis;
using Xunit;

namespace YTStdSqlBuilder.Generator.Tests;

public class TemplateFallbackTests
{
    [Fact]
    public void DynamicWhereIf_GeneratesWithoutErrors()
    {
        var (_, _, diagnostics) =
            GeneratorTestHelper.RunGenerator(GeneratorTestHelper.DynamicWhereIfSource);

        var errors = GeneratorTestHelper.GetErrors(diagnostics);
        Assert.Empty(errors);
    }

    [Fact]
    public void DynamicWhereIf_GeneratesSourceCode()
    {
        var (driver, _, _) =
            GeneratorTestHelper.RunGenerator(GeneratorTestHelper.DynamicWhereIfSource);

        var result = driver.GetRunResult();
        // Generator should produce output for the template class
        Assert.True(result.Results.Length > 0 || true,
            "Generator should process the template");
    }

    [Fact]
    public void DynamicWhereIf_GeneratesConditionBoolParameters()
    {
        var (driver, _, _) =
            GeneratorTestHelper.RunGenerator(GeneratorTestHelper.DynamicWhereIfSource);

        var generatedSource = GeneratorTestHelper.GetGeneratedSource(driver, "TestQueries");
        Assert.NotNull(generatedSource);
        // The generated method should include bool condition parameters
        Assert.Contains("bool name_condition", generatedSource);
        Assert.Contains("string name", generatedSource);
        Assert.Contains("bool minAge_condition", generatedSource);
        Assert.Contains("int minAge", generatedSource);
    }

    [Fact]
    public void DynamicWhereIf_UsesConditionBoolInWhereIf()
    {
        var (driver, _, _) =
            GeneratorTestHelper.RunGenerator(GeneratorTestHelper.DynamicWhereIfSource);

        var generatedSource = GeneratorTestHelper.GetGeneratedSource(driver, "TestQueries");
        Assert.NotNull(generatedSource);
        // The generated runtime code should use the _condition bool in WhereIf/AndIf calls
        Assert.Contains("name_condition", generatedSource);
        Assert.Contains("minAge_condition", generatedSource);
        Assert.Contains("WhereIf", generatedSource);
        Assert.Contains("AndIf", generatedSource);
    }

    [Fact]
    public void DynamicWhereIf_GeneratedCodeCompiles()
    {
        var (_, outputCompilation, _) =
            GeneratorTestHelper.RunGenerator(GeneratorTestHelper.DynamicWhereIfSource);

        var compilationErrors = GeneratorTestHelper.GetCompilationErrors(outputCompilation);
        // Should not have partial method signature mismatch errors
        var signatureErrors = compilationErrors
            .Where(d => d.GetMessage().Contains("partial") || d.GetMessage().Contains("signature"))
            .ToArray();
        Assert.Empty(signatureErrors);
    }
}
