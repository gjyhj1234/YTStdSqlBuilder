# 角色与目标

你是一个资深 .NET 高性能基础设施工程师。请在 **.NET 10.0 + NativeAOT** 环境下，设计并实现一套**极致轻量、高性能、低内存、零反射、零动态、零LINQ依赖**的日志组件（可作为类库直接引用）。

你的输出必须是：**完整可编译的代码**（非伪代码）、必要的项目文件、中文规范注释、以及最小可运行示例。  
严禁需求漂移，严禁引入未要求的复杂依赖。

---

## 强约束（必须全部满足）

1. **运行环境**
   - 目标框架：`.NET 10.0`
   - 发布方式：`NativeAOT`
   - 必须完全兼容 NativeAOT（避免所有可能触发运行时生成元数据/动态代码路径的特性）
   - 支持：`windows`、`liunx`、`macOS`
2. **禁止项（硬性）**
   - 禁止 `LINQ`（`System.Linq`）
   - 禁止反射（`System.Reflection`）
   - 禁止 `dynamic`
   - 禁止序列化框架（JSON/XML/二进制序列化等）
   - 禁止将日志对象化后再转字符串，日志内容直接基于 `string/Span<char>` 处理

3. **性能与资源**
   - 不阻塞业务线程（生产者线程只做极轻量操作）
   - 日志异步落盘，独立消费者写入
   - 低内存占用，尽量零分配或近零分配
   - 极低 GC 压力（对象池/缓冲池）
   - **全局仅允许一个日志引擎单例，不允许“每租户创建长期日志对象实例”**
   - 支持低配云服务器稳定运行

4. **日志能力**
   - 支持等级：`FATAL`、`ERROR`、`WARN`、`INFOR`、`DEBUG`
   - 等级语义：配置为较低等级时，应包含更高严重级别日志（如 DEBUG 包含全部）
   - 各等级写入不同文件名：
     - `fatal.txt`
     - `error.txt`
     - `warn.txt`
     - `infor.txt`
     - `debug.txt`
   - **仅在该等级实际有日志时才创建对应文件**
   - 若某租户某天无任何日志，不创建对应目录

5. **目录结构（必须严格）**
   - `根目录/yyyyMM/yyyyMMdd/tenantId/`
   - 示例：
     - `202602/20260202/租户ID/fatal.txt`
     - `202602/20260202/租户ID/error.txt`
     - `202602/20260202/租户ID/warn.txt`
     - `202602/20260202/租户ID/infor.txt`
     - `202602/20260202/租户ID/debug.txt`

6. **保留策略**
   - 可配置日志保留月数（按“天目录”清理）
   - 后台定时清理过期目录
   - 清理过程要容错，不影响主日志写入

7. **高阶优化（必须实现）**
   - 使用 `Span<char>`、`ReadOnlySpan<char>`、`ArrayPool<T>`、（可选）自定义 `ValueStringBuilder`
   - 零分配日志格式化（至少核心路径避免 `string.Format`、插值导致的额外分配）
   - MPMC RingBuffer（多生产者多消费者安全；若实现为“多生产者+单消费者落盘”也可，但队列结构需支持 MPMC 语义）
   - 日志雪崩保护：限速 + 丢弃策略（并记录丢弃计数，周期性写入告警）
   - 批量写入优化：消费者批量刷盘，减少 IO 次数

8. **工程与文档**
   - 全部关键类型、方法提供**完整中文 XML 注释**（`/// <summary>` 等）
   - 注释准确描述线程模型、内存策略、异常策略
   - 代码风格清晰、可维护，避免过度抽象
   - 提供 README（中文）说明使用方法、配置项、性能设计点、NativeAOT 注意事项

---

## 交付物（必须一次性给全）

请输出以下文件的完整内容（不要省略）：

1. `YTStdLogger.sln`
2. `src/YTStdLogger/YTStdLogger.csproj`
3. `src/YTStdLogger/Logging/LogLevel.cs`
4. `src/YTStdLogger/Logging/LogOptions.cs`
5. `src/YTStdLogger/Logging/LogMessage.cs`
6. `src/YTStdLogger/Logging/ILogFormatter.cs`
7. `src/YTStdLogger/Logging/DefaultLogFormatter.cs`
8. `src/YTStdLogger/Buffer/MpmcRingBuffer.cs`
9. `src/YTStdLogger/Buffer/LogMessagePool.cs`
10. `src/YTStdLogger/IO/TenantDatePathResolver.cs`
11. `src/YTStdLogger/IO/BatchedFileWriter.cs`
12. `src/YTStdLogger/Retention/LogRetentionCleaner.cs`
13. `src/YTStdLogger/Protection/LogRateLimiter.cs`
14. `src/YTStdLogger/Core/LoggerEngine.cs`
15. `src/YTStdLogger/Core/LoggerFactory.cs`
16. `src/YTStdLogger/Core/Logger.cs`
17. `samples/LoggerSample/LoggerSample.csproj`
18. `samples/LoggerSample/Program.cs`
19. `README.md`

---

## 设计细则（实现时必须遵守）

### 1) 日志等级与过滤
- 定义枚举值顺序确保比较高效（建议 `FATAL=0 ... DEBUG=4` 或反向，但必须统一并在注释说明）
- 提供 `IsEnabled(level)` 快速判断，避免多余分支
- 每条日志均需要写入对应等级与更低等级（重复写多份,如在debug等级下，将完整记录Fatal、Error、Warn、Infor、Debug的全部记录），但“是否记录”受全局最小等级控制

### 2) API 设计（业务线程友好）
- **必须使用全局单例日志入口**（例如 `LoggerFactory` 创建后全局复用一个 `LoggerEngine`）
- **必须提供静态门面类**：`Logger`（全局调用入口）
- **禁止每租户实例化日志对象并长期持有**（避免高租户规模下额外对象数量和管理复杂度）
- 初始化与关闭（必须）：
  - `void Init(LogOptions options)`
  - `ValueTask ShutdownAsync()`
- 基础写入（必须）：
  - `void Log(int tenantId, long userId, LogLevel level, string message)`
- 快捷方法（必须）：
  - `Fatal(int tenantId, long userId, string message)`
  - `Error(int tenantId, long userId, string message)`
  - `Warn(int tenantId, long userId, string message)`
  - `Info(int tenantId, long userId, string message)`
  - `Debug(int tenantId, long userId, string message)`
- 运行时租户级 Debug 开关（必须）：
  - `EnableTenantDebug(int tenantId)`
  - `DisableTenantDebug(int tenantId)`
  - `IsTenantDebugEnabled(int tenantId)`
- 语义：开启后仅该租户临时放宽到 Debug，其他租户继续遵循全局 `MinimumLevel`
- 自动带时间戳，默认内部取 UTC+本地格式化策略二选一并可配置

### 3) 零分配格式化
- `DefaultLogFormatter` 使用 `Span<char>` 拼装：时间、等级、线程ID、消息
- 避免 `DateTime.ToString("...")` 高频分配（可手写数字写入）
- 避免中间临时字符串；必要时使用 `ArrayPool<char>` 租借缓冲
- 输出行格式示例（可微调，但需固定）：
  - `[2026-02-02 12:34:56.123][ERROR][T12] message`

### 4) RingBuffer 与并发
- 实现有界 RingBuffer，容量为 2 的幂，位运算取模
- 支持多生产者并发 `TryEnqueue`
- 消费者侧支持批量 `TryDequeueBatch(Span<LogMessage>)`
- 满队列触发丢弃策略（并计数）
- 所有并发原语优先 `Interlocked` + `Volatile`，避免重锁
- 如果必须锁，锁粒度最小且在注释解释原因

### 5) 雪崩保护
- `LogRateLimiter` 按租户或全局做每秒最大日志条数限制（可配置）
- 超限后执行策略：
  - 直接丢弃
  - 丢弃并累积计数
  - 每 N 秒输出一条摘要：“过去 N 秒丢弃 X 条”
- 该摘要写入 `warn.txt`（或固定策略，需在 README 说明）

### 5.1) 租户级 Debug 动态覆盖
- 需支持程序运行过程中动态开启/关闭指定 `tenantId` 的 Debug 追踪
- 该能力仅影响目标租户，不影响其他租户
- 实现需线程安全，且不可引入重锁热点

### 6) 批量落盘
- `BatchedFileWriter` 维护“tenant + date + level => writer state”
- 批量阈值（条数）和批量最大等待时间（毫秒）双触发刷盘
- 使用 `FileStream` + 合理 `FileOptions`（顺序写）
- 明确 flush 策略：平衡性能与可靠性（可配置是否每批 Flush）

### 7) 目录与文件延迟创建
- 首条日志到来时再创建目录与文件
- 日期切换时自动写入新路径
- 旧 writer 资源回收，避免句柄泄漏

### 8) 日志保留清理
- 每天/每小时后台检查一次
- 删除早于保留某年月的 `yyyyMM` 目录
- 对非法目录名跳过，不抛致命异常
- 清理失败仅记录内部告警，不影响主流程

### 9) 生命周期管理
- `LoggerEngine` 实现 `IDisposable` / `IAsyncDisposable`
- `StopAsync`：停止接收 -> 排空队列 -> 最终落盘 -> 释放资源
- 进程退出时尽量保证已入队日志落盘

### 10) NativeAOT 兼容要点
- 项目文件中显式启用 AOT 发布设置
- 不使用依赖运行时反射的库
- 避免 `Encoding.GetEncoding(name)` 这类可能触发额外路径，优先 `Encoding.UTF8`
- 代码中避免需要动态代码生成的模式

---

## 配置项（`LogOptions`）

至少包含：
- `string RootPath`
- `LogLevel MinimumLevel`
- `int QueueCapacity`
- `int BatchSize`
- `int BatchMaxDelayMs`
- `int RetentionMonths`
- `int MaxLogsPerSecond`
- `int DropSummaryIntervalSeconds`
- `bool FlushEveryBatch`
- `bool UseUtcTimestamp`

并提供：
- 合法性校验（构造或 `Validate`）
- 推荐默认值（偏保守、低资源）

---

## 错误处理策略（必须实现）

- 业务线程写日志失败（如队列满、限速）不得抛异常到业务层
- IO 异常在消费者线程内部吞吐并计数，周期性输出到内部告警（可 stderr）
- 避免“记录日志时再次写日志导致递归”
- 所有后台线程异常必须可观测（最少提供事件/回调或内部状态）

---

## 测试与示例要求

### 示例程序 `Program.cs` 必须演示：
1. 初始化 `Logger.Init(LogOptions)`
2. 在不创建租户 Logger 对象的前提下，直接传 `tenantId + userId` 并发写入两个租户不同等级日志
3. 演示“全局较高等级 + 某租户运行时开启 Debug”场景
4. 人为制造高频日志触发限速与丢弃摘要
5. 演示日期目录与等级文件生成
6. 通过 `Logger.ShutdownAsync()` 优雅关闭并排空

### 可选附加（如果你能稳定给出）
- 一个简单压力测试方法（非 BenchmarkDotNet，避免额外依赖）
- 输出总写入条数、丢弃条数、平均批大小

---

## 输出格式要求（严格）

1. 先给“实现说明（简短）”
2. 然后按文件逐个输出完整代码，使用如下格式：

```text
// file: 相对路径
<完整内容>
```

3. 不要省略任何文件
4. 不要输出伪代码/片段
5. 不要要求我“自行补充”
6. 代码必须可直接复制构建

---

## 验收标准（你必须自检）

在给出最终答案前，逐条确认：
- [ ] 无 LINQ / 反射 / dynamic / 序列化
- [ ] 目录结构符合 yyyyMM/yyyyMMdd/tenantId/level.txt
- [ ] 队列满时有丢弃策略与计数
- [ ] 有限速与周期性丢弃摘要
- [ ] 有批量写入（条数+时间双阈值）
- [ ] 按月保留
- [ ] 业务线程不阻塞
- [ ] 全局单例日志引擎 + `Logger` 静态门面，且日志 API 直接接收 `tenantId` 与 `userId`
- [ ] 支持运行时“按租户开启/关闭 Debug”，且仅影响目标租户
- [ ] 中文 XML 注释完整
- [ ] NativeAOT 项目配置存在
- [ ] 示例可运行并展示核心能力

若任一项不满足，先修正再输出完整代码。