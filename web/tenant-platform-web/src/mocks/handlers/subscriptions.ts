import { http, HttpResponse } from 'msw'
import {
  mockTenantSubscriptions,
  mockTenantTrials,
  mockTenantSubscriptionChanges,
} from '../data/subscriptions'
import { ok, fail, paged, getPageParams } from '../data/common'

export const subscriptionsHandlers = [
  http.get('/api/tenant-subscriptions', ({ request }) => {
    const url = new URL(request.url)
    const { page, pageSize } = getPageParams(url)
    return HttpResponse.json(paged(mockTenantSubscriptions, page, pageSize))
  }),

  http.get('/api/tenant-subscriptions/:id', ({ params }) => {
    const sub = mockTenantSubscriptions.find((s) => s.Id === Number(params['id']))
    if (!sub)
      return HttpResponse.json(
        fail('error.subscription_not_found'),
        { status: 404 },
      )
    return HttpResponse.json(ok(sub))
  }),

  http.post('/api/tenant-subscriptions', async ({ request }) => {
    const body = (await request.json()) as Record<string, unknown>
    const newSub = {
      Id: mockTenantSubscriptions.length + 1,
      TenantRefId: body['TenantRefId'] as number,
      PackageVersionId: body['PackageVersionId'] as number,
      SubscriptionStatus: 'Active',
      SubscriptionType: 'Paid',
      StartedAt: new Date().toISOString(),
      ExpiresAt: '2026-12-31T23:59:59Z',
      AutoRenew: (body['AutoRenew'] as boolean) ?? false,
      CancelledAt: '',
      CreatedAt: new Date().toISOString(),
    }
    return HttpResponse.json(ok(newSub, 'operation.create_success'))
  }),

  http.put('/api/tenant-subscriptions/:id/cancel', () => {
    return HttpResponse.json(ok(null, 'operation.cancel_success'))
  }),

  http.get('/api/tenant-trials', ({ request }) => {
    const url = new URL(request.url)
    const { page, pageSize } = getPageParams(url)
    return HttpResponse.json(paged(mockTenantTrials, page, pageSize))
  }),

  http.post('/api/tenant-trials', async ({ request }) => {
    const body = (await request.json()) as Record<string, unknown>
    const newTrial = {
      Id: mockTenantTrials.length + 1,
      TenantRefId: body['TenantRefId'] as number,
      PackageVersionId: body['PackageVersionId'] as number,
      Status: 'Active',
      StartedAt: new Date().toISOString(),
      ExpiresAt: '2025-12-31T23:59:59Z',
      ConvertedSubscriptionId: null,
      CreatedAt: new Date().toISOString(),
    }
    return HttpResponse.json(ok(newTrial, 'operation.create_success'))
  }),

  http.get('/api/tenant-subscription-changes', ({ request }) => {
    const url = new URL(request.url)
    const { page, pageSize } = getPageParams(url)
    return HttpResponse.json(paged(mockTenantSubscriptionChanges, page, pageSize))
  }),
]
