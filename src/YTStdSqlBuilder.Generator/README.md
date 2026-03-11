# YTStdSqlBuilder.Generator

`YTStdSqlBuilder.Generator` 是 `YTStdSqlBuilder` 的 Roslyn Source Generator 组件，在编译期分析 `[PgSqlTemplate]` 类中的 SQL 定义方法，自动生成高性能的 SQL 构建代码。

## 工作原理

1. 用户使用 `[PgSqlTemplate]` 标记 partial 类
2. 用户使用 `[PgSqlQuery]` 标记 partial 方法声明
3. 用户编写 `Define_{MethodName}(PgSqlTemplateBuilder b)` 方法描述 SQL 结构
4. Source Generator 在编译期分析 `Define_` 方法的语法树
5. 自动生成方法实现、SQL 常量、结果 DTO、Reader 映射方法

## 生成规则

### 静态查询（无动态条件）

当 `Define_` 方法不包含 `WhereIf/AndIf/OrIf` 时，生成：
- `{MethodName}_Sql` 常量字符串
- 方法实现直接返回常量 + 参数数组
- `Logger.Debug` 调用使用 `Func<string>` 延迟求值

```csharp
// 生成的代码
public const string GetUserById_Sql = "SELECT \"u\".\"id\" ...";

public static partial PgSqlRenderResult GetUserById(int tenantId, long userId, int id)
{
    Logger.Debug(tenantId, userId, () => $"[GetUserById] SQL: {GetUserById_Sql}");
    return new PgSqlRenderResult(GetUserById_Sql, new PgSqlParam[] { new("@p0", id) });
}
```

### 动态查询（包含 WhereIf 等）

当 `Define_` 方法包含动态条件时，回退到运行时 Builder：
- 不生成 SQL 常量
- 方法实现使用运行时 Builder 构建 SQL
- 仍然生成结果 DTO（如果 SELECT 列固定）

## 方法签名要求

所有 `[PgSqlQuery]` 方法**必须**包含 `int tenantId, long userId` 作为前两个参数：

```csharp
[PgSqlQuery]
public static partial PgSqlRenderResult GetUserById(int tenantId, long userId, int id);
```

这些参数用于 `Logger.Debug` 调用中记录租户与用户信息。

## 日志集成

生成的代码自动包含 `Logger.Debug` 调用，使用 `Func<string>` 延迟求值重载：

```csharp
// 静态查询：记录方法名 + SQL
Logger.Debug(tenantId, userId, () => $"[GetUserById] SQL: {GetUserById_Sql}");

// 动态查询：记录方法入口 + 构建后的 SQL
Logger.Debug(tenantId, userId, () => "[SearchUsers] 开始构建动态SQL");
Logger.Debug(tenantId, userId, () => $"[SearchUsers] SQL: {__result__.Sql}");
```

使用 `Func<string>` 重载的好处：当 Debug 等级未启用时，不会创建插值字符串，避免不必要的 GC 压力。

## 项目引用配置

使用本 Generator 的项目需要如下配置：

```xml
<!-- 引用 Source Generator -->
<ProjectReference Include="..\..\src\YTStdSqlBuilder.Generator\YTStdSqlBuilder.Generator.csproj"
    OutputItemType="Analyzer"
    ReferenceOutputAssembly="false" />

<!-- 引用运行时库 -->
<ProjectReference Include="..\..\src\YTStdSqlBuilder\YTStdSqlBuilder.csproj" />

<!-- 引用日志库（生成代码中调用 Logger.Debug） -->
<ProjectReference Include="..\..\src\YTStdLogger\YTStdLogger.csproj" />
```

## 技术约束

- 目标框架：`netstandard2.0`（Roslyn 组件要求）
- 依赖：`Microsoft.CodeAnalysis.CSharp`（`4.12.0`）
- NativeAOT 兼容：生成的代码完全兼容 NativeAOT
