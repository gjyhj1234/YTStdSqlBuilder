using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YTStdLogger.Core;
using YTStdTenantPlatform.Entity.TenantPlatform;
using YTStdTenantPlatform.Infrastructure.Cache;
using YTStdTenantPlatform.Infrastructure.Initialization;
using YTStdTenantPlatform.Infrastructure.Initialization.Contributors;

namespace YTStdTenantPlatform.Bootstrap
{
    /// <summary>平台启动初始化器，负责建表、种子数据和缓存预热的编排</summary>
    public static class StartupInitialization
    {
        /// <summary>执行完整的平台初始化（建表 → 种子数据 → 缓存预热）</summary>
        public static async ValueTask InitializeAsync(bool includeDemoData = false)
        {
            const int tenantId = 0;
            const long userId = 0;

            Logger.Info(tenantId, userId, "[StartupInitialization] 开始平台初始化");

            // 1. 建表 / 建索引
            await CreateAllTablesAsync(tenantId, userId);

            // 2. 种子数据
            var context = new PlatformSeedContext();
            var contributors = BuildContributors(includeDemoData);
            await SeedRunner.RunAsync(contributors, context);

            // 3. 缓存预热
            await PlatformCacheWarmer.WarmUpAsync(tenantId, userId);

            Logger.Info(tenantId, userId, "[StartupInitialization] 平台初始化完成");
        }

        /// <summary>仅执行建表操作</summary>
        public static async ValueTask CreateAllTablesAsync(int tenantId, long userId)
        {
            Logger.Info(tenantId, userId, "[StartupInitialization] 开始建表");

            // 按依赖顺序建表
            // 模块 1：平台管理体系
            await PlatformUserDAL.CreateTableIfNotExists(tenantId, userId, true);
            await PlatformRoleDAL.CreateTableIfNotExists(tenantId, userId, true);
            await PlatformPermissionDAL.CreateTableIfNotExists(tenantId, userId, true);
            await PlatformRolePermissionDAL.CreateTableIfNotExists(tenantId, userId, true);
            await PlatformRoleMemberDAL.CreateTableIfNotExists(tenantId, userId, true);
            await PlatformPasswordPolicyDAL.CreateTableIfNotExists(tenantId, userId, true);
            await PlatformSecurityPolicyDAL.CreateTableIfNotExists(tenantId, userId, true);
            await PlatformIpWhitelistDAL.CreateTableIfNotExists(tenantId, userId, true);
            await PlatformMfaSettingDAL.CreateTableIfNotExists(tenantId, userId, true);
            await PlatformLoginLogDAL.CreateTableIfNotExists(tenantId, userId, false);

            // 模块 2：租户生命周期体系
            await TenantDAL.CreateTableIfNotExists(tenantId, userId, true);
            await TenantInitializationTaskDAL.CreateTableIfNotExists(tenantId, userId, false);
            await TenantLifecycleEventDAL.CreateTableIfNotExists(tenantId, userId, false);
            await TenantDataJobDAL.CreateTableIfNotExists(tenantId, userId, false);

            // 模块 3：租户信息体系
            await TenantGroupDAL.CreateTableIfNotExists(tenantId, userId, true);
            await TenantDomainDAL.CreateTableIfNotExists(tenantId, userId, true);
            await TenantTagDAL.CreateTableIfNotExists(tenantId, userId, true);
            await TenantTagBindingDAL.CreateTableIfNotExists(tenantId, userId, false);
            await TenantGroupMemberDAL.CreateTableIfNotExists(tenantId, userId, false);

            // 模块 4：租户资源管理
            await TenantResourceQuotaDAL.CreateTableIfNotExists(tenantId, userId, true);
            await TenantResourceUsageStatDAL.CreateTableIfNotExists(tenantId, userId, false);

            // 模块 5：租户配置中心
            await TenantSystemConfigDAL.CreateTableIfNotExists(tenantId, userId, true);
            await TenantFeatureFlagDAL.CreateTableIfNotExists(tenantId, userId, true);
            await TenantParameterDAL.CreateTableIfNotExists(tenantId, userId, true);
            await TenantUiBrandingDAL.CreateTableIfNotExists(tenantId, userId, true);

            // 模块 6：SaaS 套餐系统
            await SaasPackageDAL.CreateTableIfNotExists(tenantId, userId, true);
            await SaasPackageVersionDAL.CreateTableIfNotExists(tenantId, userId, true);
            await SaasPackageCapabilityDAL.CreateTableIfNotExists(tenantId, userId, true);

            // 模块 7：订阅系统
            await TenantSubscriptionDAL.CreateTableIfNotExists(tenantId, userId, true);
            await TenantTrialDAL.CreateTableIfNotExists(tenantId, userId, true);
            await TenantSubscriptionChangeDAL.CreateTableIfNotExists(tenantId, userId, false);

            // 模块 8：计费与账单系统
            await BillingInvoiceDAL.CreateTableIfNotExists(tenantId, userId, true);
            await BillingInvoiceItemDAL.CreateTableIfNotExists(tenantId, userId, false);
            await PaymentOrderDAL.CreateTableIfNotExists(tenantId, userId, true);
            await PaymentRefundDAL.CreateTableIfNotExists(tenantId, userId, true);

            // 模块 9：API 与集成平台
            await TenantApiKeyDAL.CreateTableIfNotExists(tenantId, userId, true);
            await TenantApiUsageStatDAL.CreateTableIfNotExists(tenantId, userId, false);
            await WebhookEventDAL.CreateTableIfNotExists(tenantId, userId, true);
            await TenantWebhookDAL.CreateTableIfNotExists(tenantId, userId, true);
            await TenantWebhookEventDAL.CreateTableIfNotExists(tenantId, userId, false);
            await WebhookDeliveryLogDAL.CreateTableIfNotExists(tenantId, userId, false);

            // 模块 10：平台运营体系
            await TenantDailyStatDAL.CreateTableIfNotExists(tenantId, userId, false);
            await PlatformMonitorMetricDAL.CreateTableIfNotExists(tenantId, userId, false);

            // 模块 11：日志与审计
            await OperationLogDAL.CreateTableIfNotExists(tenantId, userId, false);
            await AuditLogDAL.CreateTableIfNotExists(tenantId, userId, false);
            await SystemLogDAL.CreateTableIfNotExists(tenantId, userId, false);

            // 模块 12：通知系统
            await NotificationTemplateDAL.CreateTableIfNotExists(tenantId, userId, true);
            await NotificationDAL.CreateTableIfNotExists(tenantId, userId, false);

            // 模块 13：文件与存储
            await StorageStrategyDAL.CreateTableIfNotExists(tenantId, userId, true);
            await TenantFileDAL.CreateTableIfNotExists(tenantId, userId, true);
            await FileAccessPolicyDAL.CreateTableIfNotExists(tenantId, userId, true);

            // 模块 14：技术基础设施
            await RateLimitPolicyDAL.CreateTableIfNotExists(tenantId, userId, true);
            await DataIsolationPolicyDAL.CreateTableIfNotExists(tenantId, userId, true);
            await InfrastructureComponentDAL.CreateTableIfNotExists(tenantId, userId, true);

            Logger.Info(tenantId, userId, "[StartupInitialization] 建表完成");
        }

        /// <summary>仅执行种子数据填充</summary>
        public static async ValueTask<PlatformSeedContext> SeedDataAsync(bool includeDemoData = false)
        {
            var context = new PlatformSeedContext();
            var contributors = BuildContributors(includeDemoData);
            await SeedRunner.RunAsync(contributors, context);
            return context;
        }

        /// <summary>仅执行缓存预热</summary>
        public static async ValueTask WarmUpCacheAsync()
        {
            await PlatformCacheWarmer.WarmUpAsync(0, 0);
        }

        /// <summary>构建贡献者列表（按 Order 排序）</summary>
        private static IReadOnlyList<ISeedContributor> BuildContributors(bool includeDemoData)
        {
            var list = new List<ISeedContributor>
            {
                new PlatformUserSeedContributor(),
                new SecurityPolicySeedContributor(),
                new PermissionSeedContributor(),
                new RoleSeedContributor(),
                new PackageSeedContributor(),
                new NotificationTemplateSeedContributor(),
                new InfrastructureSeedContributor()
            };

            if (includeDemoData)
            {
                list.Add(new DemoDataSeedContributor());
            }

            // 按 Order 排序
            list.Sort((a, b) => a.Order.CompareTo(b.Order));

            return list;
        }
    }
}
