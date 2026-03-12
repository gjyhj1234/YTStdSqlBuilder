namespace YTStdEntity.Generator.Models;

/// <summary>编译期索引模型</summary>
internal sealed class IndexModel
{
    /// <summary>索引名称</summary>
    public string IndexName { get; set; } = "";

    /// <summary>索引字段列表</summary>
    public string[] Columns { get; set; } = System.Array.Empty<string>();

    /// <summary>是否为唯一索引</summary>
    public bool IsUnique { get; set; }
}
