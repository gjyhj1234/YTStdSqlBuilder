using System;
using System.Collections.Generic;
using YTStdTenantPlatform.Entity.TenantPlatform;

namespace YTStdTenantPlatform.Infrastructure.Initialization.SeedData
{
    /// <summary>默认 SaaS 套餐种子数据</summary>
    public static class DefaultPackages
    {
        /// <summary>获取默认套餐列表</summary>
        public static IReadOnlyList<SaasPackage> GetDefaultPackages()
        {
            var now = DateTime.UtcNow;
            return new[]
            {
                new SaasPackage
                {
                    PackageCode = "free",
                    PackageName = "免费版",
                    Description = "适合个人或小团队的免费入门套餐",
                    Status = "active",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new SaasPackage
                {
                    PackageCode = "standard",
                    PackageName = "标准版",
                    Description = "适合中小型企业的标准套餐",
                    Status = "active",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new SaasPackage
                {
                    PackageCode = "enterprise",
                    PackageName = "企业版",
                    Description = "适合大型企业的高级套餐，提供全量功能和专属支持",
                    Status = "active",
                    CreatedAt = now,
                    UpdatedAt = now
                }
            };
        }

        /// <summary>
        /// 获取默认套餐版本列表。
        /// 版本通过 PackageCode 关联套餐，由调用方在运行时解析实际 PackageId。
        /// </summary>
        public static IReadOnlyList<PackageVersionSeed> GetDefaultVersions()
        {
            var now = DateTime.UtcNow;
            return new[]
            {
                new PackageVersionSeed
                {
                    PackageCode = "free",
                    Version = new SaasPackageVersion
                    {
                        VersionCode = "free_v1",
                        VersionName = "免费版 V1",
                        EditionType = "free",
                        BillingCycle = "monthly",
                        Price = 0m,
                        CurrencyCode = "CNY",
                        TrialDays = 0,
                        IsDefault = true,
                        Enabled = true,
                        CreatedAt = now,
                        UpdatedAt = now
                    }
                },
                new PackageVersionSeed
                {
                    PackageCode = "standard",
                    Version = new SaasPackageVersion
                    {
                        VersionCode = "standard_v1",
                        VersionName = "标准版 V1",
                        EditionType = "standard",
                        BillingCycle = "monthly",
                        Price = 299m,
                        CurrencyCode = "CNY",
                        TrialDays = 14,
                        IsDefault = true,
                        Enabled = true,
                        CreatedAt = now,
                        UpdatedAt = now
                    }
                },
                new PackageVersionSeed
                {
                    PackageCode = "enterprise",
                    Version = new SaasPackageVersion
                    {
                        VersionCode = "enterprise_v1",
                        VersionName = "企业版 V1",
                        EditionType = "enterprise",
                        BillingCycle = "monthly",
                        Price = 999m,
                        CurrencyCode = "CNY",
                        TrialDays = 14,
                        IsDefault = true,
                        Enabled = true,
                        CreatedAt = now,
                        UpdatedAt = now
                    }
                }
            };
        }

        /// <summary>
        /// 获取默认套餐能力列表。
        /// 能力通过 VersionCode 关联版本，由调用方在运行时解析实际 PackageVersionId。
        /// </summary>
        public static IReadOnlyList<PackageCapabilitySeed> GetDefaultCapabilities()
        {
            var now = DateTime.UtcNow;
            return new[]
            {
                // ── 免费版能力 ──
                Cap("free_v1", "user_limit", "用户数量上限", "quota", "{\"limit\":5}", now),
                Cap("free_v1", "storage_limit", "存储空间上限", "quota", "{\"limit\":\"1GB\"}", now),
                Cap("free_v1", "api_limit", "API 调用上限", "quota", "{\"limit\":1000,\"period\":\"day\"}", now),

                // ── 标准版能力 ──
                Cap("standard_v1", "user_limit", "用户数量上限", "quota", "{\"limit\":50}", now),
                Cap("standard_v1", "storage_limit", "存储空间上限", "quota", "{\"limit\":\"10GB\"}", now),
                Cap("standard_v1", "api_limit", "API 调用上限", "quota", "{\"limit\":10000,\"period\":\"day\"}", now),
                Cap("standard_v1", "feature_basic", "基础功能集", "feature", "{\"scope\":\"all_basic\"}", now),

                // ── 企业版能力 ──
                Cap("enterprise_v1", "user_limit", "用户数量上限", "quota", "{\"limit\":\"unlimited\"}", now),
                Cap("enterprise_v1", "storage_limit", "存储空间上限", "quota", "{\"limit\":\"100GB\"}", now),
                Cap("enterprise_v1", "api_limit", "API 调用上限", "quota", "{\"limit\":\"unlimited\",\"period\":\"day\"}", now),
                Cap("enterprise_v1", "feature_all", "全量功能集", "feature", "{\"scope\":\"all\"}", now),
                Cap("enterprise_v1", "concurrency_limit", "并发连接上限", "quota", "{\"limit\":100}", now)
            };
        }

        /// <summary>创建能力种子数据辅助方法</summary>
        private static PackageCapabilitySeed Cap(
            string versionCode, string key, string name, string type, string value, DateTime now)
        {
            return new PackageCapabilitySeed
            {
                VersionCode = versionCode,
                Capability = new SaasPackageCapability
                {
                    CapabilityKey = key,
                    CapabilityName = name,
                    CapabilityType = type,
                    CapabilityValue = value,
                    CreatedAt = now,
                    UpdatedAt = now
                }
            };
        }

        /// <summary>套餐版本种子数据，包含套餐编码用于关联</summary>
        public class PackageVersionSeed
        {
            /// <summary>所属套餐编码</summary>
            public string PackageCode { get; set; } = "";

            /// <summary>版本实体</summary>
            public SaasPackageVersion Version { get; set; } = null!;
        }

        /// <summary>套餐能力种子数据，包含版本编码用于关联</summary>
        public class PackageCapabilitySeed
        {
            /// <summary>所属版本编码</summary>
            public string VersionCode { get; set; } = "";

            /// <summary>能力实体</summary>
            public SaasPackageCapability Capability { get; set; } = null!;
        }
    }
}
