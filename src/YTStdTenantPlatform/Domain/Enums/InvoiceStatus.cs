namespace YTStdTenantPlatform.Domain.Enums;

/// <summary>发票状态</summary>
public enum InvoiceStatus
{
    /// <summary>待开具</summary>
    Pending,

    /// <summary>已开具</summary>
    Issued,

    /// <summary>已支付</summary>
    Paid,

    /// <summary>已逾期</summary>
    Overdue,

    /// <summary>已取消</summary>
    Cancelled
}
