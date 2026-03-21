/** API — 平台用户 */
import { get, post, put } from '@/utils/http'
import type { PagedResult } from '@/types/base'
import type { PlatformUserRepDTO, CreatePlatformUserReqDTO, UpdatePlatformUserReqDTO } from '@/types/platformUser'

export type { PlatformUserRepDTO, CreatePlatformUserReqDTO }

export function getPlatformUsers(params: Record<string, string | number | undefined>) {
  return get<PagedResult<PlatformUserRepDTO>>('/api/platform-users', params)
}

export function getPlatformUser(id: number) {
  return get<PlatformUserRepDTO>(`/api/platform-users/${id}`)
}

export function createPlatformUser(data: CreatePlatformUserReqDTO) {
  return post<void>('/api/platform-users', data)
}

export function updatePlatformUser(id: number, data: UpdatePlatformUserReqDTO) {
  return put<void>(`/api/platform-users/${id}`, data)
}

export function enablePlatformUser(id: number) {
  return put<void>(`/api/platform-users/${id}/enable`)
}

export function disablePlatformUser(id: number) {
  return put<void>(`/api/platform-users/${id}/disable`)
}
