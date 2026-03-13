using YTStdSqlBuilder.Internal;

namespace YTStdSqlBuilder;

/// <summary>查询结果列的元数据</summary>
public sealed class QueryResultColumn
{
    /// <summary>列名（数据库列名）</summary>
    public string Name { get; }
    /// <summary>对应的属性名（PascalCase 格式）</summary>
    public string PropertyName { get; }
    /// <summary>列的 CLR 类型</summary>
    public Type ClrType { get; }
    /// <summary>列在结果集中的序号</summary>
    public int Ordinal { get; }
    /// <summary>是否可为空</summary>
    public bool IsNullable { get; }

    /// <summary>创建查询结果列（属性名自动转为 PascalCase）</summary>
    /// <param name="name">列名</param>
    /// <param name="clrType">CLR 类型</param>
    /// <param name="ordinal">序号</param>
    /// <param name="isNullable">是否可为空</param>
    public QueryResultColumn(string name, Type clrType, int ordinal, bool isNullable = false)
    {
        Name = Guard.NotNullOrEmpty(name);
        PropertyName = ToPascalCase(name);
        ClrType = Guard.NotNull(clrType);
        Ordinal = ordinal;
        IsNullable = isNullable;
    }

    /// <summary>创建查询结果列（指定属性名）</summary>
    /// <param name="name">列名</param>
    /// <param name="propertyName">属性名</param>
    /// <param name="clrType">CLR 类型</param>
    /// <param name="ordinal">序号</param>
    /// <param name="isNullable">是否可为空</param>
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
