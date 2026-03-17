namespace YTStdTenantPlatform.Domain.Enums;

/// <summary>Webhook投递状态</summary>
public enum WebhookDeliveryStatus
{
    /// <summary>待投递</summary>
    Pending,

    /// <summary>投递成功</summary>
    Success,

    /// <summary>投递失败</summary>
    Failed
}
