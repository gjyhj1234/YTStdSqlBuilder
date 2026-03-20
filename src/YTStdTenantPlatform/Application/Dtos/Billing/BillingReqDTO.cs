using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>创建账单发票请求参数</summary>
    public sealed class CreateBillingInvoiceReqDTO
    {
        /// <summary>租户ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>订阅ID</summary>
        public long? SubscriptionId { get; set; }
        /// <summary>发票类型</summary>
        public string InvoiceType { get; set; } = "subscription";
        /// <summary>货币编码</summary>
        public string CurrencyCode { get; set; } = "CNY";
    }

    /// <summary>创建支付单请求参数</summary>
    public sealed class CreatePaymentOrderReqDTO
    {
        /// <summary>发票ID</summary>
        public long InvoiceId { get; set; }
        /// <summary>支付方式</summary>
        public string PaymentMethod { get; set; } = "";
    }

    /// <summary>创建退款请求参数</summary>
    public sealed class CreateRefundReqDTO
    {
        /// <summary>支付单ID</summary>
        public long PaymentOrderId { get; set; }
        /// <summary>退款金额</summary>
        public decimal RefundAmount { get; set; }
        /// <summary>退款原因</summary>
        public string? RefundReason { get; set; }
    }
}
