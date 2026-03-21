/** API — 认证登录 */
import { get, post } from '@/utils/http'
import type { LoginReqDTO, LoginRepDTO, RefreshTokenReqDTO, CurrentUserRepDTO } from '@/types/auth'

export type { LoginRepDTO }

export function login(data: LoginReqDTO) {
  return post<LoginRepDTO>('/api/auth/login', data)
}

export function refreshToken(data?: RefreshTokenReqDTO) {
  return post<LoginRepDTO>('/api/auth/refresh', data)
}

export function getCurrentUser() {
  return get<CurrentUserRepDTO>('/api/auth/me')
}
