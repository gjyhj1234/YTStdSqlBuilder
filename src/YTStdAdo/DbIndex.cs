using System;

namespace YTStdAdo;

/// <summary>数据库表索引信息</summary>
public sealed class DbIndex
{
    public string IndexName { get; init; } = "";
    public string TableName { get; init; } = "";
    public bool IsUnique { get; init; }
    public string[] Columns { get; init; } = Array.Empty<string>();
}
