using Xunit;
using YTStdI18n;
using YTStdI18n.Sample;

namespace YTStdI18n.Sample;

public class I18nSampleTests
{
    public I18nSampleTests()
    {
        // Initialize for each test
        I18n.Init(Lang.ZhCn);
        I18n.Register();
    }

    [Fact]
    public void KConstants_HaveCorrectValues()
    {
        // Verify generated K constants
        Assert.Equal(0, K.Common.Success);
        Assert.Equal(1, K.Common.Failed);
        Assert.Equal(2, K.Common.NotFound);
        Assert.Equal(3, K.Common.Unauthorized);
        Assert.Equal(4, K.Common.Forbidden);
        Assert.Equal(5, K.Common.ValidationError);

        Assert.Equal(0, K.User.LoginSuccess);
        Assert.Equal(1, K.User.LoginFailed);
        Assert.Equal(2, K.User.AccountLocked);
        Assert.Equal(3, K.User.PasswordTooShort);

        Assert.Equal(0, K.Db.ConnectionFailed);
        Assert.Equal(1, K.Db.QueryTimeout);
        Assert.Equal(2, K.Db.InsertFailed);
        Assert.Equal(3, K.Db.UpdateFailed);
        Assert.Equal(4, K.Db.DeleteFailed);
    }

    [Fact]
    public void Common_ZhCn_ReturnsChineseTranslation()
    {
        I18n.DefaultLang = Lang.ZhCn;
        Assert.Equal("操作成功", K.Common.Success.Common());
        Assert.Equal("操作失败", K.Common.Failed.Common());
        Assert.Equal("未找到数据", K.Common.NotFound.Common());
    }

    [Fact]
    public void Common_En_ReturnsEnglishTranslation()
    {
        I18n.DefaultLang = Lang.En;
        Assert.Equal("Operation successful", K.Common.Success.Common());
        Assert.Equal("Operation failed", K.Common.Failed.Common());
        Assert.Equal("Data not found", K.Common.NotFound.Common());
    }

    [Fact]
    public void Common_WithLangParam_ReturnsCorrectTranslation()
    {
        Assert.Equal("操作成功", K.Common.Success.Common(Lang.ZhCn));
        Assert.Equal("Operation successful", K.Common.Success.Common(Lang.En));
    }

    [Fact]
    public void Common_WithTenantId_UsesTenantLang()
    {
        I18n.SetTenantLang(1001, Lang.En);
        Assert.Equal("Operation successful", K.Common.Success.Common(1001));
    }

    [Fact]
    public void User_ZhCn_ReturnsChineseTranslation()
    {
        I18n.DefaultLang = Lang.ZhCn;
        Assert.Equal("登录成功", K.User.LoginSuccess.User());
        Assert.Equal("用户名或密码错误", K.User.LoginFailed.User());
    }

    [Fact]
    public void User_En_ReturnsEnglishTranslation()
    {
        I18n.DefaultLang = Lang.En;
        Assert.Equal("Login successful", K.User.LoginSuccess.User());
        Assert.Equal("Invalid username or password", K.User.LoginFailed.User());
    }

    [Fact]
    public void Db_ZhCn_ReturnsChineseTranslation()
    {
        I18n.DefaultLang = Lang.ZhCn;
        Assert.Equal("数据库连接失败", K.Db.ConnectionFailed.Db());
        Assert.Equal("新增数据失败", K.Db.InsertFailed.Db());
    }

    [Fact]
    public void Db_En_ReturnsEnglishTranslation()
    {
        I18n.DefaultLang = Lang.En;
        Assert.Equal("Database connection failed", K.Db.ConnectionFailed.Db());
        Assert.Equal("Insert data failed", K.Db.InsertFailed.Db());
    }

    [Fact]
    public void LanguageSwitching_WorksAtRuntime()
    {
        I18n.DefaultLang = Lang.ZhCn;
        Assert.Equal("操作成功", K.Common.Success.Common());

        I18n.DefaultLang = Lang.En;
        Assert.Equal("Operation successful", K.Common.Success.Common());

        // Switch back
        I18n.DefaultLang = Lang.ZhCn;
        Assert.Equal("操作成功", K.Common.Success.Common());
    }

    [Fact]
    public void TenantLang_IndependentFromGlobal()
    {
        I18n.DefaultLang = Lang.ZhCn;
        I18n.SetTenantLang(2001, Lang.En);

        // Global uses Chinese
        Assert.Equal("操作成功", K.Common.Success.Common());
        // Tenant 2001 uses English
        Assert.Equal("Operation successful", K.Common.Success.Common(2001));
    }

    [Fact]
    public void StringInterpolation_WorksWithTranslation()
    {
        I18n.DefaultLang = Lang.ZhCn;
        string userName = "张三";
        string msg = $"{K.User.LoginSuccess.User()}，{userName}";
        Assert.Equal("登录成功，张三", msg);
    }
}
