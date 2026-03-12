namespace YTStdAdo;

/// <summary>更新/删除/查询 统一返回结构</summary>
public sealed class DbUdqResult
{
    public bool Success { get; init; }
    public int RowsAffected { get; init; }
    public string? ErrorMessage { get; init; }
    public string? DebugMessage { get; init; }
}
