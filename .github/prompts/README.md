# AI 提示词管理规范

## 概述

本目录包含面向 **Claude Opus 4.6**（或兼容的大语言模型）的生产级实现提示词。每个提示词对应一个独立的解决方案模块，指导 AI 生成完整、可编译、可测试的生产级代码。

---

## 目录结构

```
.github/prompts/
├── README.md                    # 本文件 — 提示词管理规范
├── sql-builder-prompt.md        # SQL Builder 实现提示词
├── logger-prompt.md             # 日志系统实现提示词
├── ado-prompt.md                # ADO 数据访问实现提示词
├── entity-prompt.md             # 实体与数据表维护实现提示词
└── i18n-prompt.md               # 国际化语言支持实现提示词
```

---

## 命名规范

提示词文件命名格式：`{模块名}-prompt.md`

| 模块 | 文件名 | 状态 | 说明 |
|------|--------|------|------|
| SQL Builder | `sql-builder-prompt.md` | ✅ 已生成源码 | SQL 构建器 + 解释器 + Source Generator |
| 日志系统 | `logger-prompt.md` | ✅ 已生成源码 | 高性能日志引擎，支持租户级 Debug 开关 |
| ADO 数据访问 | `ado-prompt.md` | 📝 提示词已优化 | 数据库访问层（连接池 + CRUD + DDL） |
| 实体管理 | `entity-prompt.md` | 📝 提示词已优化 | 实体特性 + Source Generator（DAL/CRUD/描述） |
| 国际化 | `i18n-prompt.md` | 📝 提示词已创建 | 多语言资源管理 + Source Generator（强类型键） |

后续新增模块时，按此格式命名：
- `{module-name}-prompt.md`
- 使用小写、连字符分隔的英文名称
- 以 `-prompt.md` 结尾

---

## 模块间依赖关系

```
YTStdLogger  ←── YTStdSqlBuilder.Generator（生成代码中调用 Logger.Debug）
    ↑                    ↑
    │                    │
YTStd.Ado ────→ YTStdSqlBuilder（SQL 构建结果）
    ↑
    │
YTStdEntity ────→ YTStd.Ado（数据库操作）
    ↑               ↑
    │               │
YTStdI18n ────→ YTStdLogger（日志记录）
```

所有模块共同依赖：
- **YTStdLogger** — 日志记录（`Logger.Debug/Error/Info(int tenantId, long userId, string message)`）
- **YTStdI18n** — 国际化文本（`I18n.T(int tenantId, string key)`）

---

## 日志集成规范

所有模块在实现时，必须遵循以下日志规范：

### Debug 日志
- 方法入口、关键节点、返回前均需记录 `Logger.Debug`
- 记录即时值（参数值、SQL、返回结果等），便于开发与指定租户跟踪调试
- 支持通过 `Logger.EnableTenantDebug(tenantId)` 运行时开启指定租户调试

### Error 日志
- 异常时必须记录完整堆栈信息：`Logger.Error(tenantId, userId, $"[方法名] 异常: {ex}")`
- 必须包含：方法名、SQL（如有）、参数值、完整异常 `ex.ToString()`

### 方法签名
- 所有公开方法必须包含 `int tenantId, long userId` 参数
- 用于日志中记录租户与用户信息

---

## 提示词编写规范

每个提示词文件应包含以下结构：

```markdown
# 面向 Claude Opus 4.6 的 {模块名} 实现提示词

## 依赖项目参考
> 本提示词依赖以下已有项目，详细 API 参考请查阅：
> - [已有项目参考文档](../../docs/existing-projects-reference.md)

## 1. 最终目标
## 2. 技术约束
## 3. 项目结构
## 4. 核心设计
...
## N. 日志集成要求
## N+1. 最终质量标准
## N+2. 最终指令
```

### 必要章节

1. **依赖项目参考** — 声明本模块依赖的已有项目，并引用参考文档
2. **最终目标** — 清晰描述模块的功能目标
3. **技术约束** — 框架版本、目标平台、依赖限制
4. **项目结构** — 生成代码应放置的目录结构
5. **核心设计** — 核心类型、接口、模式
6. **日志集成要求** — Logger.Debug/Error 调用规范
7. **最终质量标准** — 质量优先级排序
8. **最终指令** — 输出交付物清单

### 编写要点

- **语言**：使用中文编写（与团队保持一致）
- **精确性**：API 签名、类型名称必须精确无歧义
- **完整性**：不允许只给设计稿/伪代码/骨架
- **引用**：通过 `existing-projects-reference.md` 引用已有项目 API
- **示例**：每个核心 API 必须配有使用示例
- **日志**：每个模块必须包含日志集成要求
- **国际化**：用户可见的错误信息应使用 `I18n.T()` 国际化

---

## 如何使用提示词

### 步骤 1：准备参考文档

在使用新提示词之前，确保 `docs/existing-projects-reference.md` 已包含所有依赖项目的最新 API 参考。

### 步骤 2：向 AI 提供上下文

将以下内容提供给 Claude Opus 4.6：

1. 提示词文件本身（如 `ado-prompt.md`）
2. 已有项目参考文档（`docs/existing-projects-reference.md`）
3. 如有需要，可附加具体源代码文件

### 步骤 3：验证生成结果

- 确保生成代码可编译（`dotnet build`）
- 确保测试通过（`dotnet test`）
- 确保符合仓库的命名规范和架构规范

---

## 跨提示词引用模式

当新提示词需要使用已有项目的类型或 API 时：

```markdown
## 依赖项目参考

本模块依赖以下已有项目。完整的类型定义、API 签名与使用示例，
请查阅 [已有项目参考文档](../../docs/existing-projects-reference.md)。

### 直接依赖
- **YTStdSqlBuilder** — `PgSqlRenderResult`, `PgSqlParam` 等 SQL 构建输出类型
- **YTStdLogger** — `Logger.Debug`, `Logger.Error`, `Logger.Info` 日志方法
- **YTStdI18n** — `I18n.T()` 国际化翻译方法

### 引用示例
本模块生成的 ADO 执行方法接收 `PgSqlRenderResult` 作为输入：
```csharp
public static async Task<List<T>> QueryAsync<T>(
    NpgsqlConnection conn,
    PgSqlRenderResult query,  // 来自 YTStdSqlBuilder
    Func<NpgsqlDataReader, T> mapper)
```
```

详见 [提示词编写指南](../../docs/prompt-authoring-guide.md)。
