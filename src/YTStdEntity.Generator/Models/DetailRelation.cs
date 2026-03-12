namespace YTStdEntity.Generator.Models;

/// <summary>编译期主从关系模型</summary>
internal sealed class DetailRelation
{
    /// <summary>主表实体类名</summary>
    public string MasterClassName { get; set; } = "";

    /// <summary>主表表名</summary>
    public string MasterTableName { get; set; } = "";

    /// <summary>从表实体类名</summary>
    public string DetailClassName { get; set; } = "";

    /// <summary>从表表名</summary>
    public string DetailTableName { get; set; } = "";

    /// <summary>从表中的外键属性名</summary>
    public string ForeignKeyPropertyName { get; set; } = "";

    /// <summary>从表中的外键列名（snake_case）</summary>
    public string ForeignKeyColumnName { get; set; } = "";
}
