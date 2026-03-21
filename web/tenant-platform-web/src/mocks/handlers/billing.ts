import { http, HttpResponse } from 'msw'
import {
  mockBillingInvoices,
  mockPaymentOrders,
  mockPaymentRefunds,
} from '../data/billing'
import { ok, fail, paged, getPageParams } from '../data/common'

export const billingHandlers = [
  http.get('/api/billing-invoices', ({ request }) => {
    const url = new URL(request.url)
    const { page, pageSize } = getPageParams(url)
    return HttpResponse.json(paged(mockBillingInvoices, page, pageSize))
  }),

  http.get('/api/billing-invoices/:id', ({ params }) => {
    const inv = mockBillingInvoices.find((i) => i.Id === Number(params['id']))
    if (!inv)
      return HttpResponse.json(
        fail('error.invoice_not_found'),
        { status: 404 },
      )
    return HttpResponse.json(ok(inv))
  }),

  http.get('/api/billing-invoices/:id/items', () => {
    return HttpResponse.json(ok([]))
  }),

  http.post('/api/billing-invoices', async ({ request }) => {
    const body = (await request.json()) as Record<string, unknown>
    return HttpResponse.json(ok(body, 'operation.create_success'))
  }),

  http.put('/api/billing-invoices/:id/void', () => {
    return HttpResponse.json(ok(null, 'operation.void_success'))
  }),

  http.get('/api/payment-orders', ({ request }) => {
    const url = new URL(request.url)
    const { page, pageSize } = getPageParams(url)
    return HttpResponse.json(paged(mockPaymentOrders, page, pageSize))
  }),

  http.post('/api/payment-orders', async ({ request }) => {
    const body = (await request.json()) as Record<string, unknown>
    return HttpResponse.json(ok(body, 'operation.create_success'))
  }),

  http.get('/api/payment-refunds', ({ request }) => {
    const url = new URL(request.url)
    const { page, pageSize } = getPageParams(url)
    return HttpResponse.json(paged(mockPaymentRefunds, page, pageSize))
  }),

  http.post('/api/payment-refunds', async ({ request }) => {
    const body = (await request.json()) as Record<string, unknown>
    return HttpResponse.json(ok(body, 'operation.create_success'))
  }),
]
