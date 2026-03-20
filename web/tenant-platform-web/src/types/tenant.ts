/** 租户响应 */
export interface TenantRepDTO {
  Id: number
  TenantCode: string
  TenantName: string
  EnterpriseName: string | null
  ContactName: string | null
  ContactEmail: string | null
  LifecycleStatus: string
  IsolationMode: string
  Enabled: boolean
  OpenedAt: string | null
  ExpiresAt: string | null
  CreatedAt: string
}

/** 创建租户请求 */
export interface CreateTenantReqDTO {
  TenantCode: string
  TenantName: string
  EnterpriseName?: string
  ContactName?: string
  ContactPhone?: string
  ContactEmail?: string
  SourceType?: string
  IsolationMode?: string
  DefaultLanguage?: string
  DefaultTimezone?: string
}

/** 更新租户请求 */
export interface UpdateTenantReqDTO {
  TenantName?: string
  EnterpriseName?: string
  ContactName?: string
  ContactPhone?: string
  ContactEmail?: string
}

/** 变更租户状态请求 */
export interface TenantStatusChangeReqDTO {
  TargetStatus: string
  Reason?: string
}

/** 租户生命周期事件响应 */
export interface TenantLifecycleEventRepDTO {
  Id: number
  TenantRefId: number
  EventType: string
  FromStatus: string | null
  ToStatus: string | null
  Reason: string | null
  OccurredAt: string
}
