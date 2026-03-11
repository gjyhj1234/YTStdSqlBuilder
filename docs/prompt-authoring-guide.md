# 提示词编写指南

## 概述

本指南说明如何为本仓库编写新的 AI 实现提示词，以及如何让新提示词正确引用和使用已有项目的代码。

---

## 1. 提示词的设计目标

每个提示词的核心目标是：**让 Claude Opus 4.6 能够生成完整、可编译、生产级的代码**。

为此，提示词必须：
- 定义清晰的模块边界和职责
- 声明所有外部依赖（已有项目）
- 提供精确的 API 签名和使用示例
- 设定明确的质量标准

---

## 2. 如何引用已有项目

### 2.1 项目参考文档

所有已有项目的 API 参考集中维护在 [`docs/existing-projects-reference.md`](./existing-projects-reference.md) 中。

新提示词通过以下方式引用：

```markdown
## 依赖项目参考

> **重要**：本模块依赖以下已有项目。完整的类型定义、API 签名和使用模式，
> 请查阅 [已有项目参考文档](../../docs/existing-projects-reference.md)。
>
> AI 在实现本模块时，必须遵循参考文档中定义的类型签名，不得自行定义重复类型。

### 本模块使用的已有类型

| 类型 | 来源项目 | 用途 |
|------|---------|------|
| `PgSqlRenderResult` | YTStdSqlBuilder | SQL 构建结果，包含 Sql 和 Params |
| `PgSqlParam` | YTStdSqlBuilder | SQL 参数，包含 Name、Value、DbType |
```

### 2.2 为什么使用参考文档而非直接嵌入源代码

- **一致性**：所有提示词引用同一份参考文档，避免版本不一致
- **可维护性**：API 变更只需更新参考文档一处
- **可控性**：参考文档只包含公开 API，不暴露实现细节
- **篇幅**：避免在每个提示词中重复大量类型定义

### 2.3 何时需要附加源代码

当参考文档不够时（例如需要了解内部实现细节），可在提示词中标注：

```markdown
> **补充上下文**：实现本模块时，建议同时查阅以下源文件：
> - `src/YTStdSqlBuilder/PgSqlRenderer.cs` — 了解 SQL 渲染的性能模式
> - `src/YTStdSqlBuilder/Builders/PgSqlQueryBuilder.cs` — 了解 Builder 的链式 API 设计
```

---

## 3. 提示词模板

以下是新提示词的标准模板：

```markdown
# 面向 Claude Opus 4.6 的 {模块名} 生产级实现提示词
## {简要描述}

你是一名**顶级 .NET 基础架构工程师 / PostgreSQL & Npgsql 专家 / 高性能框架作者**。
请实现一个 {模块描述}。

这不是 demo，不是玩具组件，而是**框架基座级实现**。
请严格按"**生产级顶级标准**"完成完整代码、测试与设计说明。

---

## 依赖项目参考

> **重要**：本模块依赖以下已有项目。完整的类型定义、API 签名和使用模式，
> 请查阅 [已有项目参考文档](../../docs/existing-projects-reference.md)。

### 本模块使用的已有类型

| 类型 | 来源项目 | 用途 |
|------|---------|------|
| `...` | `...` | `...` |

---

## 1. 最终目标

{描述模块的功能目标}

---

## 2. 技术约束

- 目标框架：net10.0
- 数据库：PostgreSQL（Npgsql）
- AOT 兼容：是
- 依赖项目：{列出}

---

## 3. 项目结构

生成的代码应放置在以下位置：

| 项目 | 路径 | 说明 |
|------|------|------|
| 主项目 | `src/YTStd.{Module}/` | 运行时库 |
| 测试 | `tests/YTStd.{Module}.Tests/` | 单元测试 |
| 示例 | `samples/YTStd.{Module}.Sample/` | 使用示例（可选） |

---

## 4. 核心设计

### 4.1 核心类型

{定义核心类型和接口}

### 4.2 使用示例

{每个核心 API 的使用示例}

---

## ... (更多章节)

---

## N. 最终质量标准

若设计上有取舍，请遵循以下优先级：

**正确性 > 语义完整性 > 安全性 > 可维护性 > 可读性 > 复杂度可控 > 性能 > 易用性 > 技巧炫技**

---

## N+1. 最终指令

请现在开始实现，直接交付：

- 完整源码
- 完整测试
- 完整设计说明

不要只给思路。
不要偷懒省略。
不要输出 demo 级方案。
必须按**生产级顶级标准**完成。
```

---

## 4. 维护参考文档

### 4.1 何时更新参考文档

当以下情况发生时，需要更新 `docs/existing-projects-reference.md`：

- 已有项目添加了新的公开类型或 API
- API 签名发生变更
- 新增了一个已有项目

### 4.2 更新流程

1. 更新参考文档中对应项目的 API 章节
2. 检查所有引用该项目的提示词是否仍然兼容
3. 如有必要，更新提示词中的"本模块使用的已有类型"表格

### 4.3 自动化建议

可以在 CI 中添加检查，确保参考文档中列出的类型在源代码中确实存在：

```bash
# 检查参考文档中提到的类型是否存在于源代码中
grep -rn "public.*class\|public.*struct\|public.*record\|public.*enum" src/ | \
  grep -f <(grep '`[A-Z]' docs/existing-projects-reference.md | \
            sed 's/.*`\([A-Za-z]*\)`.*/\1/' | sort -u)
```

---

## 5. 完整示例：ADO 提示词如何引用 SQL Builder

以下展示 ADO 提示词如何正确引用 SQL Builder 项目：

### 在提示词中声明依赖

```markdown
## 依赖项目参考

本模块是 SQL Builder 的执行层，负责将 SQL Builder 生成的查询发送到 PostgreSQL 并映射结果。

> 请查阅 [已有项目参考文档](../../docs/existing-projects-reference.md) 中
> "YTStdSqlBuilder" 章节的完整类型定义。

### 本模块使用的核心类型

从 **YTStdSqlBuilder**：
- `PgSqlRenderResult` — SQL 构建结果（`.Sql` + `.Params`）
- `PgSqlParam` — 参数对象（`.Name`, `.Value`, `.DbType`）
- `PgSqlQueryBuilder` — 查询构建器（用于类型推断）

从 **YTStdCommon**：
- `DBNullable<T>` — 数据库可空值包装

### 关键交互模式

ADO 模块接收 SQL Builder 的输出并执行：

```csharp
// SQL Builder 构建查询
var query = PgSql
    .Select(user["id"], user["name"])
    .From(user)
    .Where(user["age"], Op.Gte, Param.Value(18))
    .Build();

// ADO 模块执行查询
var users = await PgAdo.QueryAsync(conn, query, reader => new {
    Id = reader.GetInt32(0),
    Name = reader.GetString(1)
});
```
```

### 在提供给 AI 时的完整上下文

向 Claude Opus 4.6 提供以下文件：

1. `.github/prompts/ado-prompt.md` — ADO 提示词
2. `docs/existing-projects-reference.md` — 已有项目参考
3. （可选）`src/YTStdSqlBuilder/PgSqlRenderResult.cs` — 如需了解具体实现

---

## 6. 检查清单

编写新提示词时，请确认：

- [ ] 文件放置在 `.github/prompts/` 目录下
- [ ] 文件名符合 `{module-name}-prompt.md` 格式
- [ ] 在 `.github/prompts/README.md` 的表格中注册
- [ ] 包含"依赖项目参考"章节并引用参考文档
- [ ] 列出所有使用的已有类型
- [ ] 提供核心 API 的使用示例
- [ ] 指定项目结构（源代码应放置的路径）
- [ ] 包含质量标准章节
- [ ] 包含最终指令章节
- [ ] `docs/existing-projects-reference.md` 已包含所有依赖项目的 API
