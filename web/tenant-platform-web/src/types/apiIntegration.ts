/** API 密钥响应 */
export interface TenantApiKeyRepDTO {
  Id: number
  TenantRefId: number
  KeyName: string
  AccessKey: string
  Status: string
  QuotaLimit: number | null
  RateLimit: number | null
  LastUsedAt: string | null
  ExpiresAt: string | null
  CreatedAt: string
}

/** 创建 API 密钥请求 */
export interface CreateApiKeyReqDTO {
  TenantRefId: number
  KeyName: string
  ExpiresAt?: string | null
}

/** 创建 API 密钥响应（仅创建时返回 SecretKey） */
export interface ApiKeyCreatedRepDTO {
  Id: number
  AccessKey: string
  SecretKey: string
}

/** API 用量统计响应 */
export interface TenantApiUsageStatRepDTO {
  Id: number
  TenantRefId: number
  ApiKeyId: number | null
  StatDate: string
  ApiPath: string
  RequestCount: number
  SuccessCount: number
  ErrorCount: number
  AverageLatencyMs: number
  CreatedAt: string
}

/** Webhook 事件响应 */
export interface WebhookEventRepDTO {
  Id: number
  EventCode: string
  EventName: string
  Description: string | null
  CreatedAt: string
}

/** Webhook 响应 */
export interface TenantWebhookRepDTO {
  Id: number
  TenantRefId: number
  WebhookName: string
  TargetUrl: string
  Status: string
  CreatedAt: string
}

/** 创建 Webhook 请求 */
export interface CreateWebhookReqDTO {
  TenantRefId: number
  WebhookName: string
  TargetUrl: string
}

/** 更新 Webhook 请求 */
export interface UpdateWebhookReqDTO {
  WebhookName?: string
  TargetUrl?: string
}

/** Webhook 投递日志响应 */
export interface WebhookDeliveryLogRepDTO {
  Id: number
  WebhookId: number
  EventId: number | null
  DeliveryStatus: string
  ResponseStatusCode: number | null
  RetryCount: number
  DeliveredAt: string | null
  CreatedAt: string
}
