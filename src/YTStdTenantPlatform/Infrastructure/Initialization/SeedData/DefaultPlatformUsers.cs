using System;
using System.Collections.Generic;
using YTStdTenantPlatform.Entity.TenantPlatform;

namespace YTStdTenantPlatform.Infrastructure.Initialization.SeedData
{
    /// <summary>默认平台用户种子数据</summary>
    public static class DefaultPlatformUsers
    {
        /// <summary>获取默认平台管理员列表</summary>
        public static IReadOnlyList<PlatformUser> GetDefaultUsers()
        {
            var now = DateTime.UtcNow;
            return new[]
            {
                new PlatformUser
                {
                    Username = "admin",
                    Email = "admin@platform.local",
                    DisplayName = "超级管理员",
                    // 占位值，SeedContributor 在写入前必须替换为真实哈希并强制首次登录重置密码
                    PasswordHash = "INIT_HASH_PLACEHOLDER",
                    PasswordSalt = "INIT_SALT_PLACEHOLDER",
                    Status = "active",
                    MfaEnabled = false,
                    FailedLoginCount = 0,
                    Remark = "系统初始化创建的默认超级管理员",
                    CreatedAt = now,
                    UpdatedAt = now
                }
            };
        }
    }
}
