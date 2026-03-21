/** API — 租户分组 */
import { get, post } from '@/utils/http'
import type { TenantGroupRepDTO, CreateTenantGroupReqDTO } from '@/types/tenantInfo'

export type { TenantGroupRepDTO, CreateTenantGroupReqDTO }

export function getTenantGroupTree() {
  return get<TenantGroupRepDTO[]>('/api/tenant-groups/tree')
}

export function getTenantGroups() {
  return get<TenantGroupRepDTO[]>('/api/tenant-groups')
}

export function createTenantGroup(data: CreateTenantGroupReqDTO) {
  return post<void>('/api/tenant-groups', data)
}
