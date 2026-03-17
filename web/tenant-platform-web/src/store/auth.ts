/** 认证状态管理 */
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { login as apiLogin, type LoginResult } from '@/api/auth'

const TOKEN_KEY = 'platform_token'
const USER_KEY = 'platform_user'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem(TOKEN_KEY))
  const userInfo = ref<LoginResult | null>(restoreUser())

  function restoreUser(): LoginResult | null {
    const raw = localStorage.getItem(USER_KEY)
    if (!raw) return null
    try { return JSON.parse(raw) as LoginResult } catch { return null }
  }

  const isLoggedIn = computed(() => !!token.value)
  const permissions = computed(() => userInfo.value?.permissions ?? [])
  const roles = computed(() => userInfo.value?.roles ?? [])
  const isSuperAdmin = computed(() => userInfo.value?.isSuperAdmin ?? false)
  const displayName = computed(() => userInfo.value?.displayName ?? '')

  /** 检查是否拥有指定权限码 */
  function hasPermission(code: string): boolean {
    if (isSuperAdmin.value) return true
    return permissions.value.includes(code)
  }

  /** 检查是否拥有任一权限码 */
  function hasAnyPermission(codes: string[]): boolean {
    if (isSuperAdmin.value) return true
    return codes.some(c => permissions.value.includes(c))
  }

  /** 登录 */
  async function login(username: string, password: string) {
    const res = await apiLogin({ username, password })
    token.value = res.data.token
    userInfo.value = res.data
    localStorage.setItem(TOKEN_KEY, res.data.token)
    localStorage.setItem(USER_KEY, JSON.stringify(res.data))
  }

  /** 登出 */
  function logout() {
    token.value = null
    userInfo.value = null
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(USER_KEY)
  }

  return {
    token,
    userInfo,
    isLoggedIn,
    permissions,
    roles,
    isSuperAdmin,
    displayName,
    hasPermission,
    hasAnyPermission,
    login,
    logout,
  }
})
