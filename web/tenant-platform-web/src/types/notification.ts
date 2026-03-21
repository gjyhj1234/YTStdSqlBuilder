/** 通知模板响应 */
export interface NotificationTemplateRepDTO {
  Id: number
  TemplateCode: string
  TemplateName: string
  Channel: string
  SubjectTemplate: string | null
  BodyTemplate: string
  Status: string
  CreatedAt: string
}

/** 创建通知模板请求 */
export interface CreateNotificationTemplateReqDTO {
  TemplateCode: string
  TemplateName: string
  Channel?: string
  SubjectTemplate?: string
  BodyTemplate: string
}

/** 更新通知模板请求 */
export interface UpdateNotificationTemplateReqDTO {
  TemplateName?: string
  SubjectTemplate?: string
  BodyTemplate?: string
}

/** 通知响应 */
export interface NotificationRepDTO {
  Id: number
  TenantRefId: number | null
  TemplateId: number | null
  Channel: string
  Recipient: string
  Subject: string | null
  Body: string
  SendStatus: string
  SentAt: string | null
  ReadAt: string | null
  CreatedAt: string
}

/** 创建通知请求 */
export interface CreateNotificationReqDTO {
  TenantRefId?: number | null
  TemplateId?: number | null
  Channel?: string
  Recipient: string
  Subject?: string
  Body: string
}
