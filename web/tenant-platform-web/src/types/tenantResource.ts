/** 资源配额响应 */
export interface TenantResourceQuotaRepDTO {
  Id: number
  TenantRefId: number
  QuotaType: string
  QuotaLimit: number
  WarningThreshold: number | null
  ResetCycle: string | null
  CreatedAt: string
}

/** 创建/更新资源配额请求 */
export interface SaveTenantResourceQuotaReqDTO {
  TenantRefId: number
  QuotaType: string
  QuotaLimit: number
  WarningThreshold?: number | null
  ResetCycle?: string
}
