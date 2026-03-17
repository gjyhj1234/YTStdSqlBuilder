/** API — 平台用户 */
import { get, post, put, type PagedResult } from '@/utils/http'

export interface PlatformUserDto {
  id: number
  username: string
  email: string
  phone: string
  displayName: string
  status: string
  mfaEnabled: boolean
  lastLoginAt: string | null
  createdAt: string
}

export interface CreatePlatformUserRequest {
  username: string
  email: string
  phone: string
  displayName: string
  password: string
  remark?: string
}

export interface UpdatePlatformUserRequest {
  displayName: string
  phone: string
  email: string
  remark?: string
}

export function getPlatformUsers(params: Record<string, string | number | undefined>) {
  return get<PagedResult<PlatformUserDto>>('/api/platform-users', params)
}

export function createPlatformUser(data: CreatePlatformUserRequest) {
  return post<{ id: number }>('/api/platform-users', data)
}

export function updatePlatformUser(id: number, data: UpdatePlatformUserRequest) {
  return put<void>(`/api/platform-users/${id}`, data)
}

export function enablePlatformUser(id: number) {
  return put<void>(`/api/platform-users/${id}/enable`)
}

export function disablePlatformUser(id: number) {
  return put<void>(`/api/platform-users/${id}/disable`)
}
