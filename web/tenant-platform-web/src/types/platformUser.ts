/** 平台用户响应 */
export interface PlatformUserRepDTO {
  Id: number
  Username: string
  Email: string
  Phone: string | null
  DisplayName: string
  Status: string
  MfaEnabled: boolean
  LastLoginAt: string | null
  CreatedAt: string
}

/** 创建平台用户请求 */
export interface CreatePlatformUserReqDTO {
  Username: string
  Email: string
  Phone?: string
  DisplayName: string
  Password: string
  Remark?: string
}

/** 更新平台用户请求 */
export interface UpdatePlatformUserReqDTO {
  DisplayName?: string
  Phone?: string
  Email?: string
  Remark?: string
}
