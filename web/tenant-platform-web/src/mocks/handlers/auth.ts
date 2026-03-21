import { http, HttpResponse } from 'msw'
import { mockLoginResult, mockCurrentUser } from '../data/auth'
import { ok, fail } from '../data/common'

export const authHandlers = [
  http.post('/api/auth/login', async ({ request }) => {
    const body = (await request.json()) as { Username: string; Password: string }
    if (body.Username === 'admin' && body.Password === 'admin123') {
      return HttpResponse.json(ok(mockLoginResult, 'auth.login_success'))
    }
    return HttpResponse.json(fail('auth.invalid_credentials', 2002), { status: 401 })
  }),

  http.post('/api/auth/refresh', () => {
    return HttpResponse.json(ok(mockLoginResult, 'auth.refresh_success'))
  }),

  http.get('/api/auth/me', () => {
    return HttpResponse.json(ok(mockCurrentUser))
  }),
]
