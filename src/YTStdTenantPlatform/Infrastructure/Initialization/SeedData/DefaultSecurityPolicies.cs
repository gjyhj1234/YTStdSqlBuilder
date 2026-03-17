using System;
using System.Collections.Generic;
using YTStdTenantPlatform.Entity.TenantPlatform;

namespace YTStdTenantPlatform.Infrastructure.Initialization.SeedData
{
    /// <summary>默认安全策略种子数据</summary>
    public static class DefaultSecurityPolicies
    {
        /// <summary>获取默认密码策略列表</summary>
        public static IReadOnlyList<PlatformPasswordPolicy> GetDefaultPasswordPolicies()
        {
            var now = DateTime.UtcNow;
            return new[]
            {
                new PlatformPasswordPolicy
                {
                    PolicyName = "默认密码策略",
                    MinLength = 8,
                    MaxLength = 64,
                    RequireUppercase = true,
                    RequireLowercase = true,
                    RequireNumber = true,
                    RequireSpecial = false,
                    PasswordExpireDays = 90,
                    PreventReuseCount = 5,
                    LoginFailLockThreshold = 5,
                    LockDurationMinutes = 30,
                    IsDefault = true,
                    CreatedAt = now,
                    UpdatedAt = now
                }
            };
        }

        /// <summary>获取默认安全策略列表</summary>
        public static IReadOnlyList<PlatformSecurityPolicy> GetDefaultSecurityPolicies()
        {
            var now = DateTime.UtcNow;
            return new[]
            {
                new PlatformSecurityPolicy
                {
                    PolicyName = "默认安全策略",
                    IpWhitelistRequired = false,
                    MfaRequired = false,
                    SessionTimeoutMinutes = 30,
                    IsDefault = true,
                    CreatedAt = now,
                    UpdatedAt = now
                }
            };
        }
    }
}
