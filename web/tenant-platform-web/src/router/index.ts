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
        meta: { title: 'route.dashboard' },
      },
      {
        path: 'platform-users',
        name: 'PlatformUsers',
        component: PlatformUsersView,
        meta: { title: 'route.platformUsers', permissions: ['platform:user:view'] },
      },
      {
        path: 'tenants',
        name: 'Tenants',
        component: TenantsView,
        meta: { title: 'route.tenants', permissions: ['tenant:list:view'] },
      },
      // ── 平台管理 ──
      { path: 'platform-roles', name: 'PlatformRoles', component: () => import('@/views/platform-roles/PlatformRolesView.vue'), meta: { title: 'route.platformRoles', permissions: ['platform:role:view'] } },
      { path: 'platform-permissions', name: 'PlatformPermissions', component: () => import('@/views/platform-permissions/PlatformPermissionsView.vue'), meta: { title: 'route.platformPermissions', permissions: ['platform:permission:view'] } },
      { path: 'platform-security', name: 'PlatformSecurity', component: () => import('@/views/platform-security/PlatformSecurityView.vue'), meta: { title: 'route.platformSecurity', permissions: ['platform:security:view'] } },
      // ── 租户信息 ──
      { path: 'tenant-groups', name: 'TenantGroups', component: () => import('@/views/tenant-groups/TenantGroupsView.vue'), meta: { title: 'route.tenantGroups', permissions: ['tenant:group:view'] } },
      { path: 'tenant-domains', name: 'TenantDomains', component: () => import('@/views/tenant-domains/TenantDomainsView.vue'), meta: { title: 'route.tenantDomains', permissions: ['tenant:domain:view'] } },
      { path: 'tenant-tags', name: 'TenantTags', component: () => import('@/views/tenant-tags/TenantTagsView.vue'), meta: { title: 'route.tenantTags', permissions: ['tenant:tag:view'] } },
      // ── 资源配额 ──
      { path: 'tenant-resources', name: 'TenantResources', component: () => import('@/views/tenant-resources/TenantResourcesView.vue'), meta: { title: 'route.tenantResources', permissions: ['tenant:resource:view'] } },
      // ── 租户配置中心 ──
      { path: 'tenant-system-configs', name: 'TenantSystemConfigs', component: () => import('@/views/tenant-config/TenantSystemConfigView.vue'), meta: { title: 'route.tenantSystemConfigs', permissions: ['tenant:config:view'] } },
      { path: 'tenant-feature-flags', name: 'TenantFeatureFlags', component: () => import('@/views/tenant-config/TenantFeatureFlagsView.vue'), meta: { title: 'route.tenantFeatureFlags', permissions: ['tenant:config:view'] } },
      { path: 'tenant-parameters', name: 'TenantParameters', component: () => import('@/views/tenant-config/TenantParametersView.vue'), meta: { title: 'route.tenantParameters', permissions: ['tenant:config:view'] } },
      // ── SaaS 套餐 ──
      { path: 'saas-packages', name: 'SaasPackages', component: () => import('@/views/packages/PackagesView.vue'), meta: { title: 'route.saasPackages', permissions: ['package:list:view'] } },
      { path: 'saas-package-versions', name: 'SaasPackageVersions', component: () => import('@/views/packages/PackageVersionsView.vue'), meta: { title: 'route.saasPackageVersions', permissions: ['package:version:view'] } },
      // ── 订阅系统 ──
      { path: 'subscriptions', name: 'Subscriptions', component: () => import('@/views/subscriptions/SubscriptionsView.vue'), meta: { title: 'route.subscriptions', permissions: ['subscription:list:view'] } },
      { path: 'subscription-trials', name: 'SubscriptionTrials', component: () => import('@/views/subscriptions/TrialsView.vue'), meta: { title: 'route.subscriptionTrials', permissions: ['subscription:trial:view'] } },
      // ── 计费与账单 ──
      { path: 'billing-invoices', name: 'BillingInvoices', component: () => import('@/views/billing/InvoicesView.vue'), meta: { title: 'route.billingInvoices', permissions: ['billing:invoice:view'] } },
      { path: 'payment-orders', name: 'PaymentOrders', component: () => import('@/views/billing/PaymentsView.vue'), meta: { title: 'route.paymentOrders', permissions: ['billing:payment:view'] } },
      { path: 'payment-refunds', name: 'PaymentRefunds', component: () => import('@/views/billing/RefundsView.vue'), meta: { title: 'route.paymentRefunds', permissions: ['billing:payment:refund'] } },
      // ── API 与集成 ──
      { path: 'api-keys', name: 'ApiKeys', component: () => import('@/views/api-integration/ApiKeysView.vue'), meta: { title: 'route.apiKeys', permissions: ['infra:apikey:view'] } },
      { path: 'webhooks', name: 'Webhooks', component: () => import('@/views/api-integration/WebhooksView.vue'), meta: { title: 'route.webhooks', permissions: ['infra:webhook:view'] } },
      { path: 'webhook-delivery-logs', name: 'WebhookDeliveryLogs', component: () => import('@/views/api-integration/WebhookDeliveryLogsView.vue'), meta: { title: 'route.webhookDeliveryLogs', permissions: ['infra:webhook:view'] } },
      // ── 平台运营 ──
      { path: 'operations', name: 'Operations', component: () => import('@/views/operations/OperationsView.vue'), meta: { title: 'route.operations', permissions: ['ops:stat:view'] } },
      // ── 日志与审计 ──
      { path: 'operation-logs', name: 'OperationLogs', component: () => import('@/views/logs/OperationLogsView.vue'), meta: { title: 'route.operationLogs', permissions: ['log:operation:view'] } },
      { path: 'audit-logs', name: 'AuditLogs', component: () => import('@/views/logs/AuditLogsView.vue'), meta: { title: 'route.auditLogs', permissions: ['log:audit:view'] } },
      { path: 'system-logs', name: 'SystemLogs', component: () => import('@/views/logs/SystemLogsView.vue'), meta: { title: 'route.systemLogs', permissions: ['log:system:view'] } },
      // ── 通知系统 ──
      { path: 'notification-templates', name: 'NotificationTemplates', component: () => import('@/views/notifications/NotificationTemplatesView.vue'), meta: { title: 'route.notificationTemplates', permissions: ['notification:template:view'] } },
      { path: 'notification-records', name: 'NotificationRecords', component: () => import('@/views/notifications/NotificationsView.vue'), meta: { title: 'route.notificationRecords', permissions: ['notification:record:view'] } },
      // ── 文件与存储 ──
      { path: 'storage-strategies', name: 'StorageStrategies', component: () => import('@/views/storage/StorageStrategiesView.vue'), meta: { title: 'route.storageStrategies', permissions: ['infra:storage:view'] } },
      { path: 'tenant-files', name: 'TenantFiles', component: () => import('@/views/storage/TenantFilesView.vue'), meta: { title: 'route.tenantFiles', permissions: ['infra:storage:view'] } },
      // ── 技术基础设施 ──
      { path: 'rate-limit-policies', name: 'RateLimitPolicies', component: () => import('@/views/infrastructure/RateLimitPoliciesView.vue'), meta: { title: 'route.rateLimitPolicies', permissions: ['infra:ratelimit:view'] } },
      { path: 'infra-components', name: 'InfraComponents', component: () => import('@/views/infrastructure/InfrastructureView.vue'), meta: { title: 'route.infraComponents', permissions: ['infra:component:view'] } },
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
