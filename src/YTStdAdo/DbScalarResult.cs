namespace YTStdAdo;

/// <summary>标量查询返回结构</summary>
public sealed class DbScalarResult<T>
{
    /// <summary>操作是否成功</summary>
    public bool Success { get; init; }
    /// <summary>查询返回的标量值</summary>
    public T? Value { get; init; }
    /// <summary>操作失败时的错误信息</summary>
    public string? ErrorMessage { get; init; }
    /// <summary>调试信息（含参数替换后的SQL及耗时）</summary>
    public string? DebugMessage { get; init; }
}
