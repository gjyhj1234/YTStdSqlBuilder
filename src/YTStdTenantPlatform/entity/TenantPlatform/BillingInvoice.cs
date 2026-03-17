using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>账单发票</summary>
[Entity(TableName = "billing_invoices", NeedAuditTable = true)]
[Index("uq_billing_invoices_invoice_no", "invoice_no", Kind = IndexKind.Unique)]
[Index("idx_invoices_tenant_status", "tenant_ref_id", "invoice_status")]
public class BillingInvoice
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>发票编号</summary>
    [Column(Length = 64, IsRequired = true)]
    public string InvoiceNo { get; set; } = "";

    /// <summary>关联租户 ID</summary>
    public long TenantRefId { get; set; }

    /// <summary>订阅 ID</summary>
    public long? SubscriptionId { get; set; }

    /// <summary>发票状态</summary>
    [Column(Length = 32, IsRequired = true)]
    public string InvoiceStatus { get; set; } = "";

    /// <summary>账期开始</summary>
    public DateTime BillingPeriodStart { get; set; }

    /// <summary>账期结束</summary>
    public DateTime BillingPeriodEnd { get; set; }

    /// <summary>小计金额</summary>
    [Column(Length = 18, IsRequired = true)]
    public decimal SubtotalAmount { get; set; }

    /// <summary>附加金额</summary>
    [Column(Length = 18, IsRequired = true)]
    public decimal ExtraAmount { get; set; }

    /// <summary>折扣金额</summary>
    [Column(Length = 18, IsRequired = true)]
    public decimal DiscountAmount { get; set; }

    /// <summary>总金额</summary>
    [Column(Length = 18, IsRequired = true)]
    public decimal TotalAmount { get; set; }

    /// <summary>货币编码</summary>
    [Column(Length = 16, IsRequired = true)]
    public string CurrencyCode { get; set; } = "";

    /// <summary>开票时间</summary>
    public DateTime? IssuedAt { get; set; }

    /// <summary>到期时间</summary>
    public DateTime? DueAt { get; set; }

    /// <summary>支付时间</summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; }
}
