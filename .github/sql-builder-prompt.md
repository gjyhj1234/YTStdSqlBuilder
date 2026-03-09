# 面向 Claude Opus 4.6 的最终生产级实现提示词  
## Npgsql / PostgreSQL 专用，高性能、低 GC、AOT 友好的 SQL Builder + Interpreter + Source Generator + 查询结果结构生成

你是一名**顶级 .NET 基础架构工程师 / PostgreSQL & Npgsql 专家 / 高性能框架作者 / Roslyn Source Generator 专家**。  
请实现一个**仅适配 PostgreSQL / Npgsql** 的、**AOT 友好**、**高性能**、**低 GC**、**生产级可维护**、**可扩展但不过度设计** 的 SQL Builder + Interpreter，并额外提供一套**与 Source Generator 协同的静态优化方案**，以及**查询结果对象结构自动生成能力**。

这不是 demo，不是玩具组件，而是**框架基座级实现**。  
请严格按“**生产级顶级标准**”完成完整代码、测试、Benchmark 与设计说明。

---

# 1. 最终目标

实现一个 SQL 结构化生成系统，支持两条执行路径，并额外支持查询结果结构生成：

## 1.1 运行时解释执行路径
在运行时基于结构化 Builder 输出：

- `string Sql`
- `PgSqlParam[] Params`

以及：

- `string DebugSql`

适用于：
- 动态 where / whereif / 复合条件 / 子查询 / DML 等场景

---

## 1.2 编译期源生成优化路径
对于**结构稳定、模板固定、参数位置固定** 的 SQL 构建代码，允许通过 **Roslyn Source Generator** 在编译期生成专用扩展方法或专用构建方法，避免运行时再走完整 Builder / Interpreter 逻辑。

生成结果的目标是：

- 生成**对应查询结果的 DTO 结构类**
  - 即根据查询的 SELECT 字段列表，自动生成对应的 DTO 结构类
- 生成**静态构建方法**，用户直接调用即可获得 SQL 与参数，运行时**不再重新拼 SQL 主体**
- 生成结果至少包含：
  - `string Sql`（编译期常量）
  - `PgSqlParam[] CreateParams(...)`（参数构造方法，入参为业务参数）
  - `string BuildDebugSql(...)`（可选，调试用）
  - DTO 结果结构类型
  - 从 `NpgsqlDataReader` 映射到 DTO 的 Reader 方法

调用体验目标示例：

```csharp
// 用户直接调用生成器产出的专用方法
var result = UserSqlGenerated.GetById(id);
// result.Sql      => 编译期常量 SQL
// result.Params   => 构造好的参数数组

// 查询结果结构类型直接可用
UserSqlGenerated.GetByIdRow row = ...;

// Reader 方法：从 DataReader 直接映射到 DTO
var dto = UserSqlGenerated.ReadGetByIdRow(reader);
```

适用于：
- 没有 `WhereIf`、没有动态结构分支、SQL 模板固定的高频热点场景
- 或结构大部分固定、少量条件可裁剪的"半静态"场景（需谨慎设计兼容策略）

---

## 1.3 查询结果对象结构自动生成
在 SQL Builder / Template 定义完成后，支持由 Source Generator **自动生成查询结果对象结构代码**，减少编码量。

目标：
- 开发者写完 SQL 的 select 返回字段后，尽量不需要再手写 DTO / Record
- 由生成器直接生成对应的结果结构类型与该类型的Reader方法，即直接从DataReader转成DTO / Record对象。
  - 结构中 DTO 类型的各个属性名根据 Select 的列名或别名，属性数据类型需要在 Select 的构建语法中说明，没有写（默认形式）为 `string`
  - 因此在实现查询列语法时需要设计可以设置数据类型的方法，例如：

```csharp
// 方式一：通过泛型方法指定列的 CLR 类型
.Select(
    user["id"].As<int>("id"),         // int Id
    user["name"].As<string>("name"),  // string Name（默认类型，可省略泛型）
    user["age"].As<int>("age"),       // int Age
    user["balance"].As<decimal>("balance"),
    Func.Count(All.Value).As<int>("order_count"))

// 方式二：通过 TypedCol 指定
.Select(
    user.Col<int>("id"),
    user.Col("name"),                 // 省略泛型时默认 string
    user.Col<int>("age"),
    Func.Count(All.Value).As<int>("order_count"))
```

  - 具体语法请你选择最适合生产使用的方案，但必须满足：
    - 能在 Select 项上显式指定 CLR 类型
    - 未指定时默认为 `string`
    - Source Generator 能从语法中提取类型信息以生成 DTO 属性
- 对于能静态生成 SQL 的查询：
  - 生成 SQL 常量
  - 生成参数构造方法
  - 生成 DebugSql 方法
  - 生成查询结果结构类型
  - 生成 该查询的Reader方法：直接从dataReader转成DTO  / Record对象
- 对于**不能静态生成常量 SQL** 的查询（例如存在 `WhereIf`）：
  - 生成查询结果结构类型
  - 即使 SQL 主体仍需运行时解释器构建
  - 生成 该查询的Reader方法：直接从dataReader转成DTO  / Record对象
这点非常重要：  
**SQL 常量是否可生成** 与 **查询结果结构是否可生成** 是两个不同维度。  
即使无法生成固定 SQL，也仍然可能生成结果结构类型。

### Reader 方法生成示例
生成的 Reader 方法必须使用**基于序号（ordinal）的高性能读取方式**，而非基于列名的低效读取。生成示例：

```csharp
// 生成器自动生成的 Reader 方法
public static GetByIdRow ReadGetByIdRow(NpgsqlDataReader reader)
{
    return new GetByIdRow
    {
        Id = reader.GetInt32(0),
        Name = reader.GetString(1),
        Age = reader.IsDBNull(2) ? null : reader.GetInt32(2),
        Balance = reader.GetDecimal(3)
    };
}
```

要求：
- 使用 `GetXxx(ordinal)` 方法，而非 `reader["columnName"]`
- 正确处理可空类型（`IsDBNull` 检查）
- 序号顺序与 SELECT 字段顺序严格一致
- 方法为 `static`，AOT 友好

---

## 1.4 两条路径必须协同，而不是割裂
最终框架应同时具备：

1. **动态解释器模式**
   - 处理动态条件、复杂条件树、子查询等

2. **静态源生成模式**
   - 处理固定模板 SQL 的极致性能路径

3. **结果结构生成模式**
   - 处理 select 结果字段的结构类型自动生成

三者不是冲突关系，而是互补关系。  
必须设计成：

- API 风格尽量统一
- Builder 语义一致
- Source Generator 复用 Builder 的结构语义或可分析约定
- 用户不需要维护两套完全不同的写法
- 用户不需要重复手写结果 DTO

---

# 2. 必须明确的核心设计判断

你必须认真评估并实现以下架构思想：

## 2.1 “解释器 + Source Generator + 结果结构生成”是合理方向
该优化方向是合理且值得纳入正式方案，原因如下：

1. 对于**纯静态 SQL 模板**，运行时再构建 SQL 主体存在可消除开销  
2. 对热点查询，编译期把 SQL 主体固化成常量，运行时仅构造参数数组，可显著减少：
   - 字符串拼接成本
   - 条件遍历成本
   - 部分对象分配
   - 渲染解释成本
3. AOT 场景下，Source Generator 比运行时动态能力更友好
4. 查询结果结构如果由生成器自动输出，可以显著降低 DAL / Repository 编码量
5. 对框架基座来说，“解释器 + 源生成器 + 结果结构生成”是非常合理的生产级优化方向

但你必须同时明确：
- **不是所有 SQL 都适合源生成 SQL 常量**
- 动态场景仍必须由解释器负责
- 不能为了 Source Generator 牺牲整体 API 可用性与实现清晰度
- 即使 SQL 常量无法生成，也应尽量生成返回结构类型

---

## 2.2 必须建立“SQL 可静态生成”与“结果结构可静态生成”双判定机制
需要区分至少两个维度：

### A. SQL 常量可静态生成
典型特征：
- SQL 结构固定
- 字段固定
- 表固定
- Join 固定
- Where 条件固定
- 参数数量固定
- 参数位置固定
- 不依赖运行时决定条件是否存在
- 不依赖可变长度 `IN` 集合（除非设计了特定策略）
- 不依赖可变结构的嵌套分组

这类可以由 Source Generator 直接生成：
- SQL 常量
- 参数构造方法
- DebugSql 方法
- 结果结构类型

### B. SQL 常量不可静态生成，但结果结构可生成
典型特征：
- `WhereIf`
- `AndIf`
- `OrIf`
- 运行时决定条件是否拼接
- 参数数量可能变化
- SQL 主体不能作为常量确定

但如果其 `SELECT` 列结构固定，则仍然可以生成：
- 返回结构类型
- 字段映射结构辅助代码
- 可选的读取扩展方法

这类必须：
- SQL 主体回退运行时解释器
- 结果结构仍尽量生成

### C. 动态解释 SQL
典型特征：
- 条件动态
- 子查询动态
- 动态排序
- 动态字段
- 动态 join
- 动态 update set
- 动态 select 项

这类必须继续由解释器处理。  
若 `SELECT` 结果结构也不稳定，则结果类型也不能静态生成，只能回退为：
- 手写 DTO
- 或动态映射方式（如后续扩展，不是本次核心）

---

## 2.3 必须支持“静态为主，动态兼容”的混合模式，但不能过度复杂
需要设计适度的折中：

### 混合模式目标
允许在大部分 SQL 固定时，对固定部分进行源生成优化，而动态部分保留少量运行时决策。

例如：
- 主体 SQL 固定
- 某几个 where 条件是 `WhereIf`
- `SELECT` 列固定

则可：
- 生成结果结构类
- SQL 主体回退运行时解释器
- 或在有限范围内尝试生成半静态模板

但是：
- 不能让 Source Generator 为了兼容所有动态场景变成第二套解释器
- 不能把工程复杂度推到不可维护

### 推荐策略
请优先采用以下生产级策略：

1. **第一优先：完整静态模板生成**
   - 适用于没有动态结构分支的 SQL
   - 生成 SQL 常量 + 参数方法 + DebugSql + 结果结构类型
   - 生成收益最大，逻辑最清晰

2. **第二优先：固定返回结构生成**
   - 即使 SQL 常量无法生成，只要 `SELECT` 结构固定，仍生成结果结构类型
   - SQL 主体由运行时解释器负责

3. **第三优先：有限动态裁剪生成（可选）**
   - 可选支持少量 `WhereIf`
   - 但必须限制范围
   - 并且要清楚边界与成本

4. **无法静态化时自动回退解释器**
   - 必须具备可靠回退机制

---

# 3. 总体架构要求

请实现一个统一框架，包含以下部分：

## 3.1 Runtime Builder / Interpreter
负责所有动态场景和通用场景。

## 3.2 Static Template Model
负责表达“可被 Source Generator 理解和生成”的 SQL 模板结构。

## 3.3 Query Result Shape Model
负责表达 Select 结果字段结构，以便生成：
- record/class/readonly struct 等结果类型
- 字段元数据
- 可选映射辅助代码

## 3.4 Roslyn Source Generator
负责扫描符合约定的 SQL 构建描述，并生成：
- 静态 SQL 常量（若可行）
- 参数构造方法
- DebugSql 方法
- 结果结构类型
- 可选读取辅助扩展

## 3.5 Fallback Strategy
当某个查询无法静态生成时：
- SQL 生成自动回退到运行时解释器
- 若返回结构稳定，则仍生成结果结构类型
- 或显式标记仅运行时模式

---

# 4. 设计原则：控制复杂度、提升可读性、避免双系统失控

这是本任务必须重点处理的部分。

## 4.1 不要把生成器和解释器做成两套完全不同的 DSL
要求：
- 尽量共用 Builder API 或共用核心结构语义
- 避免用户写两份 SQL
- 避免一个用于动态、一个用于静态，语法完全不同

## 4.2 生成器只优化“值得优化”的路径
不要试图让 Source Generator 覆盖所有 SQL 场景。  
必须承认：
- 动态条件树本质上更适合解释器
- 静态模板本质上更适合生成器
- 结果结构生成通常比 SQL 常量生成更容易静态化

## 4.3 可读性优先于过度魔法
Source Generator 的使用方式必须尽量清晰，不要搞成用户根本不知道发生了什么的黑盒。

推荐做法：
- 通过特性、约定方法、模板描述对象等方式显式标记“可静态生成”
- 生成结果命名清晰
- 有清楚的 fallback 行为

## 4.4 必须同时考虑三种复杂度
你在设计和实现中必须同时关注：

1. **运行时复杂度**
2. **编码实现复杂度**
3. **长期维护复杂度**

不能只追求某一个维度。

---

# 5. 最终功能范围

---

## 5.1 运行时解释器必须支持
完整支持：

- `SELECT`
- `DISTINCT`
- `FROM`
- `JOIN`
- `WHERE`
- `GROUP BY`
- `HAVING`
- `ORDER BY`
- `LIMIT`
- `OFFSET`
- 子查询
- `INSERT`
- `UPDATE`
- `DELETE`
- `RETURNING`
- 参数化 SQL
- DebugSql
- 字段 vs 字段
- 字段 vs 参数
- 字段 vs 子查询
- 复合括号条件树
- `WhereIf` / `AndIf` / `OrIf`
- `OnGroup` / `HavingGroup`
- `CASE WHEN ... THEN ... ELSE ... END` 表达式
- 聚合函数：`COUNT`、`SUM`、`AVG`、`MIN`、`MAX`
- 常用标量函数：`COALESCE`、`NULLIF`、`CAST`
- `UNION` / `UNION ALL`（至少预留扩展点，建议实现）
- `CTE`（`WITH ... AS`）（可选支持，建议至少预留扩展点）
- `DISTINCT ON (...)` （PostgreSQL 特有，可选支持）

---

## 5.2 Source Generator 优先支持的静态能力
必须优先支持以下“最有价值”的静态模板：

1. 固定 `SELECT ... FROM ... WHERE ...`
2. 固定 `JOIN ... ON ...`
3. 固定 `GROUP BY ... HAVING ...`
4. 固定 `ORDER BY`
5. 固定 `LIMIT/OFFSET`
6. 固定 `INSERT`
7. 固定 `UPDATE`
8. 固定 `DELETE`

参数允许是：
- 标量参数
- 固定数量参数
- 固定位置参数

并可生成：
- `Sql` 常量
- `CreateParams(...)`
- `BuildDebugSql(...)`
- 查询结果结构类型（如适用）

---

## 5.3 Source Generator 可选支持的有限动态能力
可选支持，但必须控制范围并文档化：

### 可考虑支持
- `WhereIf` 的简单叶子条件启停
- 少量固定槽位条件
- 固定数量的可选条件拼接

### 不建议默认支持
- 任意深度组动态启停
- 任意复杂子查询条件裁剪
- 集合长度可变的 `IN (...)`
- 动态 select 列表
- 动态 join 结构
- 动态 order 列

对于这些场景，应明确回退到运行时解释器。

---

## 5.4 查询结果结构生成能力
必须支持自动生成查询结果结构代码。

### 生成条件
只要 `SELECT` 返回字段结构可静态确定，即可考虑生成结果结构类型。  
即使以下情况存在，也应尽量生成结果结构：
- `WhereIf`
- 动态 where 条件
- 动态参数值
- SQL 主体回退运行时解释器

### 生成内容
至少包括：
- 结果 DTO / record / sealed class / readonly struct（你需要给出最合理的生产级选择）
- 属性名/字段名
- 字段别名映射
- **必须生成 Reader 映射方法**：从 `NpgsqlDataReader` 基于序号（ordinal）映射到 DTO
  - 不是"可选"，而是每个结果结构类型都必须附带 Reader 方法
  - 使用 `reader.GetXxx(ordinal)` 高性能方式读取
  - 正确处理可空类型（`reader.IsDBNull(ordinal)` 检查）

### 命名要求
需要提供稳定、易理解的命名策略，例如：
- 查询名 + `Row`
- 查询名 + `Result`
- 查询名 + `Record`

### 类型推断
若 SQL 列明确指定结果 CLR 类型，则按指定类型生成。  
若无法可靠推断数据库类型到 CLR 类型，不要瞎猜，必须通过：
- 显式 API 指定
- 或保守策略并说明

---

# 6. 必须实现的 Source Generator 方案

---

## 6.1 Generator 的最终职责
生成器必须能够在编译期识别符合规范的 SQL 模板描述，并生成：

1. 固定 SQL 常量（若可行）
2. 参数数组构造方法
3. 可选 DebugSql 构造方法
4. 查询结果结构类型
5. 可选读取辅助扩展
6. 可选生成专用扩展方法 / 辅助调用方法

---

## 6.2 建议的生成模式
你必须设计一个清晰、可维护、可调试的 Source Generator 使用方式。  
以下是推荐方向，允许你优化，但必须保证清晰与工程可落地。

### 关键问题：Source Generator 如何获得 SQL 模板的定义内容？

这是整个 Source Generator 方案的核心问题，你必须明确回答。  
Source Generator 在编译期运行，它需要一种方式来"知道"SQL 的结构。  
推荐方案是：**用户在 partial 方法体中使用与运行时 Builder 一致的 API 描述 SQL 结构，Source Generator 通过语法树分析提取结构信息。**

完整示例（推荐方案）：

```csharp
[PgSqlTemplate]
public static partial class UserQueries
{
    /// <summary>
    /// 用户在此方法中用 Builder API 描述 SQL 结构。
    /// Source Generator 分析此方法的语法树，提取 SQL 结构信息并生成代码。
    /// 运行时不会执行此方法体——生成器会替换为静态优化版本。
    /// </summary>
    [PgSqlQuery]
    public static partial PgSqlRenderResult GetUserById(int userId);
    
    // Source Generator 会分析与此方法同名的"定义方法"来获取 SQL 结构：
    // 定义方法约定：private static void Define_GetUserById(PgSqlTemplateBuilder b)
    // 或通过 attribute 参数指定定义方法
    
    /// <summary>
    /// SQL 结构定义方法（Source Generator 的输入源）
    /// 此方法仅在编译期被 Source Generator 分析语法树，运行时不会被调用。
    /// 必须使用 Builder API 的子集（可静态分析的部分）。
    /// </summary>
    private static void Define_GetUserById(PgSqlTemplateBuilder b)
    {
        var user = b.Table("users", "u");
        b.Select(
            user.Col<int>("id"),
            user.Col<string>("name"),
            user.Col<int>("age"))
         .From(user)
         .Where(user.Col("id"), Op.Eq, b.Param<int>("userId"));
    }
}
```

Source Generator 分析 `Define_GetUserById` 的语法树后，会生成：
- `GetUserById_Sql` 常量
- `GetUserById(int userId)` 方法实现
- `GetUserByIdRow` 结果结构类型
- `ReadGetUserByIdRow(NpgsqlDataReader reader)` Reader 方法

### 可选替代方式

#### 方式 A：通过特性参数传递 SQL 模板（适合简单场景）

```csharp
[PgSqlTemplate]
public static partial class UserSqlTemplates
{
    [PgSqlQuery(
        Sql = "SELECT \"id\", \"name\" FROM \"users\" WHERE \"id\" = @userId",
        ResultType = typeof(GetUserByIdRow))]
    public static partial PgSqlRenderResult GetUserById(int userId);
}
```

缺点：SQL 字符串失去结构化优势，复杂查询可读性差。

#### 方式 B：通过固定 Builder 表达式识别

```csharp
[PgSqlTemplate]
static partial void BuildQuery(PgSqlTemplateBuilder b);
```

由源码生成器解析固定构建逻辑。

### 你的任务
请你选择**最适合生产和维护**的一种方案，或给出更优方案，但必须说明理由。  
优先考虑：
- Source Generator 如何可靠地获取 SQL 结构信息（这是最关键的）
- 生成器实现难度合理
- 用户使用清晰
- 与运行时 Builder 能协同（共用 API 语义）
- AOT 友好
- 可测试
- 可生成结果结构类型

---

## 6.3 生成结果要求
对于静态模板，生成器至少生成类似能力：

```csharp
public static class UserSqlGenerated
{
    public const string GetById_Sql = "...";

    public static PgSqlParam[] GetById_Params(int id) { ... }

    public static string GetById_DebugSql(int id) { ... }

    public static PgSqlRenderResult GetById(int id) { ... }

    public sealed partial class GetByIdRow
    {
        public int Id { get; init; }
        public string Name { get; init; } = "";
    }

    // Reader 方法：从 DataReader 直接映射到 DTO（基于序号读取，高性能）
    public static GetByIdRow ReadGetByIdRow(NpgsqlDataReader reader)
    {
        return new GetByIdRow
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1)
        };
    }
}
```

或更优设计，但必须包含：
- SQL 主体为编译期常量或静态只读缓存
- 参数构造逻辑尽量少分配
- DebugSql 逻辑可复用字面量格式化器
- 结果结构类型自动生成
- Reader 映射方法自动生成（基于序号的高性能读取）
- 命名清晰
- 使用体验好

---

## 6.4 必须支持“扩展方法”或“生成辅助方法”的思路
用户明确提出：  
在方法编程生成后，再通过源生成器直接生成常量放到扩展中，直接调用扩展方法传参即可。

你必须认真吸收这个要求，最终方案中必须支持类似以下目标体验：

```csharp
var result = UserSqlGenerated.GetById(id);
```

或：

```csharp
var sql = UserQueryExtensions.BuildGetByIdSql(id);
```

或：

```csharp
var rowType = typeof(UserSqlGenerated.GetByIdRow);
```

总之必须体现：
- 用户最终能直接调用生成器产出的专用方法
- 无需运行时再完整构建 SQL 主体
- 并且可直接使用生成的返回结构类型

---

## 6.5 对 `WhereIf` 等动态条件的生成策略
必须明确：

### 若包含 `WhereIf`，当 SQL 常量无法可靠生成时：
- SQL 主体回退运行时解释器
- 但只要 `SELECT` 返回字段固定，仍然要生成结果结构类型
- 若参数列表可静态确定一部分，可按需生成部分辅助代码，但不要把生成器搞得过于复杂

这点必须在方案中明确写出来并实现。

---

# 7. 与运行时 Builder 的协同要求

---

## 7.1 不允许割裂语义
静态模板与动态 Builder 必须有清晰一致的语义模型，例如：
- 相同的表达式概念
- 相同的条件概念
- 相同的参数概念
- 相同的 SQL 语义
- 相同的 select 项语义
- 相同的结果结构语义

## 7.2 允许回退
若生成器判断模板不支持，必须明确回退：
- 编译警告 + 回退运行时
- 或生成包装方法内部调用解释器
- 或要求用户显式选择

但不允许静默生成错误代码。

## 7.3 生成器与解释器的格式必须一致
对于同一查询语义：
- 静态 SQL 与运行时解释 SQL 必须一致
- 参数顺序必须一致
- DebugSql 输出规则必须一致
- 结果结构字段定义必须一致

---

# 8. 重点：少用 `"u"`、提升 API 可读性

你必须特别优化 API，减少裸字符串别名。

## 8.1 推荐对象化表引用模型
优先设计：

```csharp
var user = Tables.Users.As("u");
var order = Tables.Orders.As("o");

user["id"]
user["name"]
order["user_id"]
```

或更进一步：

```csharp
var user = Tables.Users.As("u");
var order = Tables.Orders.As("o");

user.Id
user.Name
order.UserId
```

要求：
- 不必做成 ORM
- 只是轻量表结构描述
- 提升可读性
- 降低 magic string
- 保持性能

## 8.2 示例与生成模板都要体现这一点
不允许在所有示例中满屏出现：
- `"u"`
- `"o"`
- `"t1"`
- `Col.Of("u", "id")`

应更多展示：
- `table.Col("id")`
- `table["id"]`
- 预定义列属性

---

# 9. 表达式与条件设计要求

---

## 9.1 表达式模型必须支持
至少支持：

- ColumnExpr
- ParamExpr
- LiteralExpr
- FuncExpr（包含 `COUNT`、`SUM`、`AVG`、`MIN`、`MAX`、`COALESCE`、`NULLIF`、`CAST` 等）
- SubQueryExpr
- RawExpr（危险，必须显式标记）
- AllExpr / TableAllExpr
- CaseExpr（`CASE WHEN ... THEN ... ELSE ... END`）
- 可选 BinaryExpr

## 9.2 条件模型必须支持
必须是结构化条件树，支持：

- 单条件
- 条件组
- AND / OR
- 任意层嵌套
- Where / Having / Join On 共用思想

## 9.3 条件左右必须都是表达式
不能只支持“字段 + 参数”，必须支持：
- 字段 vs 字段
- 字段 vs 参数
- 字段 vs 字面量
- 字段 vs 子查询
- Exists(SubQuery)

---

# 10. 运行时 Builder / Interpreter 必须支持的完整能力

---

## 10.1 查询能力
必须支持：

- `SELECT`
- `DISTINCT`
- `FROM`
- `JOIN`
  - `INNER JOIN`
  - `LEFT JOIN`
  - 可选 `RIGHT JOIN`
- `WHERE`
- `GROUP BY`
- `HAVING`
- `ORDER BY`
- `LIMIT`
- `OFFSET`

---

## 10.2 Select 项能力
支持：

- 单字段、多字段
- 别名
- **指定列的 CLR 类型**（用于 Source Generator 生成 DTO 属性类型，默认为 `string`）
- 聚合函数（`COUNT`、`SUM`、`AVG`、`MIN`、`MAX`）
- 函数表达式（`COALESCE`、`NULLIF`、`CAST` 等）
- `CASE WHEN` 表达式
- 子查询作为 Select 项
- 原始 SQL 表达式（显式危险 API）
- `*`
- `table.*`

要求：
- 默认优先结构化表达
- 原始表达式只能通过显式危险入口使用

---

## 10.3 Join 能力
必须支持：

- `INNER JOIN`
- `LEFT JOIN`
- `ON`
- `AND ON`
- `OR ON`
- `ON GROUP`

Join 的条件模型必须与 Where / Having 一致，支持：
- 括号分组
- 字段 vs 字段
- 字段 vs 参数
- 字段 vs 字面量
- 子查询 / Exists（可根据语义支持）

---

## 10.4 Where / Having 复合条件
必须支持：
- `Where(...)`
- `WhereIf(...)`
- `And(...)`
- `AndIf(...)`
- `Or(...)`
- `OrIf(...)`
- `WhereGroup(...)`
- `AndGroup(...)`
- `OrGroup(...)`

Having 同理：
- `Having(...)`
- `AndHaving(...)`
- `OrHaving(...)`
- `HavingGroup(...)`

支持任意层嵌套，例如：

```sql
WHERE
(
    ("age" >= @p0 AND "age" <= @p1)
    OR
    ("name" ILIKE @p2 AND "is_deleted" = FALSE)
)
AND
(
    "created_at" >= @p3
    OR
    EXISTS (SELECT 1 FROM ...)
)
```

---

## 10.5 子查询能力
必须支持：

- `IN (subquery)`
- `EXISTS (subquery)`
- `SELECT (subquery) AS alias`
- `FROM (subquery) AS alias`
- 相关子查询
- 子查询中继续使用完整 Builder 能力

要求：
- 子查询参数与主查询统一编号
- DebugSql 模式同样正确
- 子查询结构化复用

---

## 10.6 字段 vs 字段 / 字段 vs 参数 / 字段 vs 子查询
必须支持：

### 字段 vs 字段
```sql
"user"."created_by" = "order"."owner_id"
```

### 字段 vs 参数
```sql
"user"."name" = @p0
```

### 字段 vs 字面量
```sql
"user"."is_deleted" = FALSE
```

### 字段 vs 子查询
```sql
"user"."dept_id" IN (SELECT ...)
```

因此条件模型必须支持“左右都是表达式”。

---

## 10.7 操作符能力
至少支持：

- `Eq`
- `NotEq`
- `Gt`
- `Gte`
- `Lt`
- `Lte`
- `Like`
- `ILike`
- `NotLike`
- `NotILike`
- `In`
- `NotIn`
- `IsNull`
- `IsNotNull`
- `Between`
- `NotBetween`
- `Exists`
- `NotExists`

要求：
- 使用高效实现方式，如 `enum + switch`
- 正确处理 null 语义

---

# 11. DML 能力

虽然重点是查询，但作为框架基座，必须实现基础 DML。

## 11.1 Insert
支持：
- `INSERT INTO`
- 指定列
- 指定值
- 单行插入
- `RETURNING`
- 可预留批量插入扩展点

## 11.2 Update
支持：
- `UPDATE`
- `SET` — 无条件设置字段值
- `SET IF` / `SetIf` — 条件成立时才设置字段值（类似 `WhereIf` 的思路）
- `WHERE`
- 可选 `RETURNING`

`SetIf` 详细说明与示例：
```csharp
var update = PgSql
    .Update(user)
    .Set(user["updated_at"], Param.Value(DateTime.UtcNow))          // 始终设置
    .SetIf(!string.IsNullOrEmpty(newName), user["name"], Param.Value(newName))  // 条件成立时才设置
    .SetIf(newAge.HasValue, user["age"], Param.Value(newAge))       // 条件成立时才设置
    .Where(user["id"], Op.Eq, Param.Value(userId));
```

行为规则：
- `SetIf(condition, column, value)` — 当 `condition` 为 `false` 时，完全忽略该 SET 项
- 如果所有 `SetIf` 条件均不成立且无无条件 `Set`，最终无任何 SET 项时应抛异常（不能生成无效 UPDATE 语句）

要求：
- 默认禁止无 where 全表更新
- 必须有显式关闭保护开关

## 11.3 Delete
支持：
- `DELETE FROM`
- `WHERE`
- 可选 `RETURNING`

要求：
- 默认禁止无 where 全表删除
- 必须有显式关闭保护开关

---

# 12. 标识符与对象化设计要求

为了减少裸字符串、提升可读性，你必须设计一套**合理但不过度复杂**的表 / 列引用模型。

## 12.1 表对象
至少支持如下思路之一，并尽量优先更易读方案：

### 方案 A：对象化表引用
```csharp
var user = Table.Def("users").As("u");
var order = Table.Def("orders").As("o");

user.Col("id")
order.Col("user_id")
```

### 方案 B：索引器简化
```csharp
user["id"]
order["user_id"]
```

### 方案 C：预定义表模型（推荐可选）
```csharp
var user = Tables.Users.As("u");
var order = Tables.Orders.As("o");

user.Id
user.Name
order.UserId
```

如果采用方案 C，请避免做成重量 ORM，只做轻量静态表描述模型即可。

## 12.2 字段对象
字段应是结构化对象，不是简单字符串。  
至少应包含：
- 所属表源引用
- 列名
- 输出时的标识符渲染规则

## 12.3 标识符转义
必须正确处理 PostgreSQL 标识符双引号规则：
- `"users"`
- `"u"."id"`
- `"schema"."users"`

需要考虑：
- 表名
- 列名
- 别名
- schema
- 子查询别名

---

# 13. 参数与调试字面量要求

## 13.1 参数模型
请设计一个轻量参数结构，例如：

```csharp
public readonly struct PgSqlParam
{
    public readonly string Name;
    public readonly object? Value;
}
```

但你必须进一步评估并决定是否增加：
- `NpgsqlDbType?`
- `DbType?`
- `Size`
- `Precision`
- `Scale`
- `Direction`

要求：
- 设计不要过重
- 要兼顾未来可扩展
- 不要为了“也许以后要用”塞太多没用字段
- 给出合理取舍

---

## 13.2 调试 SQL 字面量格式化
必须提供 PostgreSQL 调试字面量格式化器，支持至少：

- `null` => `NULL`
- `string` => 单引号包裹并转义
- `char`
- `bool` => `TRUE/FALSE`
- 数字 => invariant culture
- `Guid`
- `DateTime`
- `DateTimeOffset`
- `enum`
- 数组 / IEnumerable（用于 `IN (...)`）
- 可选：`byte[]`
- 可选：JSON 字符串

要求：
- 必须有测试覆盖
- 输出稳定、正确、可执行
- 不能本地化污染格式

---

# 14. 空值与边界策略

必须显式定义并实现这些策略：

## 14.1 `WhereIf`
- 条件不成立 => 完全忽略

## 14.2 `IN` 空集合
必须给出默认安全策略，推荐：
- `IN []` => `FALSE`
- `NOT IN []` => `TRUE`

并文档化、测试覆盖。

## 14.3 `Eq/NotEq + null`
- `Eq + null` => `IS NULL`
- `NotEq + null` => `IS NOT NULL`

## 14.4 `LIKE / ILIKE + null`
你必须明确策略：
- 忽略
- 抛异常
- 或其他合理规则

必须说明并测试。

## 14.5 `BETWEEN` 缺边界
必须明确策略并实现，例如：
- 默认抛异常
- 或降级成单边比较
- 但必须文档化且测试

## 14.6 `HAVING` 无 GroupBy
必须明确是否允许，并说明原因。

## 14.7 Update/Delete 无条件
- 默认禁止
- 明确开关允许关闭保护

## 14.8 Builder 状态校验
必须在 `Build()` / 渲染时校验 Builder 状态的有效性，例如：
- `SELECT` 无 `FROM`（子查询或特殊情况除外） — 是否报错或允许
- `UPDATE` 无 `SET` 项 — 必须报错
- `INSERT` 无列或无值 — 必须报错
- 重复调用 `From()` — 是否报错或覆盖
- 请明确每种无效状态的处理策略并测试

## 14.9 Builder 实例的可重用性与线程安全
必须明确：
- Builder 实例是**一次性使用**还是**可重用**？（推荐一次性使用）
- Builder 实例是否线程安全？（推荐不保证线程安全，明确文档说明）
- `Build()` 后是否允许继续修改？（推荐不允许，或记录清晰行为）

---

# 15. 性能设计要求

你必须主动进行以下性能设计：

## 15.1 SQL 构建
- 使用 `ValueStringBuilder` 或等价方案
- 给出完整实现
- 减少中间字符串分配

## 15.2 数据结构
- 对 select items、joins、conditions、order items、params 等集合做合理存储
- 可以使用 `List<T>`，但要说明理由和容量策略
- 不要求为避免一个小分配而极端复杂化代码

## 15.3 渲染模式复用
参数化 SQL 和 DebugSql 必须共用主渲染器，通过模式切换实现。

## 15.4 避免低级性能问题
- 避免 LINQ 热点路径
- 避免不必要闭包
- 避免大量短命字符串
- 避免不必要装箱
- 避免复杂递归导致栈问题，如有深层条件树需合理处理

## 15.5 Generator 性能目标
- 对固定模板生成 SQL 常量
- 运行时尽量不再构建 SQL 主体
- 仅构造参数数组和可选 DebugSql
- 结果结构类型直接生成，减少手写 DTO 成本

---

# 16. AOT 要求

必须兼容 NativeAOT / trimming：

- 不使用 Emit
- 不使用 DynamicMethod
- 不使用 Expression.Compile
- 不依赖运行时反射扫描作为主路径
- Source Generator 优先于运行时魔法

---

# 17. 代码风格与可维护性要求

你必须输出的是**可维护工程代码**，不是只追求跑通。

要求：

1. 命名清晰、专业
2. 类型职责边界明确
3. 注释适度，解释关键设计和性能权衡
4. 文件拆分合理，不要过碎，也不要巨石文件
5. 避免为了抽象而抽象
6. 避免为了性能而牺牲大面积可读性
7. 测试命名清晰
8. 示例代码体现易用性和可读性
9. 生成器代码同样必须可维护，不要堆满难懂分支

你必须在设计说明中明确指出：
- 哪些地方为了可读性做了取舍
- 哪些地方为了性能做了优化
- 为什么这些取舍是生产级合理的

---

# 18. 推荐 API 设计方向与示例

以下是参考风格，不要求完全照抄，但必须保留这些设计目标和完整能力。

## 18.1 Runtime Builder 示例
```csharp
var user = Tables.Users.As("u");
var order = Tables.Orders.As("o");
var paidOrder = Tables.Orders.As("o2");

var query = PgSql
    .Select(
        user["id"].As<int>("id"),                        // 指定 CLR 类型为 int
        user["name"],                                     // 默认 string
        Func.Count(All.Value).As<int>("order_count"))    // 聚合函数 + 类型
    .From(user)
    .LeftJoin(order, join => join
        .On(user["id"], Op.Eq, order["user_id"])
        .And(order["is_deleted"], Op.Eq, Literal.False))
    .WhereGroup(g => g
        .WhereGroup(x => x
            .Where(user["age"], Op.Gte, Param.Value(18))
            .And(user["age"], Op.Lte, Param.Value(60)))
        .OrGroup(x => x
            .Where(user["name"], Op.ILike, Param.Value("%tom%"))
            .And(user["is_deleted"], Op.Eq, Literal.False)))
    .And(Exists.Of(
        PgSql.Select(Literal.One)
            .From(paidOrder)
            .Where(paidOrder["user_id"], Op.Eq, user["id"])))
    .GroupBy(user["id"], user["name"])
    .Having(Func.Count(All.Value), Op.Gt, Param.Value(0))
    .OrderByDesc(user["id"])
    .Limit(20)
    .Offset(0);

var result = query.Build();
var debugSql = query.BuildDebugSql();
```

### CASE WHEN 示例
```csharp
.Select(
    user["id"].As<int>("id"),
    Case.When(user["age"], Op.Lt, Param.Value(18)).Then(Literal.Of("minor"))
        .When(user["age"], Op.Lt, Param.Value(60)).Then(Literal.Of("adult"))
        .Else(Literal.Of("senior"))
        .As<string>("age_group"))
```

要求：
- 比 `Col.Of("u", "id")` 更易读
- 比满屏 `"u"`、`"o"` 字符串更工程化
- 不引入沉重 ORM 风格
- 保持结构清晰
- Select 项支持指定 CLR 类型（用于 DTO 生成）

---

## 18.2 静态模板示例
请设计一种清晰的方式，使以下目标可实现。以下为推荐示例（与 6.2 节保持一致）：

```csharp
[PgSqlTemplate]
public static partial class UserQueries
{
    // 用户声明查询方法签名（partial 方法，由 Source Generator 生成实现）
    [PgSqlQuery]
    public static partial PgSqlRenderResult GetUserById(int userId);

    // SQL 结构定义方法（Source Generator 分析此方法的语法树）
    private static void Define_GetUserById(PgSqlTemplateBuilder b)
    {
        var user = b.Table("users", "u");
        b.Select(
            user.Col<int>("id"),
            user.Col<string>("name"),
            user.Col<int>("age"))
         .From(user)
         .Where(user.Col("id"), Op.Eq, b.Param<int>("userId"));
    }
}
```

Source Generator 会自动生成：

```csharp
// === 生成的代码 ===
public static partial class UserQueries
{
    public const string GetUserById_Sql = 
        "SELECT \"u\".\"id\", \"u\".\"name\", \"u\".\"age\" FROM \"users\" AS \"u\" WHERE \"u\".\"id\" = @p0";

    public static partial PgSqlRenderResult GetUserById(int userId)
    {
        return new PgSqlRenderResult(
            GetUserById_Sql,
            new PgSqlParam[] { new("@p0", userId) });
    }

    public sealed class GetUserByIdRow
    {
        public int Id { get; init; }
        public string Name { get; init; } = "";
        public int Age { get; init; }
    }

    public static GetUserByIdRow ReadGetUserByIdRow(NpgsqlDataReader reader)
    {
        return new GetUserByIdRow
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Age = reader.GetInt32(2)
        };
    }
}
```

或其他更优形式，但必须支持生成：

- SQL 常量
- 参数构造方法
- DebugSql 方法
- `GetUserByIdRow` 结果结构类型
- Reader 映射方法

---

## 18.3 动态 SQL 但静态结果结构示例
需要支持以下语义——当 SQL 定义中包含动态条件时，Source Generator 应：
- 检测到 `WhereIf` / `AndIf` 等动态条件 → SQL 常量**无法**生成
- 检测到 `SELECT` 列结构固定 → 结果结构类型**仍然可以**生成

```csharp
[PgSqlTemplate]
public static partial class UserQueries
{
    // 此查询包含 WhereIf，SQL 不可静态生成
    // 但 SELECT 列固定，DTO 仍可生成
    [PgSqlQuery(FallbackToInterpreter = true)]
    public static partial PgSqlRenderResult SearchUsers(
        string? name, int? minAge, int? maxAge);

    private static void Define_SearchUsers(PgSqlTemplateBuilder b)
    {
        var user = b.Table("users", "u");
        b.Select(
            user.Col<int>("id"),
            user.Col<string>("name"),
            user.Col<int>("order_count"))
         .From(user)
         .WhereIf(!string.IsNullOrEmpty(b.Param<string?>("name")),
                  user.Col("name"), Op.ILike, b.Param<string?>("name"))
         .AndIf(b.Param<int?>("minAge").HasValue,
                user.Col("age"), Op.Gte, b.Param<int?>("minAge"))
         .AndIf(b.Param<int?>("maxAge").HasValue,
                user.Col("age"), Op.Lte, b.Param<int?>("maxAge"));
    }
}
```

Source Generator 应生成：

```csharp
// === 生成的代码 ===
public static partial class UserQueries
{
    // SQL 常量不生成（因为包含 WhereIf）

    // 方法实现回退到运行时解释器
    public static partial PgSqlRenderResult SearchUsers(
        string? name, int? minAge, int? maxAge)
    {
        // 内部调用运行时 Builder/Interpreter 构建 SQL
        var user = Table.Def("users").As("u");
        var builder = PgSql
            .Select(user["id"].As<int>(), user["name"], user["order_count"].As<int>())
            .From(user)
            .WhereIf(!string.IsNullOrEmpty(name), user["name"], Op.ILike, Param.Value(name))
            .AndIf(minAge.HasValue, user["age"], Op.Gte, Param.Value(minAge))
            .AndIf(maxAge.HasValue, user["age"], Op.Lte, Param.Value(maxAge));
        return builder.Build();
    }

    // DTO 仍然可以生成（SELECT 列固定）
    public sealed class SearchUsersRow
    {
        public int Id { get; init; }
        public string Name { get; init; } = "";
        public int OrderCount { get; init; }
    }

    // Reader 方法仍然可以生成
    public static SearchUsersRow ReadSearchUsersRow(NpgsqlDataReader reader)
    {
        return new SearchUsersRow
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            OrderCount = reader.GetInt32(2)
        };
    }
}
```

这个示例清楚地展示了：
- SQL 常量是否可生成 与 结果结构是否可生成 是**两个独立维度**
- 即使 SQL 回退运行时解释器，DTO 和 Reader 方法仍然自动生成

---

# 19. 必须实现的核心类型（可调整但必须覆盖职责）

你至少需要覆盖类似职责的核心类型，可按更优方式组织：

运行时核心：
- `PgSql`
- `PgSqlQueryBuilder`
- `PgSqlInsertBuilder`
- `PgSqlUpdateBuilder`
- `PgSqlDeleteBuilder`
- `PgSqlRenderer`
- `PgSqlRenderResult`
- `PgSqlParam`
- `PgSqlRenderMode`
- `PgSqlLiteralFormatter`
- `Q`（SQL 关键字常量）

表达式 / 模型：
- `SqlExpr`
- `SqlExprKind`
- `SqlTable`
- `SqlTableSource`
- `SqlColumn`
- `SqlSelectItem`
- `SqlOrderItem`
- `SqlJoin`
- `SqlJoinType`
- `SqlAssignment`

条件：
- `SqlCondition`
- `SqlConditionGroup`
- `SqlConditionNode`
- `SqlConditionNodeKind`
- `SqlLogicalOperator`
- `SqlComparisonOperator`

模板 / 生成器：
- `PgSqlTemplateAttribute`（`[PgSqlTemplate]` — 标记包含模板定义的类）
- `PgSqlQueryAttribute`（`[PgSqlQuery]` — 标记单个查询模板方法）
- `QueryResultShape`
- `QueryResultColumn`
- `TemplateAnalyzer`（分析语法树提取 SQL 结构）
- `TemplateEmitter`（生成代码输出）
- `TemplateSupportAnalyzer`（判定是否可静态生成）
- `ResultShapeAnalyzer`（分析 SELECT 列结构以生成 DTO）
- `TemplateFallbackPolicy`

内部：
- `ValueStringBuilder`
- `Guard`
- `ThrowHelper`
- 必要的轻量集合工具（如确有必要）

注意：  
你可以适度精简，不必机械照抄，但必须实现这些职责。

---

# 20. 测试要求

必须输出完整单元测试，覆盖至少：

## 20.1 Runtime Builder / Interpreter
1. 基础 Select + From + Where
2. WhereIf / And / Or
3. 多层括号条件嵌套
4. Join On 复合条件
5. GroupBy + Having
6. Field vs Field
7. Field vs Param
8. In(数组)
9. In(空数组)
10. Exists(SubQuery)
11. Select 子查询
12. From 子查询
13. Null 语义
14. Like / ILike
15. DebugSql 字符串转义
16. DebugSql 时间格式
17. 子查询参数编号连续
18. Update/Delete 安全保护
19. Insert / Update / Delete 基础输出
20. 标识符转义正确性
21. 别名引用正确性
22. 与少 magic string 设计相关的表/列 API 可用性
23. CASE WHEN 表达式
24. 聚合函数（COUNT / SUM / AVG / MIN / MAX）
25. SetIf 条件赋值
26. Builder 状态校验（无 SET 的 UPDATE、无列的 INSERT 等）

## 20.2 Source Generator
1. 静态模板识别
2. SQL 常量生成正确性
3. 参数方法生成正确性
4. DebugSql 方法生成正确性
5. 结果结构类型生成正确性
6. 别名/字段名映射到结果属性的正确性
7. 动态 SQL 回退解释器但仍生成结果结构的正确性
8. 不支持模板的诊断/回退策略
9. Reader 方法生成正确性（序号映射、可空类型处理）
9. 生成结果与运行时解释器输出一致性

## 20.3 Benchmark
至少比较：
1. 朴素字符串拼接
2. `StringBuilder`
3. Runtime Interpreter
4. Generated Static Method

并至少覆盖：
- 简单固定查询
- 中等复杂固定查询
- 含部分动态条件查询（对比 fallback）

---

# 21. 目录结构建议

请输出完整可编译项目，建议目录结构如下，可按更优方式调整：

```text
src/
  PgSqlBuilder/
    PgSql.cs
    Q.cs
    PgSqlRenderer.cs
    PgSqlRenderResult.cs
    PgSqlParam.cs
    PgSqlRenderMode.cs
    PgSqlLiteralFormatter.cs
    ValueStringBuilder.cs

    Builders/
      PgSqlQueryBuilder.cs
      PgSqlInsertBuilder.cs
      PgSqlUpdateBuilder.cs
      PgSqlDeleteBuilder.cs
      JoinBuilder.cs
      ConditionGroupBuilder.cs

    Model/
      SqlTable.cs
      SqlTableSource.cs
      SqlColumn.cs
      SqlSelectItem.cs
      SqlOrderItem.cs
      SqlJoin.cs
      SqlJoinType.cs
      SqlAssignment.cs
      SqlIdentifier.cs
      QueryResultShape.cs
      QueryResultColumn.cs

    Expressions/
      SqlExpr.cs
      SqlExprKind.cs
      ColumnExpr.cs
      ParamExpr.cs
      LiteralExpr.cs
      FuncExpr.cs
      SubQueryExpr.cs
      RawExpr.cs
      AllExpr.cs
      CaseExpr.cs

    Conditions/
      SqlCondition.cs
      SqlConditionNode.cs
      SqlConditionNodeKind.cs
      SqlConditionGroup.cs
      SqlLogicalOperator.cs
      SqlComparisonOperator.cs

    Templates/
      PgSqlTemplateAttribute.cs
      PgSqlQueryAttribute.cs
      PgSqlTemplateBuilder.cs
      TemplateFallbackPolicy.cs

    Internal/
      Guard.cs
      ThrowHelper.cs

src/
  PgSqlBuilder.Generator/
    PgSqlTemplateGenerator.cs
    TemplateAnalysis/
      TemplateModel.cs
      TemplateAnalyzer.cs
      TemplateSupportAnalyzer.cs
      TemplateDiagnosticDescriptors.cs
      ResultShapeAnalyzer.cs
    Emission/
      TemplateEmitter.cs
      ResultShapeEmitter.cs

tests/
  PgSqlBuilder.Tests/
    QueryTests.cs
    ConditionGroupTests.cs
    SubQueryTests.cs
    DebugSqlTests.cs
    DmlTests.cs
    IdentifierTests.cs

  PgSqlBuilder.Generator.Tests/
    TemplateGenerationTests.cs
    TemplateFallbackTests.cs
    TemplateConsistencyTests.cs
    ResultShapeGenerationTests.cs

benchmarks/
  PgSqlBuilder.Benchmarks/
    QueryBenchmarks.cs
    GeneratedTemplateBenchmarks.cs
```

---

# 22. 关键设计说明中必须明确回答的问题

你在最终输出的设计说明中，必须明确回答：

1. 为什么“解释器 + Source Generator + 结果结构生成”是合理架构？
2. 哪些 SQL 适合静态生成 SQL 常量，哪些不适合？
3. 哪些查询虽然不能静态生成 SQL 常量，但仍适合生成结果结构？
4. 如何判定一个模板是否可生成 SQL 常量？
5. 如何判定一个模板是否可生成结果结构？
6. 为什么不能强行让所有 `WhereIf` 都进入 Source Generator？
7. 为什么需要 fallback 机制？
8. 如何保证静态生成与运行时解释语义一致？
9. 如何保证子查询参数编号一致？
10. 如何减少 `"u"` 这类 magic string？
11. 如何在高性能与低实现复杂度之间做平衡？
12. 哪些地方故意没有做“极限优化”，原因是什么？
13. 为什么结果结构生成能够显著减少业务编码量？
14. 结果结构类型应选用 record/class/readonly struct 的依据是什么？
15. Source Generator 如何从用户代码（Define_ 方法或其他方式）中可靠地提取 SQL 结构信息？
16. Builder 实例是否一次性使用？线程安全策略是什么？
17. Reader 方法为什么使用基于序号（ordinal）而非列名的读取方式？

---

# 23. 用户原始需求与补充需求，必须全部覆盖

你必须覆盖并落实以下全部需求：

1. AOT 下高性能、低 GC 的解释器
2. 生成最终 SQL 与 Params 提供给 ADO / Npgsql 调用
3. SQL 关键字常量化，如 `Q.Select`
4. 字段支持常量化/结构化使用
5. 使用 `ValueStringBuilder` 或等价方案构建 SQL
6. 支持 `WhereIfWithAnd`、`WhereIf` 等能力
7. 支持 `Group`、`Having`
8. 最终返回参数数组
9. 支持复杂 where 复合条件与括号嵌套
10. 支持 join on 的复合括号条件
11. 支持 having 的复合括号条件
12. 支持直接生成带实参的可执行调试 SQL
13. 支持子查询
14. 条件右侧可能是字段，而不是参数
15. 需要兼顾编码复杂度、可阅读性、维护性
16. 尽量减少 `"u"` 这种裸字符串别名的使用
17. 增加 Source Generator 协同优化路径
18. 对静态模板直接生成常量 SQL 与参数构造方法
19. 支持直接调用生成器产出的扩展/辅助方法传参执行
20. 对不适合静态生成的场景提供合理回退机制
21. 自动生成查询结果的对象结构代码
22. 即使包含 `WhereIf` 无法生成常量 SQL，只要返回字段固定，仍生成返回结构类型
23. 最终方案必须完整、全面、准确、生产级

---

# 24. 最终输出要求

请严格按以下顺序输出：

1. **总体设计说明**
2. **解释器、Source Generator 与结果结构生成协同架构说明**
3. **复杂度、可读性与性能权衡说明**
4. **静态模板识别、结果结构识别与回退策略说明**
5. **核心数据结构设计说明**
6. **完整源码**
7. **单元测试**
8. **Generator 测试**
9. **Benchmark 代码**
10. **使用示例**
11. **AOT 与性能设计说明**
12. **边界与限制说明**
13. **后续扩展建议**

要求：
- 必须输出**完整可编译代码**
- 不允许只给设计稿
- 不允许只给伪代码
- 不允许只给骨架
- 不允许省略关键文件
- 如果一次输出过长，可以分多轮连续输出，但禁止偷省略

---

# 25. 最终质量标准

若设计上有取舍，请遵循以下优先级：

**正确性 > 语义完整性 > 安全性 > 可维护性 > 可读性 > 复杂度可控 > 性能 > 易用性 > 技巧炫技**

但在不损害前几项的前提下，仍要尽量做到：

- 高性能
- 低 GC
- AOT 友好
- Source Generator 协同优化
- 结果结构自动生成
- 简洁清晰
- 工程级可维护
- 真实生产可用

---

# 26. 最终指令

请现在开始实现，直接交付：

- Runtime Builder / Interpreter 完整源码
- Source Generator 完整源码
- 查询结果结构生成完整源码
- 完整测试
- 完整 Benchmark
- 完整设计说明

不要只给思路。  
不要偷懒省略。  
不要输出 demo 级方案。  
必须按**生产级顶级标准**完成。  

