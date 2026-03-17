namespace YTStdTenantPlatform.Domain.Enums;

/// <summary>退款状态</summary>
public enum RefundStatus
{
    /// <summary>待处理</summary>
    Pending,

    /// <summary>退款成功</summary>
    Success,

    /// <summary>退款失败</summary>
    Failed,

    /// <summary>已取消</summary>
    Cancelled
}
