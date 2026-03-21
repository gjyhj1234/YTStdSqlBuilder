import { http, HttpResponse } from 'msw'
import {
  mockTenantSystemConfigs,
  mockTenantFeatureFlags,
  mockTenantParameters,
} from '../data/tenantConfig'
import { ok, fail, paged, getPageParams } from '../data/common'

export const tenantConfigHandlers = [
  http.get('/api/tenant-system-configs/:tenantRefId', ({ params }) => {
    const config = mockTenantSystemConfigs.find(
      (c) => c.TenantRefId === Number(params['tenantRefId']),
    )
    if (!config)
      return HttpResponse.json(
        fail('error.config_not_found'),
        { status: 404 },
      )
    return HttpResponse.json(ok(config))
  }),

  http.put('/api/tenant-system-configs/:tenantRefId', async ({ request }) => {
    const body = (await request.json()) as Record<string, unknown>
    return HttpResponse.json(ok(body, 'operation.update_success'))
  }),

  http.get('/api/tenant-feature-flags', ({ request }) => {
    const url = new URL(request.url)
    const { page, pageSize } = getPageParams(url)
    return HttpResponse.json(paged(mockTenantFeatureFlags, page, pageSize))
  }),

  http.post('/api/tenant-feature-flags', async ({ request }) => {
    const body = (await request.json()) as Record<string, unknown>
    const flag = {
      Id: mockTenantFeatureFlags.length + 1,
      TenantRefId: body['TenantRefId'] as number,
      FeatureKey: body['FeatureKey'] as string,
      FeatureName: body['FeatureName'] as string,
      Enabled: (body['Enabled'] as boolean) ?? false,
      Description: (body['Description'] as string) ?? '',
      CreatedAt: new Date().toISOString(),
    }
    return HttpResponse.json(ok(flag, 'operation.create_success'))
  }),

  http.put('/api/tenant-feature-flags/:id/toggle', () => {
    return HttpResponse.json(ok(null, 'operation.toggle_success'))
  }),

  http.get('/api/tenant-parameters', ({ request }) => {
    const url = new URL(request.url)
    const { page, pageSize } = getPageParams(url)
    return HttpResponse.json(paged(mockTenantParameters, page, pageSize))
  }),

  http.post('/api/tenant-parameters', async ({ request }) => {
    const body = (await request.json()) as Record<string, unknown>
    const param = {
      Id: mockTenantParameters.length + 1,
      TenantRefId: body['TenantRefId'] as number,
      ParamKey: body['ParamKey'] as string,
      ParamValue: body['ParamValue'] as string,
      ParamType: body['ParamType'] as string,
      Description: (body['Description'] as string) ?? '',
      CreatedAt: new Date().toISOString(),
    }
    return HttpResponse.json(ok(param, 'operation.create_success'))
  }),

  http.delete('/api/tenant-parameters/:id', () => {
    return HttpResponse.json(ok(null, 'operation.delete_success'))
  }),
]
