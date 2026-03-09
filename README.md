# YTStdSqlBuilder

类型安全、流式 API 的 PostgreSQL 查询构建器，搭配编译时源代码生成器。

## 特性

- **流式构建器模式** — 链式调用构建 SELECT / INSERT / UPDATE / DELETE
- **参数化查询** — 自动生成 `@p0, @p1, …`，防止 SQL 注入
- **动态条件** — `WhereIf` / `AndIf` / `OrIf` 按运行时条件决定是否拼接
- **分组条件** — `WhereGroup` / `AndGroup` / `OrGroup` 生成带括号的条件组
- **子查询** — EXISTS / IN / 标量子查询，参数自动重新编号
- **安全默认值** — UPDATE / DELETE 若无 WHERE 条件则抛出异常，需显式调用 `AllowUnsafeUpdate()` / `AllowUnsafeDelete()`
- **源代码生成器** — 编译时分析 SQL 模板，生成零开销的查询方法
- **AOT 兼容** — 支持 .NET Native AOT 编译

## 安装

主库（运行时 API）：

```xml
<PackageReference Include="YTStdSqlBuilder" />
```

源代码生成器（编译时模板）：

```xml
<PackageReference Include="YTStdSqlBuilder.Generator" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
```

---

## YTStdSqlBuilder 运行时 API

### 基本用法

```csharp
using YTStdSqlBuilder;
using YTStdSqlBuilder.Expressions;
```

#### 定义表和列

```csharp
// 定义表并指定别名
var user = Table.Def("users").As("u");
var order = Table.Def("orders").As("o");

// 也可以指定 schema
var product = Table.Def("inventory", "products").As("p");

// 通过索引器访问列
user["id"]       // → "u"."id"
user["name"]     // → "u"."name"
```

### SELECT 查询

#### 基本查询

```csharp
var user = Table.Def("users").As("u");

var result = PgSql
    .Select(user["id"].As<int>("id"), user["name"])
    .From(user)
    .Where(user["age"], Op.Gte, Param.Value(18))
    .Build();

// result.Sql    → SELECT "u"."id" AS "id", "u"."name" FROM "users" AS "u" WHERE "u"."age" >= @p0
// result.Params → [{ Name: "@p0", Value: 18 }]
```

#### SELECT *

```csharp
var result = PgSql
    .Select(AllExpr.Star)
    .From(user)
    .Build();
// → SELECT * FROM "users" AS "u"
```

#### DISTINCT

```csharp
var result = PgSql
    .Select(user["status"])
    .Distinct()
    .From(user)
    .Build();
// → SELECT DISTINCT "u"."status" FROM "users" AS "u"
```

### WHERE 条件

#### 比较运算符

```csharp
// 支持的运算符：Op.Eq, Op.NotEq, Op.Gt, Op.Gte, Op.Lt, Op.Lte,
//              Op.Like, Op.ILike, Op.NotLike, Op.NotILike,
//              Op.In, Op.NotIn, Op.IsNull, Op.IsNotNull,
//              Op.Between, Op.NotBetween, Op.Exists, Op.NotExists

PgSql.Select(user["id"]).From(user)
    .Where(user["name"], Op.Eq, Param.Value("Alice"))
    .Build();
// → ... WHERE "u"."name" = @p0

PgSql.Select(user["id"]).From(user)
    .Where(user["name"], Op.ILike, Param.Value("%alice%"))
    .Build();
// → ... WHERE "u"."name" ILIKE @p0
```

#### AND / OR 条件

```csharp
var result = PgSql
    .Select(user["id"])
    .From(user)
    .Where(user["age"], Op.Gte, Param.Value(18))
    .And(user["active"], Op.Eq, Param.Value(true))
    .Build();
// → ... WHERE "u"."age" >= @p0 AND "u"."active" = @p1

var result = PgSql
    .Select(user["id"])
    .From(user)
    .Where(user["role"], Op.Eq, Param.Value("admin"))
    .Or(user["role"], Op.Eq, Param.Value("superadmin"))
    .Build();
// → ... WHERE "u"."role" = @p0 OR "u"."role" = @p1
```

#### IN 列表

```csharp
var result = PgSql
    .Select(user["id"])
    .From(user)
    .Where(user["status"], Op.In, Param.Value(new[] { "active", "pending" }))
    .Build();
// → ... WHERE "u"."status" IN (@p0, @p1)
```

#### 动态条件（WhereIf / AndIf / OrIf）

当条件为 `false` 时，对应的 WHERE 子句不会被添加到 SQL 中：

```csharp
int? minAge = 18;
int? maxAge = null;

var result = PgSql
    .Select(user["id"])
    .From(user)
    .WhereIf(minAge.HasValue, user["age"], Op.Gte, Param.Value(minAge))
    .AndIf(maxAge.HasValue, user["age"], Op.Lte, Param.Value(maxAge))
    .Build();
// minAge 有值 → 生成 WHERE "u"."age" >= @p0
// maxAge 为 null → 跳过该条件
```

#### 分组条件（括号）

```csharp
var result = PgSql
    .Select(user["id"])
    .From(user)
    .Where(user["active"], Op.Eq, Param.Value(true))
    .AndGroup(g => g
        .Where(user["age"], Op.Gte, Param.Value(18))
        .And(user["age"], Op.Lte, Param.Value(65)))
    .Build();
// → ... WHERE "u"."active" = @p0 AND ("u"."age" >= @p1 AND "u"."age" <= @p2)
```

支持多层嵌套：

```csharp
var result = PgSql
    .Select(user["id"])
    .From(user)
    .Where(user["active"], Op.Eq, Param.Value(true))
    .AndGroup(g => g
        .Where(user["role"], Op.Eq, Param.Value("admin"))
        .OrGroup(g2 => g2
            .Where(user["age"], Op.Gte, Param.Value(21))
            .And(user["verified"], Op.Eq, Param.Value(true))))
    .Build();
// → ... WHERE "u"."active" = @p0 AND ("u"."role" = @p1 OR ("u"."age" >= @p2 AND "u"."verified" = @p3))
```

### JOIN

```csharp
var user = Table.Def("users").As("u");
var order = Table.Def("orders").As("o");

// LEFT JOIN
var result = PgSql
    .Select(user["id"], order["total"])
    .From(user)
    .LeftJoin(order, j => j.On(order["user_id"], Op.Eq, user["id"]))
    .Build();
// → ... FROM "users" AS "u" LEFT JOIN "orders" AS "o" ON "o"."user_id" = "u"."id"

// INNER JOIN
var result = PgSql
    .Select(user["name"], order["total"])
    .From(user)
    .Join(order, j => j.On(order["user_id"], Op.Eq, user["id"]))
    .Build();
// → ... FROM "users" AS "u" INNER JOIN "orders" AS "o" ON "o"."user_id" = "u"."id"

// 多个 ON 条件
var result = PgSql
    .Select(user["name"])
    .From(user)
    .Join(order, j => j
        .On(order["user_id"], Op.Eq, user["id"])
        .And(order["active"], Op.Eq, Param.Value(true)))
    .Build();
// → ... ON "o"."user_id" = "u"."id" AND "o"."active" = @p0
```

### GROUP BY / HAVING

```csharp
var result = PgSql
    .Select(user["status"], new FuncExpr("COUNT", user["id"]).As("cnt"))
    .From(user)
    .GroupBy(user["status"])
    .Having(new FuncExpr("COUNT", user["id"]), Op.Gt, Param.Value(5))
    .Build();
// → SELECT "u"."status", COUNT("u"."id") AS "cnt"
//   FROM "users" AS "u"
//   GROUP BY "u"."status"
//   HAVING COUNT("u"."id") > @p0
```

### ORDER BY / LIMIT / OFFSET

```csharp
var result = PgSql
    .Select(user["id"], user["name"])
    .From(user)
    .OrderBy(user["name"].Asc(), user["id"].Desc())
    .Limit(10)
    .Offset(20)
    .Build();
// → ... ORDER BY "u"."name" ASC, "u"."id" DESC LIMIT 10 OFFSET 20
```

### CASE 表达式

```csharp
var caseExpr = Case
    .When(user["age"], Op.Lt, Param.Value(18))
    .Then(LiteralExpr.String("minor"))
    .When(user["age"], Op.Lt, Param.Value(65))
    .Then(LiteralExpr.String("adult"))
    .Else(LiteralExpr.String("senior"));

var result = PgSql
    .Select(user["name"], caseExpr.As("age_group"))
    .From(user)
    .Build();
// → SELECT "u"."name", CASE WHEN "u"."age" < @p0 THEN 'minor'
//     WHEN "u"."age" < @p1 THEN 'adult' ELSE 'senior' END AS "age_group"
//   FROM "users" AS "u"
```

### 子查询

#### EXISTS 子查询

```csharp
var subquery = PgSql
    .Select(LiteralExpr.Number(1))
    .From(order)
    .Where(order["user_id"], Op.Eq, user["id"]);

var result = PgSql
    .Select(user["id"], user["name"])
    .From(user)
    .Where(Exists.Of(subquery))
    .Build();
// → ... WHERE EXISTS (SELECT 1 FROM "orders" AS "o" WHERE "o"."user_id" = "u"."id")
```

#### IN 子查询

```csharp
var subResult = PgSql
    .Select(order["user_id"])
    .From(order)
    .Where(order["status"], Op.Eq, Param.Value("completed"))
    .Build();

var result = PgSql
    .Select(user["name"])
    .From(user)
    .Where(user["id"], Op.In, new SubQueryExpr(subResult))
    .Build();
// → ... WHERE "u"."id" IN (SELECT "o"."user_id" FROM "orders" AS "o" WHERE "o"."status" = @p0)
```

#### 标量子查询

```csharp
var subResult = PgSql
    .Select(new FuncExpr("COUNT", order["id"]))
    .From(order)
    .Where(order["user_id"], Op.Eq, user["id"])
    .Build();

var result = PgSql
    .Select(user["name"], new SubQueryExpr(subResult).As("order_count"))
    .From(user)
    .Build();
// → SELECT "u"."name",
//     (SELECT COUNT("o"."id") FROM "orders" AS "o" WHERE "o"."user_id" = "u"."id") AS "order_count"
//   FROM "users" AS "u"
```

### INSERT

```csharp
var user = Table.Def("users").As("u");

// Columns + Values 模式
var result = PgSql.InsertInto(user)
    .Columns(user["name"], user["email"])
    .Values(Param.Value("John"), Param.Value("john@example.com"))
    .Build();
// → INSERT INTO "users" ("name", "email") VALUES (@p0, @p1)

// Set 模式（效果相同）
var result = PgSql.InsertInto(user)
    .Set(user["name"], Param.Value("Jane"))
    .Set(user["email"], Param.Value("jane@example.com"))
    .Build();
// → INSERT INTO "users" ("name", "email") VALUES (@p0, @p1)

// 带 RETURNING
var result = PgSql.InsertInto(user)
    .Set(user["name"], Param.Value("Test"))
    .Returning(user["id"])
    .Build();
// → INSERT INTO "users" ("name") VALUES (@p0) RETURNING "u"."id"
```

### UPDATE

```csharp
var user = Table.Def("users").As("u");

var result = PgSql.Update(user)
    .Set(user["name"], Param.Value("Updated"))
    .Where(user["id"], Op.Eq, Param.Value(1))
    .Build();
// → UPDATE "users" AS "u" SET "name" = @p0 WHERE "u"."id" = @p1

// 条件赋值
var result = PgSql.Update(user)
    .Set(user["name"], Param.Value("Base"))
    .SetIf(shouldUpdateEmail, user["email"], Param.Value("new@example.com"))
    .Where(user["id"], Op.Eq, Param.Value(1))
    .Build();
// shouldUpdateEmail 为 true 时包含 email 赋值，为 false 时跳过
```

> **安全提示**：不带 WHERE 的 UPDATE 会抛出 `InvalidOperationException`。如果确实需要更新所有行，请调用 `AllowUnsafeUpdate()`：

```csharp
PgSql.Update(user)
    .Set(user["active"], Param.Value(false))
    .AllowUnsafeUpdate()
    .Build();
```

### DELETE

```csharp
var user = Table.Def("users").As("u");

var result = PgSql.DeleteFrom(user)
    .Where(user["id"], Op.Eq, Param.Value(42))
    .Build();
// → DELETE FROM "users" AS "u" WHERE "u"."id" = @p0
```

> **安全提示**：不带 WHERE 的 DELETE 同样会抛出异常，需调用 `AllowUnsafeDelete()`。

### 调试 SQL

`BuildDebugSql()` 会将参数值直接内联到 SQL 中（仅用于调试，不要在生产中执行）：

```csharp
var sql = PgSql
    .Select(user["id"])
    .From(user)
    .Where(user["age"], Op.Gte, Param.Value(18))
    .And(user["name"], Op.Like, Param.Value("%test%"))
    .BuildDebugSql();
// → SELECT "u"."id" FROM "users" AS "u" WHERE "u"."age" >= 18 AND "u"."name" LIKE '%test%'
```

支持的类型内联格式：

| 类型 | 示例输出 |
|------|---------|
| `int`, `long`, `decimal`, `double` | `18`, `3.14` |
| `string` | `'Alice'`（自动转义单引号） |
| `bool` | `TRUE` / `FALSE` |
| `null` | `IS NULL` |
| `DateTime` | `'2024-01-15T10:30:00.0000000Z'` |
| `Guid` | `'12345678-...'::uuid` |

### 复杂查询示例

```csharp
var user = Table.Def("users").As("u");
var order = Table.Def("orders").As("o");

var result = PgSql
    .Select(
        user["id"].As<int>("user_id"),
        user["name"],
        new FuncExpr("SUM", order["total"]).As<decimal>("total_spent"))
    .Distinct()
    .From(user)
    .LeftJoin(order, j => j.On(order["user_id"], Op.Eq, user["id"]))
    .Where(user["active"], Op.Eq, Param.Value(true))
    .And(user["age"], Op.Gte, Param.Value(18))
    .GroupBy(user["id"], user["name"])
    .Having(new FuncExpr("SUM", order["total"]), Op.Gt, Param.Value(100m))
    .OrderBy(user["name"].Asc())
    .Limit(50)
    .Offset(0)
    .Build();
```

生成的 SQL：

```sql
SELECT DISTINCT "u"."id" AS "user_id", "u"."name", SUM("o"."total") AS "total_spent"
FROM "users" AS "u"
LEFT JOIN "orders" AS "o" ON "o"."user_id" = "u"."id"
WHERE "u"."active" = @p0 AND "u"."age" >= @p1
GROUP BY "u"."id", "u"."name"
HAVING SUM("o"."total") > @p2
ORDER BY "u"."name" ASC
LIMIT 50 OFFSET 0
```

---

## YTStdSqlBuilder.Generator 源代码生成器

源代码生成器在编译时分析 SQL 模板定义方法，自动生成高效的查询方法实现。

### 基本用法

#### 1. 定义模板类

用 `[PgSqlTemplate]` 标记一个 `public static partial` 类：

```csharp
using YTStdSqlBuilder;
using YTStdSqlBuilder.Conditions;
using YTStdSqlBuilder.Expressions;

[PgSqlTemplate]
public static partial class UserQueries
{
    // 在这里定义查询方法...
}
```

#### 2. 声明查询方法

用 `[PgSqlQuery]` 标记一个 `public static partial` 方法，返回类型为 `PgSqlRenderResult`：

```csharp
[PgSqlQuery]
public static partial PgSqlRenderResult GetUserById(int userId);
```

#### 3. 编写 Define_ 方法

为每个查询方法编写对应的 `Define_` 方法，使用 `PgSqlTemplateBuilder` 描述 SQL 结构：

```csharp
private static void Define_GetUserById(PgSqlTemplateBuilder b)
{
    var user = b.Table("users", "u");
    b.Select(
        user.Col<int>("id"),
        user.Col<string>("name"))
     .From(user)
     .Where(user.Col("id"), Op.Eq, b.Param<int>("userId"));
}
```

> **注意**：`Define_` 方法中使用 `b.Table()` 定义表，`b.Param<T>()` 定义参数（参数名必须与方法签名中的参数名一致），`user.Col<T>()` 定义带类型的列。

### 完整的静态查询示例

```csharp
using YTStdSqlBuilder;
using YTStdSqlBuilder.Conditions;
using YTStdSqlBuilder.Expressions;

[PgSqlTemplate]
public static partial class UserQueries
{
    [PgSqlQuery]
    public static partial PgSqlRenderResult GetUserById(int userId);

    private static void Define_GetUserById(PgSqlTemplateBuilder b)
    {
        var user = b.Table("users", "u");
        b.Select(
            user.Col<int>("id"),
            user.Col<string>("name"))
         .From(user)
         .Where(user.Col("id"), Op.Eq, b.Param<int>("userId"));
    }
}
```

生成器会在编译时生成以下代码（概念示例）：

```csharp
public static partial class UserQueries
{
    public const string GetUserById_Sql =
        "SELECT \"u\".\"id\", \"u\".\"name\" FROM \"users\" AS \"u\" WHERE \"u\".\"id\" = @p0";

    public static partial PgSqlRenderResult GetUserById(int userId)
    {
        return new PgSqlRenderResult(
            GetUserById_Sql,
            new PgSqlParam[] { new("@p0", userId) });
    }
}
```

调用方式：

```csharp
var result = UserQueries.GetUserById(42);
// result.Sql    → 编译时确定的 SQL 常量
// result.Params → [{ Name: "@p0", Value: 42 }]
```

### 无参数查询

```csharp
[PgSqlTemplate]
public static partial class UserQueries
{
    [PgSqlQuery]
    public static partial PgSqlRenderResult GetAllUsers();

    private static void Define_GetAllUsers(PgSqlTemplateBuilder b)
    {
        var user = b.Table("users", "u");
        b.Select(
            user.Col<int>("id"),
            user.Col<string>("name"))
         .From(user);
    }
}
```

### 多列查询

```csharp
[PgSqlTemplate]
public static partial class UserQueries
{
    [PgSqlQuery]
    public static partial PgSqlRenderResult GetUserDetails(int userId);

    private static void Define_GetUserDetails(PgSqlTemplateBuilder b)
    {
        var user = b.Table("users", "u");
        b.Select(
            user.Col<int>("id"),
            user.Col<string>("name"),
            user.Col<string>("email"),
            user.Col<bool>("is_active"))
         .From(user)
         .Where(user.Col("id"), Op.Eq, b.Param<int>("userId"));
    }
}
```

### 动态条件查询

对于含有 `WhereIf` / `AndIf` / `OrIf` 的查询，使用 `b.ConditionRef()` 声明条件引用：

```csharp
[PgSqlTemplate]
public static partial class UserQueries
{
    [PgSqlQuery]
    public static partial PgSqlRenderResult SearchUsers(string? name, int? minAge);

    private static void Define_SearchUsers(PgSqlTemplateBuilder b)
    {
        var user = b.Table("users", "u");
        b.Select(
            user.Col<int>("id"),
            user.Col<string>("name"))
         .From(user)
         .WhereIf(
             b.ConditionRef("name", "!string.IsNullOrEmpty"),
             user.Col("name"), Op.ILike, b.Param<string>("name"))
         .AndIf(
             b.ConditionRef("minAge", ".HasValue"),
             user.Col("age"), Op.Gte, b.Param<int>("minAge"));
    }
}
```

`b.ConditionRef(paramName, conditionTemplate)` 的两个参数：
- `paramName`：方法参数名
- `conditionTemplate`：条件模板，生成器会将其与参数名组合成运行时条件表达式
  - `"!string.IsNullOrEmpty"` → 生成 `!string.IsNullOrEmpty(name)`
  - `".HasValue"` → 生成 `minAge.HasValue`

动态查询生成的代码会使用运行时构建器来处理条件分支。

### 模板 API 对照表

| 运行时 API | 模板 API | 说明 |
|-----------|---------|------|
| `Table.Def("users").As("u")` | `b.Table("users", "u")` | 定义表引用 |
| `user["id"]` | `user.Col("id")` | 列引用 |
| `user["id"].As<int>()` | `user.Col<int>("id")` | 带类型的列引用 |
| `Param.Value(value)` | `b.Param<T>("paramName")` | 参数值（模板中按名称绑定） |
| `PgSql.Select(...)` | `b.Select(...)` | 开始构建查询 |
| — | `b.ConditionRef(name, template)` | 声明动态条件引用 |

---

## API 速览

### 入口点

| 类 | 方法 | 说明 |
|----|------|------|
| `PgSql` | `Select()`, `InsertInto()`, `Update()`, `DeleteFrom()` | 创建各类构建器 |
| `Table` | `Def(name)`, `Def(schema, name)` | 定义表 |
| `Param` | `Value(obj)`, `Value(obj, NpgsqlDbType)` | 创建参数表达式 |
| `Op` | `Eq`, `Gt`, `Like`, `In`, `IsNull`, … | 比较运算符 |
| `Func` | `Count()`, `Sum()`, `Avg()`, `Min()`, `Max()`, `Coalesce()`, `NullIf()` | SQL 函数 |
| `Literal` | `True`, `False`, `Null`, `One`, `Zero`, `Of()`, `Raw()` | 字面量 |
| `Case` | `When().Then().Else()` | CASE 表达式 |
| `Exists` | `Of(subquery)` | EXISTS 子查询 |
| `All` | `Value` (= `*`), `Of(table)` (= `table.*`) | 全选表达式 |

### 构建器方法

| 构建器 | 方法 |
|-------|------|
| `PgSqlQueryBuilder` | `Select()`, `Distinct()`, `From()`, `Join()`, `LeftJoin()`, `RightJoin()`, `Where()`, `WhereIf()`, `And()`, `AndIf()`, `Or()`, `OrIf()`, `WhereGroup()`, `AndGroup()`, `OrGroup()`, `GroupBy()`, `Having()`, `OrderBy()`, `OrderByDesc()`, `Limit()`, `Offset()`, `Build()`, `BuildDebugSql()` |
| `PgSqlInsertBuilder` | `Set()`, `Columns()`, `Values()`, `Returning()`, `Build()`, `BuildDebugSql()` |
| `PgSqlUpdateBuilder` | `Set()`, `SetIf()`, `Where()` / `And()` / `Or()` 及 `If` 变体, `WhereGroup()` / `AndGroup()` / `OrGroup()`, `Returning()`, `AllowUnsafeUpdate()`, `Build()`, `BuildDebugSql()` |
| `PgSqlDeleteBuilder` | `Where()` / `And()` / `Or()` 及 `If` 变体, `WhereGroup()` / `AndGroup()` / `OrGroup()`, `Returning()`, `AllowUnsafeDelete()`, `Build()`, `BuildDebugSql()` |

### 输出

| 类型 | 属性 | 说明 |
|------|------|------|
| `PgSqlRenderResult` | `Sql` | 生成的参数化 SQL 字符串 |
| | `Params` | `PgSqlParam[]` 参数数组 |
| `PgSqlParam` | `Name` | 参数名（`@p0`, `@p1`, …） |
| | `Value` | 参数值 |
| | `DbType` | 可选的 `NpgsqlDbType` |

## 许可证

详见 [LICENSE](LICENSE) 文件。
