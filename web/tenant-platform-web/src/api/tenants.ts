/** API — 租户管理 */
import { get, post, put, type PagedResult } from '@/utils/http'

export interface TenantDto {
  id: number
  tenantCode: string
  tenantName: string
  enterpriseName: string
  contactName: string
  contactEmail: string
  lifecycleStatus: string
  isolationMode: string
  enabled: boolean
  openedAt: string | null
  expiresAt: string | null
  createdAt: string
}

export interface CreateTenantRequest {
  tenantCode: string
  tenantName: string
  enterpriseName: string
  contactName: string
  contactPhone: string
  contactEmail: string
  sourceType: string
  isolationMode: string
  defaultLanguage?: string
  defaultTimezone?: string
}

export interface UpdateTenantRequest {
  tenantName: string
  enterpriseName: string
  contactName: string
  contactPhone: string
  contactEmail: string
}

export interface TenantStatusChangeRequest {
  targetStatus: string
  reason: string
}

export function getTenants(params: Record<string, string | number | undefined>) {
  return get<PagedResult<TenantDto>>('/api/tenants', params)
}

export function createTenant(data: CreateTenantRequest) {
  return post<{ id: number }>('/api/tenants', data)
}

export function updateTenant(id: number, data: UpdateTenantRequest) {
  return put<void>(`/api/tenants/${id}`, data)
}

export function changeTenantStatus(id: number, data: TenantStatusChangeRequest) {
  return put<void>(`/api/tenants/${id}/status`, data)
}
