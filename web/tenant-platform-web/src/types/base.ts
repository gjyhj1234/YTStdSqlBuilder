/** 统一响应结构 */
export interface ApiResult<T = void> {
  code: number
  message: string
  data?: T
}

/** 分页请求参数 */
export interface PagedRequest {
  Page?: number
  PageSize?: number
  Keyword?: string
  Status?: string
}

/** 分页响应结构 */
export interface PagedResult<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
  totalPages: number
}
