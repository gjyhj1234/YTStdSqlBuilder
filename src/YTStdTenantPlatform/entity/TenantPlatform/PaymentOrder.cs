using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>支付订单</summary>
[Entity(TableName = "payment_orders", NeedAuditTable = true)]
[Index("uq_payment_orders_order_no", "order_no", Kind = IndexKind.Unique)]
[Index("idx_payment_orders_tenant_status", "tenant_ref_id", "payment_status")]
public class PaymentOrder
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>订单号</summary>
    [Column(Length = 64, IsRequired = true)]
    public string OrderNo { get; set; } = "";

    /// <summary>关联租户 ID</summary>
    public long TenantRefId { get; set; }

    /// <summary>发票 ID</summary>
    public long? InvoiceId { get; set; }

    /// <summary>支付渠道</summary>
    [Column(Length = 32, IsRequired = true)]
    public string PaymentChannel { get; set; } = "";

    /// <summary>支付状态</summary>
    [Column(Length = 32, IsRequired = true)]
    public string PaymentStatus { get; set; } = "";

    /// <summary>金额</summary>
    [Column(Length = 18, IsRequired = true)]
    public decimal Amount { get; set; }

    /// <summary>货币编码</summary>
    [Column(Length = 16, IsRequired = true)]
    public string CurrencyCode { get; set; } = "";

    /// <summary>第三方交易号</summary>
    [Column(Length = 128)]
    public string? ThirdPartyTxnNo { get; set; }

    /// <summary>支付时间</summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; }
}
