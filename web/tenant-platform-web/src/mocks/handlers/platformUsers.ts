import { http, HttpResponse } from 'msw'
import { mockPlatformUsers } from '../data/platformUsers'
import { ok, paged, getPageParams } from '../data/common'

export const platformUsersHandlers = [
  http.get('/api/platform-users', ({ request }) => {
    const url = new URL(request.url)
    const { page, pageSize } = getPageParams(url)
    return HttpResponse.json(paged(mockPlatformUsers, page, pageSize))
  }),

  http.post('/api/platform-users', async ({ request }) => {
    const body = (await request.json()) as Record<string, unknown>
    const newUser = {
      Id: mockPlatformUsers.length + 1,
      Username: body['Username'] as string,
      DisplayName: body['DisplayName'] as string,
      Email: body['Email'] as string,
      Phone: body['Phone'] as string,
      Status: 'Active',
      IsSuperAdmin: false,
      LastLoginAt: '',
      CreatedAt: new Date().toISOString(),
    }
    return HttpResponse.json(ok(newUser, 'operation.create_success'))
  }),

  http.put('/api/platform-users/:id', async ({ request }) => {
    const body = (await request.json()) as Record<string, unknown>
    return HttpResponse.json(ok(body, 'operation.update_success'))
  }),

  http.put('/api/platform-users/:id/enable', () => {
    return HttpResponse.json(ok(null, 'operation.enable_success'))
  }),

  http.put('/api/platform-users/:id/disable', () => {
    return HttpResponse.json(ok(null, 'operation.disable_success'))
  }),
]
