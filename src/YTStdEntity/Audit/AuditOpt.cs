namespace YTStdEntity.Audit;

/// <summary>审计操作类型</summary>
public enum AuditOpt : byte
{
    /// <summary>插入</summary>
    Insert = (byte)'I',
    /// <summary>更新</summary>
    Update = (byte)'U',
    /// <summary>删除</summary>
    Delete = (byte)'D',
}
