using YTStdI18n;

namespace YTStdI18n.Sample;

/// <summary>
/// 简体中文语言包（基准语言）。
/// 所有分类的数组索引顺序由本类决定，其他语言包必须严格一致。
/// </summary>
[I18nResource(IsBase = true)]
public static class LangZhCn
{
    /// <summary>通用分类翻译数组。初始为 null，由 Register() 延迟初始化。</summary>
    public static string[]? Common;

    /// <summary>用户模块翻译数组。</summary>
    public static string[]? User;

    /// <summary>数据库模块翻译数组。</summary>
    public static string[]? Db;

    /// <summary>
    /// 注册语言资源。应用启动时调用。
    /// 数组中的元素顺序即为索引顺序，只能在末尾追加，严禁中间插入或删除。
    /// </summary>
    public static void Register()
    {
        Common = new string[]
        {
            /* Success         */ "操作成功",
            /* Failed          */ "操作失败",
            /* NotFound        */ "未找到数据",
            /* Unauthorized    */ "未授权访问",
            /* Forbidden       */ "禁止访问",
            /* ValidationError */ "数据验证失败",
        };

        User = new string[]
        {
            /* LoginSuccess    */ "登录成功",
            /* LoginFailed     */ "用户名或密码错误",
            /* AccountLocked   */ "账户已被锁定",
            /* PasswordTooShort*/ "密码长度不能少于 8 位",
        };

        Db = new string[]
        {
            /* ConnectionFailed */ "数据库连接失败",
            /* QueryTimeout     */ "查询超时",
            /* InsertFailed     */ "新增数据失败",
            /* UpdateFailed     */ "更新数据失败",
            /* DeleteFailed     */ "删除数据失败",
        };
    }
}

/// <summary>
/// 英语语言包。
/// 数组索引顺序必须与 LangZhCn 严格一致。缺少翻译填 null。
/// </summary>
[I18nResource]
public static class LangEn
{
    public static string[]? Common;
    public static string[]? User;
    public static string[]? Db;

    public static void Register()
    {
        Common = new string[]
        {
            /* Success         */ "Operation successful",
            /* Failed          */ "Operation failed",
            /* NotFound        */ "Data not found",
            /* Unauthorized    */ "Unauthorized access",
            /* Forbidden       */ "Access forbidden",
            /* ValidationError */ "Validation error",
        };

        User = new string[]
        {
            /* LoginSuccess    */ "Login successful",
            /* LoginFailed     */ "Invalid username or password",
            /* AccountLocked   */ "Account has been locked",
            /* PasswordTooShort*/ "Password must be at least 8 characters",
        };

        Db = new string[]
        {
            /* ConnectionFailed */ "Database connection failed",
            /* QueryTimeout     */ "Query timeout",
            /* InsertFailed     */ "Insert data failed",
            /* UpdateFailed     */ "Update data failed",
            /* DeleteFailed     */ "Delete data failed",
        };
    }
}
