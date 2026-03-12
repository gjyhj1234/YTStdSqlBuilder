# YTStdSqlBuilder

`YTStdSqlBuilder` 是一个面向 `.NET 10.0 + NativeAOT` 的 PostgreSQL SQL 构建器，提供类型安全的 API 来构建参数化 SQL 语句。

## 核心设计

- **运行时 Builder API**：通过 `PgSql.Select/Insert/Update/Delete` 流式构建 SQL
- **模板 API**：通过 `[PgSqlTemplate]` + `Define_` 方法定义 SQL 结构，由 Source Generator 在编译期生成最优代码
- **零分配渲染**：使用 `ValueStringBuilder`（ref struct + ArrayPool + stackalloc）进行 SQL 拼装
- **参数化安全**：所有用户输入通过 `@p0, @p1, ...` 参数传递，防止 SQL 注入
- **NativeAOT 兼容**：无反射、无动态、无 LINQ

## 快速使用

### 运行时 Builder API

```csharp
using YTStdSqlBuilder;

var user = Table.Def("users").As("u");

// SELECT 查询
var query = PgSql
    .Select(user["id"].As<int>("id"), user["name"], user["age"].As<int>("age"))
    .From(user)
    .Where(user["age"], Op.Gte, Param.Value(18))
    .OrderBy(user["name"].Asc())
    .Limit(10)
    .Build();

// query.Sql:  SELECT "u"."id", "u"."name", "u"."age" FROM "users" AS "u" WHERE "u"."age" >= @p0 ORDER BY "u"."name" ASC LIMIT 10
// query.Params: [PgSqlParam("@p0", 18)]

// INSERT
var insert = PgSql
    .Insert("users")
    .Columns("name", "age")
    .Values(Param.Value("张三"), Param.Value(25))
    .Build();

// UPDATE
var update = PgSql
    .Update("users")
    .Set("name", Param.Value("李四"))
    .Where(Col.Of("id"), Op.Eq, Param.Value(1))
    .Build();

// DELETE
var delete = PgSql
    .Delete("users")
    .Where(Col.Of("id"), Op.Eq, Param.Value(1))
    .Build();
```

### 模板 API（需配合 Source Generator）

```csharp
using YTStdSqlBuilder;
using YTStdSqlBuilder.Templates;

[PgSqlTemplate]
public static partial class UserQueries
{
    [PgSqlQuery]
    public static partial PgSqlRenderResult GetUserById(int tenantId, long userId, int id);

    private static void Define_GetUserById(PgSqlTemplateBuilder b)
    {
        var user = b.Table("users", "u");
        b.Select(
            user.Col<int>("id"),
            user.Col<string>("name"),
            user.Col<int>("age"))
         .From(user)
         .Where(user.Col("id"), Op.Eq, b.Param<int>("id"));
    }
}
```

Source Generator 自动生成：
- `GetUserById_Sql` 常量（编译期计算的 SQL 字符串）
- `GetUserById(...)` 方法实现（含 `Logger.Debug` 调用）
- `GetUserByIdRow` 结果 DTO 类
- `ReadGetUserByIdRow(NpgsqlDataReader)` Reader 映射方法

## 核心类型

| 类型 | 说明 |
|------|------|
| `PgSqlRenderResult` | SQL 渲染结果（`.Sql` + `.Params`） |
| `PgSqlParam` | SQL 参数（`.Name`, `.Value`, `.DbType`） |
| `PgSql` | 静态入口：`Select/Insert/Update/Delete` |
| `Table` | 表定义：`Table.Def("users").As("u")` |
| `Op` | 比较运算符：`Eq, NotEq, Gt, Gte, Lt, Lte, Like, ILike, In, NotIn, ...` |
| `Param` | 参数工厂：`Param.Value(...)` |
| `Literal` | 字面量：`Literal.True, Literal.False, Literal.Null, Literal.One` |
| `Func` | SQL 函数：`Func.Count, Func.Sum, Func.Max, ...` |

## 与 ADO 层集成

```csharp
// 构建查询
var query = PgSql.Select(user["id"], user["name"]).From(user)
    .Where(user["age"], Op.Gte, Param.Value(18)).Build();

// 传给 ADO 层执行
var (result, data) = await DB.GetListAsync(
    query.Sql, query.Params,
    reader => new { Id = reader.GetInt64(0), Name = reader.GetString(1) },
    tenantId, userId
);
```

## NativeAOT 注意事项

- 不使用 `System.Linq`、反射、`dynamic`
- 使用 `ValueStringBuilder` 进行零分配 SQL 拼装
- Source Generator 在编译期生成代码，运行时无反射
