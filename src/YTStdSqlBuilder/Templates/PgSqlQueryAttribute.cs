namespace YTStdSqlBuilder;

/// <summary>标记方法为 PostgreSQL 查询方法的特性</summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class PgSqlQueryAttribute : Attribute
{
    /// <summary>是否回退到运行时解释器</summary>
    public bool FallbackToInterpreter { get; set; }
}
