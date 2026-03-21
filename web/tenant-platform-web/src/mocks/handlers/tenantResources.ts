import { http, HttpResponse } from 'msw'
import { mockTenantResourceQuotas } from '../data/tenantResources'
import { ok, fail, paged, getPageParams } from '../data/common'

export const tenantResourcesHandlers = [
  http.get('/api/tenant-resource-quotas', ({ request }) => {
    const url = new URL(request.url)
    const { page, pageSize } = getPageParams(url)
    return HttpResponse.json(paged(mockTenantResourceQuotas, page, pageSize))
  }),

  http.get('/api/tenant-resource-quotas/:id', ({ params }) => {
    const q = mockTenantResourceQuotas.find((r) => r.Id === Number(params['id']))
    if (!q) return HttpResponse.json(fail('error.resource_quota_not_found'), { status: 404 })
    return HttpResponse.json(ok(q))
  }),

  http.post('/api/tenant-resource-quotas', async ({ request }) => {
    const body = (await request.json()) as Record<string, unknown>
    const newQuota = {
      Id: mockTenantResourceQuotas.length + 1,
      TenantRefId: body['TenantRefId'] as number,
      QuotaType: body['QuotaType'] as string,
      QuotaLimit: body['QuotaLimit'] as number,
      WarningThreshold: body['WarningThreshold'] as number,
      ResetCycle: body['ResetCycle'] as string,
      CreatedAt: new Date().toISOString(),
    }
    return HttpResponse.json(ok(newQuota, 'operation.create_success'))
  }),
]
