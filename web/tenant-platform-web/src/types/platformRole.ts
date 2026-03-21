/** 平台角色响应 */
export interface PlatformRoleRepDTO {
  Id: number
  Code: string
  Name: string
  Description: string | null
  Status: string
  CreatedAt: string
}

/** 创建角色请求 */
export interface CreatePlatformRoleReqDTO {
  Code: string
  Name: string
  Description?: string
}

/** 更新角色请求 */
export interface UpdatePlatformRoleReqDTO {
  Name?: string
  Description?: string
}

/** 角色权限绑定请求 */
export interface RolePermissionBindReqDTO {
  PermissionIds: number[]
}

/** 角色成员绑定请求 */
export interface RoleMemberBindReqDTO {
  UserIds: number[]
}
