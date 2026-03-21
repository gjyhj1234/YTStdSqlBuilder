import { http, HttpResponse } from 'msw'
import {
  mockOperationLogs,
  mockAuditLogs,
  mockSystemLogs,
} from '../data/logs'
import { ok, fail, paged, getPageParams } from '../data/common'

export const logsHandlers = [
  http.get('/api/operation-logs', ({ request }) => {
    const url = new URL(request.url)
    const { page, pageSize } = getPageParams(url)
    return HttpResponse.json(paged(mockOperationLogs, page, pageSize))
  }),

  http.get('/api/operation-logs/:id', ({ params }) => {
    const log = mockOperationLogs.find((l) => l.Id === Number(params['id']))
    if (!log)
      return HttpResponse.json(
        fail('error.log_not_found'),
        { status: 404 },
      )
    return HttpResponse.json(ok(log))
  }),

  http.get('/api/audit-logs', ({ request }) => {
    const url = new URL(request.url)
    const { page, pageSize } = getPageParams(url)
    return HttpResponse.json(paged(mockAuditLogs, page, pageSize))
  }),

  http.get('/api/audit-logs/:id', ({ params }) => {
    const log = mockAuditLogs.find((l) => l.Id === Number(params['id']))
    if (!log)
      return HttpResponse.json(
        fail('error.log_not_found'),
        { status: 404 },
      )
    return HttpResponse.json(ok(log))
  }),

  http.get('/api/system-logs', ({ request }) => {
    const url = new URL(request.url)
    const { page, pageSize } = getPageParams(url)
    return HttpResponse.json(paged(mockSystemLogs, page, pageSize))
  }),

  http.get('/api/system-logs/:id', ({ params }) => {
    const log = mockSystemLogs.find((l) => l.Id === Number(params['id']))
    if (!log)
      return HttpResponse.json(
        fail('error.log_not_found'),
        { status: 404 },
      )
    return HttpResponse.json(ok(log))
  }),
]
