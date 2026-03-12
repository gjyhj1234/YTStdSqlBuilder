using Xunit;
using YTStdI18n;

namespace YTStdI18n.Tests;

public class I18nResourceAttributeTests
{
    [Fact]
    public void IsBase_DefaultIsFalse()
    {
        var attr = new I18nResourceAttribute();
        Assert.False(attr.IsBase);
    }

    [Fact]
    public void IsBase_CanBeSetToTrue()
    {
        var attr = new I18nResourceAttribute { IsBase = true };
        Assert.True(attr.IsBase);
    }
}
