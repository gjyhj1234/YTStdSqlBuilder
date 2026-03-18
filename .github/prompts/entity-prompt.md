# 面向 Claude Opus 4.6 的 YTStdEntity + YTStdEntity.Generator 生产级实现提示词

## 实体框架与源生成器——NativeAOT 零反射实体系统

你是一名**顶级 .NET 基础架构工程师 / PostgreSQL & Npgsql 专家 / Roslyn Source Generator 专家 / 高性能框架作者**。
请实现一套面向 **.NET 10.0 + NativeAOT** 的**实体定义、数据库表维护、CRUD 操作、审计追踪、增量备份与租户分离**的完整框架。

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
| `PgSqlRenderResult` | YTStdSqlBuilder | SQL 构建结果，包含 `.Sql` 和 `.Params` |
| `PgSqlParam` | YTStdSqlBuilder | SQL 参数，包含 Name、Value、DbType |
| `ValueStringBuilder` | YTStdSqlBuilder | 零分配字符串构建器，用于动态 SQL 拼接 |
| `DB` (静态类) | YTStdAdo | 数据库执行入口，提供 InsertAsync、UpdateAsync、DeleteAsync、GetAsync、GetListAsync、CreateTable 等方法 |
| `Logger` (静态类) | YTStdLogger.Core | 日志入口，提供 `Debug(int tenantId, long userId, Func<string> message)`（延迟求值）、`Debug(int tenantId, long userId, string message)`、`Info(...)`、`Error(...)`、`Fatal(...)` |
| `NpgsqlParameter` | Npgsql | ADO.NET 参数对象 |
| `NpgsqlDbType` | Npgsql | PostgreSQL 数据类型枚举 |

### 关键交互模式

```csharp
// 1. 使用 DB 静态类执行 SQL
var rows = await DB.InsertAsync(conn, sql, parameters);
var rows = await DB.InsertTxAsync(tx, sql, parameters);
var entity = await DB.GetAsync<T>(conn, sql, parameters, readerMapper);
var list = await DB.GetListAsync<T>(conn, sql, parameters, readerMapper);
await DB.CreateTable(conn, sql);

// 2. 使用 DB 获取表/字段/索引信息
var tableInfo = DB.GetTableInfor(conn, tableName);
var fieldsInfo = DB.GetFieldsInfor(conn, tableName);
var indexInfo = DB.GetIndexInfor(conn, tableName);

// 3. 使用 Logger 记录日志
Logger.Debug(tenantId, userId, () => $"[InsertAsync] 进入方法, name={name}");
Logger.Info(tenantId, userId, $"[CreateTable] 创建表: {tableName}");
Logger.Error(tenantId, userId, $"[InsertAsync] 异常: {ex}");
```

> **补充上下文**：实现本模块时，建议同时查阅以下源文件：
> - `src/YTStdSqlBuilder/ValueStringBuilder.cs` — 了解零分配字符串构建的性能模式
> - `src/YTStdAdo/DB.cs` — 了解 DB 静态类的完整方法签名

---

## 1. 最终目标

实现两个项目：

1. **YTStdEntity**（运行时库）：提供实体基础类型（`DbNullable<T>`、`DBNULL`、`YTFieldMeta`、`AuditRecord`、`AuditQueryFilter`、`AuditDiffField`）和实体特性（`EntityAttribute`、`ColumnAttribute`、`IndexAttribute`、`DetailOfAttribute`），以及增量备份与租户分离运行时逻辑。
2. **YTStdEntity.Generator**（Roslyn Source Generator）：在编译期扫描标注了 `[Entity]` 的实体类，自动生成四类代码：
   - **DAL 类**（`{Entity}DAL.g.cs`）：建表 / 视图 / 索引 / 审计表 / 日志表 / 触发器
   - **CRUD 类**（`{Entity}CRUD.g.cs`）：类型安全的 Insert / Update / Delete / Get / GetList（含事务变体）
   - **审计查询类**（`{Entity}AuditCRUD.g.cs`）：审计记录查询 / 历史追踪 / 快照比较 / 主从表联合审计查询
   - **描述类**（`{Entity}Desc.g.cs`）：字段常量、元数据字典、索引器

---

## 2. 技术约束

| 约束项 | 值 |
|-------|-----|
| 目标框架 | net10.0（运行时库）、netstandard2.0（Generator） |
| 数据库 | PostgreSQL（Npgsql） |
| AOT 兼容 | 是——**禁止**反射、dynamic、LINQ、Expression Tree |
| 字符串构建 | 静态 SQL 使用 `const string`；动态 SQL 使用 `ValueStringBuilder` |
| 内存分配 | 最小化——优先栈分配、`Span<T>`、`ArrayPool`、`ref struct` |
| 日志 | 所有方法必须集成 `Logger.Debug`（使用 `Func<string>` 延迟求值重载） / `Logger.Info` / `Logger.Error` / `Logger.Fatal` |
| 联合主键 | **禁止**——每表仅允许一个主键字段 |
| i18n 就绪 | 用户可见错误消息使用常量 key，预留国际化扩展点 |
| 依赖项目 | YTStdSqlBuilder、YTStdAdo、YTStdLogger.Core、Npgsql |

---

## 3. 项目结构

| 项目 | 路径 | 目标框架 | 说明 |
|------|------|---------|------|
| 运行时库 | `src/YTStdEntity/` | net10.0 | 基础类型、特性、增量备份、租户分离 |
| 源生成器 | `src/YTStdEntity.Generator/` | netstandard2.0 | Roslyn Source Generator |
| 单元测试 | `tests/YTStdEntity.Tests/` | net10.0 | 完整测试覆盖 |
| 使用示例 | `samples/YTStdEntity.Sample/` | net10.0 | 示例实体 + 生成代码演示 |

### 文件布局

```
src/YTStdEntity/
├── DbNullable.cs              # DbNullable<T> 结构体
├── DBNULL.cs                   # DBNULL 辅助常量类
├── YTFieldMeta.cs              # 字段元数据
├── Audit/
│   ├── AuditOpt.cs             # 审计操作类型枚举
│   ├── AuditRecord.cs          # 审计记录
│   ├── AuditQueryFilter.cs     # 审计查询过滤条件
│   ├── AuditDiffField.cs       # 审计字段变更
│   └── MasterDetailAuditResult.cs  # 主从表审计结果基类
├── Attributes/
│   ├── EntityAttribute.cs      # 实体特性
│   ├── ColumnAttribute.cs      # 列特性
│   ├── IndexAttribute.cs       # 索引特性
│   └── DetailOfAttribute.cs    # 主从关系特性
├── Backup/
│   ├── IncrementalBackupService.cs   # 增量备份服务
│   └── IncrementalBackupOptions.cs   # 备份配置
├── Tenant/
│   ├── TenantSeparationService.cs    # 租户分离服务
│   └── TenantSeparationOptions.cs    # 分离配置
└── YTStdEntity.csproj

src/YTStdEntity.Generator/
├── EntityGenerator.cs          # Source Generator 入口
├── Emitters/
│   ├── DalEmitter.cs           # DAL 代码生成
│   ├── CrudEmitter.cs          # CRUD 代码生成
│   ├── AuditCrudEmitter.cs     # 审计查询代码生成
│   └── DescEmitter.cs          # 描述类代码生成
├── Models/
│   ├── EntityModel.cs          # 编译期实体模型
│   ├── ColumnModel.cs          # 编译期列模型
│   ├── IndexModel.cs           # 编译期索引模型
│   └── DetailRelation.cs       # 编译期主从关系模型
└── YTStdEntity.Generator.csproj
```

---

## 4. 核心基础类型

### 4.1 `DbNullable<T>` 结构体

用于 Update 操作中区分三态：**未设置**（不更新）、**设置为 null**（更新为 NULL）、**设置为具体值**（更新为该值）。

```csharp
/// <summary>
/// 三态可空结构体：区分"未设置"、"设置为具体值"和"设置为 NULL"
/// </summary>
public readonly struct DbNullable<T>
{
    /// <summary>标记是否显式设置了值（包括设置为 null）</summary>
    public bool IsSet { get; }

    /// <summary>实际的值。如果 IsSet 为 false，此值无意义</summary>
    public T? Value { get; }

    private DbNullable(T? value, bool isSet)
    {
        Value = value;
        IsSet = isSet;
    }

    /// <summary>公开构造函数：传入值即表示"设置"</summary>
    public DbNullable(T? value)
    {
        Value = value;
        IsSet = true;
    }

    /// <summary>明确表示"设置为 NULL"</summary>
    public static DbNullable<T> NullValue => new(default, true);

    /// <summary>明确表示"未设置"（默认状态）</summary>
    public static DbNullable<T> Unset => new(default, false);

    /// <summary>
    /// 隐式转换：允许直接赋值 T 或 T? 给 DbNullable&lt;T&gt;
    /// 例如：DbNullable&lt;int&gt; x = 10; 或 DbNullable&lt;string&gt; y = null;
    /// </summary>
    public static implicit operator DbNullable<T>(T? value) => new(value, true);

    public override string ToString()
    {
        if (!IsSet) return "[Unset]";
        if (Value == null) return "[Set: null]";
        return $"[Set: {Value}]";
    }
}
```

### 4.2 `DBNULL` 辅助常量类

提供语义化的 NULL 值快捷方式：

```csharp
public static class DBNULL
{
    // 基础类型
    public static DbNullable<string> StringValue => DbNullable<string>.NullValue;
    public static DbNullable<int> IntValue => DbNullable<int>.NullValue;
    public static DbNullable<long> LongValue => DbNullable<long>.NullValue;
    public static DbNullable<decimal> DecimalValue => DbNullable<decimal>.NullValue;
    public static DbNullable<DateTime> DateTimeValue => DbNullable<DateTime>.NullValue;
    public static DbNullable<TimeSpan> TimeSpanValue => DbNullable<TimeSpan>.NullValue;

    // 数组类型
    public static DbNullable<string[]> StringArrayValue => DbNullable<string[]>.NullValue;
    public static DbNullable<int[]> IntArrayValue => DbNullable<int[]>.NullValue;
    public static DbNullable<long[]> LongArrayValue => DbNullable<long[]>.NullValue;
    public static DbNullable<decimal[]> DecimalArrayValue => DbNullable<decimal[]>.NullValue;
    public static DbNullable<DateTime[]> DateTimeArrayValue => DbNullable<DateTime[]>.NullValue;
    public static DbNullable<TimeSpan[]> TimeSpanArrayValue => DbNullable<TimeSpan[]>.NullValue;
}
```

### 4.3 `YTFieldMeta` 字段元数据

```csharp
/// <summary>字段元数据，描述单个数据库列的完整信息</summary>
public sealed class YTFieldMeta
{
    /// <summary>数据库字段名称</summary>
    public string Name { get; init; }

    /// <summary>数据库字段类型（PostgreSQL 类型字符串，如 "text"、"int"、"bigint"）</summary>
    public string Type { get; init; }

    /// <summary>字段长度（仅对 varchar / decimal 有效）</summary>
    public int Length { get; init; }

    /// <summary>字段精度（仅对 decimal 有效）</summary>
    public int Precision { get; init; }

    /// <summary>是否允许为空</summary>
    public bool IsNullable { get; init; }

    /// <summary>是否为主键</summary>
    public bool IsPrimaryKey { get; init; }

    /// <summary>是否为租户字段</summary>
    public bool IsTenant { get; init; }
}
```

### 4.4 `AuditOpt` 审计操作类型枚举

```csharp
/// <summary>审计操作类型</summary>
public enum AuditOpt : byte
{
    /// <summary>插入</summary>
    Insert = (byte)'I',
    /// <summary>更新</summary>
    Update = (byte)'U',
    /// <summary>删除</summary>
    Delete = (byte)'D',
}
```

### 4.5 `AuditRecord` 审计记录

```csharp
/// <summary>
/// 通用审计记录，对应 {Entity}_Audit 表的一行数据。
/// 适用于所有启用审计的实体。
/// </summary>
public sealed class AuditRecord
{
    /// <summary>审计记录自增主键</summary>
    public long AuditId { get; init; }

    /// <summary>原表主键 id</summary>
    public long Id { get; init; }

    /// <summary>操作类型：I=插入, U=更新, D=删除</summary>
    public AuditOpt Opt { get; init; }

    /// <summary>操作时间（UTC，带时区）</summary>
    public DateTime OperatedAt { get; init; }

    /// <summary>操作人 userId</summary>
    public long OperatorId { get; init; }

    /// <summary>租户 ID（仅租户表包含）</summary>
    public int? TenantId { get; init; }

    /// <summary>操作前/时完整记录快照（JSONB 原始字符串）</summary>
    public string Snapshot { get; init; } = "";
}
```

### 4.6 `AuditQueryFilter` 审计查询过滤条件

```csharp
/// <summary>
/// 审计查询过滤条件。
/// 所有条件为 AND 逻辑，null 表示不过滤。
/// </summary>
public sealed class AuditQueryFilter
{
    /// <summary>按原表主键筛选（查看某条记录的完整变更历史）</summary>
    public long? Id { get; set; }

    /// <summary>按操作类型筛选（如只看删除操作）</summary>
    public AuditOpt? Opt { get; set; }

    /// <summary>按操作人 userId 筛选</summary>
    public long? OperatorId { get; set; }

    /// <summary>时间范围起（含），UTC</summary>
    public DateTime? StartTime { get; set; }

    /// <summary>时间范围止（含），UTC</summary>
    public DateTime? EndTime { get; set; }

    /// <summary>分页：页码（从 0 开始）</summary>
    public int PageIndex { get; set; } = 0;

    /// <summary>分页：每页大小（默认 50，最大 500）</summary>
    public int PageSize { get; set; } = 50;
}
```

### 4.7 `AuditDiffField` 审计字段变更

```csharp
/// <summary>
/// 表示两个审计快照之间单个字段的差异。
/// 由比较方法返回。
/// </summary>
public sealed class AuditDiffField
{
    /// <summary>字段名称</summary>
    public string FieldName { get; init; } = "";

    /// <summary>变更前的值（字符串形式）</summary>
    public string? OldValue { get; init; }

    /// <summary>变更后的值（字符串形式）</summary>
    public string? NewValue { get; init; }
}
```

### 4.8 `MasterDetailAuditResult` 主从表审计查询结果

```csharp
/// <summary>
/// 主从表审计查询结果（由 Source Generator 生成具体子类）。
/// 包含主表审计记录和所有相关明细表的审计记录。
/// </summary>
public class MasterDetailAuditResult
{
    /// <summary>主表审计记录列表</summary>
    public List<AuditRecord> MasterRecords { get; init; } = new();

    /// <summary>查询时间范围内的总记录数（用于分页）</summary>
    public int TotalCount { get; init; }
}
```

> **注意**：`MasterDetailAuditResult` 是基类，Source Generator 会为每个有 `[DetailOf]` 关联的主表生成具体子类（如 `OrderMasterDetailAuditResult`），子类包含各明细表的审计记录列表。

---

## 5. 实体特性定义

### 5.1 `EntityAttribute` — 实体级特性

```csharp
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class EntityAttribute : Attribute
{
    /// <summary>物理表名或视图名。为空时使用类名</summary>
    public string? TableName { get; set; }

    /// <summary>视图 SQL。设置后视为视图而非表</summary>
    public string? ViewSql { get; set; }

    /// <summary>是否需要审计表（默认 false）</summary>
    public bool NeedAuditTable { get; set; }
}
```

### 5.2 `ColumnAttribute` — 列级特性

```csharp
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class ColumnAttribute : Attribute
{
    /// <summary>数据库列名。为空时使用属性名</summary>
    public string? ColumnName { get; set; }

    /// <summary>显示标题。为空时使用属性名</summary>
    public string? Title { get; set; }

    /// <summary>是否为主键（默认 false）。系统禁止联合主键</summary>
    public bool IsPrimaryKey { get; set; }

    /// <summary>
    /// 字段长度：
    /// - string 类型：设置后使用 varchar(Length)，不设置使用 text
    /// - decimal 类型：设置后使用该值，不设置默认 12
    /// </summary>
    public int Length { get; set; }

    /// <summary>decimal 精度（默认 2）</summary>
    public int Precision { get; set; } = 2;

    /// <summary>是否必填 / NOT NULL（默认 false）</summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// 显式指定数据库字段类型（PostgreSQL 类型字符串）。
    /// 为空时根据 CLR 类型自动映射。
    /// </summary>
    public string? DbType { get; set; }
}
```

### 5.3 `IndexAttribute` — 索引特性

```csharp
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class IndexAttribute : Attribute
{
    /// <summary>索引名称</summary>
    public string IndexName { get; }

    /// <summary>索引字段列表</summary>
    public string[] Columns { get; }

    /// <summary>索引类型（默认 Normal）</summary>
    public IndexKind Kind { get; set; } = IndexKind.Normal;

    public IndexAttribute(string indexName, params string[] columns)
    {
        IndexName = indexName;
        Columns = columns;
    }
}

public enum IndexKind
{
    Normal,
    Unique
}
```

### 5.4 `DetailOfAttribute` — 主从关系特性

标识当前实体是另一个实体的**从表（明细表）**，用于 Source Generator 生成主从表联合审计查询。

```csharp
/// <summary>
/// 标识当前实体为指定主表实体的明细（从表）。
/// Source Generator 据此生成主从表联合审计查询方法。
/// 一个从表仅能属于一个主表。
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class DetailOfAttribute : Attribute
{
    /// <summary>主表实体的类型</summary>
    public Type MasterType { get; }

    /// <summary>当前从表中指向主表主键的外键属性名</summary>
    public string ForeignKey { get; set; } = "";

    public DetailOfAttribute(Type masterType)
    {
        MasterType = masterType;
    }
}
```

**使用示例**（主从表定义）：

```csharp
// === 主表：订单 ===
[Entity(TableName = "order", NeedAuditTable = true)]
[Index("idx_order_user", "user_id")]
public class Order
{
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    public int TenantId { get; set; }

    public long UserId { get; set; }

    [Column(Precision = 2)]
    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }
}

// === 从表：订单明细 ===
[Entity(TableName = "order_item", NeedAuditTable = true)]
[DetailOf(typeof(Order), ForeignKey = "OrderId")]    // ← 声明从属关系
[Index("idx_order_item_order", "order_id")]
public class OrderItem
{
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    public int TenantId { get; set; }

    /// <summary>外键：指向 Order.Id</summary>
    public long OrderId { get; set; }

    [Column(Length = 100)]
    public string ProductName { get; set; } = "";

    public int Quantity { get; set; }

    [Column(Precision = 2)]
    public decimal UnitPrice { get; set; }
}
```

> **SG 规则**：Source Generator 发现 `[DetailOf(typeof(Order), ForeignKey = "OrderId")]` 后，会在主表（`Order`）的审计查询类中生成包含从表审计记录的联合查询方法。

### 5.5 CLR → PostgreSQL 类型映射规则

Source Generator 必须按以下规则将 CLR 类型映射到 PostgreSQL 类型：

| CLR 类型 | PostgreSQL 类型 | NpgsqlDbType | 备注 |
|---------|----------------|-------------|------|
| `int` | `int` | `Integer` | |
| `long` | `bigint` | `Bigint` | |
| `string` | `text` | `Text` | 无 Length 时 |
| `string` | `varchar(N)` | `Varchar` | 有 Length 时 |
| `decimal` | `decimal(L,P)` | `Numeric` | L 默认 12，P 默认 2 |
| `DateTime` | `TIMESTAMPTZ` | `TimestampTz` | |
| `TimeSpan` | `TIME` | `Time` | |
| `bool` | `boolean` | `Boolean` | |
| `int[]` | `int[]` | `Array \| Integer` | |
| `long[]` | `bigint[]` | `Array \| Bigint` | |
| `string[]` | `text[]` | `Array \| Text` | |
| `DateTime[]` | `TIMESTAMPTZ[]` | `Array \| TimestampTz` | |
| `TimeSpan[]` | `TIME[]` | `Array \| Time` | |

### 5.6 租户表判定规则

实体是否为**租户表**的唯一判定标准：**实体类包含名为 `TenantId` 的属性**。
当实体为租户表时：
- 建表使用 PostgreSQL **分区表**语句（按 `tenant_id` 分区）
- 审计表同样包含 `tenant_id` 字段并按其分区
- 所有 CRUD 方法中 `tenantId` 参数将自动绑定到 `tenant_id` 列

### 5.7 使用示例

```csharp
[Entity(TableName = "sys_user", NeedAuditTable = true)]
[Index("idx_user_name", "name")]
[Index("idx_user_email", "email", Kind = IndexKind.Unique)]
public class SysUser
{
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    public int TenantId { get; set; }  // ← 包含此属性即为租户表

    [Column(Title = "用户名", Length = 50, IsRequired = true)]
    public string Name { get; set; }

    [Column(Title = "邮箱", Length = 200)]
    public string? Email { get; set; }

    [Column(Precision = 4)]
    public decimal? Balance { get; set; }

    public DateTime CreatedAt { get; set; }

    public string[]? Tags { get; set; }
}
```

---

## 6. Source Generator — DAL 数据库维护类

**生成文件**：`{Entity}DAL.g.cs`
**生成类名**：`public static partial class {Entity}DAL`

### 6.1 `CreateTableIfNotExists(int tenantId, long userId, bool createLogTable)`

完整执行流程：

```
1. Logger.Info → 进入方法，记录表名
2. 调用 DB.GetTableInfor 判断表/视图是否存在
3a. 如果已存在 → Logger.Info 记录"表已存在" → 返回 false
3b. 如果不存在 → 继续创建
4. 【表】生成 const string createTableSql（含分区表语句，若为租户表）
5. 【视图】使用 ViewSql 生成 const string createViewSql
6. 【日志表】若 createLogTable == true → 生成 {Entity}_Log 表 + 触发器
7. 【审计表】若 NeedAuditTable == true → 生成 {Entity}_Audit 表 + 触发器
8. 调用 DB.CreateTable 执行
9. Logger.Info → 记录创建成功
10. 异常时 Logger.Fatal → 记录完整堆栈 → 调用 Environment.FailFast 终止进程
11. 返回 bool
```

#### 6.1.1 日志表（`{Entity}_Log`）

仅当 `createLogTable == true` 时创建：

```sql
-- 日志表结构
CREATE TABLE IF NOT EXISTS "{Entity}_Log" (
    logid BIGSERIAL PRIMARY KEY,
    id BIGINT NOT NULL,           -- 原表主键 id
    opt CHAR(1) NOT NULL          -- I=插入, U=更新, D=删除
);

-- 触发器函数
CREATE OR REPLACE FUNCTION "{Entity}_Log_Trigger_Func"()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        INSERT INTO "{Entity}_Log" (id, opt) VALUES (NEW.id, 'I');
        RETURN NEW;
    ELSIF TG_OP = 'UPDATE' THEN
        INSERT INTO "{Entity}_Log" (id, opt) VALUES (NEW.id, 'U');
        RETURN NEW;
    ELSIF TG_OP = 'DELETE' THEN
        INSERT INTO "{Entity}_Log" (id, opt) VALUES (OLD.id, 'D');
        RETURN OLD;
    END IF;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

-- 触发器
CREATE TRIGGER "{Entity}_Log_Trigger"
AFTER INSERT OR UPDATE OR DELETE ON "{Entity}"
FOR EACH ROW EXECUTE FUNCTION "{Entity}_Log_Trigger_Func"();
```

**重要规则**：当 `createLogTable == true` 且 `NeedAuditTable == true` 时，日志表的触发器逻辑**合并到** `{Entity}_Audit_Trigger` 中，不创建两个独立触发器。

#### 6.1.2 审计表（`{Entity}_Audit`）

仅当 `NeedAuditTable == true` 时创建：

```sql
-- 审计表结构
CREATE TABLE IF NOT EXISTS "{Entity}_Audit" (
    audit_id BIGSERIAL PRIMARY KEY,
    id BIGINT NOT NULL,              -- 原表主键
    opt CHAR(1) NOT NULL,            -- I/U/D
    operated_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    operator_id BIGINT NOT NULL,     -- 操作人 userId
    tenant_id INT,                   -- 仅租户表时包含（并用于分区）
    snapshot JSONB NOT NULL          -- 操作前完整记录 JSON
);

-- 触发器函数（合并日志表逻辑，当 createLogTable 也为 true 时）
CREATE OR REPLACE FUNCTION "{Entity}_Audit_Trigger_Func"()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        INSERT INTO "{Entity}_Audit" (id, opt, operator_id, tenant_id, snapshot)
        VALUES (NEW.id, 'I', NEW.operator_id, NEW.tenant_id, to_jsonb(NEW));
        -- 若同时需要日志表
        INSERT INTO "{Entity}_Log" (id, opt) VALUES (NEW.id, 'I');
        RETURN NEW;
    ELSIF TG_OP = 'UPDATE' THEN
        INSERT INTO "{Entity}_Audit" (id, opt, operator_id, tenant_id, snapshot)
        VALUES (NEW.id, 'U', NEW.operator_id, NEW.tenant_id, to_jsonb(OLD));
        INSERT INTO "{Entity}_Log" (id, opt) VALUES (NEW.id, 'U');
        RETURN NEW;
    ELSIF TG_OP = 'DELETE' THEN
        INSERT INTO "{Entity}_Audit" (id, opt, operator_id, tenant_id, snapshot)
        VALUES (OLD.id, 'D', OLD.operator_id, OLD.tenant_id, to_jsonb(OLD));
        INSERT INTO "{Entity}_Log" (id, opt) VALUES (OLD.id, 'D');
        RETURN OLD;
    END IF;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;
```

审计表附属规则：
- 若原表为租户表 → 审计表也包含 `tenant_id` 并按其分区
- 审计表自身也需要 `{Entity}_Audit_Log` 表（结构与 `{Entity}_Log` 一致）
- 审计表需要 `{Entity}_Audit_Log_Trigger` 触发器（逻辑与日志表触发器一致）
**重要规则**：
- 当 `createLogTable == true` 且 `NeedAuditTable == true` 时，日志表的触发器逻辑**合并到** `{Entity}_Audit_Trigger` 中，不创建两个独立触发器。
- 当 `createLogTable == true` 且 `NeedAuditTable == true` 时，该`{Entity}_Audit`必须创建`{Entity}_audit_log`表，且同样逻辑需要给`{Entity}_audit`创建触发器记录日志。
#### 6.1.3 日志记录规范

```csharp
// 进入方法
Logger.Info(tenantId, userId, $"[CreateTableIfNotExists] 开始创建, 表名={tableName}");

// 表已存在
Logger.Info(tenantId, userId, $"[CreateTableIfNotExists] 表已存在, 跳过: {tableName}");

// 生成 SQL
Logger.Info(tenantId, userId, $"[CreateTableIfNotExists] 建表SQL: {sql}");

// 创建成功
Logger.Info(tenantId, userId, $"[CreateTableIfNotExists] 创建成功: {tableName}");

// 异常
Logger.Fatal(tenantId, userId, $"[CreateTableIfNotExists] 创建失败, 表名={tableName}, SQL={sql}, 异常: {ex}");
Environment.FailFast($"[CreateTableIfNotExists] 创建失败, 表名={tableName}");
```

### 6.2 `EnsureColumnLength(int tenantId, long userId)`

仅对**表**有效（视图直接返回 false）。

执行流程：

```
1. Logger.Info → 进入方法
2. 调用 DB.GetFieldsInfor 获取现有表字段信息
3. 循环检查每个 ColumnAttribute 定义的字段：
   a. varchar 字段：若现有长度 < ColumnAttribute.Length → 生成 ALTER 语句扩展
   b. decimal 字段：若现有长度/精度 < 定义值 → 生成 ALTER 语句扩展
   注意：只能扩展，不能缩小长度/精度
4. 使用 const string sql 存储 ALTER 语句
5. 调用 DB.CreateTable 执行
6. Logger.Info → 记录修改过程（字段名、原长度、新长度）
7. 异常时 Logger.Fatal → 记录 TableName、sql、完整堆栈 → 调用 Environment.FailFast 终止进程
8. 返回 bool
```

### 6.3 `CreateIndexIfNotExists(int tenantId, long userId)`

仅对**表**有效（视图直接返回 false）。

执行流程：

```
1. Logger.Info → 进入方法
2. 调用 DB.GetIndexInfor 获取现有索引信息
3. 遍历 IndexAttribute 定义：
   - 索引名拼接规则：{Entity}_{IndexName}
   - 已存在 → Logger.Info 记录跳过 → 返回 false
   - 不存在 → 根据 IndexKind（Normal/Unique）生成 const string sql
4. 调用 DB.CreateTable 执行
5. Logger.Info → 记录创建成功
6. 异常时 Logger.Fatal → 记录 TableName、sql、完整堆栈 → 调用 Environment.FailFast 终止进程
7. 返回 bool
```

---

## 7. Source Generator — CRUD 操作类

**生成文件**：`{Entity}CRUD.g.cs`
**生成类名**：`public static partial class {Entity}CRUD`

**通用规则**：
- 所有方法的前两个参数必须是 `int tenantId, long userId`
- 每个操作都有 **非事务**（`XxxAsync`）和**事务**（`XxxTxAsync`）两个变体
- 每个操作都有**打散参数**和**实体参数**两套重载
- 所有方法必须在每个执行步骤插入 `Logger.Debug` 调用（使用 `Func<string>` 延迟求值重载，避免不必要的字符串构建开销）

### 7.1 `InsertAsync` / `InsertTxAsync`

#### 方法签名（打散参数版）

根据实体属性自动展开，参数顺序与实体属性一致：
- 非空属性 → 参数类型与属性类型一致（如 `string name`）
- 可空属性 → 参数类型为 `T?` 且默认值 null（如 `int? age = null`）

```csharp
// 示例生成签名
public static async Task<int> InsertAsync(
    int tenantId, long userId,
    string name, int? age = null, string? email = null)
```

#### 方法签名（实体参数版）

```csharp
public static async Task<int> InsertAsync(
    int tenantId, long userId, SysUser entity)
{
    // 委托调用打散参数版
    return await InsertAsync(tenantId, userId,
        entity.Name, entity.Age, entity.Email);
}
```

#### 生成逻辑

```csharp
public static async Task<int> InsertAsync(
    int tenantId, long userId,
    string name, int? age = null)
{
    Logger.Debug(tenantId, userId, () => $"[SysUser.InsertAsync] 进入方法, name={name}, age={age}");

    const string insertSql = "INSERT INTO \"sys_user\" (\"name\", \"age\") VALUES (@name, @age)";

    Logger.Debug(tenantId, userId, () => $"[SysUser.InsertAsync] SQL={insertSql}");

    // 构建参数
    var paramList = new List<NpgsqlParameter>(2);

    // 非空字段直接添加
    var pmName = new NpgsqlParameter("name", NpgsqlDbType.Text) { Value = name };
    paramList.Add(pmName);
    Logger.Debug(tenantId, userId, () => $"[SysUser.InsertAsync] 参数: name={name}");

    // 可空字段判断
    if (age != null)
    {
        var pmAge = new NpgsqlParameter("age", NpgsqlDbType.Integer) { Value = age.Value };
        paramList.Add(pmAge);
    }
    else
    {
        var pmAge = new NpgsqlParameter("age", NpgsqlDbType.Integer) { Value = DBNull.Value };
        paramList.Add(pmAge);
    }
    Logger.Debug(tenantId, userId, () => $"[SysUser.InsertAsync] 参数: age={age}");

    try
    {
        var result = await DB.InsertAsync(conn, insertSql, paramList.ToArray());
        Logger.Debug(tenantId, userId, () => $"[SysUser.InsertAsync] 插入成功, 影响行数={result}");
        return result;
    }
    catch (Exception ex)
    {
        Logger.Error(tenantId, userId, $"[SysUser.InsertAsync] 异常: {ex}");
        throw;
    }
}
```

### 7.2 `UpdateAsync` / `UpdateTxAsync`

#### 方法签名（打散参数版）

- 主键参数类型由 `IsPrimaryKey` 属性的 CLR 类型决定（`int` 或 `long`）
- 其余字段使用 `DbNullable<T>?` 包装，默认值 `null`（表示不更新）

```csharp
public static async Task<int> UpdateAsync(
    int tenantId, long userId, long id,
    DbNullable<string>? name = null,
    DbNullable<int>? age = null,
    DbNullable<string>? email = null)
```

#### 方法签名（实体参数版）

```csharp
/// <summary>
/// 使用实体更新。注意：将触发所有字段更新，
/// 如果实体属性为 null，则对应数据库字段将更新为 NULL（请谨慎使用）。
/// </summary>
public static async Task<int> UpdateAsync(
    int tenantId, long userId, long id, SysUser entity)
{
    return await UpdateAsync(tenantId, userId, id,
        new DbNullable<string>(entity.Name),
        new DbNullable<int>(entity.Age),
        new DbNullable<string>(entity.Email));
}
```

#### 生成逻辑（动态 SQL）

```csharp
public static async Task<int> UpdateAsync(
    int tenantId, long userId, long id,
    DbNullable<string>? name = null,
    DbNullable<int>? age = null)
{
    Logger.Debug(tenantId, userId, () => $"[SysUser.UpdateAsync] 进入方法, id={id}");

    var sb = new ValueStringBuilder(stackalloc char[256]);
    sb.Append("UPDATE \"sys_user\" SET ");
    var paramList = new List<NpgsqlParameter>(3);
    var hasField = false;

    // DbNullable 三态处理
    if (name.HasValue && name.Value.IsSet)
    {
        if (hasField) sb.Append(", ");
        sb.Append("\"name\" = @name");
        paramList.Add(new NpgsqlParameter("name", NpgsqlDbType.Text)
        {
            Value = name.Value.Value ?? (object)DBNull.Value
        });
        hasField = true;
        Logger.Debug(tenantId, userId, () => $"[SysUser.UpdateAsync] SET name={name.Value.Value}");
    }

    if (age.HasValue && age.Value.IsSet)
    {
        if (hasField) sb.Append(", ");
        sb.Append("\"age\" = @age");
        paramList.Add(new NpgsqlParameter("age", NpgsqlDbType.Integer)
        {
            Value = age.Value.Value ?? (object)DBNull.Value
        });
        hasField = true;
        Logger.Debug(tenantId, userId, () => $"[SysUser.UpdateAsync] SET age={age.Value.Value}");
    }

    if (!hasField)
    {
        Logger.Debug(tenantId, userId, () => "[SysUser.UpdateAsync] 无字段需更新, 跳过");
        return 0;
    }

    sb.Append(" WHERE \"id\" = @id");
    paramList.Add(new NpgsqlParameter("id", NpgsqlDbType.Bigint) { Value = id });

    var sql = sb.ToString();
    Logger.Debug(tenantId, userId, () => $"[SysUser.UpdateAsync] SQL={sql}");

    try
    {
        var result = await DB.UpdateAsync(conn, sql, paramList.ToArray());
        Logger.Debug(tenantId, userId, () => $"[SysUser.UpdateAsync] 更新成功, 影响行数={result}");
        return result;
    }
    catch (Exception ex)
    {
        Logger.Error(tenantId, userId, $"[SysUser.UpdateAsync] 异常: {ex}");
        throw;
    }
}
```

### 7.3 `DeleteAsync` / `DeleteTxAsync`

```csharp
public static async Task<int> DeleteAsync(int tenantId, long userId, long id)
{
    Logger.Debug(tenantId, userId, () => $"[SysUser.DeleteAsync] 进入方法, id={id}");

    const string deleteSql = "DELETE FROM \"sys_user\" WHERE \"id\" = @id";

    Logger.Debug(tenantId, userId, () => $"[SysUser.DeleteAsync] SQL={deleteSql}");

    var pm = new NpgsqlParameter("id", NpgsqlDbType.Bigint) { Value = id };

    try
    {
        var result = await DB.DeleteAsync(conn, deleteSql, new[] { pm });
        Logger.Debug(tenantId, userId, () => $"[SysUser.DeleteAsync] 删除成功, 影响行数={result}");
        return result;
    }
    catch (Exception ex)
    {
        Logger.Error(tenantId, userId, $"[SysUser.DeleteAsync] 异常: {ex}");
        throw;
    }
}
```

### 7.4 `GetAsync` / `GetTxAsync`

```csharp
public static async Task<SysUser?> GetAsync(int tenantId, long userId, long id)
{
    Logger.Debug(tenantId, userId, () => $"[SysUser.GetAsync] 进入方法, id={id}");

    const string selectSql = "SELECT \"id\", \"name\", \"age\" FROM \"sys_user\" WHERE \"id\" = @id";

    Logger.Debug(tenantId, userId, () => $"[SysUser.GetAsync] SQL={selectSql}");

    var pm = new NpgsqlParameter("id", NpgsqlDbType.Bigint) { Value = id };

    try
    {
        var entity = await DB.GetAsync(conn, selectSql, new[] { pm }, reader =>
        {
            return new SysUser
            {
                Id = reader.GetInt64(0),
                Name = reader.GetString(1),
                Age = reader.IsDBNull(2) ? null : reader.GetInt32(2)
            };
        });

        Logger.Debug(tenantId, userId,
            () => $"[SysUser.GetAsync] 查询完成, 结果={(entity != null ? $"Id={entity.Id}, Name={entity.Name}" : "null")}");

        return entity;
    }
    catch (Exception ex)
    {
        Logger.Error(tenantId, userId, $"[SysUser.GetAsync] 异常: {ex}");
        throw;
    }
}
```

### 7.5 `GetListAsync` / `GetListTxAsync`

```csharp
public static async Task<List<SysUser>> GetListAsync(int tenantId, long userId)
{
    Logger.Debug(tenantId, userId, () => "[SysUser.GetListAsync] 进入方法");

    const string selectSql = "SELECT \"id\", \"name\", \"age\" FROM \"sys_user\"";

    Logger.Debug(tenantId, userId, () => $"[SysUser.GetListAsync] SQL={selectSql}");

    try
    {
        var list = await DB.GetListAsync(conn, selectSql, Array.Empty<NpgsqlParameter>(), reader =>
        {
            return new SysUser
            {
                Id = reader.GetInt64(0),
                Name = reader.GetString(1),
                Age = reader.IsDBNull(2) ? null : reader.GetInt32(2)
            };
        });

        Logger.Debug(tenantId, userId, () => $"[SysUser.GetListAsync] 查询完成, 行数={list.Count}");
        return list;
    }
    catch (Exception ex)
    {
        Logger.Error(tenantId, userId, $"[SysUser.GetListAsync] 异常: {ex}");
        throw;
    }
}
```

---

## 8. Source Generator — 实体描述类

**生成文件**：`{Entity}Desc.g.cs`
**生成类名**：`public static partial class {Entity}Desc`

### 8.1 生成内容

```csharp
/// <summary>SysUser 实体描述——编译期生成的元数据</summary>
public static partial class SysUserDesc
{
    /// <summary>数据库表名</summary>
    public const string Name = "sys_user";

    /// <summary>是否为多租户表</summary>
    public const bool IsTenant = true;

    /// <summary>字段名称常量</summary>
    public static class Fields
    {
        public const string Id = "id";
        public const string TenantId = "tenant_id";
        public const string Name = "name";
        public const string Email = "email";
        public const string Balance = "balance";
        public const string CreatedAt = "created_at";
        public const string Tags = "tags";
    }

    /// <summary>字段元数据字典</summary>
    public static readonly Dictionary<string, YTFieldMeta> DictFieldMetas = new()
    {
        [Fields.Id] = new YTFieldMeta
        {
            Name = "id", Type = "bigint", Length = 0, Precision = 0,
            IsNullable = false, IsPrimaryKey = true, IsTenant = false
        },
        [Fields.TenantId] = new YTFieldMeta
        {
            Name = "tenant_id", Type = "int", Length = 0, Precision = 0,
            IsNullable = false, IsPrimaryKey = false, IsTenant = true
        },
        [Fields.Name] = new YTFieldMeta
        {
            Name = "name", Type = "varchar", Length = 50, Precision = 0,
            IsNullable = false, IsPrimaryKey = false, IsTenant = false
        },
        // ... 其余字段同理
    };

    /// <summary>按字段名获取元数据</summary>
    public static YTFieldMeta GetFieldMeta(string fieldName)
    {
        return DictFieldMetas.TryGetValue(fieldName, out var meta)
            ? meta
            : throw new ArgumentException($"字段不存在: {fieldName}");
    }
}
```

---

## 9. Source Generator — 审计查询操作类

**生成文件**：`{Entity}AuditCRUD.g.cs`
**生成类名**：`public static partial class {Entity}AuditCRUD`

仅当实体标注 `NeedAuditTable = true` 时，Source Generator 才生成此类。

### 9.1 核心设计

审计查询系统提供三个层次的查询能力：

1. **单实体审计查询**：查询某个实体的审计历史记录（按主键、时间范围、操作类型等过滤）
2. **主从表联合审计查询**：查询主表及其所有从表的审计记录，生成完整业务操作时间线
3. **审计快照比较**：比较两条审计记录的快照差异，展示字段级变更

### 9.2 单实体审计查询

#### `GetAuditListAsync` — 分页查询审计记录列表

```csharp
/// <summary>
/// 查询 SysUser 审计记录列表（分页）。
/// 所有过滤条件为 AND 逻辑，null 表示不过滤。
/// </summary>
public static async Task<(List<AuditRecord> Records, int TotalCount)> GetAuditListAsync(
    int tenantId, long userId, AuditQueryFilter filter)
{
    Logger.Debug(tenantId, userId, () =>
        $"[SysUserAuditCRUD.GetAuditListAsync] 进入方法, filter.Id={filter.Id}, filter.Opt={filter.Opt}");

    // 动态构建 WHERE 条件
    var sb = new ValueStringBuilder(stackalloc char[512]);
    var paramList = new List<NpgsqlParameter>(8);
    var hasWhere = false;

    // 租户表必须加 tenant_id 条件
    sb.Append("WHERE \"tenant_id\" = @tenantId");
    paramList.Add(new NpgsqlParameter("tenantId", NpgsqlDbType.Integer) { Value = tenantId });
    hasWhere = true;

    if (filter.Id.HasValue)
    {
        sb.Append(" AND \"id\" = @id");
        paramList.Add(new NpgsqlParameter("id", NpgsqlDbType.Bigint) { Value = filter.Id.Value });
    }

    if (filter.Opt.HasValue)
    {
        sb.Append(" AND \"opt\" = @opt");
        paramList.Add(new NpgsqlParameter("opt", NpgsqlDbType.Char) { Value = (char)filter.Opt.Value });
    }

    if (filter.OperatorId.HasValue)
    {
        sb.Append(" AND \"operator_id\" = @operatorId");
        paramList.Add(new NpgsqlParameter("operatorId", NpgsqlDbType.Bigint) { Value = filter.OperatorId.Value });
    }

    if (filter.StartTime.HasValue)
    {
        sb.Append(" AND \"operated_at\" >= @startTime");
        paramList.Add(new NpgsqlParameter("startTime", NpgsqlDbType.TimestampTz) { Value = filter.StartTime.Value });
    }

    if (filter.EndTime.HasValue)
    {
        sb.Append(" AND \"operated_at\" <= @endTime");
        paramList.Add(new NpgsqlParameter("endTime", NpgsqlDbType.TimestampTz) { Value = filter.EndTime.Value });
    }

    var whereClause = sb.ToString();

    // 查询总数
    var countSql = $"SELECT COUNT(*) FROM \"sys_user_Audit\" {whereClause}";
    Logger.Debug(tenantId, userId, () => $"[SysUserAuditCRUD.GetAuditListAsync] CountSQL={countSql}");

    var countResult = await DB.GetScalarAsync<long>(countSql, paramList.ToArray(), tenantId, userId);
    var totalCount = (int)(countResult.Value ?? 0);

    // 查询数据（带分页）
    var pageSize = Math.Min(Math.Max(filter.PageSize, 1), 500);
    var offset = filter.PageIndex * pageSize;

    var dataSql = $"SELECT \"audit_id\", \"id\", \"opt\", \"operated_at\", \"operator_id\", \"tenant_id\", \"snapshot\" " +
                  $"FROM \"sys_user_Audit\" {whereClause} " +
                  $"ORDER BY \"operated_at\" DESC, \"audit_id\" DESC " +
                  $"LIMIT {pageSize} OFFSET {offset}";

    Logger.Debug(tenantId, userId, () => $"[SysUserAuditCRUD.GetAuditListAsync] DataSQL={dataSql}");

    var (result, records) = await DB.GetListAsync(dataSql, paramList.ToArray(), reader => new AuditRecord
    {
        AuditId = reader.GetInt64(0),
        Id = reader.GetInt64(1),
        Opt = (AuditOpt)reader.GetChar(2),
        OperatedAt = reader.GetDateTime(3),
        OperatorId = reader.GetInt64(4),
        TenantId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
        Snapshot = reader.GetString(6),
    }, tenantId, userId);

    Logger.Debug(tenantId, userId, () =>
        $"[SysUserAuditCRUD.GetAuditListAsync] 查询完成, totalCount={totalCount}, pageRecords={records?.Count ?? 0}");

    return (records ?? new List<AuditRecord>(), totalCount);
}
```

#### `GetAuditByIdAsync` — 获取单条审计记录

```csharp
/// <summary>根据审计记录主键获取单条审计记录</summary>
public static async Task<AuditRecord?> GetAuditByIdAsync(
    int tenantId, long userId, long auditId)
{
    Logger.Debug(tenantId, userId, () =>
        $"[SysUserAuditCRUD.GetAuditByIdAsync] 进入方法, auditId={auditId}");

    const string sql = "SELECT \"audit_id\", \"id\", \"opt\", \"operated_at\", \"operator_id\", " +
                       "\"tenant_id\", \"snapshot\" " +
                       "FROM \"sys_user_Audit\" WHERE \"audit_id\" = @auditId AND \"tenant_id\" = @tenantId";

    var pms = new[]
    {
        new NpgsqlParameter("auditId", NpgsqlDbType.Bigint) { Value = auditId },
        new NpgsqlParameter("tenantId", NpgsqlDbType.Integer) { Value = tenantId },
    };

    var entity = await DB.GetAsync(conn, sql, pms, reader => new AuditRecord
    {
        AuditId = reader.GetInt64(0),
        Id = reader.GetInt64(1),
        Opt = (AuditOpt)reader.GetChar(2),
        OperatedAt = reader.GetDateTime(3),
        OperatorId = reader.GetInt64(4),
        TenantId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
        Snapshot = reader.GetString(6),
    });

    Logger.Debug(tenantId, userId, () =>
        $"[SysUserAuditCRUD.GetAuditByIdAsync] 查询完成, found={entity != null}");

    return entity;
}
```

#### `GetAuditHistoryAsync` — 获取指定实体的完整审计历史

```csharp
/// <summary>
/// 获取指定原表主键 id 的完整审计历史（不分页，按时间正序）。
/// 适用于展示单条记录的完整变更轨迹。
/// </summary>
public static async Task<List<AuditRecord>> GetAuditHistoryAsync(
    int tenantId, long userId, long entityId)
{
    Logger.Debug(tenantId, userId, () =>
        $"[SysUserAuditCRUD.GetAuditHistoryAsync] 进入方法, entityId={entityId}");

    const string sql = "SELECT \"audit_id\", \"id\", \"opt\", \"operated_at\", \"operator_id\", " +
                       "\"tenant_id\", \"snapshot\" " +
                       "FROM \"sys_user_Audit\" " +
                       "WHERE \"id\" = @id AND \"tenant_id\" = @tenantId " +
                       "ORDER BY \"operated_at\" ASC, \"audit_id\" ASC";

    var pms = new[]
    {
        new NpgsqlParameter("id", NpgsqlDbType.Bigint) { Value = entityId },
        new NpgsqlParameter("tenantId", NpgsqlDbType.Integer) { Value = tenantId },
    };

    var (result, records) = await DB.GetListAsync(conn, sql, pms, reader => new AuditRecord
    {
        AuditId = reader.GetInt64(0),
        Id = reader.GetInt64(1),
        Opt = (AuditOpt)reader.GetChar(2),
        OperatedAt = reader.GetDateTime(3),
        OperatorId = reader.GetInt64(4),
        TenantId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
        Snapshot = reader.GetString(6),
    }, tenantId, userId);

    Logger.Debug(tenantId, userId, () =>
        $"[SysUserAuditCRUD.GetAuditHistoryAsync] 查询完成, count={records?.Count ?? 0}");

    return records ?? new List<AuditRecord>();
}
```

### 9.3 审计快照比较

#### `DiffAuditSnapshots` — 比较两条审计快照的字段差异

Source Generator 为每个实体生成字段级比较方法，基于 JSONB 文本按字段名逐一对比。

```csharp
/// <summary>
/// 比较两条审计记录的 Snapshot（JSONB），返回所有有差异的字段。
/// 适用于展示 Update 操作的字段级变更详情。
/// </summary>
/// <param name="tenantId">租户 ID</param>
/// <param name="userId">操作人</param>
/// <param name="oldSnapshot">变更前的快照 JSON 字符串</param>
/// <param name="newSnapshot">变更后的快照 JSON 字符串</param>
/// <returns>有差异的字段列表</returns>
public static List<AuditDiffField> DiffAuditSnapshots(
    int tenantId, long userId,
    string oldSnapshot, string newSnapshot)
{
    Logger.Debug(tenantId, userId, () =>
        "[SysUserAuditCRUD.DiffAuditSnapshots] 进入方法");

    var diffs = new List<AuditDiffField>();

    // 使用 System.Text.Json 的 JsonDocument 解析（NativeAOT 友好，不依赖反射）
    using var oldDoc = JsonDocument.Parse(oldSnapshot);
    using var newDoc = JsonDocument.Parse(newSnapshot);

    var oldRoot = oldDoc.RootElement;
    var newRoot = newDoc.RootElement;

    // SG 为每个字段生成比较代码
    // === 编译期已知所有字段名，逐个比较 ===

    // 字段: id
    {
        var oldVal = oldRoot.TryGetProperty("id", out var ov) ? ov.ToString() : null;
        var newVal = newRoot.TryGetProperty("id", out var nv) ? nv.ToString() : null;
        if (oldVal != newVal)
            diffs.Add(new AuditDiffField { FieldName = "id", OldValue = oldVal, NewValue = newVal });
    }

    // 字段: name
    {
        var oldVal = oldRoot.TryGetProperty("name", out var ov) ? ov.ToString() : null;
        var newVal = newRoot.TryGetProperty("name", out var nv) ? nv.ToString() : null;
        if (oldVal != newVal)
            diffs.Add(new AuditDiffField { FieldName = "name", OldValue = oldVal, NewValue = newVal });
    }

    // ... SG 为实体的每个字段生成相同模式的比较代码 ...

    Logger.Debug(tenantId, userId, () =>
        $"[SysUserAuditCRUD.DiffAuditSnapshots] 差异字段数={diffs.Count}");

    return diffs;
}
```

> **性能说明**：`DiffAuditSnapshots` 使用 `System.Text.Json.JsonDocument`（NativeAOT 友好），不使用反射。
> Source Generator 在编译期已知所有字段名，生成的代码是**静态字段名比较**，非动态遍历。

### 9.4 主从表联合审计查询

当实体存在 `[DetailOf]` 关系时，Source Generator 为**主表**生成联合审计查询方法。

#### 9.4.1 设计原则

- **以主表为入口**：用户通过主表的审计查询方法获取主表和所有从表的审计记录
- **从表通过 JSONB 中的外键字段关联**：审计表的 `snapshot` 包含完整记录，从表的外键值在 snapshot 中可查
- **时间范围对齐**：主表和从表使用相同的时间范围过滤
- **独立查询合并**：分别查询主表和各从表的审计表，在应用层合并结果

#### 9.4.2 SG 生成规则

Source Generator 发现某个实体有 `[DetailOf(typeof(Order))]` 标注的从表时，为主表（`Order`）生成：

1. **具体结果类**：`OrderMasterDetailAuditResult`（继承 `MasterDetailAuditResult`）
2. **联合查询方法**：`GetMasterDetailAuditAsync`

#### 9.4.3 生成的结果类

```csharp
/// <summary>
/// 订单主从表审计查询结果。
/// 由 Source Generator 根据 [DetailOf(typeof(Order))] 自动生成。
/// </summary>
public sealed class OrderMasterDetailAuditResult : MasterDetailAuditResult
{
    /// <summary>OrderItem 从表审计记录列表</summary>
    public List<AuditRecord> OrderItemRecords { get; init; } = new();

    // 如果有多个从表（如 OrderPayment），SG 会为每个从表生成对应属性：
    // public List<AuditRecord> OrderPaymentRecords { get; init; } = new();
}
```

#### 9.4.4 生成的联合查询方法

```csharp
public static partial class OrderAuditCRUD
{
    /// <summary>
    /// 查询订单的主从表联合审计记录。
    /// 按指定条件过滤主表审计记录，并自动拉取同一主键关联的所有从表审计记录。
    /// </summary>
    /// <param name="tenantId">租户 ID</param>
    /// <param name="userId">操作人</param>
    /// <param name="orderId">主表主键 id（查询该订单的完整审计轨迹）</param>
    /// <param name="filter">可选过滤条件（时间范围、操作类型等）。为 null 则查询全部。</param>
    /// <returns>主从表联合审计结果</returns>
    public static async Task<OrderMasterDetailAuditResult> GetMasterDetailAuditAsync(
        int tenantId, long userId, long orderId, AuditQueryFilter? filter = null)
    {
        Logger.Debug(tenantId, userId, () =>
            $"[OrderAuditCRUD.GetMasterDetailAuditAsync] 进入方法, orderId={orderId}");

        var result = new OrderMasterDetailAuditResult();

        // === 步骤 1：查询主表（Order）审计记录 ===
        var masterFilter = filter ?? new AuditQueryFilter();
        masterFilter.Id = orderId;  // 锁定主表主键

        var (masterRecords, masterTotal) = await OrderAuditCRUD.GetAuditListAsync(
            tenantId, userId, masterFilter);

        result.MasterRecords = masterRecords;
        result.TotalCount = masterTotal;

        Logger.Debug(tenantId, userId, () =>
            $"[OrderAuditCRUD.GetMasterDetailAuditAsync] 主表审计记录数={masterRecords.Count}");

        // === 步骤 2：查询从表（OrderItem）审计记录 ===
        // 通过 JSONB snapshot 中的外键字段 order_id 过滤
        {
            var detailSb = new ValueStringBuilder(stackalloc char[512]);
            var detailParams = new List<NpgsqlParameter>(6);

            detailSb.Append("SELECT \"audit_id\", \"id\", \"opt\", \"operated_at\", \"operator_id\", ");
            detailSb.Append("\"tenant_id\", \"snapshot\" ");
            detailSb.Append("FROM \"order_item_Audit\" ");
            detailSb.Append("WHERE \"tenant_id\" = @tenantId ");
            detailParams.Add(new NpgsqlParameter("tenantId", NpgsqlDbType.Integer) { Value = tenantId });

            // 通过 JSONB 中的外键字段筛选关联记录
            detailSb.Append("AND (snapshot->>'order_id')::bigint = @fkOrderId ");
            detailParams.Add(new NpgsqlParameter("fkOrderId", NpgsqlDbType.Bigint) { Value = orderId });

            // 应用时间范围过滤（与主表对齐）
            if (filter?.StartTime != null)
            {
                detailSb.Append("AND \"operated_at\" >= @startTime ");
                detailParams.Add(new NpgsqlParameter("startTime", NpgsqlDbType.TimestampTz)
                    { Value = filter.StartTime.Value });
            }
            if (filter?.EndTime != null)
            {
                detailSb.Append("AND \"operated_at\" <= @endTime ");
                detailParams.Add(new NpgsqlParameter("endTime", NpgsqlDbType.TimestampTz)
                    { Value = filter.EndTime.Value });
            }

            detailSb.Append("ORDER BY \"operated_at\" DESC, \"audit_id\" DESC");

            var detailSql = detailSb.ToString();
            Logger.Debug(tenantId, userId, () =>
                $"[OrderAuditCRUD.GetMasterDetailAuditAsync] 从表(OrderItem)SQL={detailSql}");

            var (detailResult, detailRecords) = await DB.GetListAsync(
                detailSql, detailParams.ToArray(), reader => new AuditRecord
                {
                    AuditId = reader.GetInt64(0),
                    Id = reader.GetInt64(1),
                    Opt = (AuditOpt)reader.GetChar(2),
                    OperatedAt = reader.GetDateTime(3),
                    OperatorId = reader.GetInt64(4),
                    TenantId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                    Snapshot = reader.GetString(6),
                }, tenantId, userId);

            result.OrderItemRecords = detailRecords ?? new List<AuditRecord>();

            Logger.Debug(tenantId, userId, () =>
                $"[OrderAuditCRUD.GetMasterDetailAuditAsync] 从表(OrderItem)记录数={result.OrderItemRecords.Count}");
        }

        // 如果有更多从表（如 OrderPayment），SG 会生成相同模式的查询代码块

        Logger.Debug(tenantId, userId, () =>
            $"[OrderAuditCRUD.GetMasterDetailAuditAsync] 完成, 主表={result.MasterRecords.Count}, 从表合计={result.OrderItemRecords.Count}");

        return result;
    }
}
```

#### 9.4.5 从表 JSONB 外键查询的索引优化

为提升从表审计查询性能，Source Generator 还应在 DAL 中为从表审计表生成 JSONB 外键索引：

```sql
-- SG 生成：在 OrderItem_Audit 表上创建 JSONB 外键索引
CREATE INDEX IF NOT EXISTS "idx_order_item_Audit_fk_order_id"
ON "order_item_Audit" (((snapshot->>'order_id')::bigint));
```

> 此索引在 `{DetailEntity}DAL.CreateTableIfNotExists` 方法中与审计表一起创建。

### 9.5 审计查询使用示例

```csharp
// === 1. 单实体审计查询 ===

// 查询 SysUser 审计记录（分页，按时间范围过滤）
var (records, total) = await SysUserAuditCRUD.GetAuditListAsync(tenantId, userId, new AuditQueryFilter
{
    StartTime = DateTime.UtcNow.AddDays(-7),
    EndTime = DateTime.UtcNow,
    PageIndex = 0,
    PageSize = 20,
});

// 查询某条用户记录的完整变更历史
var history = await SysUserAuditCRUD.GetAuditHistoryAsync(tenantId, userId, entityId: 12345);

// 获取单条审计记录
var auditRecord = await SysUserAuditCRUD.GetAuditByIdAsync(tenantId, userId, auditId: 999);

// === 2. 审计快照比较 ===
if (history.Count >= 2)
{
    var diffs = SysUserAuditCRUD.DiffAuditSnapshots(
        tenantId, userId,
        history[0].Snapshot,  // 旧快照
        history[1].Snapshot   // 新快照
    );
    // diffs: [{ FieldName="name", OldValue="张三", NewValue="李四" }]
}

// === 3. 主从表联合审计查询 ===
// 查询订单 #12345 的完整审计轨迹（包含所有订单明细的变更）
var orderAudit = await OrderAuditCRUD.GetMasterDetailAuditAsync(
    tenantId, userId, orderId: 12345,
    filter: new AuditQueryFilter
    {
        StartTime = DateTime.UtcNow.AddMonths(-1),
        EndTime = DateTime.UtcNow,
    });

// orderAudit.MasterRecords — 订单主表的审计记录
// orderAudit.OrderItemRecords — 订单明细从表的审计记录
foreach (var r in orderAudit.MasterRecords)
{
    Console.WriteLine($"[{r.OperatedAt:yyyy-MM-dd HH:mm:ss}] 主表 {r.Opt}: id={r.Id}");
}
foreach (var r in orderAudit.OrderItemRecords)
{
    Console.WriteLine($"[{r.OperatedAt:yyyy-MM-dd HH:mm:ss}] 明细 {r.Opt}: id={r.Id}");
}
```

---

## 10. 增量备份系统

### 10.1 核心设计

基于 `{Entity}_Log` 表实现变更追踪，独立后台线程定时同步到一个或多个目标库。

### 10.2 `IncrementalBackupOptions` 配置

```csharp
public sealed class IncrementalBackupOptions
{
    /// <summary>目标库连接地址列表（支持多目标库）</summary>
    public string[] TargetConnectionStrings { get; set; } = Array.Empty<string>();

    /// <summary>备份间隔（默认 30 秒）</summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(30);
}
```

### 10.3 执行流程

```
1. 后台线程按 Interval 定时触发
2. 从 {Entity}_Log 表查询变更记录：
   SELECT id, MAX(logid) AS max_logid, opt
   FROM "{Entity}_Log"
   GROUP BY id
   （获取每个 id 的最新操作）
3. 缓存当前批次的最大 logid
4. 检查目标库表结构一致性 → 调用 {Entity}DAL.CreateTableIfNotExists / EnsureColumnLength
5. 关闭目标库表的触发器
6. 对 I/U 操作：从源表批量读取数据 → 使用 UPSERT 批量写入目标库
7. 对 D 操作：在目标库执行批量删除
8. 恢复目标库表的触发器
9. 删除源 {Entity}_Log 中 logid <= max_logid 的记录
10. 所有步骤使用 Logger.Debug（`Func<string>` 延迟求值重载）记录
```

### 10.4 排除规则

- **不备份** `_Log` 结尾的表
- **不备份** `_Audit` 结尾的表（审计表仅保留在源库）

---

## 11. 租户分离系统

### 11.1 核心设计

将指定租户的数据从源库迁移到独立目标库。

### 11.2 `TenantSeparationOptions` 配置

```csharp
public sealed class TenantSeparationOptions
{
    /// <summary>目标库连接地址</summary>
    public string TargetConnectionString { get; set; } = "";

    /// <summary>要迁移的租户 ID 列表</summary>
    public int[] TenantIds { get; set; } = Array.Empty<int>();
}
```

### 11.3 执行流程

```
1. Logger.Info → 开始租户分离, 租户={tenantIds}
2. 检查目标库表结构 → 使用 {Entity}DAL 方法创建缺失表
3. 关闭目标库所有相关表的触发器
4. 租户表：迁移 WHERE tenant_id IN (@tenantIds) 的数据
5. 非租户表：全量迁移数据
6. 不迁移 _Log 结尾的表
7. 恢复目标库所有触发器
8. Logger.Info → 迁移完成
9. 每步使用 Logger.Debug（`Func<string>` 延迟求值重载）记录详细信息
10. 异常时使用 Logger.Error 记录完整堆栈
```

---

## 12. 日志集成规范（全局要求）

**所有生成方法和运行时方法都必须遵循以下日志规范**。

### 12.1 方法签名要求

所有执行方法（DAL / CRUD / 备份 / 分离）的前两个参数必须是：

```csharp
int tenantId, long userId
```

### 12.2 日志级别规范

| 场景 | 级别 | 模式 |
|------|------|------|
| 方法入口 | `Logger.Debug` | `() => $"[类名.方法名] 进入方法, 参数={值}"` |
| 参数构建 | `Logger.Debug` | `() => $"[类名.方法名] 参数: 字段名={值}"` |
| SQL 生成 | `Logger.Debug` | `() => $"[类名.方法名] SQL={sql}"` |
| 执行结果 | `Logger.Debug` | `() => $"[类名.方法名] 执行完成, 影响行数={n}"` |
| DDL 操作 | `Logger.Info` | `[类名.方法名] 创建表/修改字段/创建索引: {details}` |
| 执行耗时 | `Logger.Debug` | `() => $"[类名.方法名] 耗时={elapsed}ms"` |
| 异常捕获 | `Logger.Error` | `[类名.方法名] 异常: {ex}`（包含完整堆栈） |
| DDL 异常 | `Logger.Fatal` | `[类名.方法名] 创建失败/修改失败: {ex}` → `Environment.FailFast` |

### 12.3 日志示例

```csharp
// 方法入口——记录所有参数的即时值（使用 Func<string> 延迟求值）
Logger.Debug(tenantId, userId,
    () => $"[SysUser.InsertAsync] 进入方法, name={name}, age={age}, email={email}");

// 参数构建
Logger.Debug(tenantId, userId,
    () => "[SysUser.InsertAsync] 构建参数: name=@name(Text), age=@age(Integer)");

// SQL 生成
Logger.Debug(tenantId, userId,
    () => $"[SysUser.InsertAsync] SQL={insertSql}");

// 执行结果
Logger.Debug(tenantId, userId,
    () => $"[SysUser.InsertAsync] 插入成功, 影响行数={result}");

// DDL 操作
Logger.Info(tenantId, userId,
    $"[SysUserDAL.CreateTableIfNotExists] 创建表成功: sys_user");

// 异常——必须捕获完整堆栈
Logger.Error(tenantId, userId,
    $"[SysUser.InsertAsync] 异常: {ex}");
```

---

## 13. 性能与内存优化规范

### 13.1 SQL 字符串

| 场景 | 策略 |
|------|------|
| Insert / Delete / Get / GetList | `const string sql = "..."` — 编译期常量 |
| Update（动态字段） | `ValueStringBuilder` + 栈分配 `stackalloc char[256]` |
| DDL（建表 / 建索引） | `const string sql = "..."` — 每个 DDL 语句独立常量 |

### 13.2 参数数组

```csharp
// 已知参数数量时，预分配精确容量
var paramList = new List<NpgsqlParameter>(fieldCount);

// 单参数场景直接 new[]
var pm = new[] { new NpgsqlParameter("id", NpgsqlDbType.Bigint) { Value = id } };
```

### 13.3 通用原则

- 禁止 `string.Format`，使用字符串插值或 `ValueStringBuilder`
- 禁止 `StringBuilder`，使用 `ValueStringBuilder`（零堆分配）
- 禁止 LINQ（`Where`、`Select`、`ToList` 等），使用 `for` / `foreach` 循环
- 禁止反射（`typeof(T).GetProperties()` 等），所有类型信息在编译期解析
- 禁止 `dynamic`
- 禁止 `Expression<Func<T>>` 和表达式树
- Reader 映射使用索引访问（`reader.GetInt32(0)`），不使用列名查找

---

## 14. i18n 国际化就绪

用户可见的错误消息使用**常量 key** 模式，预留国际化扩展点：

```csharp
/// <summary>错误消息常量——预留 i18n 扩展</summary>
internal static class EntityErrors
{
    public const string FieldNotFound = "ENTITY_FIELD_NOT_FOUND";          // 字段不存在: {0}
    public const string PrimaryKeyRequired = "ENTITY_PK_REQUIRED";        // 主键不能为空
    public const string UpdateNoFields = "ENTITY_UPDATE_NO_FIELDS";       // 没有需要更新的字段
    public const string TableAlreadyExists = "ENTITY_TABLE_EXISTS";       // 表已存在: {0}
    public const string CreateTableFailed = "ENTITY_CREATE_TABLE_FAILED"; // 建表失败: {0}
}
```

后续可通过 i18n 模块（参考 `i18n-prompt.md`）将 key 映射为多语言消息。

---

## 15. 最终质量标准

若设计上有取舍，请遵循以下优先级：

**正确性 > 语义完整性 > 安全性 > 可维护性 > 可读性 > 复杂度可控 > 性能 > 易用性 > 技巧炫技**

### 验收标准（你必须自检）

在给出最终答案前，逐条确认：

- [ ] 无 LINQ / 反射 / dynamic / Expression Tree
- [ ] 所有生成代码完全兼容 NativeAOT
- [ ] `DbNullable<T>` 三态逻辑正确：未设置 / 设置为 null / 设置为值
- [ ] 所有 CRUD 方法都有 `Async` 和 `TxAsync` 变体
- [ ] 所有 CRUD 方法都有打散参数和实体参数两套重载
- [ ] Insert / Delete / Get / GetList 使用 `const string sql`
- [ ] Update 使用 `ValueStringBuilder` 动态构建
- [ ] 所有方法前两个参数为 `int tenantId, long userId`
- [ ] 每个方法的每个执行步骤都有 `Logger.Debug` 调用（使用 `Func<string>` 延迟求值重载）
- [ ] DDL 操作使用 `Logger.Info`
- [ ] DDL 异常使用 `Logger.Fatal` 并调用 `Environment.FailFast` 终止进程
- [ ] 其他异常捕获使用 `Logger.Error` 并包含完整堆栈：`$"[方法名] 异常: {ex}"`
- [ ] 租户表判定规则：包含 `TenantId` 属性即为租户表
- [ ] 租户表使用分区表创建语句
- [ ] 审计表包含 snapshot（JSONB）字段
- [ ] `{Entity}_Log` 和 `{Entity}_Audit` 触发器逻辑正确合并
- [ ] 增量备份不备份 `_Log` 结尾的表
- [ ] 租户分离不迁移 `_Log` 结尾的表
- [ ] Source Generator 目标框架为 netstandard2.0
- [ ] 运行时库目标框架为 net10.0
- [ ] 类型映射覆盖所有列出的 CLR → PostgreSQL 映射
- [ ] 错误消息使用常量 key（i18n 就绪）
- [ ] 实体描述类包含 Fields 常量类、DictFieldMetas 字典、GetFieldMeta 方法
- [ ] `AuditOpt`、`AuditRecord`、`AuditQueryFilter`、`AuditDiffField`、`MasterDetailAuditResult` 类型完整实现
- [ ] `DetailOfAttribute` 特性支持声明主从关系
- [ ] 审计查询：`GetAuditListAsync`（分页）、`GetAuditByIdAsync`、`GetAuditHistoryAsync` 三个方法
- [ ] 审计快照比较：`DiffAuditSnapshots` 方法使用 `System.Text.Json.JsonDocument`（非反射）
- [ ] 主从表联合审计查询：`GetMasterDetailAuditAsync` 方法通过 JSONB 外键关联
- [ ] 主从表审计的 JSONB 外键索引在 DAL 中生成
- [ ] 审计查询所有方法包含完整的 `Logger.Debug` 调用

若任一项不满足，先修正再输出完整代码。

---

## 16. 最终指令

请现在开始实现，直接交付：

1. **完整源码**——按§3 项目结构组织，使用如下格式逐文件输出：

```text
// file: 相对路径
<完整内容>
```

2. **完整测试**——覆盖所有核心类型和生成器逻辑
3. **完整设计说明**——简短描述关键设计决策

不要只给思路。
不要偷懒省略。
不要输出 demo 级方案。
不要输出伪代码或片段。
不要要求我"自行补充"。
代码必须可直接复制构建。
必须按**生产级顶级标准**完成。
