using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>创建账单发票请求</summary>
    public sealed class CreateBillingInvoiceReqDTO
    {
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>订阅 ID</summary>
        public long? SubscriptionId { get; set; }
        /// <summary>账期开始</summary>
        public DateTime BillingPeriodStart { get; set; }
        /// <summary>账期结束</summary>
        public DateTime BillingPeriodEnd { get; set; }
        /// <summary>货币编码</summary>
        public string CurrencyCode { get; set; } = "CNY";
    }

    /// <summary>创建支付订单请求</summary>
    public sealed class CreatePaymentOrderReqDTO
    {
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>发票 ID</summary>
        public long? InvoiceId { get; set; }
        /// <summary>支付渠道（manual/alipay/wechat/bank）</summary>
        public string PaymentChannel { get; set; } = "manual";
        /// <summary>金额</summary>
        public decimal Amount { get; set; }
        /// <summary>货币编码</summary>
        public string CurrencyCode { get; set; } = "CNY";
    }

    /// <summary>创建退款请求</summary>
    public sealed class CreateRefundReqDTO
    {
        /// <summary>支付订单 ID</summary>
        public long PaymentOrderId { get; set; }
        /// <summary>退款金额</summary>
        public decimal RefundAmount { get; set; }
        /// <summary>退款原因</summary>
        public string? RefundReason { get; set; }
    }
}
