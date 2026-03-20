/** 平台权限响应（支持树形） */
export interface PlatformPermissionRepDTO {
  Id: number
  Code: string
  Name: string
  PermissionType: string
  ParentId: number | null
  Path: string | null
  Method: string | null
  Children: PlatformPermissionRepDTO[] | null
}
