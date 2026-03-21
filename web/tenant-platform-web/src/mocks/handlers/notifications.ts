import { http, HttpResponse } from 'msw'
import {
  mockNotificationTemplates,
  mockNotifications,
} from '../data/notifications'
import { ok, fail, paged, getPageParams } from '../data/common'

export const notificationsHandlers = [
  http.get('/api/notification-templates', ({ request }) => {
    const url = new URL(request.url)
    const { page, pageSize } = getPageParams(url)
    return HttpResponse.json(paged(mockNotificationTemplates, page, pageSize))
  }),

  http.get('/api/notification-templates/:id', ({ params }) => {
    const tpl = mockNotificationTemplates.find((t) => t.Id === Number(params['id']))
    if (!tpl)
      return HttpResponse.json(
        fail('error.template_not_found'),
        { status: 404 },
      )
    return HttpResponse.json(ok(tpl))
  }),

  http.post('/api/notification-templates', async ({ request }) => {
    const body = (await request.json()) as Record<string, unknown>
    const newTpl = {
      Id: mockNotificationTemplates.length + 1,
      TemplateCode: body['TemplateCode'] as string,
      TemplateName: body['TemplateName'] as string,
      Channel: body['Channel'] as string,
      SubjectTemplate: body['SubjectTemplate'] as string,
      BodyTemplate: body['BodyTemplate'] as string,
      Status: 'Active',
      CreatedAt: new Date().toISOString(),
    }
    return HttpResponse.json(ok(newTpl, 'operation.create_success'))
  }),

  http.put('/api/notification-templates/:id', async ({ request }) => {
    const body = (await request.json()) as Record<string, unknown>
    return HttpResponse.json(ok(body, 'operation.update_success'))
  }),

  http.put('/api/notification-templates/:id/enable', () => {
    return HttpResponse.json(ok(null, 'operation.enable_success'))
  }),

  http.put('/api/notification-templates/:id/disable', () => {
    return HttpResponse.json(ok(null, 'operation.disable_success'))
  }),

  http.get('/api/notifications', ({ request }) => {
    const url = new URL(request.url)
    const { page, pageSize } = getPageParams(url)
    return HttpResponse.json(paged(mockNotifications, page, pageSize))
  }),

  http.get('/api/notifications/:id', ({ params }) => {
    const n = mockNotifications.find((n) => n.Id === Number(params['id']))
    if (!n)
      return HttpResponse.json(
        fail('error.notification_not_found'),
        { status: 404 },
      )
    return HttpResponse.json(ok(n))
  }),

  http.post('/api/notifications', async ({ request }) => {
    const body = (await request.json()) as Record<string, unknown>
    return HttpResponse.json(ok(body, 'operation.create_success'))
  }),

  http.put('/api/notifications/:id/read', () => {
    return HttpResponse.json(ok(null, 'operation.mark_read_success'))
  }),
]
