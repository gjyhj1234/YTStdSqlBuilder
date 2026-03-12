using Xunit;
using YTStdI18n;

namespace YTStdI18n.Tests;

public class I18nCoreTests
{
    [Fact]
    public void Init_SetsDefaultLang()
    {
        I18nCore.Init(Lang.En);
        Assert.Equal(Lang.En, I18nCore.DefaultLang);

        // Reset to default
        I18nCore.Init(Lang.ZhCn);
    }

    [Fact]
    public void DefaultLang_CanBeSetAndGet()
    {
        I18nCore.Init(Lang.ZhCn);
        Assert.Equal(Lang.ZhCn, I18nCore.DefaultLang);

        I18nCore.DefaultLang = Lang.En;
        Assert.Equal(Lang.En, I18nCore.DefaultLang);

        // Reset
        I18nCore.DefaultLang = Lang.ZhCn;
    }

    [Fact]
    public void SetTenantLang_SetsAndGetsTenantLanguage()
    {
        I18nCore.Init(Lang.ZhCn);
        I18nCore.SetTenantLang(1001, Lang.En);

        Assert.Equal(Lang.En, I18nCore.GetTenantLang(1001));
    }

    [Fact]
    public void GetTenantLang_UnsetTenant_ReturnsDefaultLang()
    {
        I18nCore.Init(Lang.ZhCn);

        // Use a unique tenant ID that won't conflict
        Assert.Equal(Lang.ZhCn, I18nCore.GetTenantLang(99999));
    }
}
