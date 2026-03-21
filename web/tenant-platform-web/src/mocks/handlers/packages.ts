import { http, HttpResponse } from 'msw'
import {
  mockSaasPackages,
  mockSaasPackageVersions,
  mockSaasPackageCapabilities,
} from '../data/packages'
import { ok, fail, paged, getPageParams } from '../data/common'

export const packagesHandlers = [
  http.get('/api/saas-packages', ({ request }) => {
    const url = new URL(request.url)
    const { page, pageSize } = getPageParams(url)
    return HttpResponse.json(paged(mockSaasPackages, page, pageSize))
  }),

  http.get('/api/saas-packages/:id', ({ params }) => {
    const pkg = mockSaasPackages.find((p) => p.Id === Number(params['id']))
    if (!pkg)
      return HttpResponse.json(
        fail('error.package_not_found'),
        { status: 404 },
      )
    return HttpResponse.json(ok(pkg))
  }),

  http.post('/api/saas-packages', async ({ request }) => {
    const body = (await request.json()) as Record<string, unknown>
    const newPkg = {
      Id: mockSaasPackages.length + 1,
      PackageCode: body['PackageCode'] as string,
      PackageName: body['PackageName'] as string,
      Description: (body['Description'] as string) ?? '',
      Status: 'Active',
      CreatedAt: new Date().toISOString(),
    }
    return HttpResponse.json(ok(newPkg, 'operation.create_success'))
  }),

  http.put('/api/saas-packages/:id', async ({ request }) => {
    const body = (await request.json()) as Record<string, unknown>
    return HttpResponse.json(ok(body, 'operation.update_success'))
  }),

  http.put('/api/saas-packages/:id/enable', () => {
    return HttpResponse.json(ok(null, 'operation.enable_success'))
  }),

  http.put('/api/saas-packages/:id/disable', () => {
    return HttpResponse.json(ok(null, 'operation.disable_success'))
  }),

  http.get('/api/saas-packages/:packageId/versions', ({ params, request }) => {
    const url = new URL(request.url)
    const { page, pageSize } = getPageParams(url)
    const versions = mockSaasPackageVersions.filter(
      (v) => v.PackageId === Number(params['packageId']),
    )
    return HttpResponse.json(paged(versions, page, pageSize))
  }),

  http.post('/api/saas-packages/:packageId/versions', async ({ request, params }) => {
    const body = (await request.json()) as Record<string, unknown>
    const newVersion = {
      Id: mockSaasPackageVersions.length + 1,
      PackageId: Number(params['packageId']),
      VersionCode: body['VersionCode'] as string,
      VersionName: body['VersionName'] as string,
      EditionType: body['EditionType'] as string,
      BillingCycle: body['BillingCycle'] as string,
      Price: body['Price'] as number,
      CurrencyCode: (body['CurrencyCode'] as string) ?? 'CNY',
      TrialDays: (body['TrialDays'] as number) ?? 0,
      IsDefault: false,
      Enabled: true,
      EffectiveFrom: new Date().toISOString(),
      EffectiveTo: '2026-12-31T23:59:59Z',
      CreatedAt: new Date().toISOString(),
    }
    return HttpResponse.json(ok(newVersion, 'operation.create_success'))
  }),

  http.get(
    '/api/saas-package-versions/:packageVersionId/capabilities',
    ({ params, request }) => {
      const url = new URL(request.url)
      const { page, pageSize } = getPageParams(url)
      const caps = mockSaasPackageCapabilities.filter(
        (c) => c.PackageVersionId === Number(params['packageVersionId']),
      )
      return HttpResponse.json(paged(caps, page, pageSize))
    },
  ),

  http.post(
    '/api/saas-package-versions/:packageVersionId/capabilities',
    async ({ request, params }) => {
      const body = (await request.json()) as Record<string, unknown>
      const newCap = {
        Id: mockSaasPackageCapabilities.length + 1,
        PackageVersionId: Number(params['packageVersionId']),
        CapabilityKey: body['CapabilityKey'] as string,
        CapabilityName: body['CapabilityName'] as string,
        CapabilityType: body['CapabilityType'] as string,
        CapabilityValue: body['CapabilityValue'] as string,
        CreatedAt: new Date().toISOString(),
      }
      return HttpResponse.json(ok(newCap, 'operation.create_success'))
    },
  ),
]
