/** API — 平台权限 */
import { get } from '@/utils/http'
import type { PlatformPermissionRepDTO } from '@/types/platformPermission'

export type { PlatformPermissionRepDTO }

export function getPermissionTree() {
  return get<PlatformPermissionRepDTO[]>('/api/platform-permissions/tree')
}

export function getPermissions() {
  return get<PlatformPermissionRepDTO[]>('/api/platform-permissions')
}

export function getPermission(id: number) {
  return get<PlatformPermissionRepDTO>(`/api/platform-permissions/${id}`)
}

export function getPermissionByCode(code: string) {
  return get<PlatformPermissionRepDTO>(`/api/platform-permissions/code/${code}`)
}
