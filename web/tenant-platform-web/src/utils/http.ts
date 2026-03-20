import { getCurrentLocale, translateText } from '@/locales'
import { ApiError, handleApiError } from '@/utils/errorHandler'
import type { ApiResult, PagedResult } from '@/types/base'

export type { ApiResult, PagedResult }
export type { PagedRequest } from '@/types/base'

const BASE_URL = import.meta.env.VITE_API_BASE_URL || ''

function getToken(): string | null {
  return localStorage.getItem('platform_token')
}

/** 通用请求方法 */
async function request<T>(url: string, options: RequestInit = {}): Promise<ApiResult<T>> {
  const token = getToken()
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    'Accept-Language': getCurrentLocale(),
    ...(options.headers as Record<string, string> || {}),
  }
  if (token) {
    headers['Authorization'] = `Bearer ${token}`
  }

  const response = await fetch(`${BASE_URL}${url}`, {
    ...options,
    headers,
  })

  if (response.status === 401) {
    localStorage.removeItem('platform_token')
    localStorage.removeItem('platform_user')
    window.location.href = '/login'
    throw new ApiError(2006, 'auth.token_invalid')
  }

  const result: ApiResult<T> = await response.json()
  if (result.code !== 0) {
    const error = new ApiError(result.code, result.message)
    handleApiError(error)
    throw error
  }
  return result
}

/** GET 请求 */
export function get<T>(url: string, params?: Record<string, string | number | undefined>): Promise<ApiResult<T>> {
  let query = ''
  if (params) {
    const entries = Object.entries(params).filter(([, v]) => v !== undefined && v !== '')
    if (entries.length > 0) {
      query = '?' + entries.map(([k, v]) => `${encodeURIComponent(k)}=${encodeURIComponent(String(v))}`).join('&')
    }
  }
  return request<T>(url + query, { method: 'GET' })
}

/** POST 请求 */
export function post<T>(url: string, body?: unknown): Promise<ApiResult<T>> {
  return request<T>(url, { method: 'POST', body: body ? JSON.stringify(body) : undefined })
}

/** PUT 请求 */
export function put<T>(url: string, body?: unknown): Promise<ApiResult<T>> {
  return request<T>(url, { method: 'PUT', body: body ? JSON.stringify(body) : undefined })
}

/** DELETE 请求 */
export function del<T>(url: string): Promise<ApiResult<T>> {
  return request<T>(url, { method: 'DELETE' })
}
