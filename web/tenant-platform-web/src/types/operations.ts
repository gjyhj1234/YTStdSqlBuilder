/** 租户每日统计响应 */
export interface TenantDailyStatRepDTO {
  Id: number
  TenantRefId: number
  StatDate: string
  ActiveUserCount: number
  NewUserCount: number
  ApiCallCount: number
  StorageBytes: number
  ResourceScore: number
  CreatedAt: string
}

/** 平台监控指标响应 */
export interface PlatformMonitorMetricRepDTO {
  Id: number
  ComponentName: string
  MetricType: string
  MetricKey: string
  MetricValue: number
  MetricUnit: string | null
  CollectedAt: string
}
