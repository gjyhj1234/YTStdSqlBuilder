using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>账单发票列表项</summary>
    public sealed class BillingInvoiceRepDTO
    {
        /// <summary>发票 ID</summary>
        public long Id { get; set; }
        /// <summary>发票编号</summary>
        public string InvoiceNo { get; set; } = "";
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>订阅 ID</summary>
        public long? SubscriptionId { get; set; }
        /// <summary>发票状态</summary>
        public string InvoiceStatus { get; set; } = "";
        /// <summary>账期开始</summary>
        public DateTime BillingPeriodStart { get; set; }
        /// <summary>账期结束</summary>
        public DateTime BillingPeriodEnd { get; set; }
        /// <summary>小计金额</summary>
        public decimal SubtotalAmount { get; set; }
        /// <summary>附加金额</summary>
        public decimal ExtraAmount { get; set; }
        /// <summary>折扣金额</summary>
        public decimal DiscountAmount { get; set; }
        /// <summary>总金额</summary>
        public decimal TotalAmount { get; set; }
        /// <summary>货币编码</summary>
        public string CurrencyCode { get; set; } = "";
        /// <summary>开票时间</summary>
        public DateTime? IssuedAt { get; set; }
        /// <summary>到期时间</summary>
        public DateTime? DueAt { get; set; }
        /// <summary>支付时间</summary>
        public DateTime? PaidAt { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>账单发票明细列表项</summary>
    public sealed class BillingInvoiceItemRepDTO
    {
        /// <summary>明细 ID</summary>
        public long Id { get; set; }
        /// <summary>发票 ID</summary>
        public long InvoiceId { get; set; }
        /// <summary>项目类型</summary>
        public string ItemType { get; set; } = "";
        /// <summary>项目名称</summary>
        public string ItemName { get; set; } = "";
        /// <summary>数量</summary>
        public decimal Quantity { get; set; }
        /// <summary>单价</summary>
        public decimal UnitPrice { get; set; }
        /// <summary>金额</summary>
        public decimal Amount { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>支付订单列表项</summary>
    public sealed class PaymentOrderRepDTO
    {
        /// <summary>订单 ID</summary>
        public long Id { get; set; }
        /// <summary>订单号</summary>
        public string OrderNo { get; set; } = "";
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>发票 ID</summary>
        public long? InvoiceId { get; set; }
        /// <summary>支付渠道</summary>
        public string PaymentChannel { get; set; } = "";
        /// <summary>支付状态</summary>
        public string PaymentStatus { get; set; } = "";
        /// <summary>金额</summary>
        public decimal Amount { get; set; }
        /// <summary>货币编码</summary>
        public string CurrencyCode { get; set; } = "";
        /// <summary>第三方交易号</summary>
        public string? ThirdPartyTxnNo { get; set; }
        /// <summary>支付时间</summary>
        public DateTime? PaidAt { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>支付退款列表项</summary>
    public sealed class PaymentRefundRepDTO
    {
        /// <summary>退款 ID</summary>
        public long Id { get; set; }
        /// <summary>退款编号</summary>
        public string RefundNo { get; set; } = "";
        /// <summary>支付订单 ID</summary>
        public long PaymentOrderId { get; set; }
        /// <summary>退款状态</summary>
        public string RefundStatus { get; set; } = "";
        /// <summary>退款金额</summary>
        public decimal RefundAmount { get; set; }
        /// <summary>退款原因</summary>
        public string? RefundReason { get; set; }
        /// <summary>退款时间</summary>
        public DateTime? RefundedAt { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }
}
