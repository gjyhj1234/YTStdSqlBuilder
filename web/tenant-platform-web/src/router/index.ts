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
      // ── 以下路由占位，后续阶段补充 ──
      { path: 'platform-roles', name: 'PlatformRoles', component: () => import('@/views/PlaceholderView.vue'), meta: { title: '角色管理' } },
      { path: 'platform-permissions', name: 'PlatformPermissions', component: () => import('@/views/PlaceholderView.vue'), meta: { title: '权限管理' } },
      { path: 'platform-security', name: 'PlatformSecurity', component: () => import('@/views/PlaceholderView.vue'), meta: { title: '安全设置' } },
      { path: 'tenant-groups', name: 'TenantGroups', component: () => import('@/views/PlaceholderView.vue'), meta: { title: '租户分组' } },
      { path: 'tenant-domains', name: 'TenantDomains', component: () => import('@/views/PlaceholderView.vue'), meta: { title: '域名管理' } },
      { path: 'tenant-tags', name: 'TenantTags', component: () => import('@/views/PlaceholderView.vue'), meta: { title: '标签管理' } },
      { path: 'tenant-system-configs', name: 'TenantSystemConfigs', component: () => import('@/views/PlaceholderView.vue'), meta: { title: '系统配置' } },
      { path: 'tenant-feature-flags', name: 'TenantFeatureFlags', component: () => import('@/views/PlaceholderView.vue'), meta: { title: '功能开关' } },
      { path: 'tenant-parameters', name: 'TenantParameters', component: () => import('@/views/PlaceholderView.vue'), meta: { title: '参数管理' } },
      { path: 'saas-packages', name: 'SaasPackages', component: () => import('@/views/PlaceholderView.vue'), meta: { title: '套餐管理' } },
      { path: 'saas-package-versions', name: 'SaasPackageVersions', component: () => import('@/views/PlaceholderView.vue'), meta: { title: '版本管理' } },
      { path: 'subscriptions', name: 'Subscriptions', component: () => import('@/views/PlaceholderView.vue'), meta: { title: '订阅管理' } },
      { path: 'subscription-trials', name: 'SubscriptionTrials', component: () => import('@/views/PlaceholderView.vue'), meta: { title: '试用管理' } },
      { path: 'billing-invoices', name: 'BillingInvoices', component: () => import('@/views/PlaceholderView.vue'), meta: { title: '账单管理' } },
      { path: 'payment-orders', name: 'PaymentOrders', component: () => import('@/views/PlaceholderView.vue'), meta: { title: '支付单' } },
      { path: 'api-keys', name: 'ApiKeys', component: () => import('@/views/PlaceholderView.vue'), meta: { title: 'API Key 管理' } },
      { path: 'webhooks', name: 'Webhooks', component: () => import('@/views/PlaceholderView.vue'), meta: { title: 'Webhook 管理' } },
      { path: 'operations', name: 'Operations', component: () => import('@/views/PlaceholderView.vue'), meta: { title: '平台运营' } },
      { path: 'operation-logs', name: 'OperationLogs', component: () => import('@/views/PlaceholderView.vue'), meta: { title: '操作日志' } },
      { path: 'audit-logs', name: 'AuditLogs', component: () => import('@/views/PlaceholderView.vue'), meta: { title: '审计日志' } },
      { path: 'system-logs', name: 'SystemLogs', component: () => import('@/views/PlaceholderView.vue'), meta: { title: '系统日志' } },
      { path: 'notification-templates', name: 'NotificationTemplates', component: () => import('@/views/PlaceholderView.vue'), meta: { title: '消息模板' } },
      { path: 'notification-records', name: 'NotificationRecords', component: () => import('@/views/PlaceholderView.vue'), meta: { title: '通知记录' } },
      { path: 'storage-strategies', name: 'StorageStrategies', component: () => import('@/views/PlaceholderView.vue'), meta: { title: '存储策略' } },
      { path: 'tenant-files', name: 'TenantFiles', component: () => import('@/views/PlaceholderView.vue'), meta: { title: '文件管理' } },
      { path: 'rate-limit-policies', name: 'RateLimitPolicies', component: () => import('@/views/PlaceholderView.vue'), meta: { title: '限流策略' } },
      { path: 'infra-components', name: 'InfraComponents', component: () => import('@/views/PlaceholderView.vue'), meta: { title: '基础设施组件' } },
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
