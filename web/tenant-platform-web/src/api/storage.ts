/** API — 存储管理 */
import { get, post, put, del } from '@/utils/http'
import type { PagedResult } from '@/types/base'
import type { StorageStrategyRepDTO, CreateStorageStrategyReqDTO, UpdateStorageStrategyReqDTO, TenantFileRepDTO, FileAccessPolicyRepDTO, SaveFileAccessPolicyReqDTO } from '@/types/storage'

export type { StorageStrategyRepDTO, TenantFileRepDTO, FileAccessPolicyRepDTO, CreateStorageStrategyReqDTO, SaveFileAccessPolicyReqDTO }

/* ---------- 存储策略 ---------- */

export function getStorageStrategies(params: Record<string, string | number | undefined>) {
  return get<PagedResult<StorageStrategyRepDTO>>('/api/storage-strategies', params)
}

export function getStorageStrategy(id: number) {
  return get<StorageStrategyRepDTO>(`/api/storage-strategies/${id}`)
}

export function createStorageStrategy(data: CreateStorageStrategyReqDTO) {
  return post<void>('/api/storage-strategies', data)
}

export function updateStorageStrategy(id: number, data: UpdateStorageStrategyReqDTO) {
  return put<void>(`/api/storage-strategies/${id}`, data)
}

export function enableStorageStrategy(id: number) {
  return put<void>(`/api/storage-strategies/${id}/enable`)
}

export function disableStorageStrategy(id: number) {
  return put<void>(`/api/storage-strategies/${id}/disable`)
}

/* ---------- 租户文件 ---------- */

export function getTenantFiles(params: Record<string, string | number | undefined>) {
  return get<PagedResult<TenantFileRepDTO>>('/api/tenant-files', params)
}

export function getTenantFile(id: number) {
  return get<TenantFileRepDTO>(`/api/tenant-files/${id}`)
}

export function deleteTenantFile(id: number) {
  return del<void>(`/api/tenant-files/${id}`)
}

/* ---------- 文件访问策略 ---------- */

export function getFileAccessPolicies(params: Record<string, string | number | undefined>) {
  return get<PagedResult<FileAccessPolicyRepDTO>>('/api/file-access-policies', params)
}

export function saveFileAccessPolicy(data: SaveFileAccessPolicyReqDTO) {
  return post<void>('/api/file-access-policies', data)
}
