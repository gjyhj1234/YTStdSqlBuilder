namespace YTStdAdo;

/// <summary>数据库表字段信息</summary>
public sealed class DbField
{
    public string FieldName { get; init; } = "";
    public string DataType { get; init; } = "";
    public int? MaxLength { get; init; }
    public int? NumericPrecision { get; init; }
    public int? NumericScale { get; init; }
    public bool IsNullable { get; init; }
    public bool IsPrimaryKey { get; init; }
}
