using Xunit;

namespace YTStdEntity.Generator.Tests;

public class EntityGeneratorTests
{
    [Fact]
    public void Generator_CanBeInstantiated()
    {
        var generator = new EntityGenerator();
        Assert.NotNull(generator);
    }
}
