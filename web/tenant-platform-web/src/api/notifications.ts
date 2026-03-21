/** API — 通知管理 */
import { get, post, put } from '@/utils/http'
import type { PagedResult } from '@/types/base'
import type { NotificationTemplateRepDTO, CreateNotificationTemplateReqDTO, UpdateNotificationTemplateReqDTO, NotificationRepDTO, CreateNotificationReqDTO } from '@/types/notification'

export type { NotificationTemplateRepDTO, NotificationRepDTO, CreateNotificationTemplateReqDTO, CreateNotificationReqDTO }

/* ---------- 通知模板 ---------- */

export function getNotificationTemplates(params: Record<string, string | number | undefined>) {
  return get<PagedResult<NotificationTemplateRepDTO>>('/api/notification-templates', params)
}

export function getNotificationTemplate(id: number) {
  return get<NotificationTemplateRepDTO>(`/api/notification-templates/${id}`)
}

export function createNotificationTemplate(data: CreateNotificationTemplateReqDTO) {
  return post<void>('/api/notification-templates', data)
}

export function updateNotificationTemplate(id: number, data: UpdateNotificationTemplateReqDTO) {
  return put<void>(`/api/notification-templates/${id}`, data)
}

export function enableNotificationTemplate(id: number) {
  return put<void>(`/api/notification-templates/${id}/enable`)
}

export function disableNotificationTemplate(id: number) {
  return put<void>(`/api/notification-templates/${id}/disable`)
}

/* ---------- 通知 ---------- */

export function getNotifications(params: Record<string, string | number | undefined>) {
  return get<PagedResult<NotificationRepDTO>>('/api/notifications', params)
}

export function getNotification(id: number) {
  return get<NotificationRepDTO>(`/api/notifications/${id}`)
}

export function createNotification(data: CreateNotificationReqDTO) {
  return post<void>('/api/notifications', data)
}

export function markNotificationRead(id: number) {
  return put<void>(`/api/notifications/${id}/read`)
}
