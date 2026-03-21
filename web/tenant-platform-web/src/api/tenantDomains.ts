/** API — 租户域名 */
import { get, post } from '@/utils/http'
import type { TenantDomainRepDTO, CreateTenantDomainReqDTO } from '@/types/tenantInfo'

export type { TenantDomainRepDTO, CreateTenantDomainReqDTO }

export function getTenantDomains() {
  return get<TenantDomainRepDTO[]>('/api/tenant-domains')
}

export function createTenantDomain(data: CreateTenantDomainReqDTO) {
  return post<void>('/api/tenant-domains', data)
}
