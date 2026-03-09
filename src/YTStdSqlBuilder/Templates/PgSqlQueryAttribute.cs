namespace YTStdSqlBuilder;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class PgSqlQueryAttribute : Attribute
{
    public bool FallbackToInterpreter { get; set; }
}
