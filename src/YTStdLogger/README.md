# YTStdLogger

`YTStdLogger` 是一个面向 `.NET 10.0 + NativeAOT` 的轻量高性能日志组件，核心目标是低延迟、低分配、低 GC 压力，并保持 NativeAOT 兼容。

## 设计要点

- 业务线程仅做轻量入队，不进行 IO。
- 独立消费者线程批量格式化并落盘。
- 使用有界 `MPMC RingBuffer`，容量为 2 的幂。
- 使用 `ArrayPool<T>` 复用消息缓冲与格式化缓冲。
- 使用 `Span<char>` 进行日志行拼装。
- 支持限速与丢弃计数，并周期性写入 `warn.txt` 摘要。
- 按 `yyyyMM/yyyyMMdd/tenantId/level.txt` 输出。
- 文件与目录延迟创建，仅在实际有日志时创建。
- 后台定时清理超保留月数的日志目录，失败不影响主流程。

## 目录结构

```text
{RootPath}/yyyyMM/yyyyMMdd/{tenantId}/fatal.txt
{RootPath}/yyyyMM/yyyyMMdd/{tenantId}/error.txt
{RootPath}/yyyyMM/yyyyMMdd/{tenantId}/warn.txt
{RootPath}/yyyyMM/yyyyMMdd/{tenantId}/infor.txt
{RootPath}/yyyyMM/yyyyMMdd/{tenantId}/debug.txt
```

## 快速使用

```csharp
LogOptions options = new LogOptions
{
    RootPath = "./logs",
    MinimumLevel = LogLevel.Debug
};

Logger.Init(options);
Logger.EnableTenantDebug(1002); // 仅租户1002临时开启Debug
Logger.Info(1001, 90001, "hello");

// 推荐使用 Func<string> 延迟求值重载，避免 Debug 未启用时产生不必要的字符串分配
Logger.Debug(1001, 90001, () => $"[SomeMethod] 参数详情: id={id}, name={name}");

await Logger.ShutdownAsync();
```

说明：默认采用全局单例日志引擎，业务侧通过 `Logger` 直接传入 `tenantId` 与 `userId`，避免每租户创建日志对象。

## API 概览

### 两种重载

每个日志等级方法（Fatal/Error/Warn/Info/Debug）均提供两种重载：

```csharp
// 1. 直接传入字符串 — 适用于 Error/Fatal 等始终启用的等级
Logger.Error(tenantId, userId, $"[方法名] 异常: {ex}");

// 2. Func<string> 延迟求值 — 推荐用于 Debug 等可能被全局禁用的等级
// 仅在该等级启用时才调用工厂方法构建字符串，避免不必要的 GC 压力
Logger.Debug(tenantId, userId, () => $"[方法名] SQL: {sql}, 参数: {paramValue}");
```

### 完整方法列表

| 方法 | 直接字符串 | 延迟求值 |
|------|-----------|---------|
| Fatal | `Fatal(int tenantId, long userId, string message)` | `Fatal(int tenantId, long userId, Func<string> messageFactory)` |
| Error | `Error(int tenantId, long userId, string message)` | `Error(int tenantId, long userId, Func<string> messageFactory)` |
| Warn | `Warn(int tenantId, long userId, string message)` | `Warn(int tenantId, long userId, Func<string> messageFactory)` |
| Info | `Info(int tenantId, long userId, string message)` | `Info(int tenantId, long userId, Func<string> messageFactory)` |
| Debug | `Debug(int tenantId, long userId, string message)` | `Debug(int tenantId, long userId, Func<string> messageFactory)` |

### 何时使用哪种重载

- **Debug**：推荐使用 `Func<string>` 重载，因为生产环境通常禁用 Debug，避免无意义字符串分配
- **Info**：如果日志消息包含复杂插值，推荐使用 `Func<string>` 重载
- **Error/Fatal**：通常使用直接字符串重载，因为这些等级几乎始终启用

## 供其他工程自动编码调用的设置建议

当你在其他项目（Visual Studio 2026 + GPT-5.3-codex）接入本库时，建议在目标项目的 `.github/copilot-instructions.md` 写入如下约束：

```text
项目统一使用 YTStdLogger.Core.Logger 静态门面。
禁止创建每租户 Logger 对象。
应用启动时调用 Logger.Init(LogOptions)。
业务日志统一调用 Logger.Info/Error/Warn/Debug/Fatal(tenantId, userId, message) 或 Func<string> 延迟求值重载。
Debug 日志推荐使用 Func<string> 重载：Logger.Debug(tenantId, userId, () => $"...");
应用退出时调用 await Logger.ShutdownAsync()。
日志目录结构必须为 yyyyMM/yyyyMMdd/tenantId/level.txt。
```

这样可让自动代码生成在所有工程中保持统一调用方式。

## 运行时按租户开启 Debug

当系统全局日志级别较高（例如 `Infor`）时，可只为某个租户临时开启 Debug 追踪：

```csharp
Logger.EnableTenantDebug(tenantId);
Logger.Debug(tenantId, userId, () => "仅该租户会放行的调试日志");
Logger.DisableTenantDebug(tenantId);
```

说明：该开关仅影响目标租户，不改变其他租户和全局 `MinimumLevel`。

## 配置项说明

- `RootPath`：日志根目录。
- `MinimumLevel`：最小日志等级阈值（`level <= MinimumLevel` 视为启用）。
- `QueueCapacity`：环形队列容量（2 的幂）。
- `BatchSize`：消费者每次最大处理条数。
- `BatchMaxDelayMs`：批次最大等待时间。
- `RetentionMonths`：日志保留月数。
- `MaxLogsPerSecond`：每秒最大写入速率。
- `DropSummaryIntervalSeconds`：丢弃摘要输出周期。
- `FlushEveryBatch`：是否每批次刷盘。
- `UseUtcTimestamp`：是否使用 UTC 时间。

## NativeAOT 注意事项

- 项目启用了 `PublishAot` 与 `IsAotCompatible`。
- 未使用反射、`dynamic`、`LINQ`、序列化框架。
- 编码固定使用 `Encoding.UTF8`。
- 未依赖运行时动态代码生成。

## 运行示例

```bash
dotnet run --project samples/LoggerSample/LoggerSample.csproj
```

运行后可在示例输出目录下看到租户分目录和各等级日志文件。
