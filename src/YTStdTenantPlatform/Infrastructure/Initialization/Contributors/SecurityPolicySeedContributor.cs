using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YTStdAdo;
using YTStdTenantPlatform.Entity.TenantPlatform;
using YTStdTenantPlatform.Infrastructure.Initialization.SeedData;

namespace YTStdTenantPlatform.Infrastructure.Initialization.Contributors
{
    /// <summary>安全策略种子数据贡献者</summary>
    public sealed class SecurityPolicySeedContributor : ISeedContributor
    {
        /// <summary>贡献者名称</summary>
        public string Name => "SecurityPolicy";

        /// <summary>执行顺序</summary>
        public int Order => 20;

        /// <summary>执行幂等初始化</summary>
        public async ValueTask SeedAsync(PlatformSeedContext context)
        {
            int tid = context.TenantId;
            long uid = context.SystemUserId;

            // ── 密码策略 ──
            var (pwResult, existingPwPolicies) = await PlatformPasswordPolicyCRUD.GetListAsync(tid, uid);
            var existingPwMap = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            if (pwResult.Success && existingPwPolicies != null)
            {
                foreach (var p in existingPwPolicies)
                {
                    if (!string.IsNullOrEmpty(p.PolicyName))
                    {
                        existingPwMap[p.PolicyName] = p.Id;
                    }
                }
            }

            var defaultPwPolicies = DefaultSecurityPolicies.GetDefaultPasswordPolicies();
            foreach (var policy in defaultPwPolicies)
            {
                if (existingPwMap.TryGetValue(policy.PolicyName, out long existingId))
                {
                    context.PasswordPolicyIdMap[policy.PolicyName] = existingId;
                    context.Log("[SecurityPolicy] 密码策略已存在，跳过: " + policy.PolicyName);
                    continue;
                }

                DbInsResult ins = await PlatformPasswordPolicyCRUD.InsertAsync(tid, uid, policy);
                if (ins.Success)
                {
                    context.PasswordPolicyIdMap[policy.PolicyName] = ins.Id;
                    context.Log("[SecurityPolicy] 插入密码策略: " + policy.PolicyName + " => Id=" + ins.Id);
                }
                else
                {
                    context.Log("[SecurityPolicy] 插入密码策略失败: " + policy.PolicyName + ", 错误: " + ins.ErrorMessage);
                }
            }

            // ── 安全策略 ──
            var (secResult, existingSecPolicies) = await PlatformSecurityPolicyCRUD.GetListAsync(tid, uid);
            var existingSecMap = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            if (secResult.Success && existingSecPolicies != null)
            {
                foreach (var s in existingSecPolicies)
                {
                    if (!string.IsNullOrEmpty(s.PolicyName))
                    {
                        existingSecMap[s.PolicyName] = s.Id;
                    }
                }
            }

            var defaultSecPolicies = DefaultSecurityPolicies.GetDefaultSecurityPolicies();
            foreach (var policy in defaultSecPolicies)
            {
                if (existingSecMap.ContainsKey(policy.PolicyName))
                {
                    context.Log("[SecurityPolicy] 安全策略已存在，跳过: " + policy.PolicyName);
                    continue;
                }

                DbInsResult ins = await PlatformSecurityPolicyCRUD.InsertAsync(tid, uid, policy);
                if (ins.Success)
                {
                    context.Log("[SecurityPolicy] 插入安全策略: " + policy.PolicyName + " => Id=" + ins.Id);
                }
                else
                {
                    context.Log("[SecurityPolicy] 插入安全策略失败: " + policy.PolicyName + ", 错误: " + ins.ErrorMessage);
                }
            }
        }
    }
}
