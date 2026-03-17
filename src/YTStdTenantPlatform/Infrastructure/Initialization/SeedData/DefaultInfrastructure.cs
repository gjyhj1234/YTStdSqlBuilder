using System;
using System.Collections.Generic;
using YTStdTenantPlatform.Entity.TenantPlatform;

namespace YTStdTenantPlatform.Infrastructure.Initialization.SeedData
{
    /// <summary>默认基础设施种子数据</summary>
    public static class DefaultInfrastructure
    {
        /// <summary>获取默认限流策略列表</summary>
        public static IReadOnlyList<RateLimitPolicy> GetDefaultRateLimitPolicies()
        {
            var now = DateTime.UtcNow;
            return new[]
            {
                new RateLimitPolicy
                {
                    SubjectType = "api",
                    SubjectKey = "global",
                    WindowSeconds = 60,
                    LimitCount = 1000,
                    Status = "active",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new RateLimitPolicy
                {
                    SubjectType = "tenant",
                    SubjectKey = "default",
                    WindowSeconds = 60,
                    LimitCount = 100,
                    Status = "active",
                    CreatedAt = now,
                    UpdatedAt = now
                }
            };
        }

        /// <summary>获取默认数据隔离策略列表</summary>
        public static IReadOnlyList<DataIsolationPolicy> GetDefaultDataIsolationPolicies()
        {
            var now = DateTime.UtcNow;
            return new[]
            {
                new DataIsolationPolicy
                {
                    IsolationType = "tenant_isolation",
                    PolicyName = "默认租户隔离策略",
                    PolicyConfig = "{\"mode\":\"shared_database\",\"row_level_security\":true}",
                    Status = "active",
                    CreatedAt = now,
                    UpdatedAt = now
                }
            };
        }

        /// <summary>获取默认基础设施组件列表</summary>
        public static IReadOnlyList<InfrastructureComponent> GetDefaultComponents()
        {
            var now = DateTime.UtcNow;
            return new[]
            {
                new InfrastructureComponent
                {
                    ComponentType = "cache",
                    ComponentName = "本地缓存",
                    Status = "active",
                    ComponentConfig = "{\"provider\":\"memory\",\"default_ttl_seconds\":300}",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new InfrastructureComponent
                {
                    ComponentType = "scheduler",
                    ComponentName = "任务调度器",
                    Status = "active",
                    ComponentConfig = "{\"provider\":\"built_in\",\"max_concurrent_jobs\":10}",
                    CreatedAt = now,
                    UpdatedAt = now
                }
            };
        }
    }
}
