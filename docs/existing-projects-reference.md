# 已有项目参考文档

> **用途**：本文档为新提示词（如 ADO、日志等）提供已有项目的 API 参考。
> 新提示词应引用本文档而非重复定义已有类型。
>
> **更新时机**：当已有项目的公开 API 发生变更时，同步更新本文档。

---

## 目录

- [YTStdSqlBuilder（SQL 构建器）](#ytstdsqlbuildersql-构建器)
- [YTStdSqlBuilder.Generator（源代码生成器）](#ytstdsqlbuildergenerator源代码生成器)

---


## YTStdSqlBuilder（SQL 构建器）

### 项目信息

| 属性 | 值 |
|------|-----|
| 路径 | `src/YTStdSqlBuilder/` |
| 目标框架 | net10.0 |
| NuGet 包 | YTStdSqlBuilder |
| 依赖 | Npgsql |

### 核心输出类型

```csharp
// SQL 构建结果
public readonly struct PgSqlRenderResult
{
    public readonly string Sql;
    public readonly PgSqlParam[] Params;
    public PgSqlRenderResult(string sql, PgSqlParam[] @params);
}

// SQL 参数
public readonly struct PgSqlParam
{
    public readonly string Name;
    public readonly NpgsqlDbType? DbType;
    public readonly object? Value;
    public PgSqlParam(string name, object? value, NpgsqlDbType? dbType = null);
}
```

### 入口点静态类

```csharp
// 主入口 — 创建各类 SQL Builder
public static class PgSql
{
    public static PgSqlQueryBuilder Select(params SqlSelectItem[] items);
    public static PgSqlQueryBuilder Select(params SqlExpr[] exprs);
    public static PgSqlInsertBuilder InsertInto(SqlTableSource table);
    public static PgSqlUpdateBuilder Update(SqlTableSource table);
    public static PgSqlDeleteBuilder DeleteFrom(SqlTableSource table);
}

// 参数创建
public static class Param
{
    public static ParamExpr Value(object? value);
    public static ParamExpr Value(object? value, NpgsqlDbType dbType);
}

// 表定义
public static class Table
{
    public static SqlTable Def(string name);
    public static SqlTable Def(string schema, string name);
}

// 比较运算符
public static class Op
{
    // 基本比较
    public static readonly SqlComparisonOperator Eq;       // =
    public static readonly SqlComparisonOperator NotEq;    // <>
    public static readonly SqlComparisonOperator Gt;       // >
    public static readonly SqlComparisonOperator Gte;      // >=
    public static readonly SqlComparisonOperator Lt;       // <
    public static readonly SqlComparisonOperator Lte;      // <=

    // 字符串匹配
    public static readonly SqlComparisonOperator Like;     // LIKE
    public static readonly SqlComparisonOperator ILike;    // ILIKE
    public static readonly SqlComparisonOperator NotLike;  // NOT LIKE
    public static readonly SqlComparisonOperator NotILike; // NOT ILIKE

    // 集合
    public static readonly SqlComparisonOperator In;       // IN
    public static readonly SqlComparisonOperator NotIn;    // NOT IN

    // NULL 检查
    public static readonly SqlComparisonOperator IsNull;    // IS NULL
    public static readonly SqlComparisonOperator IsNotNull; // IS NOT NULL

    // 范围
    public static readonly SqlComparisonOperator Between;    // BETWEEN
    public static readonly SqlComparisonOperator NotBetween; // NOT BETWEEN

    // 子查询
    public static readonly SqlComparisonOperator Exists;     // EXISTS
    public static readonly SqlComparisonOperator NotExists;  // NOT EXISTS

    // PostgreSQL 数组运算符
    public static readonly SqlComparisonOperator ArrayContains;    // @>
    public static readonly SqlComparisonOperator ArrayContainedBy; // <@
    public static readonly SqlComparisonOperator ArrayOverlaps;    // &&
}

// 字面量
public static class Literal
{
    public static readonly LiteralExpr True;
    public static readonly LiteralExpr False;
    public static readonly LiteralExpr Null;
    public static readonly LiteralExpr One;
    public static readonly LiteralExpr Zero;
    public static LiteralExpr Of(string value);
    public static LiteralExpr Of(int value);
    public static LiteralExpr Of(long value);
    public static LiteralExpr Of(decimal value);
    public static LiteralExpr Of(double value);
    public static LiteralExpr Raw(string sqlText);
}

// SQL 函数
public static class Func
{
    public static FuncExpr Count(SqlExpr expr);
    public static FuncExpr Sum(SqlExpr expr);
    public static FuncExpr Avg(SqlExpr expr);
    public static FuncExpr Min(SqlExpr expr);
    public static FuncExpr Max(SqlExpr expr);
    public static FuncExpr Coalesce(params SqlExpr[] exprs);
    public static FuncExpr NullIf(SqlExpr a, SqlExpr b);
}

// SELECT * 表达式
public static class All
{
    public static readonly AllExpr Value;
    public static AllExpr Of(SqlTableSource table);
}

// CASE 表达式
public static class Case
{
    public static CaseWhenBuilder When(SqlExpr left, SqlComparisonOperator op, SqlExpr right);
}

// EXISTS 子查询
public static class Exists
{
    public static SubQueryExpr Of(PgSqlQueryBuilder subQuery);
}
```

### 枚举类型

```csharp
public enum SqlComparisonOperator
{
    Eq, NotEq, Gt, Gte, Lt, Lte,
    Like, ILike, NotLike, NotILike,
    In, NotIn,
    IsNull, IsNotNull,
    Between, NotBetween,
    Exists, NotExists,
    ArrayContains,      // @> (PostgreSQL 数组包含)
    ArrayContainedBy,   // <@ (PostgreSQL 被数组包含)
    ArrayOverlaps       // && (PostgreSQL 数组重叠)
}

public enum SqlLogicalOperator { And, Or }
public enum SqlJoinType { Inner, Left, Right }
public enum PgSqlRenderMode { Parameterized, DebugSql }
```

### 表达式类型体系

```csharp
// 表达式基类
public abstract class SqlExpr
{
    public abstract SqlExprKind Kind { get; }
    public SqlSelectItem As<T>(string? alias = null);
    public SqlSelectItem As(string alias);
    public SqlOrderItem Asc();
    public SqlOrderItem Desc();
}

// 列表达式
public sealed class ColumnExpr : SqlExpr { public SqlColumn Column { get; } }

// 参数表达式
public sealed class ParamExpr : SqlExpr { public object? Value { get; } public NpgsqlDbType? DbType { get; } }

// 字面量表达式
public sealed class LiteralExpr : SqlExpr { public string SqlText { get; } public object? Value { get; } }

// 函数表达式
public sealed class FuncExpr : SqlExpr { public string FunctionName { get; } public SqlExpr[] Arguments { get; } }

// 原始 SQL 表达式
public sealed class RawExpr : SqlExpr { public string SqlText { get; } }

// 子查询表达式
public sealed class SubQueryExpr : SqlExpr { public string Sql { get; } public PgSqlParam[] Params { get; } }
```

### 模型类型

```csharp
// 表定义
public sealed class SqlTable
{
    public string Name { get; }
    public string? Schema { get; }
    public SqlTableSource As(string alias);
}

// 表源（带别名）
public sealed class SqlTableSource
{
    public SqlTable Table { get; }
    public string Alias { get; }
    public ColumnExpr this[string columnName] { get; }  // 索引器访问列
    public ColumnExpr Col(string columnName);
    public ColumnExpr Col<T>(string columnName);        // 带类型信息的列
}

// SQL SELECT 项
public sealed class SqlSelectItem
{
    public SqlExpr Expression { get; }
    public string? Alias { get; }
    public Type? ClrType { get; }
    public static implicit operator SqlSelectItem(SqlExpr expr);
}
```

### Builder API

#### PgSqlQueryBuilder（SELECT 查询构建器）

```csharp
public sealed class PgSqlQueryBuilder
{
    // SELECT
    public PgSqlQueryBuilder Distinct();

    // FROM
    public PgSqlQueryBuilder From(SqlTableSource table);
    public PgSqlQueryBuilder From(PgSqlQueryBuilder subQuery, string alias);

    // JOIN
    public PgSqlQueryBuilder Join(SqlTableSource table, Action<JoinBuilder> configure);
    public PgSqlQueryBuilder LeftJoin(SqlTableSource table, Action<JoinBuilder> configure);
    public PgSqlQueryBuilder RightJoin(SqlTableSource table, Action<JoinBuilder> configure);

    // WHERE（条件）
    public PgSqlQueryBuilder Where(SqlExpr left, SqlComparisonOperator op, SqlExpr right);
    public PgSqlQueryBuilder Where(SqlExpr expr);
    public PgSqlQueryBuilder WhereIf(bool condition, SqlExpr left, SqlComparisonOperator op, SqlExpr right);
    public PgSqlQueryBuilder And(SqlExpr left, SqlComparisonOperator op, SqlExpr right);
    public PgSqlQueryBuilder AndIf(bool condition, SqlExpr left, SqlComparisonOperator op, SqlExpr right);
    public PgSqlQueryBuilder Or(SqlExpr left, SqlComparisonOperator op, SqlExpr right);
    public PgSqlQueryBuilder OrIf(bool condition, SqlExpr left, SqlComparisonOperator op, SqlExpr right);

    // 条件组
    public PgSqlQueryBuilder WhereGroup(Action<ConditionGroupBuilder> group);
    public PgSqlQueryBuilder AndGroup(Action<ConditionGroupBuilder> group);
    public PgSqlQueryBuilder OrGroup(Action<ConditionGroupBuilder> group);

    // GROUP BY / HAVING
    public PgSqlQueryBuilder GroupBy(params SqlExpr[] exprs);
    public PgSqlQueryBuilder Having(SqlExpr left, SqlComparisonOperator op, SqlExpr right);

    // ORDER BY
    public PgSqlQueryBuilder OrderBy(params SqlOrderItem[] items);
    public PgSqlQueryBuilder OrderBy(params SqlExpr[] exprs);
    public PgSqlQueryBuilder OrderByDesc(params SqlExpr[] exprs);

    // LIMIT / OFFSET
    public PgSqlQueryBuilder Limit(int limit);
    public PgSqlQueryBuilder Offset(int offset);

    // 构建
    public PgSqlRenderResult Build();
    public string BuildDebugSql();
}
```

#### PgSqlInsertBuilder（INSERT 构建器）

```csharp
public sealed class PgSqlInsertBuilder
{
    public PgSqlInsertBuilder Set(ColumnExpr column, SqlExpr value);
    public PgSqlInsertBuilder Columns(params ColumnExpr[] columns);
    public PgSqlInsertBuilder Values(params SqlExpr[] values);
    public PgSqlInsertBuilder Returning(params SqlSelectItem[] items);
    public PgSqlRenderResult Build();
    public string BuildDebugSql();
}
```

#### PgSqlUpdateBuilder（UPDATE 构建器）

```csharp
public sealed class PgSqlUpdateBuilder
{
    public PgSqlUpdateBuilder Set(ColumnExpr column, SqlExpr value);
    public PgSqlUpdateBuilder SetIf(bool condition, ColumnExpr column, SqlExpr value);
    // WHERE 方法同 PgSqlQueryBuilder
    public PgSqlUpdateBuilder AllowUnsafeUpdate();
    public PgSqlRenderResult Build();
    public string BuildDebugSql();
}
```

#### PgSqlDeleteBuilder（DELETE 构建器）

```csharp
public sealed class PgSqlDeleteBuilder
{
    // WHERE 方法同 PgSqlQueryBuilder
    public PgSqlDeleteBuilder AllowUnsafeDelete();
    public PgSqlRenderResult Build();
    public string BuildDebugSql();
}
```

### 使用示例

#### 基本 SELECT 查询

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

#### 动态条件查询

```csharp
var user = Table.Def("users").As("u");
var result = PgSql
    .Select(user["id"], user["name"])
    .From(user)
    .WhereIf(hasName, user["name"], Op.ILike, Param.Value(name))
    .AndIf(hasAge, user["age"], Op.Gte, Param.Value(minAge))
    .Build();
```

#### INSERT + RETURNING

```csharp
var result = PgSql.InsertInto(user)
    .Set(user["name"], Param.Value("Test"))
    .Returning(user["id"])
    .Build();
// → INSERT INTO "users" ("name") VALUES (@p0) RETURNING "u"."id"
```

#### JOIN 查询

```csharp
var user = Table.Def("users").As("u");
var order = Table.Def("orders").As("o");
var result = PgSql
    .Select(user["id"], user["name"], order["total"])
    .From(user)
    .LeftJoin(order, j => j.On(order["user_id"], Op.Eq, user["id"]))
    .Build();
```

#### 条件组（嵌套括号）

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

#### 子查询（EXISTS）

```csharp
var subquery = PgSql
    .Select(Literal.Of(1))
    .From(order)
    .Where(order["user_id"], Op.Eq, user["id"]);

var result = PgSql
    .Select(user["id"], user["name"])
    .From(user)
    .Where(Exists.Of(subquery))
    .Build();
```

---

## YTStdSqlBuilder.Generator（源代码生成器）

### 项目信息

| 属性 | 值 |
|------|-----|
| 路径 | `src/YTStdSqlBuilder.Generator/` |
| 目标框架 | netstandard2.0（Roslyn 组件要求） |
| NuGet 包 | YTStdSqlBuilder.Generator |
| 引用方式 | `OutputItemType="Analyzer" ReferenceOutputAssembly="false"` |

### 核心属性

```csharp
// 标记模板类
[AttributeUsage(AttributeTargets.Class)]
public sealed class PgSqlTemplateAttribute : Attribute { }

// 标记查询方法
[AttributeUsage(AttributeTargets.Method)]
public sealed class PgSqlQueryAttribute : Attribute { }
```

### 模板 API（仅用于编译期分析，不在运行时调用）

```csharp
public sealed class PgSqlTemplateBuilder
{
    public TemplateTableSource Table(string tableName, string alias);
    public ParamExpr Param<T>(string paramName);
    public ParamExpr Param(string paramName);
    public TemplateQueryBuilder Select(params SqlSelectItem[] items);
}

public sealed class TemplateTableSource
{
    public string TableName { get; }
    public string Alias { get; }
    public ColumnExpr Col(string name);
    public ColumnExpr Col<T>(string name);
}

public sealed class TemplateQueryBuilder
{
    public TemplateQueryBuilder From(TemplateTableSource table);
    public TemplateQueryBuilder Where(SqlExpr left, SqlComparisonOperator op, SqlExpr right);
    public TemplateQueryBuilder WhereIf(SqlExpr left, SqlComparisonOperator op, SqlExpr right);
    public TemplateQueryBuilder And(SqlExpr left, SqlComparisonOperator op, SqlExpr right);
    public TemplateQueryBuilder AndIf(SqlExpr left, SqlComparisonOperator op, SqlExpr right);
    public TemplateQueryBuilder Or(SqlExpr left, SqlComparisonOperator op, SqlExpr right);
    public TemplateQueryBuilder OrIf(SqlExpr left, SqlComparisonOperator op, SqlExpr right);
    public TemplateQueryBuilder LeftJoin(TemplateTableSource table, Action<object> onBuilder);
    public TemplateQueryBuilder InnerJoin(TemplateTableSource table, Action<object> onBuilder);
    public TemplateQueryBuilder GroupBy(params SqlExpr[] fields);
    public TemplateQueryBuilder Having(SqlExpr left, SqlComparisonOperator op, SqlExpr right);
    public TemplateQueryBuilder OrderBy(params SqlExpr[] fields);
    public TemplateQueryBuilder OrderByDesc(params SqlExpr[] fields);
    public TemplateQueryBuilder Limit(int count);
    public TemplateQueryBuilder Offset(int count);
}
```

### Source Generator 使用示例

```csharp
// 定义模板类（必须是 public static partial class）
[PgSqlTemplate]
public static partial class UserQueries
{
    // 静态查询（编译期生成 SQL 常量）
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

    // 动态查询（WhereIf — 生成带 bool 条件参数的方法）
    [PgSqlQuery]
    public static partial PgSqlRenderResult SearchUsers(
        bool name_condition, string name,
        bool minAge_condition, int minAge);

    private static void Define_SearchUsers(PgSqlTemplateBuilder b)
    {
        var user = b.Table("users", "u");
        b.Select(
            user.Col<int>("id"),
            user.Col<string>("name"))
         .From(user)
         .WhereIf(user.Col("name"), Op.ILike, b.Param<string>("name"))
         .AndIf(user.Col("age"), Op.Gte, b.Param<int>("minAge"));
    }
}
```

### 生成器输出

对于静态查询，生成器输出：
- `public const string {MethodName}_Sql` — 编译期 SQL 常量
- `public static partial PgSqlRenderResult {MethodName}(...)` — 构造参数数组并返回结果

对于动态查询（含 WhereIf/AndIf/OrIf），生成器输出：
- 运行时 Builder 调用代码
- 使用 `bool {paramName}_condition` 参数控制条件是否添加

对于有类型化 SELECT 列的查询，生成器额外输出：
- `public sealed class {MethodName}Row` — DTO 结构类
- `public static {MethodName}Row Read{MethodName}Row(NpgsqlDataReader reader)` — Reader 映射方法

---

## 关键设计模式

1. **Builder 模式**：所有构建器使用流式接口（链式调用），最后调用 `Build()` 生成结果
2. **表达式树**：SQL 表示为 `SqlExpr` 表达式树，渲染时遍历生成 SQL 文本
3. **安全性**：UPDATE / DELETE 没有 WHERE 时抛出异常，需显式调用 `AllowUnsafe*()`
4. **参数化**：默认使用 `@p0, @p1, ...` 参数占位符，防止 SQL 注入
5. **高性能**：渲染器使用 `ref struct RenderContext` + `ValueStringBuilder` + `ArrayPool`，零分配
6. **AOT 友好**：所有关键路径无反射，Source Generator 编译期解析
