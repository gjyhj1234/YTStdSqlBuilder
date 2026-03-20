/** 租户分组响应（支持树形） */
export interface TenantGroupRepDTO {
  Id: number
  GroupCode: string
  GroupName: string
  Description: string | null
  ParentId: number | null
  Children: TenantGroupRepDTO[] | null
  CreatedAt: string
}

/** 创建租户分组请求 */
export interface CreateTenantGroupReqDTO {
  GroupCode: string
  GroupName: string
  Description?: string
  ParentId?: number | null
}

/** 租户域名响应 */
export interface TenantDomainRepDTO {
  Id: number
  TenantRefId: number
  Domain: string
  DomainType: string
  IsPrimary: boolean
  VerificationStatus: string
  CreatedAt: string
}

/** 创建租户域名请求 */
export interface CreateTenantDomainReqDTO {
  TenantRefId: number
  Domain: string
  DomainType?: string
}

/** 租户标签响应 */
export interface TenantTagRepDTO {
  Id: number
  TagKey: string
  TagValue: string
  TagType: string
  Description: string | null
  CreatedAt: string
}

/** 创建租户标签请求 */
export interface CreateTenantTagReqDTO {
  TagKey: string
  TagValue: string
  TagType?: string
  Description?: string
}

/** 批量绑定标签请求 */
export interface TagBindReqDTO {
  TenantRefId: number
  TagIds: number[]
}
