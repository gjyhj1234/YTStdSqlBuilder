/** API — 租户资源配额 */
import { get, post } from '@/utils/http'
import type { PagedResult } from '@/types/base'
import type { TenantResourceQuotaRepDTO, SaveTenantResourceQuotaReqDTO } from '@/types/tenantResource'

export type { TenantResourceQuotaRepDTO, SaveTenantResourceQuotaReqDTO }

export function getTenantResourceQuotas(params: Record<string, string | number | undefined>) {
  return get<PagedResult<TenantResourceQuotaRepDTO>>('/api/tenant-resource-quotas', params)
}

export function getTenantResourceQuota(id: number) {
  return get<TenantResourceQuotaRepDTO>(`/api/tenant-resource-quotas/${id}`)
}

export function saveTenantResourceQuota(data: SaveTenantResourceQuotaReqDTO) {
  return post<void>('/api/tenant-resource-quotas', data)
}
