/** API — 审计与日志 */
import { get } from '@/utils/http'
import type { PagedResult } from '@/types/base'
import type { OperationLogRepDTO, AuditLogRepDTO, SystemLogRepDTO } from '@/types/logs'

export type { OperationLogRepDTO, AuditLogRepDTO, SystemLogRepDTO }

/* ---------- 操作日志 ---------- */

export function getOperationLogs(params: Record<string, string | number | undefined>) {
  return get<PagedResult<OperationLogRepDTO>>('/api/operation-logs', params)
}

export function getOperationLog(id: number) {
  return get<OperationLogRepDTO>(`/api/operation-logs/${id}`)
}

/* ---------- 审计日志 ---------- */

export function getAuditLogs(params: Record<string, string | number | undefined>) {
  return get<PagedResult<AuditLogRepDTO>>('/api/audit-logs', params)
}

export function getAuditLog(id: number) {
  return get<AuditLogRepDTO>(`/api/audit-logs/${id}`)
}

/* ---------- 系统日志 ---------- */

export function getSystemLogs(params: Record<string, string | number | undefined>) {
  return get<PagedResult<SystemLogRepDTO>>('/api/system-logs', params)
}

export function getSystemLog(id: number) {
  return get<SystemLogRepDTO>(`/api/system-logs/${id}`)
}
