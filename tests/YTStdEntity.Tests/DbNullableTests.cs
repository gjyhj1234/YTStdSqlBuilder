using Xunit;
using YTStdEntity;

namespace YTStdEntity.Tests;

public class DbNullableTests
{
    [Fact]
    public void Unset_IsSetIsFalse()
    {
        var value = DbNullable<int>.Unset;
        Assert.False(value.IsSet);
    }

    [Fact]
    public void NullValue_IsSetIsTrue_ValueIsNull()
    {
        var value = DbNullable<string>.NullValue;
        Assert.True(value.IsSet);
        Assert.Null(value.Value);
    }

    [Fact]
    public void Constructor_WithValue_IsSetIsTrue()
    {
        var value = new DbNullable<int>(42);
        Assert.True(value.IsSet);
        Assert.Equal(42, value.Value);
    }

    [Fact]
    public void ImplicitConversion_SetsValue()
    {
        DbNullable<int> value = 99;
        Assert.True(value.IsSet);
        Assert.Equal(99, value.Value);
    }

    [Fact]
    public void ImplicitConversion_Null_SetsNull()
    {
        DbNullable<string> value = (string?)null;
        Assert.True(value.IsSet);
        Assert.Null(value.Value);
    }

    [Fact]
    public void ToString_Unset_ReturnsUnset()
    {
        var value = DbNullable<int>.Unset;
        Assert.Equal("[Unset]", value.ToString());
    }

    [Fact]
    public void ToString_SetNull_ReturnsSetNull()
    {
        var value = DbNullable<string>.NullValue;
        Assert.Equal("[Set: null]", value.ToString());
    }

    [Fact]
    public void ToString_SetValue_ReturnsSetValue()
    {
        var value = new DbNullable<int>(42);
        Assert.Equal("[Set: 42]", value.ToString());
    }
}
