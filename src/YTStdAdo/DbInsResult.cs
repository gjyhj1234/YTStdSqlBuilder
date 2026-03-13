namespace YTStdAdo;

/// <summary>插入操作返回结构</summary>
public sealed class DbInsResult
{
    /// <summary>操作是否成功</summary>
    public bool Success { get; init; }
    /// <summary>插入记录的自增ID（RETURNING id）</summary>
    public long Id { get; init; }
    /// <summary>操作失败时的错误信息</summary>
    public string? ErrorMessage { get; init; }
    /// <summary>调试信息（含参数替换后的SQL及耗时）</summary>
    public string? DebugMessage { get; init; }
}
