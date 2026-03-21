/** API — 账单与支付 */
import { get, post, put } from '@/utils/http'
import type { PagedResult } from '@/types/base'
import type { BillingInvoiceRepDTO, CreateBillingInvoiceReqDTO, BillingInvoiceItemRepDTO, PaymentOrderRepDTO, CreatePaymentOrderReqDTO, PaymentRefundRepDTO, CreateRefundReqDTO } from '@/types/billing'

export type { BillingInvoiceRepDTO, BillingInvoiceItemRepDTO, PaymentOrderRepDTO, PaymentRefundRepDTO, CreateBillingInvoiceReqDTO, CreatePaymentOrderReqDTO, CreateRefundReqDTO }

/* ---------- 账单 ---------- */

export function getInvoices(params: Record<string, string | number | undefined>) {
  return get<PagedResult<BillingInvoiceRepDTO>>('/api/billing-invoices', params)
}

export function getInvoice(id: number) {
  return get<BillingInvoiceRepDTO>(`/api/billing-invoices/${id}`)
}

export function createInvoice(data: CreateBillingInvoiceReqDTO) {
  return post<void>('/api/billing-invoices', data)
}

export function voidInvoice(id: number) {
  return put<void>(`/api/billing-invoices/${id}/void`)
}

export function getInvoiceItems(invoiceId: number, params: Record<string, string | number | undefined>) {
  return get<PagedResult<BillingInvoiceItemRepDTO>>(`/api/billing-invoices/${invoiceId}/items`, params)
}

/* ---------- 支付订单 ---------- */

export function getPaymentOrders(params: Record<string, string | number | undefined>) {
  return get<PagedResult<PaymentOrderRepDTO>>('/api/payment-orders', params)
}

export function createPaymentOrder(data: CreatePaymentOrderReqDTO) {
  return post<void>('/api/payment-orders', data)
}

/* ---------- 退款 ---------- */

export function getPaymentRefunds(params: Record<string, string | number | undefined>) {
  return get<PagedResult<PaymentRefundRepDTO>>('/api/payment-refunds', params)
}

export function createRefund(data: CreateRefundReqDTO) {
  return post<void>('/api/payment-refunds', data)
}
