/** API — 租户管理 */
import { get, post, put } from '@/utils/http'
import type { PagedResult } from '@/types/base'
import type { TenantRepDTO, CreateTenantReqDTO, UpdateTenantReqDTO, TenantStatusChangeReqDTO, TenantLifecycleEventRepDTO } from '@/types/tenant'

export type { TenantRepDTO, CreateTenantReqDTO }

export function getTenants(params: Record<string, string | number | undefined>) {
  return get<PagedResult<TenantRepDTO>>('/api/tenants', params)
}

export function getTenant(id: number) {
  return get<TenantRepDTO>(`/api/tenants/${id}`)
}

export function createTenant(data: CreateTenantReqDTO) {
  return post<void>('/api/tenants', data)
}

export function updateTenant(id: number, data: UpdateTenantReqDTO) {
  return put<void>(`/api/tenants/${id}`, data)
}

export function changeTenantStatus(id: number, data: TenantStatusChangeReqDTO) {
  return put<void>(`/api/tenants/${id}/status`, data)
}

export function getTenantLifecycleEvents(id: number, params: Record<string, string | number | undefined>) {
  return get<PagedResult<TenantLifecycleEventRepDTO>>(`/api/tenants/${id}/lifecycle-events`, params)
}
