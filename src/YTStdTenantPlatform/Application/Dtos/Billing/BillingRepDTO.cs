using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>账单发票响应数据</summary>
    public sealed class BillingInvoiceRepDTO
    {
        /// <summary>发票ID</summary>
        public long Id { get; set; }
        /// <summary>租户ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>发票编号</summary>
        public string InvoiceNo { get; set; } = "";
        /// <summary>发票类型</summary>
        public string InvoiceType { get; set; } = "";
        /// <summary>发票状态</summary>
        public string InvoiceStatus { get; set; } = "";
        /// <summary>总金额</summary>
        public decimal TotalAmount { get; set; }
        /// <summary>货币编码</summary>
        public string CurrencyCode { get; set; } = "";
        /// <summary>到期日</summary>
        public DateTime? DueDate { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>账单发票明细响应数据</summary>
    public sealed class BillingInvoiceItemRepDTO
    {
        /// <summary>明细ID</summary>
        public long Id { get; set; }
        /// <summary>发票ID</summary>
        public long InvoiceId { get; set; }
        /// <summary>项目名称</summary>
        public string ItemName { get; set; } = "";
        /// <summary>数量</summary>
        public int Quantity { get; set; }
        /// <summary>单价</summary>
        public decimal UnitPrice { get; set; }
        /// <summary>金额</summary>
        public decimal Amount { get; set; }
    }

    /// <summary>支付单响应数据</summary>
    public sealed class PaymentOrderRepDTO
    {
        /// <summary>支付单ID</summary>
        public long Id { get; set; }
        /// <summary>发票ID</summary>
        public long InvoiceId { get; set; }
        /// <summary>支付单号</summary>
        public string OrderNo { get; set; } = "";
        /// <summary>支付金额</summary>
        public decimal PaymentAmount { get; set; }
        /// <summary>支付方式</summary>
        public string PaymentMethod { get; set; } = "";
        /// <summary>支付状态</summary>
        public string PaymentStatus { get; set; } = "";
        /// <summary>支付时间</summary>
        public DateTime? PaidAt { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>退款响应数据</summary>
    public sealed class PaymentRefundRepDTO
    {
        /// <summary>退款ID</summary>
        public long Id { get; set; }
        /// <summary>支付单ID</summary>
        public long PaymentOrderId { get; set; }
        /// <summary>退款单号</summary>
        public string RefundNo { get; set; } = "";
        /// <summary>退款金额</summary>
        public decimal RefundAmount { get; set; }
        /// <summary>退款状态</summary>
        public string RefundStatus { get; set; } = "";
        /// <summary>退款原因</summary>
        public string? RefundReason { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }
}
