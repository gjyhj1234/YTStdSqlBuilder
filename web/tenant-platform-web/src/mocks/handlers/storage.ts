import { http, HttpResponse } from 'msw'
import {
  mockStorageStrategies,
  mockTenantFiles,
  mockFileAccessPolicies,
} from '../data/storage'
import { ok, fail, paged, getPageParams } from '../data/common'

export const storageHandlers = [
  http.get('/api/storage-strategies', ({ request }) => {
    const url = new URL(request.url)
    const { page, pageSize } = getPageParams(url)
    return HttpResponse.json(paged(mockStorageStrategies, page, pageSize))
  }),

  http.get('/api/storage-strategies/:id', ({ params }) => {
    const s = mockStorageStrategies.find((s) => s.Id === Number(params['id']))
    if (!s)
      return HttpResponse.json(
        fail('error.storage_strategy_not_found'),
        { status: 404 },
      )
    return HttpResponse.json(ok(s))
  }),

  http.post('/api/storage-strategies', async ({ request }) => {
    const body = (await request.json()) as Record<string, unknown>
    const newStrategy = {
      Id: mockStorageStrategies.length + 1,
      StrategyCode: body['StrategyCode'] as string,
      StrategyName: body['StrategyName'] as string,
      ProviderType: body['ProviderType'] as string,
      BucketName: body['BucketName'] as string,
      BasePath: body['BasePath'] as string,
      Status: 'Active',
      CreatedAt: new Date().toISOString(),
    }
    return HttpResponse.json(ok(newStrategy, 'operation.create_success'))
  }),

  http.put('/api/storage-strategies/:id', async ({ request }) => {
    const body = (await request.json()) as Record<string, unknown>
    return HttpResponse.json(ok(body, 'operation.update_success'))
  }),

  http.put('/api/storage-strategies/:id/enable', () => {
    return HttpResponse.json(ok(null, 'operation.enable_success'))
  }),

  http.put('/api/storage-strategies/:id/disable', () => {
    return HttpResponse.json(ok(null, 'operation.disable_success'))
  }),

  http.get('/api/tenant-files', ({ request }) => {
    const url = new URL(request.url)
    const { page, pageSize } = getPageParams(url)
    return HttpResponse.json(paged(mockTenantFiles, page, pageSize))
  }),

  http.get('/api/tenant-files/:id', ({ params }) => {
    const f = mockTenantFiles.find((f) => f.Id === Number(params['id']))
    if (!f)
      return HttpResponse.json(
        fail('error.file_not_found'),
        { status: 404 },
      )
    return HttpResponse.json(ok(f))
  }),

  http.delete('/api/tenant-files/:id', () => {
    return HttpResponse.json(ok(null, 'operation.delete_success'))
  }),

  http.get('/api/file-access-policies', ({ request }) => {
    const url = new URL(request.url)
    const { page, pageSize } = getPageParams(url)
    return HttpResponse.json(paged(mockFileAccessPolicies, page, pageSize))
  }),

  http.post('/api/file-access-policies', async ({ request }) => {
    const body = (await request.json()) as Record<string, unknown>
    const newPolicy = {
      Id: mockFileAccessPolicies.length + 1,
      FileId: body['FileId'] as number,
      SubjectType: body['SubjectType'] as string,
      SubjectId: body['SubjectId'] as number,
      PermissionCode: body['PermissionCode'] as string,
      CreatedAt: new Date().toISOString(),
    }
    return HttpResponse.json(ok(newPolicy, 'operation.create_success'))
  }),
]
