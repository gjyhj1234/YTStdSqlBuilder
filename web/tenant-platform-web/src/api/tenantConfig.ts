/** API — 租户配置（系统配置、功能开关、参数） */
import { get, post, put, del } from '@/utils/http'
import type { PagedResult } from '@/types/base'
import type { TenantSystemConfigRepDTO, UpdateTenantSystemConfigReqDTO, TenantFeatureFlagRepDTO, SaveTenantFeatureFlagReqDTO, TenantParameterRepDTO, SaveTenantParameterReqDTO } from '@/types/tenantConfig'

export type { TenantSystemConfigRepDTO, TenantFeatureFlagRepDTO, TenantParameterRepDTO, SaveTenantFeatureFlagReqDTO, SaveTenantParameterReqDTO }

/* ---------- 系统配置 ---------- */

export function getTenantSystemConfig(tenantRefId: number) {
  return get<TenantSystemConfigRepDTO>(`/api/tenant-system-configs/${tenantRefId}`)
}

export function updateTenantSystemConfig(tenantRefId: number, data: UpdateTenantSystemConfigReqDTO) {
  return put<void>(`/api/tenant-system-configs/${tenantRefId}`, data)
}

/* ---------- 功能开关 ---------- */

export function getTenantFeatureFlags(params: Record<string, string | number | undefined>) {
  return get<PagedResult<TenantFeatureFlagRepDTO>>('/api/tenant-feature-flags', params)
}

export function saveTenantFeatureFlag(data: SaveTenantFeatureFlagReqDTO) {
  return post<void>('/api/tenant-feature-flags', data)
}

export function toggleFeatureFlag(id: number, enabled: boolean) {
  return put<void>(`/api/tenant-feature-flags/${id}/toggle?enabled=${enabled}`)
}

/* ---------- 参数 ---------- */

export function getTenantParameters(params: Record<string, string | number | undefined>) {
  return get<PagedResult<TenantParameterRepDTO>>('/api/tenant-parameters', params)
}

export function saveTenantParameter(data: SaveTenantParameterReqDTO) {
  return post<void>('/api/tenant-parameters', data)
}

export function deleteTenantParameter(id: number) {
  return del<void>(`/api/tenant-parameters/${id}`)
}
