/** 权限检查 composable */
import { useAuthStore } from '@/store/auth'

export function usePermission() {
  const auth = useAuthStore()

  /** 是否拥有指定权限 */
  function has(code: string): boolean {
    return auth.hasPermission(code)
  }

  /** 是否拥有任一权限 */
  function hasAny(codes: string[]): boolean {
    return auth.hasAnyPermission(codes)
  }

  return { has, hasAny }
}
