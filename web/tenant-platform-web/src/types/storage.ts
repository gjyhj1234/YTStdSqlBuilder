/** 存储策略响应 */
export interface StorageStrategyRepDTO {
  Id: number
  StrategyCode: string
  StrategyName: string
  ProviderType: string
  BucketName: string | null
  BasePath: string | null
  Status: string
  CreatedAt: string
}

/** 创建存储策略请求 */
export interface CreateStorageStrategyReqDTO {
  StrategyCode: string
  StrategyName: string
  ProviderType?: string
  BucketName?: string
  BasePath?: string
}

/** 更新存储策略请求 */
export interface UpdateStorageStrategyReqDTO {
  StrategyName?: string
  BucketName?: string
  BasePath?: string
}

/** 租户文件响应 */
export interface TenantFileRepDTO {
  Id: number
  TenantRefId: number
  StorageStrategyId: number | null
  FileName: string
  FilePath: string
  FileExt: string | null
  MimeType: string | null
  FileSize: number
  UploaderType: string
  UploaderId: number | null
  Visibility: string
  DownloadCount: number
  CreatedAt: string
}

/** 文件访问策略响应 */
export interface FileAccessPolicyRepDTO {
  Id: number
  FileId: number
  SubjectType: string
  SubjectId: string | null
  PermissionCode: string
  CreatedAt: string
}

/** 创建/更新文件访问策略请求 */
export interface SaveFileAccessPolicyReqDTO {
  FileId: number
  SubjectType: string
  SubjectId?: string
  PermissionCode?: string
}
