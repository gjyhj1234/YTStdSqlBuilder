namespace YTStdTenantPlatform.Domain.Enums;

/// <summary>支付状态</summary>
public enum PaymentStatus
{
    /// <summary>待支付</summary>
    Pending,

    /// <summary>已支付</summary>
    Paid,

    /// <summary>支付失败</summary>
    Failed,

    /// <summary>已取消</summary>
    Cancelled,

    /// <summary>已退款</summary>
    Refunded,

    /// <summary>部分退款</summary>
    PartialRefunded
}
