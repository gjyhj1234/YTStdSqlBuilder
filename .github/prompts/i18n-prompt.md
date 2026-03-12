# 面向 Claude Opus 4.6 的国际化语言支持（i18n）生产级实现提示词
## 基于数组索引的极致高性能、零 GC、低内存、AOT 友好的多语言资源管理系统

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
| `Logger` | YTStdLogger | 日志记录（`Logger.Debug(int, long, Func<string>)`, `Logger.Error(int, long, string)`, `Logger.Info(int, long, string)`） |

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
   - **禁止 `Dictionary<string, string>`** — 必须使用 **`string[]` 数组** 存储翻译文本
   - **禁止字符串键查找** — 必须使用 **`int` 常量索引** 进行翻译查找

3. **性能目标**
   - 翻译查找 O(1)：使用数组下标直接索引，比 Dictionary 更快
   - 零分配翻译路径：热路径上不产生额外堆分配，直接返回数组中的 string 引用
   - 零哈希计算：无需字符串哈希，int 索引直接定位
   - 极低内存占用：string[] 数组比 Dictionary 少约 40% 内存开销（无桶数组、无 Entry 数组、无键字符串存储）
   - 极佳缓存局部性：连续内存布局，CPU 缓存友好
   - 支持即时切换语言
   - 集中管理所有语言资源，便于维护

4. **数组管理强制规范**
   - **只能往数组末尾追加，严禁中间插入或删除**
   - 以主语言（简体中文 `LangZhCn`）为基准，定义 Key 的顺序
   - 其他语言包必须**严格按照主语言的索引顺序**填充对应翻译
   - 如果某语言没有对应翻译，必须填 `null` 占位

---

## 性能对比分析：数组索引 vs Dictionary

| 维度 | Dictionary<string, string> | string[] 数组索引 | 优势 |
|------|---------------------------|-------------------|------|
| 查找方式 | 字符串哈希 → 桶定位 → 链表遍历 → 字符串相等比较 | `array[intIndex]` 直接内存偏移 | 数组快 3-5x |
| 内存占用 | 桶数组 + Entry[] + Key 字符串 + Value 字符串 | 仅 Value 字符串数组 | 数组省约 40% |
| GC 压力 | Dictionary 内部有多个数组对象 | 单个 string[] 对象 | 数组极低 |
| 缓存局部性 | 哈希跳跃访问，缓存不友好 | 连续内存，CPU L1/L2 友好 | 数组优 |
| 键存储 | 每个键都存储为 string 对象 | int 常量编译期内联，零存储 | 数组零开销 |
| 分配路径 | TryGetValue 本身零分配，但键需要字符串对象 | 纯数组下标，零分配 | 两者均零分配 |

**结论：数组索引方案在性能、内存、GC 三个维度全面优于 Dictionary 方案。**

---

## 1. 最终目标

实现一套集中管理、极致高性能的多语言支持系统，具备以下能力：

1. **集中式语言资源管理** — 所有语言文本按分类定义在数组中，便于维护
2. **编译期常量键** — 通过 Source Generator 自动生成 `int` 常量索引，消除 magic string
3. **编译期强校验** — Source Generator 在编译时验证所有语言包的数组顺序和长度一致性
4. **运行时即时切换** — 支持运行时切换当前语言，无需重启
5. **租户级语言偏好** — 不同租户可使用不同语言
6. **带参数的翻译** — 支持翻译文本中嵌入动态值（使用 string interpolation）
7. **回退策略** — 找不到翻译时回退到默认语言（中文），再找不到返回空字符串
8. **AOT 友好** — 完全兼容 NativeAOT，无反射依赖

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
| 源生成器 | `src/YTStdI18n.Generator/` | 编译期生成强类型键 + 编译期校验 |
| 测试 | `tests/YTStdI18n.Tests/` | 单元测试 |
| 示例 | `samples/YTStdI18n.Sample/` | 使用示例 |

---

## 4. 核心设计

### 4.1 语言枚举

```csharp
/// <summary>
/// 支持的语言枚举。
/// 可按需扩展，新增语言只需添加枚举值 + 对应翻译数组。
/// </summary>
public enum Lang : byte
{
    /// <summary>简体中文（默认，基准语言）</summary>
    ZhCn = 0,
    /// <summary>英语</summary>
    En = 1,
    /// <summary>日语</summary>
    Ja = 2,
    /// <summary>繁体中文</summary>
    ZhTw = 3,
}
```

### 4.2 语言资源定义方式 — 基于 `string[]` 数组

语言资源使用纯 C# 静态数组定义，按分类（Category）组织。

**以主语言 `LangZhCn` 为基准**，定义每个分类的顺序。其他语言包必须严格遵循同一索引顺序。

```csharp
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
            /* 0 */ "操作成功",
            /* 1 */ "操作失败",
            /* 2 */ "未找到数据",
            /* 3 */ "未授权访问",
            /* 4 */ "禁止访问",
            /* 5 */ "数据验证失败",
        };

        User = new string[]
        {
            /* 0 */ "登录成功",
            /* 1 */ "用户名或密码错误",
            /* 2 */ "账户已被锁定",
            /* 3 */ "密码长度不能少于 8 位",
        };

        Db = new string[]
        {
            /* 0 */ "数据库连接失败",
            /* 1 */ "查询超时",
            /* 2 */ "新增数据失败",
            /* 3 */ "更新数据失败",
            /* 4 */ "删除数据失败",
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
            /* 0 */ "Operation successful",
            /* 1 */ "Operation failed",
            /* 2 */ "Data not found",
            /* 3 */ "Unauthorized access",
            /* 4 */ "Access forbidden",
            /* 5 */ "Validation error",
        };

        User = new string[]
        {
            /* 0 */ "Login successful",
            /* 1 */ "Invalid username or password",
            /* 2 */ "Account has been locked",
            /* 3 */ "Password must be at least 8 characters",
        };

        Db = new string[]
        {
            /* 0 */ "Database connection failed",
            /* 1 */ "Query timeout",
            /* 2 */ "Insert data failed",
            /* 3 */ "Update data failed",
            /* 4 */ "Delete data failed",
        };
    }
}
```

### 4.3 Source Generator — 生成 `K` 常量类 + 编译期校验

Source Generator 扫描所有 `[I18nResource]` 类，以基准语言为模板，自动生成 `int` 常量索引类。

**生成的 `K` 类（由 Source Generator 自动生成）：**

```csharp
// === 由 Source Generator 自动生成 ===
/// <summary>
/// 国际化键常量类。
/// 每个分类下的常量值为该翻译在对应数组中的索引位置。
/// 由 Source Generator 根据基准语言 LangZhCn 自动生成。
/// </summary>
public static class K
{
    public static class Common
    {
        public const int Success = 0;
        public const int Failed = 1;
        public const int NotFound = 2;
        public const int Unauthorized = 3;
        public const int Forbidden = 4;
        public const int ValidationError = 5;
    }

    public static class User
    {
        public const int LoginSuccess = 0;
        public const int LoginFailed = 1;
        public const int AccountLocked = 2;
        public const int PasswordTooShort = 3;
    }

    public static class Db
    {
        public const int ConnectionFailed = 0;
        public const int QueryTimeout = 1;
        public const int InsertFailed = 2;
        public const int UpdateFailed = 3;
        public const int DeleteFailed = 4;
    }
}
```

**Source Generator 自动生成 `K` 类中的常量名推导规则：**

| 数组字段名 | 数组索引 | 中文值 | 生成的常量 | 说明 |
|-----------|---------|-------|-----------|------|
| `Common` | `0` | `"操作成功"` | `K.Common.Success` | 取中文含义的英文 PascalCase |
| `Common` | `1` | `"操作失败"` | `K.Common.Failed` | |
| `Db` | `2` | `"新增数据失败"` | `K.Db.InsertFailed` | |

> **常量名规则**：Source Generator 不解析中文。常量名由开发者在基准语言数组的注释中以 `/* 索引 名称 */` 形式标注。
> 例如 `/* 0 Success */`、`/* 1 Failed */` 等。
> 或者由 SG 根据同名英文语言包 `LangEn` 中对应索引位置的英文文本推导 PascalCase 名称。
> **推荐方案**：在基准语言的数组注释中显式标注常量名，SG 解析注释提取名称：

```csharp
Common = new string[]
{
    /* Success        */ "操作成功",
    /* Failed         */ "操作失败",
    /* NotFound       */ "未找到数据",
    /* Unauthorized   */ "未授权访问",
    /* Forbidden      */ "禁止访问",
    /* ValidationError*/ "数据验证失败",
};
```

### 4.4 全局静态入口 — `I18n` 静态类

```csharp
/// <summary>
/// 国际化全局入口。
/// 提供语言切换、租户语言偏好、分类翻译查找功能。
/// </summary>
public static class I18n
{
    /// <summary>初始化国际化系统。应用启动时调用一次。</summary>
    public static void Init(Lang defaultLang = Lang.ZhCn);

    /// <summary>
    /// 注册所有语言包。内部调用各语言类的 Register() 方法。
    /// 由 Source Generator 自动生成此方法的实现。
    /// </summary>
    public static void Register();

    /// <summary>获取或设置全局默认语言。线程安全。</summary>
    public static Lang DefaultLang { get; set; }

    /// <summary>设置指定租户的语言偏好。</summary>
    public static void SetTenantLang(int tenantId, Lang lang);

    /// <summary>获取指定租户的语言偏好，未设置则返回全局默认语言。</summary>
    public static Lang GetTenantLang(int tenantId);
}
```

### 4.5 分类翻译查找 — 基于 `int` 扩展方法

每个分类生成一个 `int` 扩展方法，实现按语言数组索引查找：

```csharp
/// <summary>
/// 由 Source Generator 自动生成的翻译扩展方法。
/// 每个分类（Common、User、Db）对应一个扩展方法。
/// </summary>
public static class I18nExtensions
{
    /// <summary>
    /// 获取 Common 分类的翻译（使用全局默认语言）。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Common(this int index)
    {
        return I18n.DefaultLang switch
        {
            Lang.En => LangEn.Common?[index] ?? LangZhCn.Common![index],
            Lang.Ja => LangJa.Common?[index] ?? LangZhCn.Common![index],
            Lang.ZhTw => LangZhTw.Common?[index] ?? LangZhCn.Common![index],
            _ => LangZhCn.Common![index],
        };
    }

    /// <summary>
    /// 获取 Common 分类的翻译（指定语言）。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Common(this int index, Lang lang)
    {
        return lang switch
        {
            Lang.En => LangEn.Common?[index] ?? LangZhCn.Common![index],
            Lang.Ja => LangJa.Common?[index] ?? LangZhCn.Common![index],
            Lang.ZhTw => LangZhTw.Common?[index] ?? LangZhCn.Common![index],
            _ => LangZhCn.Common![index],
        };
    }

    /// <summary>
    /// 获取 Common 分类的翻译（使用指定租户的语言偏好）。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Common(this int index, int tenantId)
    {
        return index.Common(I18n.GetTenantLang(tenantId));
    }

    // === 以下为 User、Db 等其他分类的扩展方法，模式相同 ===

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string User(this int index)
    {
        return I18n.DefaultLang switch
        {
            Lang.En => LangEn.User?[index] ?? LangZhCn.User![index],
            _ => LangZhCn.User![index],
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string User(this int index, Lang lang) { /* 同上模式 */ }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string User(this int index, int tenantId) { /* 同上模式 */ }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Db(this int index) { /* 同上模式 */ }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Db(this int index, Lang lang) { /* 同上模式 */ }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Db(this int index, int tenantId) { /* 同上模式 */ }
}
```

**回退策略**：
- 当目标语言的数组为 null（未注册）或对应位置为 null（缺失翻译）时，回退到基准语言 `LangZhCn`
- 如果基准语言也没有（理论上不应发生），返回空字符串

---

## 5. 使用示例

### 5.1 初始化

```csharp
// 应用启动时
I18n.Init(Lang.ZhCn);
I18n.Register();  // 内部调用 LangZhCn.Register()、LangEn.Register() 等
```

### 5.2 基本翻译

```csharp
// 使用全局默认语言
string msg = K.Common.Success.Common();   // "操作成功"

// 指定语言
string msgEn = K.Common.Success.Common(Lang.En);   // "Operation successful"

// 使用租户偏好语言
I18n.SetTenantLang(1001, Lang.En);
string msgTenant = K.Common.Success.Common(1001);   // "Operation successful"

// 数据库相关
string dbErr = K.Db.InsertFailed.Db();   // "新增数据失败"
string dbErrEn = K.Db.InsertFailed.Db(Lang.En);   // "Insert data failed"
```

### 5.3 带参数的翻译（使用 string interpolation，**禁止 string.Format**）

```csharp
// ✅ 正确：使用 string interpolation
string userName = "张三";
int errorCount = 5;
string msg = $"{K.User.LoginSuccess.User()}，{userName}";
// => "登录成功，张三"

// ✅ 正确：复杂插值
string detailMsg = $"{K.Common.ValidationError.Common()}: 共 {errorCount} 个错误";
// => "数据验证失败: 共 5 个错误"

// ❌ 错误：禁止使用 string.Format
// string msg = string.Format(I18n.T(key), userName);  // 禁止！
```

### 5.4 在 ADO / 数据库操作中的集成

ADO 层**仅** CRUD 操作返回的 `ErrorMessage` 使用国际化，日志本身不使用国际化：

```csharp
public static async Task<DbInsResult> InsertUserAsync(int tenantId, long userId, string name, int age)
{
    // ADO 层 Debug 日志不使用国际化，直接使用中文
    Logger.Debug(tenantId, userId, () => $"[InsertUserAsync] 开始插入用户: name={name}, age={age}");
    try
    {
        // ... 执行数据库操作 ...
        Logger.Debug(tenantId, userId, () => $"[InsertUserAsync] 插入成功, id={result.Id}");
        return result;
    }
    catch (Exception ex)
    {
        // ADO 层 Error 日志不使用国际化
        Logger.Error(tenantId, userId, $"[InsertUserAsync] 执行异常: {ex}");
        return new DbInsResult
        {
            Success = false,
            // ErrorMessage 使用国际化语言，用于返回前端展示（按租户语言偏好）
            ErrorMessage = K.Db.InsertFailed.Db(tenantId),
            // DebugMessage 是堆栈信息，不使用国际化
            DebugMessage = $"SQL=..., 异常={ex}"
        };
    }
}
```

### 5.5 即时切换语言

```csharp
I18n.DefaultLang = Lang.ZhCn;
Console.WriteLine(K.Common.Success.Common());   // "操作成功"

I18n.DefaultLang = Lang.En;
Console.WriteLine(K.Common.Success.Common());   // "Operation successful"

// 租户级切换
I18n.SetTenantLang(1001, Lang.Ja);
Console.WriteLine(K.Common.Success.Common(1001));   // 日语翻译（如已注册）
```

---

## 6. 内部实现要求

### 6.1 语言资源存储

语言资源直接存储在各语言类的静态 `string[]?` 字段中，无需额外的 Store 类：

- 每个语言类（`LangZhCn`、`LangEn` 等）拥有各分类的 `string[]?` 静态字段
- `Register()` 方法负责初始化数组
- 初始化后数组为只读（不再修改），天然线程安全

### 6.2 翻译查找流程

```
K.Common.Success.Common()
  → index = 0（编译期常量，内联到调用点）
  → 根据 DefaultLang 选择对应语言类的 Common 数组
  → array[0] 直接返回（单次内存偏移，零哈希、零比较）
  → 如果为 null → 回退到 LangZhCn.Common![0]
```

### 6.3 线程安全

- `DefaultLang` 使用 `volatile`
- 租户偏好使用 `ConcurrentDictionary<int, Lang>`
- 各语言数组在 `Register()` 后只读，天然线程安全

---

## 7. Source Generator 设计

### 7.1 SG 职责清单

Source Generator 必须完成以下全部职责：

1. **生成 `K` 常量类**：扫描基准语言的所有分类数组，为每个元素生成 `int` 常量
2. **生成 `I18nExtensions` 扩展方法类**：为每个分类生成 `Common()`、`User()`、`Db()` 等扩展方法
3. **生成 `I18n.Register()` 方法**：自动调用所有语言类的 `Register()` 方法
4. **编译期强校验**：验证所有语言包的一致性

### 7.2 编译期强校验规则（必须实现）

Source Generator 必须在编译时执行以下校验，任一失败则**报错并阻止编译**：

| 校验项 | 规则 | 错误代码 |
|--------|------|---------|
| 数组长度一致 | 每个非基准语言包的每个分类数组长度必须等于基准语言对应数组长度 | `I18N001` |
| 分类字段一致 | 每个非基准语言包必须包含与基准语言完全相同的分类字段名集合 | `I18N002` |
| 索引顺序不可变 | 如果检测到已有 K 类的常量值与当前数组位置不一致（索引偏移），报错 | `I18N003` |
| 基准语言无 null | 基准语言的数组中不允许 null 值（基准语言必须提供所有翻译） | `I18N004` |
| 只能末尾追加 | 新增翻译只能追加到数组末尾，已有索引位置的值不可删除或移动 | `I18N005` |

**校验示例：**

```csharp
// ❌ 编译失败 — LangEn.Common 数组长度(5) ≠ LangZhCn.Common 数组长度(6)
// Error I18N001: Language pack 'LangEn' category 'Common' array length 5 does not match base language length 6

// ❌ 编译失败 — LangEn 缺少 Db 分类
// Error I18N002: Language pack 'LangEn' is missing category 'Db' which exists in base language 'LangZhCn'

// ❌ 编译失败 — 基准语言 Common[2] 为 null
// Error I18N004: Base language 'LangZhCn' category 'Common' index 2 must not be null
```

### 7.3 常量名生成规则

推荐使用注释标注方式，在基准语言数组中标注每个元素的常量名：

```csharp
Common = new string[]
{
    /* Success         */ "操作成功",
    /* Failed          */ "操作失败",
    /* NotFound        */ "未找到数据",
};
```

Source Generator 解析规则：
1. 解析数组初始化表达式中每个元素前的 `/* 名称 */` 注释
2. 注释名即为生成的常量名（PascalCase）
3. 常量值为该元素在数组中的索引位置（从 0 开始）
4. 如果没有注释，SG 报编译错误 `I18N006: Missing constant name comment for index N in category 'X'`

### 7.4 触发条件

- `[I18nResource]` 特性标记在语言包类上
- `[I18nResource(IsBase = true)]` 标记基准语言
- Source Generator 使用 `ForAttributeWithMetadataName` 发现

### 7.5 生成文件清单

| 生成文件 | 说明 |
|---------|------|
| `K.g.cs` | 常量索引类 |
| `I18nExtensions.g.cs` | 分类扩展方法 |
| `I18n.Register.g.cs` | 自动注册方法 |

---

## 8. 在其他工程中的集成

### 8.1 ADO 工程
- **仅** CRUD 操作返回结构中的 `ErrorMessage` 字段使用国际化：`K.Db.InsertFailed.Db(tenantId)`
- ADO 层的 `Logger.Info` / `Logger.Debug` / `Logger.Error` 日志**不使用**国际化语言，直接使用中文
- `DebugMessage` 是堆栈信息，用于开发调试，**不使用**国际化语言
- DDL 操作不使用国际化（DDL 日志使用固定中文）

### 8.2 Entity 工程
- CRUD 方法中的用户提示使用国际化键：`K.Db.UpdateFailed.Db(tenantId)`

### 8.3 SQL Builder 工程
- 内部不需要国际化，上层代码返回错误信息时使用

---

## 9. 性能设计要点

1. **翻译查找 O(1) — 数组下标直接索引**：`array[intIndex]` 单次内存偏移，比 Dictionary 快 3-5x
2. **零分配热路径**：直接返回数组中的 string 引用，无额外分配
3. **零哈希计算**：int 常量在编译期内联，运行时无需字符串哈希
4. **极低内存**：string[] 比 Dictionary 少约 40% 内存开销
5. **CPU 缓存友好**：连续内存布局，L1/L2 缓存命中率高
6. **`AggressiveInlining`**：扩展方法标记内联，JIT 可将整个调用链优化为直接数组访问
7. **`string interpolation` > `string.Format`**：编译器优化为 `DefaultInterpolatedStringHandler`
8. **语言切换无锁**：volatile 读写
9. **编译期验证**：避免运行时因数组不匹配导致 `IndexOutOfRangeException`

---

## 10. 日志集成

```csharp
// 初始化
Logger.Info(0, 0L, $"[I18n] 初始化完成, 默认语言: {defaultLang}");

// 注册
Logger.Info(0, 0L, "[I18n] 语言包注册完成");

// 租户语言切换
Logger.Debug(tenantId, 0L, () => $"[I18n] 设置租户语言: tenantId={tenantId}, lang={lang}");
```

---

## 11. 质量标准

优先级：**正确性 > 语义完整性 > 安全性 > 性能 > 可维护性 > 可读性 > 易用性**

验收清单：
- [ ] 无 LINQ / 反射 / dynamic / 序列化
- [ ] 无 `string.Format`，全部使用 `string interpolation`
- [ ] 无 `.resx` 文件
- [ ] 无 `Dictionary<string, string>`，全部使用 `string[]` 数组
- [ ] NativeAOT 完全兼容
- [ ] 翻译查找 O(1)：数组下标直接索引
- [ ] 运行时即时切换语言
- [ ] 租户级语言偏好
- [ ] 回退策略正确（null 回退到基准语言）
- [ ] Source Generator 生成 `int` 常量索引类 `K`
- [ ] Source Generator 生成分类扩展方法
- [ ] Source Generator 编译期校验所有语言包一致性
- [ ] 编译期强制：数组只能末尾追加，禁止中间插入或删除
- [ ] 编译期强制：所有语言包数组长度与基准语言一致
- [ ] 编译期强制：基准语言数组无 null 值
- [ ] 线程安全
- [ ] 完整中文 XML 注释
- [ ] 与 ADO / Entity / Logger 集成示例

---

## 12. 最终指令

请现在开始实现，直接交付：

1. `src/YTStdI18n/` — 运行时库完整源码
2. `src/YTStdI18n.Generator/` — Source Generator 完整源码（含编译期校验）
3. `tests/YTStdI18n.Tests/` — 完整单元测试
4. `samples/YTStdI18n.Sample/` — 使用示例

必须输出**完整可编译代码**。不要只给设计稿、伪代码或骨架。
必须按**生产级顶级标准**完成。
