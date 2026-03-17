namespace YTStdTenantPlatform.Domain.Enums;

/// <summary>生命周期事件类型</summary>
public enum LifecycleEventType
{
    /// <summary>注册</summary>
    Register,

    /// <summary>开通</summary>
    Open,

    /// <summary>启用</summary>
    Enable,

    /// <summary>暂停</summary>
    Suspend,

    /// <summary>恢复</summary>
    Resume,

    /// <summary>关闭</summary>
    Close,

    /// <summary>删除</summary>
    Delete,

    /// <summary>过期</summary>
    Expire
}
