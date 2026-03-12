namespace YTStdEntity;

/// <summary>字段元数据，描述单个数据库列的完整信息</summary>
public sealed class YTFieldMeta
{
    /// <summary>数据库字段名称</summary>
    public string Name { get; init; } = "";

    /// <summary>数据库字段类型（PostgreSQL 类型字符串）</summary>
    public string Type { get; init; } = "";

    /// <summary>字段长度（仅对 varchar / decimal 有效）</summary>
    public int Length { get; init; }

    /// <summary>字段精度（仅对 decimal 有效）</summary>
    public int Precision { get; init; }

    /// <summary>是否允许为空</summary>
    public bool IsNullable { get; init; }

    /// <summary>是否为主键</summary>
    public bool IsPrimaryKey { get; init; }

    /// <summary>是否为租户字段</summary>
    public bool IsTenant { get; init; }
}
