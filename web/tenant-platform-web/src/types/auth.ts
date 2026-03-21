/** 登录请求 */
export interface LoginReqDTO {
  Username: string
  Password: string
}

/** 登录/刷新 响应 */
export interface LoginRepDTO {
  Token: string
  ExpiresIn: number
  UserId: number
  Username: string
  DisplayName: string
  RequirePasswordReset: boolean
  Roles: string[]
  Permissions: string[]
  IsSuperAdmin: boolean
}

/** 刷新令牌请求 */
export interface RefreshTokenReqDTO {
  Token?: string
}

/** 当前用户响应 */
export interface CurrentUserRepDTO {
  UserId: number
  Username: string
  DisplayName: string
  IsSuperAdmin: boolean
}
