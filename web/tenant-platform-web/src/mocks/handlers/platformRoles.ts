import { http, HttpResponse } from 'msw'
import { mockPlatformRoles } from '../data/platformRoles'
import { ok, fail, paged, getPageParams } from '../data/common'

export const platformRolesHandlers = [
  http.get('/api/platform-roles', ({ request }) => {
    const url = new URL(request.url)
    const { page, pageSize } = getPageParams(url)
    return HttpResponse.json(paged(mockPlatformRoles, page, pageSize))
  }),

  http.get('/api/platform-roles/:id', ({ params }) => {
    const role = mockPlatformRoles.find((r) => r.Id === Number(params['id']))
    if (!role) return HttpResponse.json(fail('error.role_not_found'), { status: 404 })
    return HttpResponse.json(ok(role))
  }),

  http.post('/api/platform-roles', async ({ request }) => {
    const body = (await request.json()) as Record<string, unknown>
    const newRole = {
      Id: mockPlatformRoles.length + 1,
      Code: body['Code'] as string,
      Name: body['Name'] as string,
      Description: body['Description'] as string,
      Status: 'Active',
      CreatedAt: new Date().toISOString(),
    }
    return HttpResponse.json(ok(newRole, 'operation.create_success'))
  }),

  http.put('/api/platform-roles/:id', async ({ request }) => {
    const body = (await request.json()) as Record<string, unknown>
    return HttpResponse.json(ok(body, 'operation.update_success'))
  }),

  http.put('/api/platform-roles/:id/enable', () => {
    return HttpResponse.json(ok(null, 'operation.enable_success'))
  }),

  http.put('/api/platform-roles/:id/disable', () => {
    return HttpResponse.json(ok(null, 'operation.disable_success'))
  }),

  http.post('/api/platform-roles/:id/permissions', () => {
    return HttpResponse.json(ok(null, 'operation.bind_permissions_success'))
  }),

  http.post('/api/platform-roles/:id/members', () => {
    return HttpResponse.json(ok(null, 'operation.bind_members_success'))
  }),
]
