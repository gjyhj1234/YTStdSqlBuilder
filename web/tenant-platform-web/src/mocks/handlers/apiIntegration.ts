import { http, HttpResponse } from 'msw'
import {
  mockTenantApiKeys,
  mockTenantWebhooks,
  mockWebhookDeliveryLogs,
} from '../data/apiIntegration'
import { ok, paged, getPageParams } from '../data/common'

export const apiIntegrationHandlers = [
  http.get('/api/tenant-api-keys', ({ request }) => {
    const url = new URL(request.url)
    const { page, pageSize } = getPageParams(url)
    return HttpResponse.json(paged(mockTenantApiKeys, page, pageSize))
  }),

  http.post('/api/tenant-api-keys', async ({ request }) => {
    const body = (await request.json()) as Record<string, unknown>
    const result = {
      Id: mockTenantApiKeys.length + 1,
      TenantRefId: body['TenantRefId'] as number,
      KeyName: body['KeyName'] as string,
      AccessKey: `ak_new_${Date.now()}`,
      SecretKey: `sk_new_${Date.now()}`,
      Status: 'Active',
      ExpiresAt: '2026-12-31T00:00:00Z',
      CreatedAt: new Date().toISOString(),
    }
    return HttpResponse.json(ok(result, 'operation.create_success'))
  }),

  http.put('/api/tenant-api-keys/:id/disable', () => {
    return HttpResponse.json(ok(null, 'operation.disable_success'))
  }),

  http.get('/api/tenant-api-usage-stats', ({ request }) => {
    const url = new URL(request.url)
    const { page, pageSize } = getPageParams(url)
    const stats = [
      {
        Id: 1,
        TenantRefId: 1,
        ApiKeyId: 1,
        StatDate: '2025-06-10',
        CallCount: 1580,
        ErrorCount: 12,
        CreatedAt: '2025-06-11T00:05:00Z',
      },
    ]
    return HttpResponse.json(paged(stats, page, pageSize))
  }),

  http.get('/api/webhook-events', () => {
    const events = [
      { Id: 1, EventCode: 'subscription.created', EventName: '订阅创建', CreatedAt: '2025-01-01T00:00:00Z' },
      { Id: 2, EventCode: 'invoice.paid', EventName: '账单已支付', CreatedAt: '2025-01-01T00:00:00Z' },
      { Id: 3, EventCode: 'payment.success', EventName: '支付成功', CreatedAt: '2025-01-01T00:00:00Z' },
    ]
    return HttpResponse.json(ok(events))
  }),

  http.get('/api/tenant-webhooks', ({ request }) => {
    const url = new URL(request.url)
    const { page, pageSize } = getPageParams(url)
    return HttpResponse.json(paged(mockTenantWebhooks, page, pageSize))
  }),

  http.post('/api/tenant-webhooks', async ({ request }) => {
    const body = (await request.json()) as Record<string, unknown>
    const newWebhook = {
      Id: mockTenantWebhooks.length + 1,
      TenantRefId: body['TenantRefId'] as number,
      WebhookUrl: body['WebhookUrl'] as string,
      EventTypes: body['EventTypes'] as string,
      Secret: `whsec_${Date.now()}`,
      Status: 'Active',
      CreatedAt: new Date().toISOString(),
    }
    return HttpResponse.json(ok(newWebhook, 'operation.create_success'))
  }),

  http.put('/api/tenant-webhooks/:id', async ({ request }) => {
    const body = (await request.json()) as Record<string, unknown>
    return HttpResponse.json(ok(body, 'operation.update_success'))
  }),

  http.put('/api/tenant-webhooks/:id/enable', () => {
    return HttpResponse.json(ok(null, 'operation.enable_success'))
  }),

  http.put('/api/tenant-webhooks/:id/disable', () => {
    return HttpResponse.json(ok(null, 'operation.disable_success'))
  }),

  http.get('/api/webhook-delivery-logs', ({ request }) => {
    const url = new URL(request.url)
    const { page, pageSize } = getPageParams(url)
    return HttpResponse.json(paged(mockWebhookDeliveryLogs, page, pageSize))
  }),
]
