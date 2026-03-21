import { http, HttpResponse } from 'msw'
import { mockTenantGroups, mockFlatTenantGroups } from '../data/tenantGroups'
import { ok, paged, getPageParams } from '../data/common'

export const tenantGroupsHandlers = [
  http.get('/api/tenant-groups/tree', () => {
    return HttpResponse.json(ok(mockTenantGroups))
  }),

  http.get('/api/tenant-groups', ({ request }) => {
    const url = new URL(request.url)
    const { page, pageSize } = getPageParams(url)
    return HttpResponse.json(paged(mockFlatTenantGroups, page, pageSize))
  }),

  http.post('/api/tenant-groups', async ({ request }) => {
    const body = (await request.json()) as Record<string, unknown>
    const newGroup = {
      Id: mockFlatTenantGroups.length + 1,
      GroupCode: body['GroupCode'] as string,
      GroupName: body['GroupName'] as string,
      ParentId: (body['ParentId'] as number | null) ?? null,
      SortOrder: (body['SortOrder'] as number) ?? 0,
      Description: (body['Description'] as string) ?? '',
      CreatedAt: new Date().toISOString(),
      Children: [],
    }
    return HttpResponse.json(ok(newGroup, 'operation.create_success'))
  }),
]
