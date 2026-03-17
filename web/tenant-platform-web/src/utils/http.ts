/** HTTP 请求封装层 — 统一 Token 注入、错误处理、响应解包 */

/** 后端标准响应格式 */
export interface ApiResult<T = unknown> {
  success: boolean
  message: string
  data: T
  traceId?: string
}

/** 分页请求参数 */
export interface PagedRequest {
  page: number
  pageSize: number
  keyword?: string
  status?: string
}

/** 分页响应 */
export interface PagedResult<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
  totalPages: number
}

const BASE_URL = import.meta.env.VITE_API_BASE_URL || ''

function getToken(): string | null {
  return localStorage.getItem('platform_token')
}

/** 通用请求方法 */
async function request<T>(url: string, options: RequestInit = {}): Promise<ApiResult<T>> {
  const token = getToken()
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
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
    window.location.href = '/login'
    throw new Error('未授权，请重新登录')
  }

  const result: ApiResult<T> = await response.json()
  if (!result.success) {
    throw new Error(result.message || '请求失败')
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
