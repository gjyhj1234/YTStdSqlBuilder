import * as P from './permissions'

/** 菜单项类型 */
export interface MenuItem {
  /** 菜单标识 */
  key: string
  /** 显示名称 */
  label: string
  /** 图标（dx-icon 名称） */
  icon?: string
  /** 路由路径 */
  path?: string
  /** 所需权限码（拥有任一即可见） */
  permissions?: string[]
  /** 子菜单 */
  children?: MenuItem[]
}

/** 侧边栏菜单定义 — 映射 architecture.md 14 个模块 */
export const menuItems: MenuItem[] = [
  {
    key: 'dashboard',
    label: '仪表盘',
    icon: 'home',
    path: '/dashboard',
  },
  {
    key: 'platform-management',
    label: '平台管理体系',
    icon: 'group',
    children: [
      { key: 'platform-users', label: '用户管理', path: '/platform-users', permissions: [P.PLATFORM_USER_VIEW] },
      { key: 'platform-roles', label: '角色管理', path: '/platform-roles', permissions: [P.PLATFORM_ROLE_VIEW] },
      { key: 'platform-permissions', label: '权限管理', path: '/platform-permissions', permissions: [P.PLATFORM_PERMISSION_VIEW] },
      { key: 'platform-security', label: '安全设置', path: '/platform-security', permissions: [P.PLATFORM_SECURITY_VIEW] },
    ],
  },
  {
    key: 'tenant-lifecycle',
    label: '租户生命周期体系',
    icon: 'globe',
    children: [
      { key: 'tenants', label: '租户管理', path: '/tenants', permissions: [P.TENANT_LIST_VIEW] },
      { key: 'tenant-groups', label: '租户分组', path: '/tenant-groups', permissions: [P.TENANT_GROUP_VIEW] },
      { key: 'tenant-domains', label: '域名管理', path: '/tenant-domains', permissions: [P.TENANT_DOMAIN_VIEW] },
      { key: 'tenant-tags', label: '标签管理', path: '/tenant-tags', permissions: [P.TENANT_TAG_VIEW] },
      { key: 'tenant-resources', label: '资源配额', path: '/tenant-resources', permissions: [P.TENANT_RESOURCE_VIEW] },
    ],
  },
  {
    key: 'tenant-config',
    label: '租户配置中心',
    icon: 'preferences',
    children: [
      { key: 'tenant-system-configs', label: '系统配置', path: '/tenant-system-configs', permissions: [P.TENANT_CONFIG_VIEW] },
      { key: 'tenant-feature-flags', label: '功能开关', path: '/tenant-feature-flags', permissions: [P.TENANT_CONFIG_VIEW] },
      { key: 'tenant-parameters', label: '参数管理', path: '/tenant-parameters', permissions: [P.TENANT_CONFIG_VIEW] },
    ],
  },
  {
    key: 'saas-packages',
    label: 'SaaS 套餐系统',
    icon: 'box',
    children: [
      { key: 'packages', label: '套餐管理', path: '/saas-packages', permissions: [P.PACKAGE_LIST_VIEW] },
      { key: 'package-versions', label: '版本管理', path: '/saas-package-versions', permissions: [P.PACKAGE_VERSION_VIEW] },
    ],
  },
  {
    key: 'subscriptions',
    label: '订阅系统',
    icon: 'clock',
    children: [
      { key: 'subscription-list', label: '订阅管理', path: '/subscriptions', permissions: [P.SUBSCRIPTION_LIST_VIEW] },
      { key: 'subscription-trials', label: '试用管理', path: '/subscription-trials', permissions: [P.SUBSCRIPTION_TRIAL_VIEW] },
    ],
  },
  {
    key: 'billing',
    label: '计费与账单系统',
    icon: 'money',
    children: [
      { key: 'billing-invoices', label: '账单管理', path: '/billing-invoices', permissions: [P.BILLING_INVOICE_VIEW] },
      { key: 'payment-orders', label: '支付单', path: '/payment-orders', permissions: [P.BILLING_PAYMENT_VIEW] },
      { key: 'payment-refunds', label: '退款管理', path: '/payment-refunds', permissions: [P.BILLING_PAYMENT_REFUND] },
    ],
  },
  {
    key: 'api-integration',
    label: 'API 与集成平台',
    icon: 'key',
    children: [
      { key: 'api-keys', label: 'API Key 管理', path: '/api-keys', permissions: [P.INFRA_APIKEY_VIEW] },
      { key: 'webhooks', label: 'Webhook 管理', path: '/webhooks', permissions: [P.INFRA_WEBHOOK_VIEW] },
      { key: 'webhook-delivery-logs', label: '推送日志', path: '/webhook-delivery-logs', permissions: [P.INFRA_WEBHOOK_VIEW] },
    ],
  },
  {
    key: 'operations',
    label: '平台运营体系',
    icon: 'chart',
    path: '/operations',
    permissions: [P.OPS_STAT_VIEW],
  },
  {
    key: 'logs',
    label: '日志与审计',
    icon: 'textdocument',
    children: [
      { key: 'operation-logs', label: '操作日志', path: '/operation-logs', permissions: [P.LOG_OPERATION_VIEW] },
      { key: 'audit-logs', label: '审计日志', path: '/audit-logs', permissions: [P.LOG_AUDIT_VIEW] },
      { key: 'system-logs', label: '系统日志', path: '/system-logs', permissions: [P.LOG_SYSTEM_VIEW] },
    ],
  },
  {
    key: 'notifications',
    label: '通知系统',
    icon: 'bell',
    children: [
      { key: 'notification-templates', label: '消息模板', path: '/notification-templates', permissions: [P.NOTIFICATION_TEMPLATE_VIEW] },
      { key: 'notification-records', label: '通知记录', path: '/notification-records', permissions: [P.NOTIFICATION_RECORD_VIEW] },
    ],
  },
  {
    key: 'files',
    label: '文件与存储',
    icon: 'folder',
    children: [
      { key: 'storage-strategies', label: '存储策略', path: '/storage-strategies', permissions: [P.INFRA_STORAGE_VIEW] },
      { key: 'tenant-files', label: '文件管理', path: '/tenant-files', permissions: [P.INFRA_STORAGE_VIEW] },
    ],
  },
  {
    key: 'infrastructure',
    label: '技术基础设施',
    icon: 'toolbox',
    children: [
      { key: 'rate-limit-policies', label: '限流策略', path: '/rate-limit-policies', permissions: [P.INFRA_RATELIMIT_VIEW] },
      { key: 'infra-components', label: '基础设施组件', path: '/infra-components', permissions: [P.INFRA_COMPONENT_VIEW] },
    ],
  },
]
