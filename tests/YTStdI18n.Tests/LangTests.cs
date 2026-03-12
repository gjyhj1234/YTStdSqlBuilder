using Xunit;
using YTStdI18n;

namespace YTStdI18n.Tests;

public class LangTests
{
    [Fact]
    public void LangEnum_ZhCn_IsDefault()
    {
        Assert.Equal((byte)0, (byte)Lang.ZhCn);
    }

    [Fact]
    public void LangEnum_En_HasCorrectValue()
    {
        Assert.Equal((byte)1, (byte)Lang.En);
    }

    [Fact]
    public void LangEnum_Ja_HasCorrectValue()
    {
        Assert.Equal((byte)2, (byte)Lang.Ja);
    }

    [Fact]
    public void LangEnum_ZhTw_HasCorrectValue()
    {
        Assert.Equal((byte)3, (byte)Lang.ZhTw);
    }
}
