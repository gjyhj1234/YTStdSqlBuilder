namespace YTStdEntity.Generator.Models;

/// <summary>编译期列模型</summary>
internal sealed class ColumnModel
{
    /// <summary>CLR 属性名</summary>
    public string PropertyName { get; set; } = "";

    /// <summary>数据库列名（snake_case）</summary>
    public string ColumnName { get; set; } = "";

    /// <summary>显示标题</summary>
    public string? Title { get; set; }

    /// <summary>CLR 类型全名</summary>
    public string ClrTypeName { get; set; } = "";

    /// <summary>PostgreSQL 类型</summary>
    public string PgType { get; set; } = "";

    /// <summary>NpgsqlDbType 枚举名</summary>
    public string NpgsqlDbTypeName { get; set; } = "";

    /// <summary>是否为主键</summary>
    public bool IsPrimaryKey { get; set; }

    /// <summary>是否可空</summary>
    public bool IsNullable { get; set; }

    /// <summary>是否必填</summary>
    public bool IsRequired { get; set; }

    /// <summary>字段长度</summary>
    public int Length { get; set; }

    /// <summary>字段精度</summary>
    public int Precision { get; set; } = 2;

    /// <summary>是否为租户字段</summary>
    public bool IsTenantField { get; set; }
}
