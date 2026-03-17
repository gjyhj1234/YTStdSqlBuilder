using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using YTStdTenantPlatform.Entity.TenantPlatform;

namespace YTStdTenantPlatform.Infrastructure.Initialization.SeedData
{
    /// <summary>默认平台用户种子数据</summary>
    public static class DefaultPlatformUsers
    {
        /// <summary>获取默认平台管理员列表（密码使用安全随机值，首次登录必须重置）</summary>
        public static IReadOnlyList<PlatformUser> GetDefaultUsers()
        {
            var now = DateTime.UtcNow;
            // 生成密码学安全的随机哈希，确保初始密码不可猜测
            var randomHash = GenerateSecureRandomHash();
            var randomSalt = GenerateSecureRandomHash();
            return new[]
            {
                new PlatformUser
                {
                    Username = "admin",
                    Email = "admin@platform.local",
                    DisplayName = "超级管理员",
                    PasswordHash = randomHash,
                    PasswordSalt = randomSalt,
                    Status = "active",
                    MfaEnabled = false,
                    FailedLoginCount = 0,
                    // 设置密码立即过期，强制首次登录重置
                    PasswordExpiresAt = now,
                    Remark = "系统初始化创建的默认超级管理员，首次登录需重置密码",
                    CreatedAt = now,
                    UpdatedAt = now
                }
            };
        }

        /// <summary>生成密码学安全的随机哈希值</summary>
        private static string GenerateSecureRandomHash()
        {
            var bytes = new byte[32];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}
