/** 认证状态管理 */
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { login as apiLogin, refreshToken as apiRefresh, type LoginRepDTO } from '@/api/auth'

const TOKEN_KEY = 'platform_token'
const USER_KEY = 'platform_user'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem(TOKEN_KEY))
  const userInfo = ref<LoginRepDTO | null>(restoreUser())

  function restoreUser(): LoginRepDTO | null {
    const raw = localStorage.getItem(USER_KEY)
    if (!raw) return null
    try { return JSON.parse(raw) as LoginRepDTO } catch { return null }
  }

  const isLoggedIn = computed(() => !!token.value)
  const permissions = computed(() => userInfo.value?.Permissions ?? [])
  const roles = computed(() => userInfo.value?.Roles ?? [])
  const isSuperAdmin = computed(() => userInfo.value?.IsSuperAdmin ?? false)
  const displayName = computed(() => userInfo.value?.DisplayName ?? '')

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

  /** 保存登录信息 */
  function saveLoginData(data: LoginRepDTO) {
    token.value = data.Token
    userInfo.value = data
    localStorage.setItem(TOKEN_KEY, data.Token)
    localStorage.setItem(USER_KEY, JSON.stringify(data))
  }

  /** 登录 */
  async function login(username: string, password: string) {
    const res = await apiLogin({ Username: username, Password: password })
    saveLoginData(res.data!)
  }

  /** 刷新令牌 */
  async function refresh() {
    const res = await apiRefresh({ Token: token.value ?? undefined })
    saveLoginData(res.data!)
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
    refresh,
    logout,
  }
})
