# YTStd

高性能 .NET 10.0 + NativeAOT 服务端基础框架套件。

## 概览

YTStd 是一套面向生产级服务端应用的 .NET 基础框架，设计原则：**零反射、零 LINQ、零分配热路径、NativeAOT 完全兼容**。

| 模块 | 说明 |
|------|------|
| **YTStdSqlBuilder** | 类型安全的 PostgreSQL 查询构建器 + 编译时源代码生成器 |
| **YTStdAdo** | 零分配 PostgreSQL 数据库访问层（Npgsql） |
| **YTStdLogger** | 高性能异步日志框架，支持租户级 Debug 覆盖 |
| **YTStdEntity** | 实体框架 + Source Generator（自动生成 DAL / CRUD / 审计 / 描述类） |
| **YTStdI18n** | 数组索引 O(1) 国际化系统 + Source Generator（编译期校验） |

## 项目结构

```
YTStd/
├── src/
│   ├── YTStdSqlBuilder/            # SQL 查询构建器（运行时 API）
│   ├── YTStdSqlBuilder.Generator/  # SQL 模板源代码生成器
│   ├── YTStdAdo/                   # 数据库访问层
│   ├── YTStdLogger/                # 日志框架
│   ├── YTStdEntity/                # 实体定义 + 运行时类型
│   ├── YTStdEntity.Generator/      # 实体源代码生成器
│   ├── YTStdI18n/                  # 国际化运行时库
│   └── YTStdI18n.Generator/        # 国际化源代码生成器
├── tests/                          # 单元测试
├── samples/                        # 使用示例
├── benchmarks/                     # 性能基准测试
└── docs/                           # 文档
```

## 技术要求

- **.NET 10.0** — 目标框架
- **NativeAOT** — 完全兼容 AOT 编译
- **PostgreSQL / Npgsql** — 数据库
- **禁止**：Reflection、dynamic、LINQ、Expression Tree、序列化框架

## 各模块简介

### YTStdSqlBuilder — SQL 查询构建器

流式 API 构建 PostgreSQL 参数化查询，防止 SQL 注入。

```csharp
var user = Table.Def("users").As("u");
var result = PgSql.Select(user["id"], user["name"])
    .From(user)
    .Where(user["age"], Op.Gte, Param.Value(18))
    .Build();
// → SELECT "u"."id", "u"."name" FROM "users" AS "u" WHERE "u"."age" >= @p0
```

**源代码生成器**：编译时分析 SQL 模板，生成零开销查询方法。

### YTStdAdo — 数据库访问层

基于 Npgsql 的零分配数据库操作封装。

### YTStdLogger — 日志框架

高性能异步日志，支持延迟求值（`Func<string>`），避免未启用日志等级的字符串分配。

```csharp
Logger.Debug(tenantId, userId, () => $"查询用户: id={userId}");
```

### YTStdEntity — 实体框架

通过 `[Entity]` / `[Column]` / `[Index]` 特性定义实体，Source Generator 自动生成：

- **DAL** — 建表 / 视图 / 索引 / 审计表 DDL
- **CRUD** — Insert / Update / Delete / Get / GetList
- **审计查询** — 审计记录查询 / 历史 / 快照比较
- **描述类** — 字段常量 / 元数据

```csharp
[Entity(TableName = "sys_user", NeedAuditTable = true)]
[Index("idx_user_email", "email", Kind = IndexKind.Unique)]
public class SysUser
{
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    [Column(Title = "用户名", Length = 50, IsRequired = true)]
    public string Name { get; set; } = "";
}
```

### YTStdI18n — 国际化

数组索引 O(1) 翻译查找，比 Dictionary 快 3-5 倍。Source Generator 编译期校验语言包一致性。

```csharp
I18n.Init(Lang.ZhCn);
I18n.Register();

// 使用生成的常量键
string msg = K.Common.Success.Common();   // → "操作成功"

I18n.DefaultLang = Lang.En;
string msg2 = K.Common.Success.Common();   // → "Operation successful"

// 租户级语言偏好
I18n.SetTenantLang(1001, Lang.En);
string msg3 = K.Common.Success.Common(1001);  // → 租户使用英文翻译
```

## 构建与测试

```bash
# 构建
dotnet build YTStd.slnx

# 运行测试
dotnet test YTStd.slnx

# 运行性能基准
dotnet run --project benchmarks/YTStdSqlBuilder.Benchmarks -c Release
```

## 许可证

详见 [LICENSE](LICENSE) 文件。
