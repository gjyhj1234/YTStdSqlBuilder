/** 操作日志响应 */
export interface OperationLogRepDTO {
  Id: number
  TenantRefId: number | null
  OperatorType: string
  OperatorId: number | null
  Action: string
  ResourceType: string | null
  ResourceId: string | null
  IpAddress: string | null
  OperationResult: string
  CreatedAt: string
}

/** 审计日志响应 */
export interface AuditLogRepDTO {
  Id: number
  TenantRefId: number | null
  AuditType: string
  Severity: string
  SubjectType: string | null
  SubjectId: string | null
  ComplianceTag: string | null
  CreatedAt: string
}

/** 系统日志响应 */
export interface SystemLogRepDTO {
  Id: number
  ServiceName: string
  LogLevel: string
  TraceId: string | null
  Message: string
  CreatedAt: string
}
