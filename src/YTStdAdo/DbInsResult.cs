namespace YTStdAdo;

/// <summary>插入操作返回结构</summary>
public sealed class DbInsResult
{
    public bool Success { get; init; }
    public long Id { get; init; }
    public string? ErrorMessage { get; init; }
    public string? DebugMessage { get; init; }
}
