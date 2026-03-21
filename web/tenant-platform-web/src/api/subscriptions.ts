/** API — 订阅管理 */
import { get, post, put } from '@/utils/http'
import type { PagedResult } from '@/types/base'
import type { TenantSubscriptionRepDTO, CreateSubscriptionReqDTO, TenantTrialRepDTO, CreateTrialReqDTO, TenantSubscriptionChangeRepDTO } from '@/types/subscription'

export type { TenantSubscriptionRepDTO, TenantTrialRepDTO, TenantSubscriptionChangeRepDTO, CreateSubscriptionReqDTO, CreateTrialReqDTO }

/* ---------- 订阅 ---------- */

export function getSubscriptions(params: Record<string, string | number | undefined>) {
  return get<PagedResult<TenantSubscriptionRepDTO>>('/api/tenant-subscriptions', params)
}

export function getSubscription(id: number) {
  return get<TenantSubscriptionRepDTO>(`/api/tenant-subscriptions/${id}`)
}

export function createSubscription(data: CreateSubscriptionReqDTO) {
  return post<void>('/api/tenant-subscriptions', data)
}

export function cancelSubscription(id: number) {
  return put<void>(`/api/tenant-subscriptions/${id}/cancel`)
}

/* ---------- 试用 ---------- */

export function getTrials(params: Record<string, string | number | undefined>) {
  return get<PagedResult<TenantTrialRepDTO>>('/api/tenant-trials', params)
}

export function createTrial(data: CreateTrialReqDTO) {
  return post<void>('/api/tenant-trials', data)
}

/* ---------- 变更记录 ---------- */

export function getSubscriptionChanges(params: Record<string, string | number | undefined>) {
  return get<PagedResult<TenantSubscriptionChangeRepDTO>>('/api/tenant-subscription-changes', params)
}
