using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using YTStdTenantPlatform.Entity.TenantPlatform;
using YTStdTenantPlatform.Infrastructure.Cache;
using YTStdTenantPlatform.Infrastructure.Initialization;
using YTStdTenantPlatform.Infrastructure.Initialization.SeedData;

namespace YTStdTenantPlatform.Tests
{
    /// <summary>初始化数据与基础设施测试</summary>
    public class InitializationTests
    {
        // ============================================================
        // SeedData 强类型数据清单测试
        // ============================================================

        [Fact]
        public void DefaultPlatformUsers_ContainsAdmin()
        {
            var users = DefaultPlatformUsers.GetDefaultUsers();
            Assert.NotEmpty(users);

            var admin = users.FirstOrDefault(u => u.Username == "admin");
            Assert.NotNull(admin);
            Assert.Equal("admin@platform.local", admin.Email);
            Assert.Equal("超级管理员", admin.DisplayName);
            Assert.Equal("active", admin.Status);
            Assert.False(admin.MfaEnabled);
            // 密码使用安全随机哈希，不得是明文占位符
            Assert.NotEmpty(admin.PasswordHash);
            Assert.NotEqual("INIT_HASH_PLACEHOLDER", admin.PasswordHash);
            // 密码应设置为已过期，强制首次登录重置
            Assert.NotNull(admin.PasswordExpiresAt);
        }

        [Fact]
        public void DefaultPermissions_HasTopLevelMenus()
        {
            var seeds = DefaultPermissions.GetDefaultPermissions();
            Assert.NotEmpty(seeds);

            // 验证顶级菜单存在
            var topMenus = seeds.Where(s =>
                s.Permission.PermissionType == "menu" &&
                string.IsNullOrEmpty(s.ParentCode))
                .ToList();
            Assert.NotEmpty(topMenus);

            // 验证包含平台管理和租户管理菜单
            Assert.Contains(seeds, s => s.Permission.Code == "platform:management");
            Assert.Contains(seeds, s => s.Permission.Code == "tenant:management");
            Assert.Contains(seeds, s => s.Permission.Code == "package:management");
        }

        [Fact]
        public void DefaultPermissions_HasApiPermissions()
        {
            var seeds = DefaultPermissions.GetDefaultPermissions();
            var apiPerms = seeds.Where(s => s.Permission.PermissionType == "api").ToList();
            Assert.NotEmpty(apiPerms);

            // API 权限应有 Path 和 Method
            foreach (var seed in apiPerms)
            {
                Assert.False(string.IsNullOrEmpty(seed.Permission.Path),
                    "API 权限 " + seed.Permission.Code + " 应有 Path");
                Assert.False(string.IsNullOrEmpty(seed.Permission.Method),
                    "API 权限 " + seed.Permission.Code + " 应有 Method");
            }
        }

        [Fact]
        public void DefaultPermissions_CodesAreUnique()
        {
            var seeds = DefaultPermissions.GetDefaultPermissions();
            var codes = seeds.Select(s => s.Permission.Code).ToList();
            var distinct = codes.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            Assert.Equal(distinct.Count, codes.Count);
        }

        [Fact]
        public void DefaultPermissions_ParentCodesReferExistingCodes()
        {
            var seeds = DefaultPermissions.GetDefaultPermissions();
            var allCodes = new HashSet<string>(
                seeds.Select(s => s.Permission.Code),
                StringComparer.OrdinalIgnoreCase);

            foreach (var seed in seeds)
            {
                if (!string.IsNullOrEmpty(seed.ParentCode))
                {
                    Assert.True(allCodes.Contains(seed.ParentCode),
                        "权限 " + seed.Permission.Code + " 引用了不存在的父级 " + seed.ParentCode);
                }
            }
        }

        [Fact]
        public void DefaultRoles_HasSuperAdmin()
        {
            var roles = DefaultRoles.GetDefaultRoles();
            Assert.NotEmpty(roles);

            var superAdmin = roles.FirstOrDefault(r => r.Code == "super_admin");
            Assert.NotNull(superAdmin);
            Assert.Equal("超级管理员", superAdmin.Name);
            Assert.Equal("active", superAdmin.Status);
        }

        [Fact]
        public void DefaultRoles_SuperAdminGetsAllPermissions()
        {
            var allPerms = DefaultPermissions.GetDefaultPermissions();
            var bindings = DefaultRoles.GetRolePermissionBindings(allPerms);
            Assert.NotEmpty(bindings);

            // super_admin 应绑定到所有权限
            var superAdminBindings = bindings
                .Where(b => b.RoleCode == "super_admin")
                .ToList();
            Assert.NotEmpty(superAdminBindings);

            // 应该至少包含所有权限
            Assert.True(superAdminBindings.Count >= allPerms.Count,
                "超级管理员角色应绑定所有权限");
        }

        [Fact]
        public void DefaultRoles_AdminUserBoundToSuperAdmin()
        {
            var memberBindings = DefaultRoles.GetRoleMemberBindings();
            Assert.NotEmpty(memberBindings);

            var adminBinding = memberBindings.FirstOrDefault(
                b => b.RoleCode == "super_admin" && b.Username == "admin");
            Assert.NotNull(adminBinding);
        }

        [Fact]
        public void DefaultSecurityPolicies_HasDefaults()
        {
            var passwordPolicies = DefaultSecurityPolicies.GetDefaultPasswordPolicies();
            Assert.NotEmpty(passwordPolicies);

            var defaultPp = passwordPolicies.FirstOrDefault(p => p.IsDefault);
            Assert.NotNull(defaultPp);
            Assert.True(defaultPp.MinLength >= 8);
            Assert.True(defaultPp.LoginFailLockThreshold > 0);

            var securityPolicies = DefaultSecurityPolicies.GetDefaultSecurityPolicies();
            Assert.NotEmpty(securityPolicies);

            var defaultSp = securityPolicies.FirstOrDefault(p => p.IsDefault);
            Assert.NotNull(defaultSp);
            Assert.True(defaultSp.SessionTimeoutMinutes > 0);
        }

        [Fact]
        public void DefaultPackages_HasThreePackages()
        {
            var packages = DefaultPackages.GetDefaultPackages();
            Assert.Equal(3, packages.Count);

            // 验证免费、标准、企业
            Assert.Contains(packages, p => p.PackageCode == "free");
            Assert.Contains(packages, p => p.PackageCode == "standard");
            Assert.Contains(packages, p => p.PackageCode == "enterprise");
        }

        [Fact]
        public void DefaultPackages_EachHasVersionAndCapabilities()
        {
            var packages = DefaultPackages.GetDefaultPackages();
            var versions = DefaultPackages.GetDefaultVersions();
            var capabilities = DefaultPackages.GetDefaultCapabilities();

            Assert.NotEmpty(versions);
            Assert.NotEmpty(capabilities);

            // 每个套餐至少有一个版本
            var packageCodes = packages.Select(p => p.PackageCode).ToHashSet();
            var versionPackageCodes = versions.Select(v => v.PackageCode).ToHashSet();
            foreach (var code in packageCodes)
            {
                Assert.True(versionPackageCodes.Contains(code),
                    "套餐 " + code + " 应有对应的版本");
            }
        }

        [Fact]
        public void DefaultNotificationTemplates_HasRequiredTemplates()
        {
            var templates = DefaultNotificationTemplates.GetDefaultTemplates();
            Assert.True(templates.Count >= 5);

            // 验证必要模板存在
            Assert.Contains(templates, t => t.TemplateCode == "tenant_welcome");
            Assert.Contains(templates, t => t.TemplateCode == "subscription_expire_warning");
            Assert.Contains(templates, t => t.TemplateCode == "payment_success");
            Assert.Contains(templates, t => t.TemplateCode == "password_reset");

            // 模板编码唯一
            var codes = templates.Select(t => t.TemplateCode).ToList();
            Assert.Equal(codes.Distinct().Count(), codes.Count);
        }

        [Fact]
        public void DefaultInfrastructure_HasRateLimitAndIsolation()
        {
            var rateLimits = DefaultInfrastructure.GetDefaultRateLimitPolicies();
            Assert.NotEmpty(rateLimits);

            var isolations = DefaultInfrastructure.GetDefaultDataIsolationPolicies();
            Assert.NotEmpty(isolations);

            var components = DefaultInfrastructure.GetDefaultComponents();
            Assert.NotEmpty(components);
        }

        [Fact]
        public void DemoTenantData_IsSeparateFromProduction()
        {
            var tenants = DemoTenantData.GetDemoTenants();
            Assert.NotEmpty(tenants);
            var tenant = tenants[0];
            Assert.Equal("DEMO001", tenant.TenantCode);
            Assert.Equal("演示租户", tenant.TenantName);
        }

        // ============================================================
        // 基础设施组件测试
        // ============================================================

        [Fact]
        public void PlatformSeedContext_InitializesEmpty()
        {
            var context = new PlatformSeedContext();
            Assert.Equal(0, context.TenantId);
            Assert.Equal(0, context.SystemUserId);
            Assert.Empty(context.PermissionIdMap);
            Assert.Empty(context.RoleIdMap);
            Assert.Empty(context.UserIdMap);
            Assert.Empty(context.PackageIdMap);
            Assert.Empty(context.Logs);
        }

        [Fact]
        public void PlatformSeedContext_CanStoreIdMappings()
        {
            var context = new PlatformSeedContext();
            context.UserIdMap["admin"] = 1;
            context.RoleIdMap["super_admin"] = 1;
            context.PermissionIdMap["platform:management"] = 1;
            context.PackageIdMap["free"] = 1;

            Assert.Equal(1, context.UserIdMap["admin"]);
            Assert.Equal(1, context.RoleIdMap["super_admin"]);
            Assert.Equal(1, context.PermissionIdMap["platform:management"]);
            Assert.Equal(1, context.PackageIdMap["free"]);
        }

        [Fact]
        public void PlatformSeedContext_LogsTimestamped()
        {
            var context = new PlatformSeedContext();
            context.Log("测试日志");
            Assert.Single(context.Logs);
            Assert.Contains("测试日志", context.Logs[0]);
            // 应包含时间戳格式
            Assert.Contains(":", context.Logs[0]);
        }

        [Fact]
        public void PlatformCacheWarmer_HasEmptyInitialState()
        {
            PlatformCacheWarmer.ClearAll();
            Assert.Empty(PlatformCacheWarmer.PermissionCache);
            Assert.Empty(PlatformCacheWarmer.RolePermissionCache);
            Assert.Empty(PlatformCacheWarmer.UserRoleCache);
            Assert.Empty(PlatformCacheWarmer.FeatureFlagCache);
            Assert.Null(PlatformCacheWarmer.ConfigSnapshot);
        }

        [Fact]
        public void PlatformConfigSnapshot_StoresDefaults()
        {
            var passwordPolicy = new PlatformPasswordPolicy
            {
                PolicyName = "test",
                MinLength = 8,
                MaxLength = 64,
                IsDefault = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var securityPolicy = new PlatformSecurityPolicy
            {
                PolicyName = "test",
                SessionTimeoutMinutes = 30,
                IsDefault = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var snapshot = new PlatformConfigSnapshot(passwordPolicy, securityPolicy);
            Assert.NotNull(snapshot.DefaultPasswordPolicy);
            Assert.NotNull(snapshot.DefaultSecurityPolicy);
            Assert.Equal("test", snapshot.DefaultPasswordPolicy.PolicyName);
        }

        // ============================================================
        // Contributor 顺序与命名测试
        // ============================================================

        [Fact]
        public void AllContributors_HaveCorrectOrder()
        {
            var contributors = new ISeedContributor[]
            {
                new YTStdTenantPlatform.Infrastructure.Initialization.Contributors.PlatformUserSeedContributor(),
                new YTStdTenantPlatform.Infrastructure.Initialization.Contributors.SecurityPolicySeedContributor(),
                new YTStdTenantPlatform.Infrastructure.Initialization.Contributors.PermissionSeedContributor(),
                new YTStdTenantPlatform.Infrastructure.Initialization.Contributors.RoleSeedContributor(),
                new YTStdTenantPlatform.Infrastructure.Initialization.Contributors.PackageSeedContributor(),
                new YTStdTenantPlatform.Infrastructure.Initialization.Contributors.NotificationTemplateSeedContributor(),
                new YTStdTenantPlatform.Infrastructure.Initialization.Contributors.InfrastructureSeedContributor(),
                new YTStdTenantPlatform.Infrastructure.Initialization.Contributors.DemoDataSeedContributor()
            };

            // 验证顺序
            for (int i = 1; i < contributors.Length; i++)
            {
                Assert.True(contributors[i - 1].Order <= contributors[i].Order,
                    contributors[i - 1].Name + " (Order=" + contributors[i - 1].Order +
                    ") 应在 " + contributors[i].Name + " (Order=" + contributors[i].Order + ") 之前");
            }
        }

        [Fact]
        public void AllContributors_HaveNonEmptyNames()
        {
            var contributors = new ISeedContributor[]
            {
                new YTStdTenantPlatform.Infrastructure.Initialization.Contributors.PlatformUserSeedContributor(),
                new YTStdTenantPlatform.Infrastructure.Initialization.Contributors.SecurityPolicySeedContributor(),
                new YTStdTenantPlatform.Infrastructure.Initialization.Contributors.PermissionSeedContributor(),
                new YTStdTenantPlatform.Infrastructure.Initialization.Contributors.RoleSeedContributor(),
                new YTStdTenantPlatform.Infrastructure.Initialization.Contributors.PackageSeedContributor(),
                new YTStdTenantPlatform.Infrastructure.Initialization.Contributors.NotificationTemplateSeedContributor(),
                new YTStdTenantPlatform.Infrastructure.Initialization.Contributors.InfrastructureSeedContributor(),
                new YTStdTenantPlatform.Infrastructure.Initialization.Contributors.DemoDataSeedContributor()
            };

            foreach (var c in contributors)
            {
                Assert.False(string.IsNullOrEmpty(c.Name),
                    "贡献者类型 " + c.GetType().Name + " 应有非空名称");
            }
        }
    }
}
