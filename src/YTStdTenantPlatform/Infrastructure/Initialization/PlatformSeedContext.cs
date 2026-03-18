using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YTStdAdo;

namespace YTStdTenantPlatform.Infrastructure.Initialization;

/// <summary>平台种子数据上下文，用于在贡献者之间共享 ID 映射</summary>
public sealed class PlatformSeedContext
{
    /// <summary>平台级 tenantId（平台表不分租户，使用 0）</summary>
    public int TenantId { get; } = 0;

    /// <summary>初始化操作使用的系统 userId</summary>
    public long SystemUserId { get; } = 0;

    /// <summary>权限 Code → Id 映射（由 PermissionSeedContributor 填充）</summary>
    public Dictionary<string, long> PermissionIdMap { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>角色 Code → Id 映射（由 RoleSeedContributor 填充）</summary>
    public Dictionary<string, long> RoleIdMap { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>用户 Username → Id 映射（由 PlatformUserSeedContributor 填充）</summary>
    public Dictionary<string, long> UserIdMap { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>套餐 PackageCode → Id 映射</summary>
    public Dictionary<string, long> PackageIdMap { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>套餐版本 VersionCode → Id 映射</summary>
    public Dictionary<string, long> PackageVersionIdMap { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>密码策略 PolicyName → Id 映射</summary>
    public Dictionary<string, long> PasswordPolicyIdMap { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>初始化过程日志记录</summary>
    public List<string> Logs { get; } = new();

    /// <summary>记录初始化日志</summary>
    public void Log(string message)
    {
        Logs.Add($"[{DateTime.UtcNow:HH:mm:ss.fff}] {message}");
    }

    /// <summary>为初始化业务显式分配 long 主键</summary>
    public async ValueTask<long> GetNextLongIdAsync()
    {
        return await DB.GetNextLongIdAsync();
    }
}
