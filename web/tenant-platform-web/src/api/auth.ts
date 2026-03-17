/** API — 认证登录 */
import { post } from '@/utils/http'

export interface LoginRequest {
  username: string
  password: string
}

export interface LoginResult {
  token: string
  userId: number
  username: string
  displayName: string
  roles: string[]
  permissions: string[]
  isSuperAdmin: boolean
}

export function login(data: LoginRequest) {
  return post<LoginResult>('/api/auth/login', data)
}

export function logout() {
  return post<void>('/api/auth/logout')
}
