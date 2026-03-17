using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YTStdLogger.Core;

namespace YTStdTenantPlatform.Infrastructure.Initialization;

/// <summary>种子数据执行器，按顺序运行所有贡献者</summary>
public static class SeedRunner
{
    /// <summary>执行所有种子贡献者的幂等初始化</summary>
    public static async ValueTask RunAsync(IReadOnlyList<ISeedContributor> contributors, PlatformSeedContext context)
    {
        context.Log("=== 平台初始化开始 ===");
        Logger.Info(context.TenantId, context.SystemUserId, "[SeedRunner] 开始执行初始化");

        foreach (var contributor in contributors)
        {
            context.Log($"[{contributor.Name}] 开始 (Order={contributor.Order})");
            Logger.Info(context.TenantId, context.SystemUserId,
                "[SeedRunner] 执行贡献者: " + contributor.Name + " (Order=" + contributor.Order + ")");

            try
            {
                await contributor.SeedAsync(context);
                context.Log($"[{contributor.Name}] 完成");
            }
            catch (Exception ex)
            {
                context.Log($"[{contributor.Name}] 失败: {ex.Message}");
                Logger.Error(context.TenantId, context.SystemUserId,
                    "[SeedRunner] 贡献者执行失败: " + contributor.Name + ", 错误: " + ex.Message);
                throw;
            }
        }

        context.Log("=== 平台初始化完成 ===");
        Logger.Info(context.TenantId, context.SystemUserId, "[SeedRunner] 初始化完成");
    }
}
