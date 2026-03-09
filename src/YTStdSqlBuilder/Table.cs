namespace YTStdSqlBuilder;

public static class Table
{
    public static SqlTable Def(string name) => new(name);
    public static SqlTable Def(string schema, string name) => new(name, schema);
}
