namespace YTStdSqlBuilder;

/// <summary>标记类为 PostgreSQL SQL 模板类的特性</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class PgSqlTemplateAttribute : Attribute
{
}
