import * as P from './permissions'

/** 菜单项类型 */
export interface MenuItem {
  /** 菜单标识 */
  key: string
  /** i18n key */
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
    label: 'menu.dashboard',
    icon: 'home',
    path: '/dashboard',
  },
  {
    key: 'platform-management',
    label: 'menu.platformManagement',
    icon: 'group',
    children: [
      { key: 'platform-users', label: 'menu.platformUsers', path: '/platform-users', permissions: [P.PLATFORM_USER_VIEW] },
      { key: 'platform-roles', label: 'menu.platformRoles', path: '/platform-roles', permissions: [P.PLATFORM_ROLE_VIEW] },
      { key: 'platform-permissions', label: 'menu.platformPermissions', path: '/platform-permissions', permissions: [P.PLATFORM_PERMISSION_VIEW] },
      { key: 'platform-security', label: 'menu.platformSecurity', path: '/platform-security', permissions: [P.PLATFORM_SECURITY_VIEW] },
    ],
  },
  {
    key: 'tenant-lifecycle',
    label: 'menu.tenantLifecycle',
    icon: 'globe',
    children: [
      { key: 'tenants', label: 'menu.tenants', path: '/tenants', permissions: [P.TENANT_LIST_VIEW] },
      { key: 'tenant-groups', label: 'menu.tenantGroups', path: '/tenant-groups', permissions: [P.TENANT_GROUP_VIEW] },
      { key: 'tenant-domains', label: 'menu.tenantDomains', path: '/tenant-domains', permissions: [P.TENANT_DOMAIN_VIEW] },
      { key: 'tenant-tags', label: 'menu.tenantTags', path: '/tenant-tags', permissions: [P.TENANT_TAG_VIEW] },
      { key: 'tenant-resources', label: 'menu.tenantResources', path: '/tenant-resources', permissions: [P.TENANT_RESOURCE_VIEW] },
    ],
  },
  {
    key: 'tenant-config',
    label: 'menu.tenantConfig',
    icon: 'preferences',
    children: [
      { key: 'tenant-system-configs', label: 'menu.tenantSystemConfigs', path: '/tenant-system-configs', permissions: [P.TENANT_CONFIG_VIEW] },
      { key: 'tenant-feature-flags', label: 'menu.tenantFeatureFlags', path: '/tenant-feature-flags', permissions: [P.TENANT_CONFIG_VIEW] },
      { key: 'tenant-parameters', label: 'menu.tenantParameters', path: '/tenant-parameters', permissions: [P.TENANT_CONFIG_VIEW] },
    ],
  },
  {
    key: 'saas-packages',
    label: 'menu.saasPackages',
    icon: 'box',
    children: [
      { key: 'packages', label: 'menu.packages', path: '/saas-packages', permissions: [P.PACKAGE_LIST_VIEW] },
      { key: 'package-versions', label: 'menu.packageVersions', path: '/saas-package-versions', permissions: [P.PACKAGE_VERSION_VIEW] },
    ],
  },
  {
    key: 'subscriptions',
    label: 'menu.subscriptions',
    icon: 'clock',
    children: [
      { key: 'subscription-list', label: 'menu.subscriptionList', path: '/subscriptions', permissions: [P.SUBSCRIPTION_LIST_VIEW] },
      { key: 'subscription-trials', label: 'menu.subscriptionTrials', path: '/subscription-trials', permissions: [P.SUBSCRIPTION_TRIAL_VIEW] },
    ],
  },
  {
    key: 'billing',
    label: 'menu.billing',
    icon: 'money',
    children: [
      { key: 'billing-invoices', label: 'menu.billingInvoices', path: '/billing-invoices', permissions: [P.BILLING_INVOICE_VIEW] },
      { key: 'payment-orders', label: 'menu.paymentOrders', path: '/payment-orders', permissions: [P.BILLING_PAYMENT_VIEW] },
      { key: 'payment-refunds', label: 'menu.paymentRefunds', path: '/payment-refunds', permissions: [P.BILLING_PAYMENT_REFUND] },
    ],
  },
  {
    key: 'api-integration',
    label: 'menu.apiIntegration',
    icon: 'key',
    children: [
      { key: 'api-keys', label: 'menu.apiKeys', path: '/api-keys', permissions: [P.INFRA_APIKEY_VIEW] },
      { key: 'webhooks', label: 'menu.webhooks', path: '/webhooks', permissions: [P.INFRA_WEBHOOK_VIEW] },
      { key: 'webhook-delivery-logs', label: 'menu.webhookDeliveryLogs', path: '/webhook-delivery-logs', permissions: [P.INFRA_WEBHOOK_VIEW] },
    ],
  },
  {
    key: 'operations',
    label: 'menu.operations',
    icon: 'chart',
    path: '/operations',
    permissions: [P.OPS_STAT_VIEW],
  },
  {
    key: 'logs',
    label: 'menu.logs',
    icon: 'textdocument',
    children: [
      { key: 'operation-logs', label: 'menu.operationLogs', path: '/operation-logs', permissions: [P.LOG_OPERATION_VIEW] },
      { key: 'audit-logs', label: 'menu.auditLogs', path: '/audit-logs', permissions: [P.LOG_AUDIT_VIEW] },
      { key: 'system-logs', label: 'menu.systemLogs', path: '/system-logs', permissions: [P.LOG_SYSTEM_VIEW] },
    ],
  },
  {
    key: 'notifications',
    label: 'menu.notifications',
    icon: 'bell',
    children: [
      { key: 'notification-templates', label: 'menu.notificationTemplates', path: '/notification-templates', permissions: [P.NOTIFICATION_TEMPLATE_VIEW] },
      { key: 'notification-records', label: 'menu.notificationRecords', path: '/notification-records', permissions: [P.NOTIFICATION_RECORD_VIEW] },
    ],
  },
  {
    key: 'files',
    label: 'menu.files',
    icon: 'folder',
    children: [
      { key: 'storage-strategies', label: 'menu.storageStrategies', path: '/storage-strategies', permissions: [P.INFRA_STORAGE_VIEW] },
      { key: 'tenant-files', label: 'menu.tenantFiles', path: '/tenant-files', permissions: [P.INFRA_STORAGE_VIEW] },
    ],
  },
  {
    key: 'infrastructure',
    label: 'menu.infrastructure',
    icon: 'toolbox',
    children: [
      { key: 'rate-limit-policies', label: 'menu.rateLimitPolicies', path: '/rate-limit-policies', permissions: [P.INFRA_RATELIMIT_VIEW] },
      { key: 'infra-components', label: 'menu.infraComponents', path: '/infra-components', permissions: [P.INFRA_COMPONENT_VIEW] },
    ],
  },
]
