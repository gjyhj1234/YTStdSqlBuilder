import { http, HttpResponse } from 'msw'
import { mockTenants } from '../data/tenants'
import { ok, paged, getPageParams } from '../data/common'

export const tenantsHandlers = [
  http.get('/api/tenants', ({ request }) => {
    const url = new URL(request.url)
    const { page, pageSize } = getPageParams(url)
    return HttpResponse.json(paged(mockTenants, page, pageSize))
  }),

  http.post('/api/tenants', async ({ request }) => {
    const body = (await request.json()) as Record<string, unknown>
    const newTenant = {
      Id: mockTenants.length + 1,
      TenantCode: `T${Date.now()}`,
      TenantName: body['TenantName'] as string,
      EnterpriseName: body['EnterpriseName'] as string,
      ContactName: body['ContactName'] as string,
      ContactEmail: body['ContactEmail'] as string,
      LifecycleStatus: 'Active',
      IsolationMode: body['IsolationMode'] as string ?? 'Shared',
      Enabled: true,
      OpenedAt: new Date().toISOString(),
      ExpiresAt: '2026-12-31T23:59:59Z',
      CreatedAt: new Date().toISOString(),
    }
    return HttpResponse.json(ok(newTenant, 'operation.create_success'))
  }),

  http.put('/api/tenants/:id', async ({ request }) => {
    const body = (await request.json()) as Record<string, unknown>
    return HttpResponse.json(ok(body, 'operation.update_success'))
  }),

  http.put('/api/tenants/:id/status', () => {
    return HttpResponse.json(ok(null, 'operation.status_change_success'))
  }),
]
