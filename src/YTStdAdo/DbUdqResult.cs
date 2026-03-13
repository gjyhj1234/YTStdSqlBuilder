namespace YTStdAdo;

/// <summary>更新/删除/查询 统一返回结构</summary>
public sealed class DbUdqResult
{
    /// <summary>操作是否成功</summary>
    public bool Success { get; init; }
    /// <summary>受影响的行数</summary>
    public int RowsAffected { get; init; }
    /// <summary>操作失败时的错误信息</summary>
    public string? ErrorMessage { get; init; }
    /// <summary>调试信息（含参数替换后的SQL及耗时）</summary>
    public string? DebugMessage { get; init; }
}
