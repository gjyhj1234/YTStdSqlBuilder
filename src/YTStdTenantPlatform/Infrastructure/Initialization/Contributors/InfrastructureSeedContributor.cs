using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YTStdAdo;
using YTStdTenantPlatform.Entity.TenantPlatform;
using YTStdTenantPlatform.Infrastructure.Initialization.SeedData;

namespace YTStdTenantPlatform.Infrastructure.Initialization.Contributors
{
    /// <summary>基础设施种子数据贡献者</summary>
    public sealed class InfrastructureSeedContributor : ISeedContributor
    {
        /// <summary>贡献者名称</summary>
        public string Name => "Infrastructure";

        /// <summary>执行顺序</summary>
        public int Order => 70;

        /// <summary>执行幂等初始化</summary>
        public async ValueTask SeedAsync(PlatformSeedContext context)
        {
            int tid = context.TenantId;
            long uid = context.SystemUserId;

            // ── 1. 限流策略 ──
            var (rlResult, existingRLs) = await RateLimitPolicyCRUD.GetListAsync(tid, uid);
            var existingRLSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (rlResult.Success && existingRLs != null)
            {
                foreach (var rl in existingRLs)
                {
                    if (!string.IsNullOrEmpty(rl.SubjectType) && !string.IsNullOrEmpty(rl.SubjectKey))
                    {
                        existingRLSet.Add(rl.SubjectType + ":" + rl.SubjectKey);
                    }
                }
            }

            var defaultRLs = DefaultInfrastructure.GetDefaultRateLimitPolicies();
            foreach (var rl in defaultRLs)
            {
                string key = rl.SubjectType + ":" + rl.SubjectKey;
                if (existingRLSet.Contains(key))
                {
                    context.Log("[Infrastructure] 限流策略已存在，跳过: " + key);
                    continue;
                }

                DbInsResult ins = await RateLimitPolicyCRUD.InsertAsync(tid, uid, rl);
                if (ins.Success)
                {
                    context.Log("[Infrastructure] 插入限流策略: " + key + " => Id=" + ins.Id);
                }
                else
                {
                    context.Log("[Infrastructure] 插入限流策略失败: " + key + ", 错误: " + ins.ErrorMessage);
                }
            }

            // ── 2. 数据隔离策略 ──
            var (diResult, existingDIs) = await DataIsolationPolicyCRUD.GetListAsync(tid, uid);
            var existingDISet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (diResult.Success && existingDIs != null)
            {
                foreach (var di in existingDIs)
                {
                    if (!string.IsNullOrEmpty(di.IsolationType) && !string.IsNullOrEmpty(di.PolicyName))
                    {
                        existingDISet.Add(di.IsolationType + ":" + di.PolicyName);
                    }
                }
            }

            var defaultDIs = DefaultInfrastructure.GetDefaultDataIsolationPolicies();
            foreach (var di in defaultDIs)
            {
                string key = di.IsolationType + ":" + di.PolicyName;
                if (existingDISet.Contains(key))
                {
                    context.Log("[Infrastructure] 隔离策略已存在，跳过: " + key);
                    continue;
                }

                DbInsResult ins = await DataIsolationPolicyCRUD.InsertAsync(tid, uid, di);
                if (ins.Success)
                {
                    context.Log("[Infrastructure] 插入隔离策略: " + key + " => Id=" + ins.Id);
                }
                else
                {
                    context.Log("[Infrastructure] 插入隔离策略失败: " + key + ", 错误: " + ins.ErrorMessage);
                }
            }

            // ── 3. 基础设施组件 ──
            var (compResult, existingComps) = await InfrastructureComponentCRUD.GetListAsync(tid, uid);
            var existingCompSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (compResult.Success && existingComps != null)
            {
                foreach (var c in existingComps)
                {
                    if (!string.IsNullOrEmpty(c.ComponentType) && !string.IsNullOrEmpty(c.ComponentName))
                    {
                        existingCompSet.Add(c.ComponentType + ":" + c.ComponentName);
                    }
                }
            }

            var defaultComps = DefaultInfrastructure.GetDefaultComponents();
            foreach (var comp in defaultComps)
            {
                string key = comp.ComponentType + ":" + comp.ComponentName;
                if (existingCompSet.Contains(key))
                {
                    context.Log("[Infrastructure] 组件已存在，跳过: " + key);
                    continue;
                }

                DbInsResult ins = await InfrastructureComponentCRUD.InsertAsync(tid, uid, comp);
                if (ins.Success)
                {
                    context.Log("[Infrastructure] 插入组件: " + key + " => Id=" + ins.Id);
                }
                else
                {
                    context.Log("[Infrastructure] 插入组件失败: " + key + ", 错误: " + ins.ErrorMessage);
                }
            }
        }
    }
}
