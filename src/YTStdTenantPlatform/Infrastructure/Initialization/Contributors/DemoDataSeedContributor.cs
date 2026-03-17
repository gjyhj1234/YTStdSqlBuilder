using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YTStdAdo;
using YTStdTenantPlatform.Entity.TenantPlatform;
using YTStdTenantPlatform.Infrastructure.Initialization.SeedData;

namespace YTStdTenantPlatform.Infrastructure.Initialization.Contributors
{
    /// <summary>
    /// 演示数据种子贡献者。
    /// 仅用于开发和演示环境，不应在生产环境中使用。
    /// </summary>
    public sealed class DemoDataSeedContributor : ISeedContributor
    {
        /// <summary>贡献者名称</summary>
        public string Name => "DemoData";

        /// <summary>执行顺序</summary>
        public int Order => 100;

        /// <summary>执行幂等初始化（演示数据，非生产环境使用）</summary>
        public async ValueTask SeedAsync(PlatformSeedContext context)
        {
            int tid = context.TenantId;
            long uid = context.SystemUserId;

            context.Log("[DemoData] ⚠ 开始插入演示数据（仅限开发/演示环境）");

            // ── 1. 插入演示租户 ──
            var (tenantResult, existingTenants) = await TenantCRUD.GetListAsync(tid, uid);
            var existingTenantMap = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            if (tenantResult.Success && existingTenants != null)
            {
                foreach (var t in existingTenants)
                {
                    if (!string.IsNullOrEmpty(t.TenantCode))
                    {
                        existingTenantMap[t.TenantCode] = t.Id;
                    }
                }
            }

            var demoTenants = DemoTenantData.GetDemoTenants();
            foreach (var tenant in demoTenants)
            {
                if (existingTenantMap.TryGetValue(tenant.TenantCode, out long existingId))
                {
                    context.Log("[DemoData] 演示租户已存在，跳过: " + tenant.TenantCode);

                    // 确保后续域名操作能使用租户 ID
                    existingTenantMap[tenant.TenantCode] = existingId;
                    continue;
                }

                DbInsResult ins = await TenantCRUD.InsertAsync(tid, uid, tenant);
                if (ins.Success)
                {
                    existingTenantMap[tenant.TenantCode] = ins.Id;
                    context.Log("[DemoData] 插入演示租户: " + tenant.TenantCode + " => Id=" + ins.Id);
                }
                else
                {
                    context.Log("[DemoData] 插入演示租户失败: " + tenant.TenantCode + ", 错误: " + ins.ErrorMessage);
                }
            }

            // ── 2. 插入演示租户域名 ──
            var (domainResult, existingDomains) = await TenantDomainCRUD.GetListAsync(tid, uid);
            var existingDomainSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (domainResult.Success && existingDomains != null)
            {
                foreach (var d in existingDomains)
                {
                    if (!string.IsNullOrEmpty(d.Domain))
                    {
                        existingDomainSet.Add(d.Domain);
                    }
                }
            }

            var demoDomains = DemoTenantData.GetDemoTenantDomains();
            foreach (var dSeed in demoDomains)
            {
                if (existingDomainSet.Contains(dSeed.Domain.Domain))
                {
                    context.Log("[DemoData] 演示域名已存在，跳过: " + dSeed.Domain.Domain);
                    continue;
                }

                if (!existingTenantMap.TryGetValue(dSeed.TenantCode, out long tenantId))
                {
                    context.Log("[DemoData] 域名关联的租户未找到: " + dSeed.TenantCode);
                    continue;
                }

                dSeed.Domain.TenantRefId = tenantId;
                DbInsResult ins = await TenantDomainCRUD.InsertAsync(tid, uid, dSeed.Domain);
                if (ins.Success)
                {
                    context.Log("[DemoData] 插入演示域名: " + dSeed.Domain.Domain + " => Id=" + ins.Id);
                }
                else
                {
                    context.Log("[DemoData] 插入演示域名失败: " + dSeed.Domain.Domain + ", 错误: " + ins.ErrorMessage);
                }
            }

            context.Log("[DemoData] ⚠ 演示数据插入完成");
        }
    }
}
