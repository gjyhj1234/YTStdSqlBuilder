import { http, HttpResponse } from 'msw'
import { mockTenantDomains } from '../data/tenantDomains'
import { ok, paged, getPageParams } from '../data/common'

export const tenantDomainsHandlers = [
  http.get('/api/tenant-domains', ({ request }) => {
    const url = new URL(request.url)
    const { page, pageSize } = getPageParams(url)
    return HttpResponse.json(paged(mockTenantDomains, page, pageSize))
  }),

  http.post('/api/tenant-domains', async ({ request }) => {
    const body = (await request.json()) as Record<string, unknown>
    const newDomain = {
      Id: mockTenantDomains.length + 1,
      TenantRefId: body['TenantRefId'] as number,
      Domain: body['Domain'] as string,
      DomainType: body['DomainType'] as string,
      IsPrimary: false,
      VerificationStatus: 'Pending',
      CreatedAt: new Date().toISOString(),
    }
    return HttpResponse.json(ok(newDomain, 'operation.create_success'))
  }),
]
