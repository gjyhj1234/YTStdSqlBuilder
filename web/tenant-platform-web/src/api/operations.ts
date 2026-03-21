/** API — 平台运营 */
import { get } from '@/utils/http'
import type { PagedResult } from '@/types/base'
import type { TenantDailyStatRepDTO, PlatformMonitorMetricRepDTO } from '@/types/operations'

export type { TenantDailyStatRepDTO, PlatformMonitorMetricRepDTO }

export function getDailyStats(params: Record<string, string | number | undefined>) {
  return get<PagedResult<TenantDailyStatRepDTO>>('/api/tenant-daily-stats', params)
}

export function getMonitorMetrics(params: Record<string, string | number | undefined>) {
  return get<PagedResult<PlatformMonitorMetricRepDTO>>('/api/platform-monitor-metrics', params)
}
