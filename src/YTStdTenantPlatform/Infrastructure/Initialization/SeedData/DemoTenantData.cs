using System;
using System.Collections.Generic;
using YTStdTenantPlatform.Entity.TenantPlatform;

namespace YTStdTenantPlatform.Infrastructure.Initialization.SeedData
{
    /// <summary>
    /// 演示租户种子数据。
    /// 仅用于开发和演示环境，不应在生产环境中使用。
    /// </summary>
    public static class DemoTenantData
    {
        /// <summary>获取演示租户列表</summary>
        public static IReadOnlyList<Tenant> GetDemoTenants()
        {
            var now = DateTime.UtcNow;
            return new[]
            {
                new Tenant
                {
                    TenantCode = "DEMO001",
                    TenantName = "演示租户",
                    EnterpriseName = "演示企业有限公司",
                    ContactName = "张三",
                    ContactPhone = "13800000000",
                    ContactEmail = "demo@example.com",
                    SourceType = "platform",
                    LifecycleStatus = "active",
                    DefaultLanguage = "zh-CN",
                    DefaultTimezone = "Asia/Shanghai",
                    IsolationMode = "shared_database",
                    Enabled = true,
                    OpenedAt = now,
                    ActivatedAt = now,
                    CreatedAt = now,
                    UpdatedAt = now
                }
            };
        }

        /// <summary>
        /// 获取演示租户域名列表。
        /// 域名通过 TenantCode 关联租户，由调用方在运行时解析实际 TenantRefId。
        /// </summary>
        public static IReadOnlyList<DemoTenantDomainSeed> GetDemoTenantDomains()
        {
            var now = DateTime.UtcNow;
            return new[]
            {
                new DemoTenantDomainSeed
                {
                    TenantCode = "DEMO001",
                    Domain = new TenantDomain
                    {
                        Domain = "demo.platform.local",
                        DomainType = "subdomain",
                        IsPrimary = true,
                        VerificationStatus = "verified",
                        VerifiedAt = now,
                        CreatedAt = now
                    }
                }
            };
        }

        /// <summary>演示租户域名种子数据，包含租户编码用于关联</summary>
        public class DemoTenantDomainSeed
        {
            /// <summary>所属租户编码</summary>
            public string TenantCode { get; set; } = "";

            /// <summary>域名实体</summary>
            public TenantDomain Domain { get; set; } = null!;
        }
    }
}
