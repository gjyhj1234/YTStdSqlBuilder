namespace YTStdAdo;

/// <summary>标量查询返回结构</summary>
public sealed class DbScalarResult<T>
{
    public bool Success { get; init; }
    public T? Value { get; init; }
    public string? ErrorMessage { get; init; }
    public string? DebugMessage { get; init; }
}
