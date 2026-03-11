# 仓库架构说明

## 概述

本仓库是一个 .NET 10.0 nativeAOT 仓库 ，包含多个相互关联的基础设施解决方案。所有项目均针对 PostgreSQL / Npgsql 生态，遵循 **高性能、低 GC、AOT 友好** 的设计原则。

---

## 仓库目录结构

```
YTStdSqlBuilder/                              # 仓库根目录
│
├── .github/
│   └── prompts/                              # AI 提示词目录（Claude Opus 4.6）
│       ├── README.md                         # 提示词命名规范与使用说明
│       ├── sql-builder-prompt.md             # SQL Builder 实现提示词
│       ├── logger-prompt.md                 # 日志系统实现提示词（待补充）
│       └── ado-prompt.md                     # ADO 数据访问实现提示词（待补充）
│
├── docs/                                     # 项目文档
│   ├── architecture.md                       # 本文件 — 仓库架构说明
│   ├── prompt-authoring-guide.md             # 提示词编写指南
│   └── existing-projects-reference.md        # 已有项目参考文档（供新提示词引用）
│
├── src/                                      # 源代码
│   ├── YTStdCommon/                         # 通用工具库（待添加）
│   ├── YTStdi18n/                         # 国际化语言库（待添加）
│   ├── YTStdSqlBuilder/                      # SQL Builder 运行时库
│   ├── YTStdSqlBuilder.Generator/            # SQL Builder 源代码生成器
│   ├── YTStdLogger/                        # 日志解决方案
│   └── YTStdAdo/                            # ADO 数据访问解决方案（待添加）
│
├── tests/                                    # 测试项目
│   ├── YTStdSqlBuilder.Tests/                # SQL Builder 运行时测试
│   ├── YTStdSqlBuilder.Generator.Tests/      # SQL Builder 源代码生成器测试
│   ├── YTStdCommon.Tests/                   # 通用工具库测试（待添加）
│   ├── YTStdLogger.Tests/                  # 日志解决方案测试
│   └── YTStdAdo.Tests/                      # ADO 数据访问测试（待添加）
│
├── benchmarks/                               # 性能基准测试
│   └── YTStdSqlBuilder.Benchmarks/           # SQL Builder 性能测试
│
├── samples/                                  # 示例项目
│   └── YTStdSqlBuilder.Sample/               # SQL Builder 使用示例
│
├── README.md                                 # 仓库主文档
└── YTStdSqlBuilder.sln                       # Visual Studio 解决方案文件
```

---



## 项目详细说明

### YTStdCommon（待添加）

**目的：** 提供所有解决方案共用的基础类型与结构。

**典型内容：**
- 通用的结构体、枚举、接口定义
- 跨项目共享的常量与工具方法

**依赖关系：** 无外部依赖（最底层项目）

---

### YTStdi18n（待添加）

**目的：** 提供国际化语言支持。

**典型内容：**
- 多语言资源管理
- 本地化字符串处理
- 日期、时间、数字格式化

**依赖关系：** 无外部依赖（最底层项目）

---

### YTStdLogger（日志解决方案）

**目的：** 结构化日志系统。

**状态：** 待添加（用户已有代码，后续合并）

**依赖关系：** 无外部依赖（最底层项目）

---

### YTStdSqlBuilder（SQL 构建器运行时）
- `DBNullable<T>` — 数据库可空类型包装
- 通用的结构体、枚举、接口定义
- 跨项目共享的常量与工具方法

**依赖关系：** 无外部依赖（最底层项目）

---

### YTStdSqlBuilder（SQL 构建器运行时）

**目的：** 类型安全、流式 API 的 PostgreSQL 查询构建器。

**核心能力：**
- SELECT / INSERT / UPDATE / DELETE 构建
- 参数化查询（防 SQL 注入）
- 动态条件（WhereIf / AndIf / OrIf）
- 子查询支持（EXISTS / IN / Scalar）
- 高性能渲染（ValueStringBuilder，零分配）

**依赖关系：** YTStdCommon, Npgsql

---

### YTStdSqlBuilder.Generator（源代码生成器）

**目的：** Roslyn Source Generator，在编译期分析 SQL 模板，生成静态优化的查询方法。

**核心能力：**
- 编译期 SQL 常量生成
- 参数构造方法生成
- DTO 结果结构自动生成
- DataReader 映射方法生成

**依赖关系：** Microsoft.CodeAnalysis (Roslyn)，netstandard2.0 目标框架

---



### YTStd.Ado（ADO 数据访问）

**目的：** 基于 Npgsql 的数据访问层，配合 SQL Builder 使用。

**状态：** 待添加（需新提示词驱动生成）

**依赖关系：** YTStdCommon, YTStdSqlBuilder, Npgsql

---

## 命名规范

| 类别 | 命名规则 | 示例 |
|------|---------|------|
| 源代码项目 | `YTStd.{模块名}` 或 `YTStd{模块名}` | `YTStdCommon`, `YTStdSqlBuilder` |
| 源代码生成器 | `{项目名}.Generator` | `YTStdSqlBuilder.Generator` |
| 测试项目 | `{项目名}.Tests` | `YTStdSqlBuilder.Tests` |
| 性能测试 | `{项目名}.Benchmarks` | `YTStdSqlBuilder.Benchmarks` |
| 示例项目 | `{项目名}.Sample` | `YTStdSqlBuilder.Sample` |
| AI 提示词 | `{模块名}-prompt.md` | `sql-builder-prompt.md` |

---

## 构建与测试

```bash
# 构建所有项目
dotnet build

# 运行所有测试
dotnet test

# 运行特定项目测试
dotnet test tests/YTStdSqlBuilder.Tests/
dotnet test tests/YTStdSqlBuilder.Generator.Tests/

# 运行示例项目测试
dotnet test samples/YTStdSqlBuilder.Sample/

# 运行性能基准测试
dotnet run --project benchmarks/YTStdSqlBuilder.Benchmarks/ -c Release
```

---

## 设计原则

1. **正确性** > 语义完整性 > 安全性 > 可维护性 > 可读性 > 复杂度可控 > 性能 > 易用性
2. **AOT 友好** — 所有项目均支持 .NET Native AOT 编译
3. **高性能、低 GC** — 使用 `ref struct`、`Span<T>`、`stackalloc`、`ArrayPool` 等技术
4. **PostgreSQL 专用** — 仅适配 PostgreSQL / Npgsql，不做多数据库抽象
