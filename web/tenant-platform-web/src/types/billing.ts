/** 发票响应 */
export interface BillingInvoiceRepDTO {
  Id: number
  InvoiceNo: string
  TenantRefId: number
  SubscriptionId: number | null
  InvoiceStatus: string
  BillingPeriodStart: string
  BillingPeriodEnd: string
  SubtotalAmount: number
  ExtraAmount: number
  DiscountAmount: number
  TotalAmount: number
  CurrencyCode: string
  IssuedAt: string | null
  DueAt: string | null
  PaidAt: string | null
  CreatedAt: string
}

/** 创建发票请求 */
export interface CreateBillingInvoiceReqDTO {
  TenantRefId: number
  SubscriptionId?: number | null
  BillingPeriodStart: string
  BillingPeriodEnd: string
  CurrencyCode?: string
}

/** 发票明细响应 */
export interface BillingInvoiceItemRepDTO {
  Id: number
  InvoiceId: number
  ItemType: string
  ItemName: string
  Quantity: number
  UnitPrice: number
  Amount: number
  CreatedAt: string
}

/** 支付订单响应 */
export interface PaymentOrderRepDTO {
  Id: number
  OrderNo: string
  TenantRefId: number
  InvoiceId: number | null
  PaymentChannel: string
  PaymentStatus: string
  Amount: number
  CurrencyCode: string
  ThirdPartyTxnNo: string | null
  PaidAt: string | null
  CreatedAt: string
}

/** 创建支付订单请求 */
export interface CreatePaymentOrderReqDTO {
  TenantRefId: number
  InvoiceId?: number | null
  PaymentChannel?: string
  Amount: number
  CurrencyCode?: string
}

/** 退款响应 */
export interface PaymentRefundRepDTO {
  Id: number
  RefundNo: string
  PaymentOrderId: number
  RefundStatus: string
  RefundAmount: number
  RefundReason: string | null
  RefundedAt: string | null
  CreatedAt: string
}

/** 创建退款请求 */
export interface CreateRefundReqDTO {
  PaymentOrderId: number
  RefundAmount: number
  RefundReason?: string
}
