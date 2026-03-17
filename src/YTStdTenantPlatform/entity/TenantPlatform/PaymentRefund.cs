using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>支付退款</summary>
[Entity(TableName = "payment_refunds", NeedAuditTable = true)]
[DetailOf(typeof(PaymentOrder), ForeignKey = "PaymentOrderId")]
[Index("uq_payment_refunds_refund_no", "refund_no", Kind = IndexKind.Unique)]
public class PaymentRefund
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>退款编号</summary>
    [Column(Length = 64, IsRequired = true)]
    public string RefundNo { get; set; } = "";

    /// <summary>支付订单 ID</summary>
    public long PaymentOrderId { get; set; }

    /// <summary>退款状态</summary>
    [Column(Length = 32, IsRequired = true)]
    public string RefundStatus { get; set; } = "";

    /// <summary>退款金额</summary>
    [Column(Length = 18, IsRequired = true)]
    public decimal RefundAmount { get; set; }

    /// <summary>退款原因</summary>
    public string? RefundReason { get; set; }

    /// <summary>退款时间</summary>
    public DateTime? RefundedAt { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; }
}
