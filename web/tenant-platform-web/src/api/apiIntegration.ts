/** API — API 密钥与 Webhook */
import { get, post, put } from '@/utils/http'
import type { PagedResult } from '@/types/base'
import type { TenantApiKeyRepDTO, CreateApiKeyReqDTO, ApiKeyCreatedRepDTO, TenantApiUsageStatRepDTO, WebhookEventRepDTO, TenantWebhookRepDTO, CreateWebhookReqDTO, UpdateWebhookReqDTO, WebhookDeliveryLogRepDTO } from '@/types/apiIntegration'

export type { TenantApiKeyRepDTO, ApiKeyCreatedRepDTO, TenantApiUsageStatRepDTO, WebhookEventRepDTO, TenantWebhookRepDTO, WebhookDeliveryLogRepDTO, CreateApiKeyReqDTO, CreateWebhookReqDTO }

/* ---------- API 密钥 ---------- */

export function getApiKeys(params: Record<string, string | number | undefined>) {
  return get<PagedResult<TenantApiKeyRepDTO>>('/api/tenant-api-keys', params)
}

export function createApiKey(data: CreateApiKeyReqDTO) {
  return post<ApiKeyCreatedRepDTO>('/api/tenant-api-keys', data)
}

export function disableApiKey(id: number) {
  return put<void>(`/api/tenant-api-keys/${id}`)
}

/* ---------- API 使用统计 ---------- */

export function getApiUsageStats(params: Record<string, string | number | undefined>) {
  return get<PagedResult<TenantApiUsageStatRepDTO>>('/api/tenant-api-usage-stats', params)
}

/* ---------- Webhook 事件 ---------- */

export function getWebhookEvents(params: Record<string, string | number | undefined>) {
  return get<PagedResult<WebhookEventRepDTO>>('/api/webhook-events', params)
}

/* ---------- Webhook ---------- */

export function getWebhooks(params: Record<string, string | number | undefined>) {
  return get<PagedResult<TenantWebhookRepDTO>>('/api/tenant-webhooks', params)
}

export function createWebhook(data: CreateWebhookReqDTO) {
  return post<void>('/api/tenant-webhooks', data)
}

export function updateWebhook(id: number, data: UpdateWebhookReqDTO) {
  return put<void>(`/api/tenant-webhooks/${id}`, data)
}

export function enableWebhook(id: number) {
  return put<void>(`/api/tenant-webhooks/${id}/enable`)
}

export function disableWebhook(id: number) {
  return put<void>(`/api/tenant-webhooks/${id}/disable`)
}

/* ---------- Webhook 投递日志 ---------- */

export function getWebhookDeliveryLogs(params: Record<string, string | number | undefined>) {
  return get<PagedResult<WebhookDeliveryLogRepDTO>>('/api/webhook-delivery-logs', params)
}
