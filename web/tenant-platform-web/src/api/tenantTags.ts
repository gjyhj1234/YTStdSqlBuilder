/** API — 租户标签 */
import { get, post } from '@/utils/http'
import type { PagedResult } from '@/types/base'
import type { TenantTagRepDTO, CreateTenantTagReqDTO, TagBindReqDTO } from '@/types/tenantInfo'

export type { TenantTagRepDTO, CreateTenantTagReqDTO }

export function getTenantTags(params: Record<string, string | number | undefined>) {
  return get<PagedResult<TenantTagRepDTO>>('/api/tenant-tags', params)
}

export function createTenantTag(data: CreateTenantTagReqDTO) {
  return post<void>('/api/tenant-tags', data)
}

export function bindTags(data: TagBindReqDTO) {
  return post<void>('/api/tenant-tags/bind', data)
}
