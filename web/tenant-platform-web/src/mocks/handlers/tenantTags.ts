import { http, HttpResponse } from 'msw'
import { mockTenantTags } from '../data/tenantTags'
import { ok, paged, getPageParams } from '../data/common'

export const tenantTagsHandlers = [
  http.get('/api/tenant-tags', ({ request }) => {
    const url = new URL(request.url)
    const { page, pageSize } = getPageParams(url)
    return HttpResponse.json(paged(mockTenantTags, page, pageSize))
  }),

  http.post('/api/tenant-tags', async ({ request }) => {
    const body = (await request.json()) as Record<string, unknown>
    const newTag = {
      Id: mockTenantTags.length + 1,
      TagCode: body['TagCode'] as string,
      TagName: body['TagName'] as string,
      TagCategory: body['TagCategory'] as string,
      CreatedAt: new Date().toISOString(),
    }
    return HttpResponse.json(ok(newTag, 'operation.create_success'))
  }),

  http.post('/api/tenant-tags/bind', () => {
    return HttpResponse.json(ok(null, 'operation.bind_success'))
  }),
]
