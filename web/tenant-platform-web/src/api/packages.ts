/** API — SaaS 套餐 */
import { get, post, put } from '@/utils/http'
import type { PagedResult } from '@/types/base'
import type { SaasPackageRepDTO, CreateSaasPackageReqDTO, UpdateSaasPackageReqDTO, SaasPackageVersionRepDTO, CreateSaasPackageVersionReqDTO, SaasPackageCapabilityRepDTO, SaveSaasPackageCapabilityReqDTO } from '@/types/saasPackage'

export type { SaasPackageRepDTO, SaasPackageVersionRepDTO, SaasPackageCapabilityRepDTO, CreateSaasPackageReqDTO, CreateSaasPackageVersionReqDTO, SaveSaasPackageCapabilityReqDTO }

/* ---------- 套餐 ---------- */

export function getPackages(params: Record<string, string | number | undefined>) {
  return get<PagedResult<SaasPackageRepDTO>>('/api/saas-packages', params)
}

export function getPackage(id: number) {
  return get<SaasPackageRepDTO>(`/api/saas-packages/${id}`)
}

export function createPackage(data: CreateSaasPackageReqDTO) {
  return post<void>('/api/saas-packages', data)
}

export function updatePackage(id: number, data: UpdateSaasPackageReqDTO) {
  return put<void>(`/api/saas-packages/${id}`, data)
}

export function enablePackage(id: number) {
  return put<void>(`/api/saas-packages/${id}/enable`)
}

export function disablePackage(id: number) {
  return put<void>(`/api/saas-packages/${id}/disable`)
}

/* ---------- 版本 ---------- */

export function getPackageVersions(packageId: number, params: Record<string, string | number | undefined>) {
  return get<PagedResult<SaasPackageVersionRepDTO>>(`/api/saas-package-versions/${packageId}`, params)
}

export function createPackageVersion(packageId: number, data: CreateSaasPackageVersionReqDTO) {
  return post<void>(`/api/saas-package-versions/${packageId}`, data)
}

/* ---------- 能力 ---------- */

export function getPackageCapabilities(packageVersionId: number, params: Record<string, string | number | undefined>) {
  return get<PagedResult<SaasPackageCapabilityRepDTO>>(`/api/saas-package-capabilities/${packageVersionId}`, params)
}

export function savePackageCapability(packageVersionId: number, data: SaveSaasPackageCapabilityReqDTO) {
  return post<void>(`/api/saas-package-capabilities/${packageVersionId}`, data)
}
