/** 权限码常量 — 与后端 DefaultPermissions 保持一致 */

// ── 平台管理 ──
export const PLATFORM_USER_VIEW = 'platform:user:view'
export const PLATFORM_USER_CREATE = 'platform:user:create'
export const PLATFORM_USER_UPDATE = 'platform:user:update'
export const PLATFORM_USER_DELETE = 'platform:user:delete'
export const PLATFORM_USER_RESET_PASSWORD = 'platform:user:reset_password'
export const PLATFORM_USER_LOCK = 'platform:user:lock'
export const PLATFORM_USER_UNLOCK = 'platform:user:unlock'

export const PLATFORM_ROLE_VIEW = 'platform:role:view'
export const PLATFORM_ROLE_CREATE = 'platform:role:create'
export const PLATFORM_ROLE_UPDATE = 'platform:role:update'
export const PLATFORM_ROLE_DELETE = 'platform:role:delete'
export const PLATFORM_ROLE_ASSIGN_PERMISSION = 'platform:role:assign_permission'
export const PLATFORM_ROLE_ASSIGN_MEMBER = 'platform:role:assign_member'

export const PLATFORM_PERMISSION_VIEW = 'platform:permission:view'

export const PLATFORM_SECURITY_VIEW = 'platform:security:view'
export const PLATFORM_SECURITY_CREATE = 'platform:security:create'
export const PLATFORM_SECURITY_UPDATE = 'platform:security:update'
export const PLATFORM_SECURITY_DELETE = 'platform:security:delete'

// ── 租户管理 ──
export const TENANT_LIST_VIEW = 'tenant:list:view'
export const TENANT_LIST_CREATE = 'tenant:list:create'
export const TENANT_LIST_UPDATE = 'tenant:list:update'
export const TENANT_LIST_DELETE = 'tenant:list:delete'
export const TENANT_LIST_ACTIVATE = 'tenant:list:activate'
export const TENANT_LIST_SUSPEND = 'tenant:list:suspend'
export const TENANT_LIST_CLOSE = 'tenant:list:close'

export const TENANT_GROUP_VIEW = 'tenant:group:view'
export const TENANT_GROUP_CREATE = 'tenant:group:create'
export const TENANT_GROUP_UPDATE = 'tenant:group:update'
export const TENANT_GROUP_DELETE = 'tenant:group:delete'

export const TENANT_TAG_VIEW = 'tenant:tag:view'
export const TENANT_TAG_CREATE = 'tenant:tag:create'
export const TENANT_TAG_UPDATE = 'tenant:tag:update'
export const TENANT_TAG_DELETE = 'tenant:tag:delete'

export const TENANT_DOMAIN_VIEW = 'tenant:domain:view'
export const TENANT_DOMAIN_CREATE = 'tenant:domain:create'
export const TENANT_DOMAIN_UPDATE = 'tenant:domain:update'
export const TENANT_DOMAIN_DELETE = 'tenant:domain:delete'
export const TENANT_DOMAIN_VERIFY = 'tenant:domain:verify'

export const TENANT_CONFIG_VIEW = 'tenant:config:view'
export const TENANT_CONFIG_CREATE = 'tenant:config:create'
export const TENANT_CONFIG_UPDATE = 'tenant:config:update'
export const TENANT_CONFIG_DELETE = 'tenant:config:delete'

// ── 套餐管理 ──
export const PACKAGE_LIST_VIEW = 'package:list:view'
export const PACKAGE_LIST_CREATE = 'package:list:create'
export const PACKAGE_LIST_UPDATE = 'package:list:update'
export const PACKAGE_LIST_DELETE = 'package:list:delete'

export const PACKAGE_VERSION_VIEW = 'package:version:view'
export const PACKAGE_VERSION_CREATE = 'package:version:create'

export const PACKAGE_CAPABILITY_VIEW = 'package:capability:view'
export const PACKAGE_CAPABILITY_CREATE = 'package:capability:create'

// ── 订阅管理 ──
export const SUBSCRIPTION_LIST_VIEW = 'subscription:list:view'
export const SUBSCRIPTION_LIST_CREATE = 'subscription:list:create'
export const SUBSCRIPTION_LIST_UPDATE = 'subscription:list:update'
export const SUBSCRIPTION_LIST_CANCEL = 'subscription:list:cancel'
export const SUBSCRIPTION_LIST_RENEW = 'subscription:list:renew'
export const SUBSCRIPTION_LIST_UPGRADE = 'subscription:list:upgrade'

export const SUBSCRIPTION_TRIAL_VIEW = 'subscription:trial:view'
export const SUBSCRIPTION_TRIAL_CREATE = 'subscription:trial:create'

export const SUBSCRIPTION_CHANGE_VIEW = 'subscription:change:view'

// ── 计费管理 ──
export const BILLING_INVOICE_VIEW = 'billing:invoice:view'
export const BILLING_INVOICE_CREATE = 'billing:invoice:create'
export const BILLING_INVOICE_UPDATE = 'billing:invoice:update'
export const BILLING_INVOICE_VOID = 'billing:invoice:void'

export const BILLING_PAYMENT_VIEW = 'billing:payment:view'
export const BILLING_PAYMENT_REFUND = 'billing:payment:refund'

// ── 通知管理 ──
export const NOTIFICATION_TEMPLATE_VIEW = 'notification:template:view'
export const NOTIFICATION_TEMPLATE_CREATE = 'notification:template:create'
export const NOTIFICATION_TEMPLATE_UPDATE = 'notification:template:update'
export const NOTIFICATION_TEMPLATE_TEST = 'notification:template:test'

export const NOTIFICATION_RECORD_VIEW = 'notification:record:view'
export const NOTIFICATION_RECORD_RESEND = 'notification:record:resend'

// ── 日志与审计 ──
export const LOG_OPERATION_VIEW = 'log:operation:view'
export const LOG_OPERATION_EXPORT = 'log:operation:export'
export const LOG_AUDIT_VIEW = 'log:audit:view'
export const LOG_AUDIT_EXPORT = 'log:audit:export'
export const LOG_SYSTEM_VIEW = 'log:system:view'

// ── 基础设施 ──
export const INFRA_RATELIMIT_VIEW = 'infra:ratelimit:view'
export const INFRA_RATELIMIT_CREATE = 'infra:ratelimit:create'
export const INFRA_RATELIMIT_UPDATE = 'infra:ratelimit:update'
export const INFRA_RATELIMIT_DELETE = 'infra:ratelimit:delete'

export const INFRA_ISOLATION_VIEW = 'infra:isolation:view'
export const INFRA_ISOLATION_CREATE = 'infra:isolation:create'
export const INFRA_ISOLATION_UPDATE = 'infra:isolation:update'
export const INFRA_ISOLATION_DELETE = 'infra:isolation:delete'

export const INFRA_COMPONENT_VIEW = 'infra:component:view'
export const INFRA_COMPONENT_CREATE = 'infra:component:create'
export const INFRA_COMPONENT_UPDATE = 'infra:component:update'
export const INFRA_COMPONENT_DELETE = 'infra:component:delete'
export const INFRA_COMPONENT_RESTART = 'infra:component:restart'
export const INFRA_COMPONENT_HEALTH_CHECK = 'infra:component:health_check'
