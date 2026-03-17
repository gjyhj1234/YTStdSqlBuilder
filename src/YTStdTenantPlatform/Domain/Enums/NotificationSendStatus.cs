namespace YTStdTenantPlatform.Domain.Enums;

/// <summary>通知发送状态</summary>
public enum NotificationSendStatus
{
    /// <summary>待发送</summary>
    Pending,

    /// <summary>已发送</summary>
    Sent,

    /// <summary>发送失败</summary>
    Failed,

    /// <summary>已读</summary>
    Read
}
