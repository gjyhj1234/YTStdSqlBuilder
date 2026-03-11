# 面向 Claude Opus 4.6 的国际化语言支持（i18n）生产级实现提示词
## 高性能、低 GC、AOT 友好的多语言资源管理系统

你是一名**顶级 .NET 基础架构工程师 / 高性能框架作者 / NativeAOT 专家**。
请实现一个**仅用于 .NET 10.0 + NativeAOT** 环境下的**极致轻量、高性能、低内存、零反射、零动态、零 LINQ 依赖**的国际化语言支持组件。

这不是 demo，不是玩具组件，而是**框架基座级实现**。
请严格按"**生产级顶级标准**"完成完整代码、测试与设计说明。
严禁需求漂移，严禁引入未要求的复杂依赖。

---

## 依赖项目参考

> **重要**：本模块依赖以下已有项目。完整的类型定义、API 签名和使用模式，
> 请查阅 [已有项目参考文档](../../docs/existing-projects-reference.md)。
>
> AI 在实现本模块时，必须遵循参考文档中定义的类型签名，不得自行定义重复类型。

### 本模块使用的已有类型

| 类型 | 来源项目 | 用途 |
|------|---------|------|
| `Logger` | YTStdLogger | 日志记录（`Logger.Debug`, `Logger.Error`, `Logger.Info`） |

---

## 强约束（必须全部满足）

1. **运行环境**
   - 目标框架：`.NET 10.0`
   - 发布方式：`NativeAOT`
   - 必须完全兼容 NativeAOT
   - 支持：`Windows`、`Linux`、`macOS`

2. **禁止项（硬性）**
   - 禁止 `System.Linq`
   - 禁止反射（`System.Reflection`）
   - 禁止 `dynamic`
   - 禁止序列化框架（JSON/XML/二进制序列化等）
   - **禁止 `string.Format`** — 必须使用 **`string interpolation`**（`$"..."`）以获得最优性能
   - 禁止 `ResourceManager`（基于反射，NativeAOT 不友好）
   - 禁止 `.resx` 文件（运行时解析依赖反射）

3. **性能目标**
   - 翻译查找 O(1)：使用 `Dictionary<string, string>` 或更高效结构
   - 零分配翻译路径：热路径上不产生额外堆分配
   - 支持即时切换语言（如果性能与内存影响不大）
   - 集中管理所有语言资源，便于维护

---

## 1. 最终目标

实现一套集中管理、高性能的多语言支持系统，具备以下能力：

1. **集中式语言资源管理** — 所有语言文本定义在一个中心位置，便于维护
2. **编译期常量键** — 通过 Source Generator 自动生成强类型键常量，消除 magic string
3. **运行时即时切换** — 支持运行时切换当前语言，无需重启
4. **租户级语言偏好** — 不同租户可使用不同语言
5. **带参数的翻译** — 支持翻译文本中嵌入动态值（使用 string interpolation）
6. **回退策略** — 找不到翻译时回退到默认语言（如中文），再找不到返回键名
7. **AOT 友好** — 完全兼容 NativeAOT，无反射依赖

---

## 2. 技术约束

- 目标框架：net10.0
- AOT 兼容：是
- 依赖项目：YTStdLogger
- NuGet 依赖：无（纯 C# 实现）

---

## 3. 项目结构

| 项目 | 路径 | 说明 |
|------|------|------|
| 主项目 | `src/YTStdI18n/` | 运行时国际化库 |
| 源生成器 | `src/YTStdI18n.Generator/` | 编译期生成强类型键 |
| 测试 | `tests/YTStdI18n.Tests/` | 单元测试 |
| 示例 | `samples/YTStdI18n.Sample/` | 使用示例 |

---

## 4. 核心设计

### 4.1 语言枚举

```csharp
/// <summary>
/// 支持的语言枚举。
/// 可按需扩展，新增语言只需添加枚举值 + 对应翻译。
/// </summary>
public enum Lang : byte
{
    /// <summary>简体中文（默认）</summary>
    ZhCn = 0,
    /// <summary>英语</summary>
    En = 1,
    /// <summary>日语</summary>
    Ja = 2,
    /// <summary>繁体中文</summary>
    ZhTw = 3,
}
```

### 4.2 全局静态入口 — `I18n` 静态类

```csharp
public static class I18n
{
    /// <summary>初始化国际化系统。应用启动时调用一次。</summary>
    public static void Init(Lang defaultLang = Lang.ZhCn);

    /// <summary>注册语言资源包。</summary>
    public static void Register(Lang lang, IReadOnlyDictionary<string, string> resources);

    /// <summary>获取或设置全局默认语言。线程安全。</summary>
    public static Lang DefaultLang { get; set; }

    /// <summary>设置指定租户的语言偏好。</summary>
    public static void SetTenantLang(int tenantId, Lang lang);

    /// <summary>获取指定租户的语言偏好，未设置则返回全局默认语言。</summary>
    public static Lang GetTenantLang(int tenantId);

    /// <summary>根据键获取翻译文本（使用全局默认语言）。</summary>
    public static string T(string key);

    /// <summary>根据键获取指定语言的翻译文本。</summary>
    public static string T(string key, Lang lang);

    /// <summary>根据键获取指定租户语言偏好的翻译文本。</summary>
    public static string T(int tenantId, string key);
}
```

### 4.3 语言资源定义方式

语言资源使用纯 C# 静态字典定义，**不使用 .resx 文件**：

```csharp
[I18nResource]
public static class LangZhCn
{
    public static readonly Dictionary<string, string> Resources = new()
    {
        // === 通用 ===
        ["common.success"] = "操作成功",
        ["common.failed"] = "操作失败",
        ["common.not_found"] = "未找到数据",
        ["common.unauthorized"] = "未授权访问",
        ["common.forbidden"] = "禁止访问",
        ["common.validation_error"] = "数据验证失败",

        // === 用户模块 ===
        ["user.login_success"] = "登录成功",
        ["user.login_failed"] = "用户名或密码错误",
        ["user.account_locked"] = "账户已被锁定",
        ["user.password_too_short"] = "密码长度不能少于 8 位",

        // === 数据库 ===
        ["db.connection_failed"] = "数据库连接失败",
        ["db.query_timeout"] = "查询超时",
        ["db.insert_failed"] = "新增数据失败",
        ["db.update_failed"] = "更新数据失败",
        ["db.delete_failed"] = "删除数据失败",
    };
}

[I18nResource]
public static class LangEn
{
    public static readonly Dictionary<string, string> Resources = new()
    {
        ["common.success"] = "Operation successful",
        ["common.failed"] = "Operation failed",
        ["common.not_found"] = "Data not found",
        ["common.unauthorized"] = "Unauthorized access",
        ["common.forbidden"] = "Access forbidden",
        ["common.validation_error"] = "Validation error",

        ["user.login_success"] = "Login successful",
        ["user.login_failed"] = "Invalid username or password",
        ["user.account_locked"] = "Account has been locked",
        ["user.password_too_short"] = "Password must be at least 8 characters",

        ["db.connection_failed"] = "Database connection failed",
        ["db.query_timeout"] = "Query timeout",
        ["db.insert_failed"] = "Insert data failed",
        ["db.update_failed"] = "Update data failed",
        ["db.delete_failed"] = "Delete data failed",
    };
}
```

### 4.4 Source Generator — 强类型键生成

Source Generator 扫描语言包字典中的所有键，自动生成强类型常量类：

```csharp
// === 由 Source Generator 自动生成 ===
public static class K
{
    public static class Common
    {
        public const string Success = "common.success";
        public const string Failed = "common.failed";
        public const string NotFound = "common.not_found";
        public const string Unauthorized = "common.unauthorized";
        public const string Forbidden = "common.forbidden";
        public const string ValidationError = "common.validation_error";
    }

    public static class User
    {
        public const string LoginSuccess = "user.login_success";
        public const string LoginFailed = "user.login_failed";
        public const string AccountLocked = "user.account_locked";
        public const string PasswordTooShort = "user.password_too_short";
    }

    public static class Db
    {
        public const string ConnectionFailed = "db.connection_failed";
        public const string QueryTimeout = "db.query_timeout";
        public const string InsertFailed = "db.insert_failed";
        public const string UpdateFailed = "db.update_failed";
        public const string DeleteFailed = "db.delete_failed";
    }
}
```

---

## 5. 使用示例

### 5.1 初始化

```csharp
I18n.Init(Lang.ZhCn);
I18n.Register(Lang.ZhCn, LangZhCn.Resources);
I18n.Register(Lang.En, LangEn.Resources);
```

### 5.2 基本翻译

```csharp
// 使用全局默认语言
string msg = I18n.T(K.Common.Success);   // "操作成功"

// 指定语言
string msgEn = I18n.T(K.Common.Success, Lang.En);   // "Operation successful"

// 使用租户偏好语言
I18n.SetTenantLang(1001, Lang.En);
string msgTenant = I18n.T(1001, K.Common.Success);   // "Operation successful"
```

### 5.3 带参数的翻译（使用 string interpolation，**禁止 string.Format**）

```csharp
// ✅ 正确：使用 string interpolation
string userName = "张三";
int errorCount = 5;
string msg = $"{I18n.T(K.User.LoginSuccess)}，{userName}";
// => "登录成功，张三"

// ✅ 正确：复杂插值
string detailMsg = $"{I18n.T(K.Common.ValidationError)}: 共 {errorCount} 个错误";
// => "数据验证失败: 共 5 个错误"

// ❌ 错误：禁止使用 string.Format
// string msg = string.Format(I18n.T(key), userName);  // 禁止！
```

### 5.4 在 ADO / 数据库操作中的集成

```csharp
public static async Task<DbInsResult> InsertUserAsync(int tenantId, long userId, string name, int age)
{
    Logger.Debug(tenantId, userId, $"[InsertUserAsync] 开始插入用户: name={name}, age={age}");
    try
    {
        // ... 执行数据库操作 ...
        Logger.Debug(tenantId, userId, $"[InsertUserAsync] 插入成功, id={result.Id}");
        return result;
    }
    catch (Exception ex)
    {
        Logger.Error(tenantId, userId, $"[InsertUserAsync] {I18n.T(tenantId, K.Db.InsertFailed)}: {ex}");
        return new DbInsResult { Success = false, ErrorMessage = I18n.T(tenantId, K.Db.InsertFailed) };
    }
}
```

### 5.5 即时切换语言

```csharp
I18n.DefaultLang = Lang.ZhCn;
Console.WriteLine(I18n.T(K.Common.Success));   // "操作成功"

I18n.DefaultLang = Lang.En;
Console.WriteLine(I18n.T(K.Common.Success));   // "Operation successful"

// 租户级切换
I18n.SetTenantLang(1001, Lang.Ja);
Console.WriteLine(I18n.T(1001, K.Common.Success));   // 日语翻译（如已注册）
```

---

## 6. 内部实现要求

### 6.1 语言资源存储

```csharp
internal static class I18nStore
{
    // 按 Lang 枚举值索引，O(1) 查找
    private static readonly Dictionary<string, string>?[] _resources
        = new Dictionary<string, string>?[8];

    // 租户语言偏好：tenantId => Lang
    private static readonly ConcurrentDictionary<int, Lang> _tenantLangs = new();

    // 全局默认语言
    private static volatile Lang _defaultLang = Lang.ZhCn;
}
```

### 6.2 翻译查找流程

```
T(key) → _resources[(int)DefaultLang]?.TryGetValue(key, out val)
       → 未找到 → _resources[(int)FallbackLang]?.TryGetValue(key, out val)
       → 仍未找到 → 返回 key 本身
```

### 6.3 线程安全

- `DefaultLang` 使用 `volatile`
- 租户偏好使用 `ConcurrentDictionary<int, Lang>`
- 资源注册后只读，无需加锁

---

## 7. Source Generator 设计

### 7.1 键名转换规则

| 字典键 | 生成的常量路径 |
|--------|--------------|
| `"common.success"` | `K.Common.Success` |
| `"user.login_failed"` | `K.User.LoginFailed` |
| `"order.item.not_found"` | `K.Order.Item.NotFound` |

规则：按 `.` 分割为层级，每层 PascalCase，下划线转 PascalCase。

### 7.2 触发条件

- `[I18nResource]` 特性标记在语言包类上
- Source Generator 使用 `ForAttributeWithMetadataName` 发现

---

## 8. 在其他工程中的集成

### 8.1 ADO 工程
- 错误信息使用 `I18n.T(tenantId, key)` 返回国际化文本

### 8.2 Entity 工程
- CRUD 方法中的用户提示使用国际化键

### 8.3 SQL Builder 工程
- 内部不需要国际化，上层代码返回错误信息时使用

---

## 9. 性能设计要点

1. 翻译查找 O(1)：数组索引 + Dictionary
2. 零分配热路径：直接返回字典中的 string 引用
3. **string interpolation > string.Format**：编译器优化为 `DefaultInterpolatedStringHandler`
4. 语言切换无锁：volatile 读写
5. 内存最小化：只读字典，不重复存储

---

## 10. 日志集成

```csharp
// 初始化
Logger.Info(0, 0L, $"[I18n] 初始化完成, 默认语言: {defaultLang}");
Logger.Info(0, 0L, $"[I18n] 注册语言包: {lang}, 键数量: {resources.Count}");

// 租户语言切换
Logger.Debug(tenantId, 0L, $"[I18n] 设置租户语言: tenantId={tenantId}, lang={lang}");

// 翻译键未找到（Debug 级别）
Logger.Debug(tenantId, 0L, $"[I18n] 翻译键未找到: key={key}, lang={lang}");
```

---

## 11. 质量标准

优先级：**正确性 > 语义完整性 > 安全性 > 性能 > 可维护性 > 可读性 > 易用性**

验收清单：
- [ ] 无 LINQ / 反射 / dynamic / 序列化
- [ ] 无 `string.Format`，全部使用 `string interpolation`
- [ ] 无 `.resx` 文件
- [ ] NativeAOT 完全兼容
- [ ] 翻译查找 O(1)
- [ ] 运行时即时切换语言
- [ ] 租户级语言偏好
- [ ] 回退策略正确
- [ ] Source Generator 生成强类型键
- [ ] 线程安全
- [ ] 完整中文 XML 注释
- [ ] 与 ADO / Entity / Logger 集成示例

---

## 12. 最终指令

请现在开始实现，直接交付：

1. `src/YTStdI18n/` — 运行时库完整源码
2. `src/YTStdI18n.Generator/` — Source Generator 完整源码
3. `tests/YTStdI18n.Tests/` — 完整单元测试
4. `samples/YTStdI18n.Sample/` — 使用示例

必须输出**完整可编译代码**。不要只给设计稿、伪代码或骨架。
必须按**生产级顶级标准**完成。
