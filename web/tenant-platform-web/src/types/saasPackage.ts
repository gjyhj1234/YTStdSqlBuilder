/** 套餐响应 */
export interface SaasPackageRepDTO {
  Id: number
  PackageCode: string
  PackageName: string
  Description: string | null
  Status: string
  CreatedAt: string
}

/** 创建套餐请求 */
export interface CreateSaasPackageReqDTO {
  PackageCode: string
  PackageName: string
  Description?: string
}

/** 更新套餐请求 */
export interface UpdateSaasPackageReqDTO {
  PackageName?: string
  Description?: string
}

/** 套餐版本响应 */
export interface SaasPackageVersionRepDTO {
  Id: number
  PackageId: number
  VersionCode: string
  VersionName: string
  EditionType: string
  BillingCycle: string
  Price: number
  CurrencyCode: string
  TrialDays: number
  IsDefault: boolean
  Enabled: boolean
  EffectiveFrom: string | null
  EffectiveTo: string | null
  CreatedAt: string
}

/** 创建套餐版本请求 */
export interface CreateSaasPackageVersionReqDTO {
  PackageId: number
  VersionCode: string
  VersionName: string
  EditionType: string
  BillingCycle?: string
  Price: number
  CurrencyCode?: string
  TrialDays?: number
  IsDefault?: boolean
}

/** 套餐能力响应 */
export interface SaasPackageCapabilityRepDTO {
  Id: number
  PackageVersionId: number
  CapabilityKey: string
  CapabilityName: string
  CapabilityType: string
  CapabilityValue: string
  CreatedAt: string
}

/** 创建/更新套餐能力请求 */
export interface SaveSaasPackageCapabilityReqDTO {
  PackageVersionId: number
  CapabilityKey: string
  CapabilityName: string
  CapabilityType: string
  CapabilityValue: string
}
