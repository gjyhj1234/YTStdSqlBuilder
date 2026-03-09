using YTStdSqlBuilder.Internal;

namespace YTStdSqlBuilder;

public sealed class QueryResultColumn
{
    public string Name { get; }
    public string PropertyName { get; }
    public Type ClrType { get; }
    public int Ordinal { get; }
    public bool IsNullable { get; }

    public QueryResultColumn(string name, Type clrType, int ordinal, bool isNullable = false)
    {
        Name = Guard.NotNullOrEmpty(name);
        PropertyName = ToPascalCase(name);
        ClrType = Guard.NotNull(clrType);
        Ordinal = ordinal;
        IsNullable = isNullable;
    }

    public QueryResultColumn(string name, string propertyName, Type clrType, int ordinal, bool isNullable = false)
    {
        Name = Guard.NotNullOrEmpty(name);
        PropertyName = Guard.NotNullOrEmpty(propertyName);
        ClrType = Guard.NotNull(clrType);
        Ordinal = ordinal;
        IsNullable = isNullable;
    }

    private static string ToPascalCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;

        Span<char> chars = stackalloc char[name.Length];
        bool capitalizeNext = true;
        int writeIndex = 0;

        for (int i = 0; i < name.Length; i++)
        {
            char c = name[i];
            if (c == '_' || c == '-')
            {
                capitalizeNext = true;
                continue;
            }

            chars[writeIndex++] = capitalizeNext ? char.ToUpperInvariant(c) : c;
            capitalizeNext = false;
        }

        return new string(chars.Slice(0, writeIndex));
    }
}
