using System;

namespace YTStdAdo;

/// <summary>数据库表索引信息</summary>
public sealed class DbIndex
{
    /// <summary>索引名称</summary>
    public string IndexName { get; init; } = "";
    /// <summary>所属表名称</summary>
    public string TableName { get; init; } = "";
    /// <summary>是否为唯一索引</summary>
    public bool IsUnique { get; init; }
    /// <summary>包含的列名列表</summary>
    public string[] Columns { get; init; } = Array.Empty<string>();
}
