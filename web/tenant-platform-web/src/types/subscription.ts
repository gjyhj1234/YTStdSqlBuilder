/** 订阅响应 */
export interface TenantSubscriptionRepDTO {
  Id: number
  TenantRefId: number
  PackageVersionId: number
  SubscriptionStatus: string
  SubscriptionType: string
  StartedAt: string
  ExpiresAt: string
  AutoRenew: boolean
  CancelledAt: string | null
  CreatedAt: string
}

/** 创建订阅请求 */
export interface CreateSubscriptionReqDTO {
  TenantRefId: number
  PackageVersionId: number
  SubscriptionType?: string
  AutoRenew?: boolean
}

/** 试用响应 */
export interface TenantTrialRepDTO {
  Id: number
  TenantRefId: number
  PackageVersionId: number | null
  Status: string
  StartedAt: string
  ExpiresAt: string
  ConvertedSubscriptionId: number | null
  CreatedAt: string
}

/** 创建试用请求 */
export interface CreateTrialReqDTO {
  TenantRefId: number
  PackageVersionId: number
}

/** 订阅变更响应 */
export interface TenantSubscriptionChangeRepDTO {
  Id: number
  TenantRefId: number
  SubscriptionId: number | null
  ChangeType: string
  FromPackageVersionId: number | null
  ToPackageVersionId: number | null
  EffectiveAt: string
  Remark: string | null
  CreatedAt: string
}
