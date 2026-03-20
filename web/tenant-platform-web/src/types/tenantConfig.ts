/** 租户系统配置响应 */
export interface TenantSystemConfigRepDTO {
  Id: number
  TenantRefId: number
  SystemName: string | null
  LogoUrl: string | null
  SystemTheme: string | null
  DefaultLanguage: string | null
  DefaultTimezone: string | null
  UpdatedAt: string
}

/** 更新租户系统配置请求 */
export interface UpdateTenantSystemConfigReqDTO {
  SystemName?: string
  LogoUrl?: string
  SystemTheme?: string
  DefaultLanguage?: string
  DefaultTimezone?: string
}

/** 功能开关响应 */
export interface TenantFeatureFlagRepDTO {
  Id: number
  TenantRefId: number
  FeatureKey: string
  FeatureName: string
  Enabled: boolean
  RolloutType: string
  UpdatedAt: string
}

/** 创建/更新功能开关请求 */
export interface SaveTenantFeatureFlagReqDTO {
  TenantRefId: number
  FeatureKey: string
  FeatureName: string
  Enabled: boolean
  RolloutType?: string
}

/** 租户参数响应 */
export interface TenantParameterRepDTO {
  Id: number
  TenantRefId: number
  ParamKey: string
  ParamName: string
  ParamType: string
  ParamValue: string
  UpdatedAt: string
}

/** 创建/更新租户参数请求 */
export interface SaveTenantParameterReqDTO {
  TenantRefId: number
  ParamKey: string
  ParamName: string
  ParamType?: string
  ParamValue: string
}
