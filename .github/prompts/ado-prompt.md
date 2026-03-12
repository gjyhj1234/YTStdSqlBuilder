# 面向 Claude Opus 4.6 的 ADO 数据访问层生产级实现提示词

## PostgreSQL / Npgsql 专用，高性能、低 GC、AOT 友好的 ADO 数据访问层

你是一名**顶级 .NET 基础架构工程师 / PostgreSQL & Npgsql 专家 / 高性能框架作者**。
请在 **.NET 10.0 + NativeAOT** 环境下，实现一套**极致轻量、高性能、低内存、零反射、零动态、零 LINQ 依赖**的 PostgreSQL ADO 数据访问层类库。

这不是 demo，不是玩具组件，而是**框架基座级实现**。
请严格按"**生产级顶级标准**"完成完整代码、测试与设计说明。

你的输出必须是：**完整可编译的代码**（非伪代码）、完整的项目文件、中文规范注释、以及最小可运行示例。
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
| `PgSqlRenderResult` | YTStdSqlBuilder | SQL 构建结果（`.Sql` + `.Params`） |
| `PgSqlParam` | YTStdSqlBuilder | SQL 参数（`.Name`, `.Value`, `.DbType`） |
| `Logger` | YTStdLogger.Core | 全局静态日志门面 |
| `LogLevel` | YTStdLogger | 日志等级枚举 |

### 关键交互模式

ADO 模块是 SQL Builder 的执行层，接收 `PgSqlRenderResult` 并执行：

```csharp
// SQL Builder 构建查询
var query = PgSql
    .Select(user["id"], user["name"])
    .From(user)
    .Where(user["age"], Op.Gte, Param.Value(18))
    .Build();

// ADO 模块执行查询 — 使用 query.Sql 和 query.Params
var result = await DB.GetListAsync(query.Sql, query.Params, reader => new {
    Id = reader.GetInt32(0),
    Name = reader.GetString(1)
}, tenantId, userId);
```

> **补充上下文**：实现本模块时，建议同时查阅以下源文件：
> - `src/YTStdSqlBuilder/PgSqlRenderResult.cs` — 了解 SQL 渲染结果结构
> - `src/YTStdLogger/Core/Logger.cs` — 了解 Logger 静态门面 API

---

## 1. 最终目标

实现一套生产级 PostgreSQL ADO 数据访问层，具备以下能力：

1. **连接池管理** — 自定义连接池，支持最大/最小连接数、超时、重试、注销
2. **CRUD 操作** — 完整的增删改查方法，支持独立事务和批量事务两种模式
3. **DDL 操作** — 表创建、字段修改、索引创建，均有前置检查
4. **全链路日志** — 所有方法通过 `Logger` 记录完整的执行过程与异常
5. **高性能** — 最小化内存分配，适合低配云服务器

---

## 2. 技术约束

### 2.1 运行环境

| 属性 | 值 |
|------|-----|
| 目标框架 | `net10.0` |
| 发布方式 | `NativeAOT` |
| 数据库 | PostgreSQL（Npgsql） |
| NuGet 依赖 | `Npgsql` |
| 项目引用 | `YTStdSqlBuilder`、`YTStdLogger` |

### 2.2 禁止项（硬性）

- 禁止 `LINQ`（`System.Linq`）
- 禁止反射（`System.Reflection`）
- 禁止 `dynamic`
- 禁止序列化框架
- 禁止引入未要求的第三方 NuGet 包

### 2.3 性能要求

- 支持在低配云服务器（1 核 1G 内存）上稳定运行
- 最小化内存分配，优先使用 `ValueStringBuilder`、`stackalloc`、`ArrayPool<T>`
- 减少缓存，仅获取必要数据
- 连接池复用，避免频繁创建/销毁连接

---

## 3. 项目结构

生成的代码应放置在以下位置：

| 项目 | 路径 | 说明 |
|------|------|------|
| 主项目 | `src/YTStdAdo/` | ADO 数据访问层运行时库 |
| 测试 | `tests/YTStdAdo.Tests/` | 单元测试 |
| 示例 | `samples/YTStdAdo.Sample/` | 使用示例 |

---

## 4. 核心类型定义

### 4.1 返回结构

```csharp
/// <summary>更新/删除/查询 统一返回结构</summary>
public sealed class DbUdqResult
{
    public bool Success { get; init; }
    public int RowsAffected { get; init; }
    public string? ErrorMessage { get; init; }
    public string? DebugMessage { get; init; }
}

/// <summary>插入操作返回结构</summary>
public sealed class DbInsResult
{
    public bool Success { get; init; }
    public long Id { get; init; }
    public string? ErrorMessage { get; init; }
    public string? DebugMessage { get; init; }
}

/// <summary>标量查询返回结构</summary>
public sealed class DbScalarResult<T>
{
    public bool Success { get; init; }
    public T? Value { get; init; }
    public string? ErrorMessage { get; init; }
    public string? DebugMessage { get; init; }
}
```

### 4.2 DDL 状态枚举

```csharp
public enum DDLStatus
{
    Success,
    Failed,
    Existed
}
```

### 4.3 数据库元数据类型

```csharp
/// <summary>数据库表字段信息</summary>
public sealed class DbField
{
    public string FieldName { get; init; } = "";
    public string DataType { get; init; } = "";
    public int? MaxLength { get; init; }
    public int? NumericPrecision { get; init; }
    public int? NumericScale { get; init; }
    public bool IsNullable { get; init; }
    public bool IsPrimaryKey { get; init; }
}

/// <summary>数据库表索引信息</summary>
public sealed class DbIndex
{
    public string IndexName { get; init; } = "";
    public string TableName { get; init; } = "";
    public bool IsUnique { get; init; }
    public string[] Columns { get; init; } = Array.Empty<string>();
}
```

---

## 5. 连接池设计

### 5.1 配置项（`DbOptions`）

```csharp
public sealed class DbOptions
{
    /// <summary>PostgreSQL 主机地址</summary>
    public string Host { get; init; } = "localhost";
    
    /// <summary>PostgreSQL 端口</summary>
    public int Port { get; init; } = 5432;
    
    /// <summary>数据库名称</summary>
    public string Database { get; init; } = "";
    
    /// <summary>数据库用户名</summary>
    public string Username { get; init; } = "";
    
    /// <summary>数据库密码</summary>
    public string Password { get; init; } = "";
    
    /// <summary>最小连接数（初始化时自动创建）</summary>
    public int MinPoolSize { get; init; } = 2;

    /// <summary>最大连接数</summary>
    public int MaxPoolSize { get; init; } = 20;

    /// <summary>连接超时时间（秒）</summary>
    public int ConnectionTimeoutSeconds { get; init; } = 30;

    /// <summary>连接重试次数</summary>
    public int RetryCount { get; init; } = 3;

    /// <summary>连接池中空闲连接的注销时间（秒）</summary>
    public int IdleTimeoutSeconds { get; init; } = 300;
    
    /// <summary>
    /// 构建 Npgsql 连接字符串。对象化设置避免拼接字符串导致错误。
    /// </summary>
    public string BuildConnectionString()
    {
        return $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password};" +
               $"Timeout={ConnectionTimeoutSeconds};Command Timeout={ConnectionTimeoutSeconds}";
    }
}
```

### 5.2 连接池要求

- 初始化时自动创建 `MinPoolSize` 个连接
- 达到 `MaxPoolSize` 后等待或抛出超时异常
- 空闲连接超过 `IdleTimeoutSeconds` 后自动回收（但保持最小数量）
- 连接获取失败时按 `RetryCount` 重试
- 连接归还时验证连接状态，无效连接丢弃并补充
- 使用 `ConcurrentQueue` 或类似无锁结构管理空闲连接
- 连接池的所有关键操作必须记录 `Logger.Debug` 日志

---

## 6. DB 静态类 — 公共 API 设计

`DB` 是全局静态入口类，所有方法均为 `public static`。

### 6.1 初始化

```csharp
public static class DB
{
    /// <summary>初始化连接池，应用启动时调用一次</summary>
    public static void Init(DbOptions options);

    /// <summary>优雅关闭连接池</summary>
    public static ValueTask ShutdownAsync();
}
```

### 6.2 事务管理

#### `GetBatchAsync` — 获取事务批处理

```csharp
/// <summary>
/// 从连接池获取连接，创建事务和 NpgsqlBatch，并自动执行 set_config 设置 userId。
/// </summary>
public static async ValueTask<NpgsqlBatch> GetBatchAsync(int tenantId, long userId)
```

**执行过程**：

```csharp
// 1. 从连接池获取连接
var conn = GetConnection();
Logger.Debug(tenantId, userId, () => $"[GetBatchAsync] 从连接池获取连接成功");

// 2. 创建事务
var tx = await conn.BeginTransactionAsync();
Logger.Debug(tenantId, userId, () => $"[GetBatchAsync] 事务创建成功");

// 3. 创建 NpgsqlBatch 并关联事务
var batch = new NpgsqlBatch(conn, tx);

// 4. 执行 set_config 设置 userId（用于触发器中获取操作人）
var cmd1 = new NpgsqlBatchCommand("SELECT set_config('app.user_id', @userId::text, true)");
cmd1.Parameters.AddWithValue("userId", userId);
batch.BatchCommands.Add(cmd1);

Logger.Debug(tenantId, userId, () => $"[GetBatchAsync] 已添加 set_config 命令，userId={userId}");

// 5. 返回 batch
return batch;
```

#### `GetConnection` — 从连接池获取连接

```csharp
/// <summary>从连接池获取一个可用连接</summary>
public static NpgsqlConnection GetConnection()
```

#### `ReturnConnection` — 归还连接到连接池

```csharp
/// <summary>归还连接到连接池。验证连接状态，无效连接丢弃并补充新连接。</summary>
public static void ReturnConnection(NpgsqlConnection conn)
```

#### `BatchCommitAsync` — 批量提交事务

```csharp
/// <summary>提交事务中的所有批处理命令</summary>
public static async ValueTask<DbUdqResult> BatchCommitAsync(NpgsqlBatch batch)
```

**执行过程**：
1. 记录 `Logger.Debug` — 开始提交，命令数量
2. `await batch.ExecuteNonQueryAsync()`
3. `await batch.Transaction!.CommitAsync()`
4. 记录执行时间
5. 归还连接到连接池
6. 异常时：回滚事务、记录 `Logger.Error`、归还连接、返回失败结果

### 6.3 CRUD 操作

#### 6.3.1 插入数据

```csharp
/// <summary>独立事务插入（自动获取事务、提交、归还连接）</summary>
public static async ValueTask<DbInsResult> InsertAsync(
    string sql, PgSqlParam[] parameters, int tenantId, long userId)

/// <summary>在已有事务中追加插入命令（不提交）</summary>
public static ValueTask<DbInsResult> InsertTxAsync(
    NpgsqlBatch batch, string sql, PgSqlParam[] parameters, int tenantId, long userId)
```

**独立事务（`InsertAsync`）执行过程**：

```csharp
Stopwatch? sw = Logger.IsTenantDebugEnabled(tenantId) ? Stopwatch.StartNew() : null;
Logger.Debug(tenantId, userId, () => $"[InsertAsync] 开始执行，SQL={sql}");

// 1. 获取事务批处理
var batch = await GetBatchAsync(tenantId, userId);

// 2. 构建 NpgsqlBatchCommand 并添加参数
var cmd = new NpgsqlBatchCommand(sql);
for (int i = 0; i < parameters.Length; i++)
{
    var p = parameters[i];
    Logger.Debug(tenantId, userId, () =>
        $"[InsertAsync] 参数[{i}]: Name={p.Name}, Value={p.Value}, DbType={p.DbType}");
    // PgSqlParam 必须指定 DbType 以提升 Npgsql 参数绑定性能
    cmd.Parameters.AddWithValue(p.Name, p.DbType, p.Value ?? DBNull.Value);
}
batch.BatchCommands.Add(cmd);

// 3. 执行并提交
// 注意：INSERT 语句应包含 RETURNING id，使用 ExecuteReaderAsync 获取返回的 ID
using var reader = await batch.ExecuteReaderAsync();
long id = 0;
if (await reader.ReadAsync())
    id = reader.GetInt64(0);
await reader.CloseAsync();
await batch.Transaction!.CommitAsync();

sw?.Stop();
Logger.Debug(tenantId, userId, () =>
    $"[InsertAsync] 执行完成，耗时={sw?.ElapsedMilliseconds ?? 0}ms，Id={id}");

// 4. 归还连接
ReturnConnection(batch.Connection!);
```

**事务模式（`InsertTxAsync`）**：
- 使用传入的 `batch` 构建 `NpgsqlBatchCommand`
- 仅添加命令，**不提交**
- 记录 `Logger.Debug` 日志

#### 6.3.2 更新数据

```csharp
/// <summary>独立事务更新</summary>
public static async ValueTask<DbUdqResult> UpdateAsync(
    string sql, PgSqlParam[] parameters, int tenantId, long userId)

/// <summary>在已有事务中追加更新命令（不提交）</summary>
public static ValueTask UpdateTxAsync(
    NpgsqlBatch batch, string sql, PgSqlParam[] parameters, int tenantId, long userId)
```

执行过程同 `InsertAsync`，区别在于返回 `DbUdqResult`（`RowsAffected` 为受影响行数）。

#### 6.3.3 删除数据

```csharp
/// <summary>独立事务删除</summary>
public static async ValueTask<DbUdqResult> DeleteAsync(
    string sql, PgSqlParam[] parameters, int tenantId, long userId)

/// <summary>在已有事务中追加删除命令（不提交）</summary>
public static ValueTask DeleteTxAsync(
    NpgsqlBatch batch, string sql, PgSqlParam[] parameters, int tenantId, long userId)
```

执行过程同 `UpdateAsync`。

#### 6.3.4 查询多行数据

```csharp
/// <summary>独立连接查询多行，使用 mapper 映射</summary>
public static async ValueTask<(DbUdqResult Result, List<T>? Data)> GetListAsync<T>(
    string sql, PgSqlParam[] parameters,
    Func<NpgsqlDataReader, T> mapper,
    int tenantId, long userId)

/// <summary>在已有事务中查询多行</summary>
public static async ValueTask<(DbUdqResult Result, List<T>? Data)> GetListTxAsync<T>(
    NpgsqlBatch batch, string sql, PgSqlParam[] parameters,
    Func<NpgsqlDataReader, T> mapper,
    int tenantId, long userId)

/// <summary>独立连接查询多行，直接写入 Utf8JsonWriter（零 DTO 分配）</summary>
public static async ValueTask<DbUdqResult> GetListAsync(
    string sql, PgSqlParam[] parameters,
    Utf8JsonWriter writer, Action<Utf8JsonWriter, NpgsqlDataReader> writerMap,
    int tenantId, long userId)
```

**`GetListAsync<T>` 执行过程**：

```csharp
Stopwatch? sw = Logger.IsTenantDebugEnabled(tenantId) ? Stopwatch.StartNew() : null;
Logger.Debug(tenantId, userId, () => $"[GetListAsync] 开始执行，SQL={sql}");

// 1. 获取连接
var conn = GetConnection();

// 2. 创建 NpgsqlCommand 并添加参数
using var cmd = new NpgsqlCommand(sql, conn);
for (int i = 0; i < parameters.Length; i++)
{
    // ... 添加参数并记录 Logger.Debug
}

// 3. 执行并读取
using var reader = await cmd.ExecuteReaderAsync();
var list = new List<T>();
int rowCount = 0;
while (await reader.ReadAsync())
{
    list.Add(mapper(reader));
    rowCount++;
}

sw?.Stop();
Logger.Debug(tenantId, userId, () =>
    $"[GetListAsync] 执行完成，行数={rowCount}，耗时={sw?.ElapsedMilliseconds ?? 0}ms");

// 4. 归还连接
ReturnConnection(conn);

return (new DbUdqResult { Success = true, RowsAffected = rowCount }, list);
```

#### 6.3.5 查询标量值

```csharp
/// <summary>独立连接查询单个值</summary>
public static async ValueTask<DbScalarResult<T>> GetScalarAsync<T>(
    string sql, PgSqlParam[] parameters, int tenantId, long userId)

/// <summary>在已有事务中查询单个值</summary>
public static async ValueTask<DbScalarResult<T>> GetScalarTxAsync<T>(
    NpgsqlBatch batch, string sql, PgSqlParam[] parameters, int tenantId, long userId)
```

### 6.4 DDL 操作（表结构管理）

所有 DDL 方法不接受 `tenantId`/`userId` 参数，内部使用 `tenantId = 0, userId = 0` 记录日志。

#### 6.4.1 查询表信息

```csharp
/// <summary>检查数据表是否存在，RowsAffected == 1 表示存在</summary>
public static async ValueTask<DbUdqResult> GetTableInfor(string tableName)
```

内部调用 `GetListAsync`，查询 `information_schema.tables`。

#### 6.4.2 查询字段信息

```csharp
/// <summary>获取数据表字段信息列表</summary>
public static async ValueTask<(DbUdqResult Result, List<DbField>? Data)> GetFieldsInfor(
    string tableName, Func<NpgsqlDataReader, DbField> mapper)
```

内部调用 `GetListAsync`，查询 `information_schema.columns`。

#### 6.4.3 查询索引信息

```csharp
/// <summary>获取数据表索引信息列表</summary>
public static async ValueTask<(DbUdqResult Result, List<DbIndex>? Data)> GetIndexesInfor(
    string tableName, Func<NpgsqlDataReader, DbIndex> mapper)
```

内部调用 `GetListAsync`，查询 `pg_indexes`。

#### 6.4.4 创建表

```csharp
/// <summary>创建数据库表（存在则返回 Existed）</summary>
public static async ValueTask<DDLStatus> CreateTable(string tableName, string sql)
```

**执行过程**：

```csharp
Logger.Info(0, 0, $"[CreateTable] 开始创建表: {tableName}");

// 1. 检查表是否存在
var exists = await GetTableInfor(tableName);
if (exists.RowsAffected == 1)
{
    Logger.Info(0, 0, $"[CreateTable] 表已存在: {tableName}");
    return DDLStatus.Existed;
}

// 2. 执行建表 SQL
try
{
    var conn = GetConnection();
    using var cmd = new NpgsqlCommand(sql, conn);
    await cmd.ExecuteNonQueryAsync();
    ReturnConnection(conn);
    Logger.Info(0, 0,
        $"[CreateTable] 创建成功: {tableName}，SQL={sql}");
    return DDLStatus.Success;
}
catch (Exception ex)
{
    Logger.Fatal(0, 0,
        $"[CreateTable] 创建失败: {tableName}，SQL={sql}，异常: {ex}");
    Environment.FailFast($"DDL 操作失败，程序终止: {ex.Message}");
    return DDLStatus.Failed; // unreachable
}
```

#### 6.4.5 修改表字段

```csharp
/// <summary>
/// 修改数据库表字段。字段不存在则创建；
/// 字段存在时仅当新长度 > 原长度时才修改，否则返回 Failed。
/// </summary>
public static async ValueTask<DDLStatus> AlterTable(
    string tableName, string fieldName,
    string dataType, string length, string precision,
    bool nullable, bool isPrimaryKey)
```

**业务逻辑**：
1. 调用 `GetFieldsInfor` 查询字段是否存在
2. 字段不存在 → 执行 `ALTER TABLE ... ADD COLUMN ...` → 返回 `Success`
3. 字段存在 → 比较类型/长度/精度
   - 新长度 > 原始长度 → 执行 `ALTER TABLE ... ALTER COLUMN ...` → 返回 `Success`
   - 其他情况 → 返回 `Failed`
4. 无论成功失败，均记录完整日志（执行时间、SQL、执行状态）

#### 6.4.6 创建索引

```csharp
/// <summary>创建数据库表索引（已存在则返回 Existed）</summary>
public static async ValueTask<DDLStatus> CreateIndex(
    string indexName, string tableName, string fieldNames, bool unique)
```

**执行过程**：
1. 查询索引是否已存在
2. 已存在 → 返回 `Existed`
3. 不存在 → 执行 `CREATE [UNIQUE] INDEX ...` → 成功返回 `Success`，失败返回 `Failed`
4. 必须记录完整日志

---

## 7. 日志集成规范（关键章节）

### 7.1 Logger API 参考

本模块使用 `YTStdLogger.Core.Logger` 静态门面类。核心 API：

```csharp
// 日志记录
Logger.Debug(int tenantId, long userId, string message)
Logger.Info(int tenantId, long userId, string message)
Logger.Warn(int tenantId, long userId, string message)
Logger.Error(int tenantId, long userId, string message)
Logger.Fatal(int tenantId, long userId, string message)

// 延迟求值重载（避免未启用时创建字符串）
Logger.Debug(int tenantId, long userId, Func<string> messageFactory)
Logger.Info(int tenantId, long userId, Func<string> messageFactory)
Logger.Error(int tenantId, long userId, Func<string> messageFactory)
Logger.Fatal(int tenantId, long userId, Func<string> messageFactory)

// 运行时租户级 Debug 开关
Logger.EnableTenantDebug(int tenantId)
Logger.DisableTenantDebug(int tenantId)
Logger.IsTenantDebugEnabled(int tenantId)
```

### 7.2 日志记录原则

1. **所有执行方法必须包含 `Logger.Debug` 调用**，记录每个关键步骤的即时值
2. **方法签名含 `int tenantId, long userId` 的方法** — 直接传递给 Logger
3. **方法签名不含 `tenantId`/`userId` 的方法**（DDL 操作等）— 使用 `tenantId = 0, userId = 0`
4. **运行时可通过 `Logger.EnableTenantDebug(tenantId)` 开启特定租户的 Debug 追踪**
5. **Debug 日志仅在 CRUD 操作过程中使用。DDL 操作使用 `Logger.Info` 记录执行过程。**

### 7.3 Debug 日志必须覆盖的步骤

每个执行方法的 Debug 日志必须覆盖以下环节：

| 步骤 | 日志内容 | 示例 |
|------|---------|------|
| 方法入口 | 方法名、SQL、参数数量 | `[InsertAsync] 开始执行，SQL=INSERT INTO...，参数数量=3` |
| 参数详情 | 每个参数的名称、值、DbType | `[InsertAsync] 参数[0]: Name=@p0, Value=张三, DbType=Text` |
| 连接获取 | 连接池状态 | `[InsertAsync] 从连接池获取连接成功` |
| 事务创建 | 事务状态 | `[InsertAsync] 事务创建成功` |
| SQL 执行 | 执行开始 | `[InsertAsync] 开始执行 SQL` |
| 执行结果 | 受影响行数/返回 ID/读取行数 | `[InsertAsync] 执行完成，Id=12345` |
| 执行耗时 | 毫秒级计时 | `[InsertAsync] 耗时=23ms` |
| 连接归还 | 归还确认 | `[InsertAsync] 连接已归还连接池` |

### 7.4 错误日志规范

异常发生时，必须使用 `Logger.Error` 记录以下所有信息：

```csharp
catch (Exception ex)
{
    sw?.Stop();

    // ADO 层日志不使用国际化，直接使用中文
    Logger.Error(tenantId, userId,
        $"[InsertAsync] 执行异常: {ex}");

    return new DbInsResult
    {
        Success = false,
        // ErrorMessage 使用国际化语言，用于返回前端展示（按租户语言偏好）
        ErrorMessage = K.Db.InsertFailed.Db(tenantId),
        // DebugMessage 是堆栈信息，用于开发与调试，不使用国际化
        DebugMessage = $"SQL={sql}，操作人=tenantId:{tenantId}/userId:{userId}，异常={ex}"
    };
}
```

**错误日志必须包含**：
- 完整异常信息（`ex.ToString()`，包含堆栈跟踪）
- 原始 SQL 语句
- 所有参数的名称和值
- 操作人信息（`tenantId`、`userId`）

**ADO 层日志国际化规则**：
- ADO 层内部日志（`Logger.Info` / `Logger.Debug` / `Logger.Error`）**不使用国际化**，直接使用中文字符串
- 仅 CRUD 返回结果中的 `ErrorMessage` 使用 `K.Db.InsertFailed.Db(tenantId)` 等数组索引方式进行国际化，用于前端展示
- `DebugMessage` 包含堆栈跟踪等调试信息，不使用国际化

### 7.5 辅助方法 — `BuildDebugInfo`

使用 `ValueStringBuilder` 拼接调试信息，避免多次字符串分配：

```csharp
/// <summary>将参数值格式化为 SQL 字面量（用于生成可执行 SQL）</summary>
private static string FormatParamValue(object? value)
{
    if (value is null or DBNull) return "NULL";
    if (value is string s) return $"'{s.Replace("'", "''")}'";
    if (value is bool b) return b ? "true" : "false";
    if (value is DateTime dt) return $"'{dt:yyyy-MM-dd HH:mm:ss.fff}'";
    if (value is DateTimeOffset dto) return $"'{dto:yyyy-MM-dd HH:mm:ss.fffzzz}'";
    return value.ToString() ?? "NULL";
}

/// <summary>拼接 SQL 调试信息（参数替换为实际值的可执行 SQL）</summary>
private static string BuildDebugInfo(
    string sql, PgSqlParam[] parameters,
    int tenantId, long userId, long elapsedMs)
{
    // 使用 stackalloc + ValueStringBuilder 构建
    Span<char> buffer = stackalloc char[512];
    var sb = new ValueStringBuilder(buffer);

    sb.Append("ExecutableSQL=");
    // 将参数占位符替换为实际值，生成可直接在 pgAdmin 执行的 SQL
    var executableSql = sql;
    for (int i = parameters.Length - 1; i >= 0; i--)
    {
        // 反向替换避免 @p1 替换掉 @p10 中的 @p1
        executableSql = executableSql.Replace(parameters[i].Name,
            FormatParamValue(parameters[i].Value));
    }
    sb.Append(executableSql);

    sb.Append("，操作人=tenantId:");
    sb.Append(tenantId);
    sb.Append("/userId:");
    sb.Append(userId);
    sb.Append("，耗时=");
    sb.Append(elapsedMs);
    sb.Append("ms");

    return sb.ToString();
}
```

---

## 8. 性能优化要求

### 8.1 内存分配最小化

| 场景 | 优化策略 |
|------|---------|
| 字符串拼接（日志、调试信息） | 使用 `ValueStringBuilder` + `stackalloc` |
| 参数数组构建 | 短数组（≤8个参数）使用 `stackalloc`，长数组使用 `ArrayPool<T>` |
| 连接池管理 | `ConcurrentQueue<NpgsqlConnection>`，无锁获取/归还 |
| 方法返回值 | 使用 `ValueTask` 代替 `Task` 减少异步状态机分配 |
| 循环中避免闭包 | 使用 `for` 循环而非 `foreach`（在关键路径上） |

### 8.2 连接生命周期

```
GetConnection() → 使用 → ReturnConnection()
       ↓                          ↓
   连接池获取              验证状态 → 有效则归还
                                    → 无效则丢弃并补充
```

### 8.3 异步优化

- 所有 IO 操作使用 `async/await`
- 返回 `ValueTask` 或 `ValueTask<T>`（减少热路径上的分配）
- 不使用 `ConfigureAwait(false)`（NativeAOT 中不需要）

---

## 9. 使用示例

### 9.1 初始化

```csharp
// 应用启动时初始化
DB.Init(new DbOptions
{
    Host = "localhost",
    Port = 5432,
    Database = "mydb",
    Username = "admin",
    Password = "xxx",
    MinPoolSize = 2,
    MaxPoolSize = 20,
    ConnectionTimeoutSeconds = 30,
    RetryCount = 3,
    IdleTimeoutSeconds = 300
});
```

### 9.2 独立事务插入

```csharp
var result = await DB.InsertAsync(
    "INSERT INTO users (name, age) VALUES (@p0, @p1) RETURNING id",
    new[] {
        new PgSqlParam("@p0", "张三"),
        new PgSqlParam("@p1", 25)
    },
    tenantId: 1001,
    userId: 5001
);

if (result.Success)
    Console.WriteLine($"插入成功，新 ID={result.Id}");
```

### 9.3 批量事务操作

```csharp
// 1. 获取事务批处理
var batch = await DB.GetBatchAsync(tenantId: 1001, userId: 5001);

try
{
    // 2. 追加多个操作（不提交）
    await DB.InsertTxAsync(batch,
        "INSERT INTO orders (user_id, amount) VALUES (@p0, @p1) RETURNING id",
        new[] { new PgSqlParam("@p0", 5001), new PgSqlParam("@p1", 99.9m) },
        1001, 5001);

    await DB.UpdateTxAsync(batch,
        "UPDATE users SET order_count = order_count + 1 WHERE id = @p0",
        new[] { new PgSqlParam("@p0", 5001) },
        1001, 5001);

    // 3. 批量提交
    var commitResult = await DB.BatchCommitAsync(batch);
    if (commitResult.Success)
        Console.WriteLine($"事务提交成功，受影响行数={commitResult.RowsAffected}");
}
catch
{
    // 异常时 BatchCommitAsync 内部已处理回滚和日志
    throw;
}
```

### 9.4 查询多行数据

```csharp
// 使用 mapper 映射
var (result, users) = await DB.GetListAsync(
    "SELECT id, name, age FROM users WHERE age >= @p0",
    new[] { new PgSqlParam("@p0", 18) },
    reader => new UserDto
    {
        Id = reader.GetInt64(0),
        Name = reader.GetString(1),
        Age = reader.GetInt32(2)
    },
    tenantId: 1001,
    userId: 5001
);

// 直接写入 JSON
using var stream = new MemoryStream();
using var jsonWriter = new Utf8JsonWriter(stream);
var jsonResult = await DB.GetListAsync(
    "SELECT id, name FROM users",
    Array.Empty<PgSqlParam>(),
    jsonWriter,
    (writer, reader) =>
    {
        writer.WriteStartObject();
        writer.WriteNumber("id", reader.GetInt64(0));
        writer.WriteString("name", reader.GetString(1));
        writer.WriteEndObject();
    },
    tenantId: 1001,
    userId: 5001
);
```

### 9.5 DDL 操作

```csharp
// 创建表
var status = await DB.CreateTable("users",
    "CREATE TABLE users (id BIGSERIAL PRIMARY KEY, name TEXT NOT NULL, age INT)");
// status: Success / Existed / Failed

// 修改字段
var alterStatus = await DB.AlterTable("users", "name", "varchar", "200", "", true, false);

// 创建索引
var indexStatus = await DB.CreateIndex("idx_users_name", "users", "name", unique: false);
```

### 9.6 与 SQL Builder 集成

```csharp
var user = Table.Def("users").As("u");

// 构建查询
var query = PgSql
    .Select(user["id"], user["name"], user["age"])
    .From(user)
    .Where(user["age"], Op.Gte, Param.Value(18))
    .OrderBy(user["name"].Asc())
    .Limit(10)
    .Build();

// 使用 ADO 执行
var (result, data) = await DB.GetListAsync(
    query.Sql, query.Params,
    reader => new { Id = reader.GetInt64(0), Name = reader.GetString(1) },
    tenantId, userId
);
```

---

## 10. 最终质量标准

若设计上有取舍，请遵循以下优先级：

**正确性 > 语义完整性 > 安全性 > 可维护性 > 可读性 > 复杂度可控 > 性能 > 易用性 > 技巧炫技**

### 10.1 代码质量

- 全部关键类型、方法提供**完整中文 XML 注释**（`/// <summary>` 等）
- 注释准确描述线程模型、内存策略、异常策略
- 代码风格清晰、可维护，避免过度抽象

### 10.2 验收自检清单

在给出最终答案前，逐条确认：

- [ ] 无 LINQ / 反射 / dynamic / 序列化
- [ ] NativeAOT 项目配置正确（`<PublishAot>true</PublishAot>`）
- [ ] 连接池正确实现（最大/最小/超时/重试/注销）
- [ ] 所有 CRUD 方法支持独立事务和批量事务两种模式
- [ ] 所有 DDL 方法有前置检查（存在性、字段比较）
- [ ] 所有 CRUD 执行方法包含 `Logger.Debug` 调用（入口、参数、执行、结果、耗时）
- [ ] 所有 DDL 执行方法包含 `Logger.Info` 调用（入口、执行、结果）
- [ ] 所有 CRUD 异常使用 `Logger.Error` 记录完整信息（`ex.ToString()`、SQL、参数、操作人）
- [ ] DDL 异常使用 `Logger.Fatal` 记录并调用 `Environment.FailFast` 终止程序
- [ ] DDL 方法使用 `tenantId = 0, userId = 0` 记录日志
- [ ] 返回结构中 `DebugMessage` 包含可执行 SQL（参数替换为实际值）
- [ ] 使用 `ValueStringBuilder` / `stackalloc` 优化字符串拼接
- [ ] 使用 `ValueTask` 减少异步分配
- [ ] 中文 XML 注释完整
- [ ] 项目引用 `YTStdSqlBuilder` 和 `YTStdLogger` 正确
- [ ] 示例程序可独立运行

若任一项不满足，先修正再输出完整代码。

---

## 11. 最终指令

请现在开始实现，直接交付：

- 完整源码（所有 `.cs` 文件、`.csproj` 文件）
- 完整测试
- 完整设计说明

不要只给思路。
不要偷懒省略。
不要输出 demo 级方案。
必须按**生产级顶级标准**完成。

### 输出格式要求

1. 先给"实现说明（简短）"
2. 然后按文件逐个输出完整代码，使用如下格式：

```text
// file: 相对路径
<完整内容>
```

3. 不要省略任何文件
4. 不要输出伪代码 / 片段
5. 不要要求用户"自行补充"
6. 代码必须可直接复制构建