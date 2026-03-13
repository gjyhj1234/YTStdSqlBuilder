namespace YTStdAdo;

/// <summary>数据库表字段信息</summary>
public sealed class DbField
{
    /// <summary>字段名称</summary>
    public string FieldName { get; init; } = "";
    /// <summary>数据类型（如 varchar、integer、numeric 等）</summary>
    public string DataType { get; init; } = "";
    /// <summary>最大长度（仅适用于字符类型）</summary>
    public int? MaxLength { get; init; }
    /// <summary>数值精度（总位数）</summary>
    public int? NumericPrecision { get; init; }
    /// <summary>数值小数位数</summary>
    public int? NumericScale { get; init; }
    /// <summary>是否允许为空</summary>
    public bool IsNullable { get; init; }
    /// <summary>是否为主键</summary>
    public bool IsPrimaryKey { get; init; }
}
