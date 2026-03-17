import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '@/store/auth'

import MainLayout from '@/layouts/MainLayout.vue'
import LoginView from '@/views/login/LoginView.vue'
import DashboardView from '@/views/dashboard/DashboardView.vue'
import PlatformUsersView from '@/views/platform-users/PlatformUsersView.vue'
import TenantsView from '@/views/tenants/TenantsView.vue'

const routes: RouteRecordRaw[] = [
  {
    path: '/login',
    name: 'Login',
    component: LoginView,
    meta: { requiresAuth: false },
  },
  {
    path: '/',
    component: MainLayout,
    meta: { requiresAuth: true },
    children: [
      { path: '', redirect: '/dashboard' },
      {
        path: 'dashboard',
        name: 'Dashboard',
        component: DashboardView,
        meta: { title: '仪表盘' },
      },
      {
        path: 'platform-users',
        name: 'PlatformUsers',
        component: PlatformUsersView,
        meta: { title: '用户管理', permissions: ['platform:user:view'] },
      },
      {
        path: 'tenants',
        name: 'Tenants',
        component: TenantsView,
        meta: { title: '租户管理', permissions: ['tenant:list:view'] },
      },
      // ── 平台管理 ──
      { path: 'platform-roles', name: 'PlatformRoles', component: () => import('@/views/platform-roles/PlatformRolesView.vue'), meta: { title: '角色管理', permissions: ['platform:role:view'] } },
      { path: 'platform-permissions', name: 'PlatformPermissions', component: () => import('@/views/platform-permissions/PlatformPermissionsView.vue'), meta: { title: '权限管理', permissions: ['platform:permission:view'] } },
      { path: 'platform-security', name: 'PlatformSecurity', component: () => import('@/views/platform-security/PlatformSecurityView.vue'), meta: { title: '安全设置', permissions: ['platform:security:view'] } },
      // ── 租户信息 ──
      { path: 'tenant-groups', name: 'TenantGroups', component: () => import('@/views/tenant-groups/TenantGroupsView.vue'), meta: { title: '租户分组', permissions: ['tenant:group:view'] } },
      { path: 'tenant-domains', name: 'TenantDomains', component: () => import('@/views/tenant-domains/TenantDomainsView.vue'), meta: { title: '域名管理', permissions: ['tenant:domain:view'] } },
      { path: 'tenant-tags', name: 'TenantTags', component: () => import('@/views/tenant-tags/TenantTagsView.vue'), meta: { title: '标签管理', permissions: ['tenant:tag:view'] } },
      // ── 资源配额 ──
      { path: 'tenant-resources', name: 'TenantResources', component: () => import('@/views/tenant-resources/TenantResourcesView.vue'), meta: { title: '资源配额', permissions: ['tenant:resource:view'] } },
      // ── 租户配置中心 ──
      { path: 'tenant-system-configs', name: 'TenantSystemConfigs', component: () => import('@/views/tenant-config/TenantSystemConfigView.vue'), meta: { title: '系统配置', permissions: ['tenant:config:view'] } },
      { path: 'tenant-feature-flags', name: 'TenantFeatureFlags', component: () => import('@/views/tenant-config/TenantFeatureFlagsView.vue'), meta: { title: '功能开关', permissions: ['tenant:config:view'] } },
      { path: 'tenant-parameters', name: 'TenantParameters', component: () => import('@/views/tenant-config/TenantParametersView.vue'), meta: { title: '参数管理', permissions: ['tenant:config:view'] } },
      // ── SaaS 套餐 ──
      { path: 'saas-packages', name: 'SaasPackages', component: () => import('@/views/packages/PackagesView.vue'), meta: { title: '套餐管理', permissions: ['package:list:view'] } },
      { path: 'saas-package-versions', name: 'SaasPackageVersions', component: () => import('@/views/packages/PackageVersionsView.vue'), meta: { title: '版本管理', permissions: ['package:version:view'] } },
      // ── 订阅系统 ──
      { path: 'subscriptions', name: 'Subscriptions', component: () => import('@/views/subscriptions/SubscriptionsView.vue'), meta: { title: '订阅管理', permissions: ['subscription:list:view'] } },
      { path: 'subscription-trials', name: 'SubscriptionTrials', component: () => import('@/views/subscriptions/TrialsView.vue'), meta: { title: '试用管理', permissions: ['subscription:trial:view'] } },
      // ── 计费与账单 ──
      { path: 'billing-invoices', name: 'BillingInvoices', component: () => import('@/views/billing/InvoicesView.vue'), meta: { title: '账单管理', permissions: ['billing:invoice:view'] } },
      { path: 'payment-orders', name: 'PaymentOrders', component: () => import('@/views/billing/PaymentsView.vue'), meta: { title: '支付单', permissions: ['billing:payment:view'] } },
      { path: 'payment-refunds', name: 'PaymentRefunds', component: () => import('@/views/billing/RefundsView.vue'), meta: { title: '退款管理', permissions: ['billing:payment:refund'] } },
      // ── API 与集成 ──
      { path: 'api-keys', name: 'ApiKeys', component: () => import('@/views/api-integration/ApiKeysView.vue'), meta: { title: 'API Key 管理', permissions: ['infra:apikey:view'] } },
      { path: 'webhooks', name: 'Webhooks', component: () => import('@/views/api-integration/WebhooksView.vue'), meta: { title: 'Webhook 管理', permissions: ['infra:webhook:view'] } },
      { path: 'webhook-delivery-logs', name: 'WebhookDeliveryLogs', component: () => import('@/views/api-integration/WebhookDeliveryLogsView.vue'), meta: { title: '推送日志', permissions: ['infra:webhook:view'] } },
      // ── 平台运营 ──
      { path: 'operations', name: 'Operations', component: () => import('@/views/operations/OperationsView.vue'), meta: { title: '平台运营', permissions: ['ops:stat:view'] } },
      // ── 日志与审计 ──
      { path: 'operation-logs', name: 'OperationLogs', component: () => import('@/views/logs/OperationLogsView.vue'), meta: { title: '操作日志', permissions: ['log:operation:view'] } },
      { path: 'audit-logs', name: 'AuditLogs', component: () => import('@/views/logs/AuditLogsView.vue'), meta: { title: '审计日志', permissions: ['log:audit:view'] } },
      { path: 'system-logs', name: 'SystemLogs', component: () => import('@/views/logs/SystemLogsView.vue'), meta: { title: '系统日志', permissions: ['log:system:view'] } },
      // ── 通知系统 ──
      { path: 'notification-templates', name: 'NotificationTemplates', component: () => import('@/views/notifications/NotificationTemplatesView.vue'), meta: { title: '消息模板', permissions: ['notification:template:view'] } },
      { path: 'notification-records', name: 'NotificationRecords', component: () => import('@/views/notifications/NotificationsView.vue'), meta: { title: '通知记录', permissions: ['notification:record:view'] } },
      // ── 文件与存储 ──
      { path: 'storage-strategies', name: 'StorageStrategies', component: () => import('@/views/storage/StorageStrategiesView.vue'), meta: { title: '存储策略', permissions: ['infra:storage:view'] } },
      { path: 'tenant-files', name: 'TenantFiles', component: () => import('@/views/storage/TenantFilesView.vue'), meta: { title: '文件管理', permissions: ['infra:storage:view'] } },
      // ── 技术基础设施 ──
      { path: 'rate-limit-policies', name: 'RateLimitPolicies', component: () => import('@/views/PlaceholderView.vue'), meta: { title: '限流策略', permissions: ['infra:ratelimit:view'] } },
      { path: 'infra-components', name: 'InfraComponents', component: () => import('@/views/infrastructure/InfrastructureView.vue'), meta: { title: '基础设施组件', permissions: ['infra:component:view'] } },
    ],
  },
  { path: '/:pathMatch(.*)*', redirect: '/dashboard' },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

/** 路由守卫 — 鉴权 + 权限检查 */
router.beforeEach((to, _from, next) => {
  const auth = useAuthStore()

  if (to.meta.requiresAuth !== false && !auth.isLoggedIn) {
    return next({ path: '/login', query: { redirect: to.fullPath } })
  }

  if (to.meta.requiresAuth === false && auth.isLoggedIn && to.path === '/login') {
    return next('/dashboard')
  }

  const requiredPermissions = to.meta.permissions as string[] | undefined
  if (requiredPermissions && requiredPermissions.length > 0) {
    if (!auth.hasAnyPermission(requiredPermissions)) {
      return next('/dashboard')
    }
  }

  next()
})

export default router

declare module 'vue-router' {
  interface RouteMeta {
    requiresAuth?: boolean
    title?: string
    permissions?: string[]
  }
}
