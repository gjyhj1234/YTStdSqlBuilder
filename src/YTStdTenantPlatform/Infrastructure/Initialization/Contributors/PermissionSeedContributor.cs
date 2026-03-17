using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YTStdAdo;
using YTStdTenantPlatform.Entity.TenantPlatform;
using YTStdTenantPlatform.Infrastructure.Initialization.SeedData;

namespace YTStdTenantPlatform.Infrastructure.Initialization.Contributors
{
    /// <summary>权限种子数据贡献者</summary>
    public sealed class PermissionSeedContributor : ISeedContributor
    {
        /// <summary>贡献者名称</summary>
        public string Name => "Permission";

        /// <summary>执行顺序</summary>
        public int Order => 30;

        /// <summary>执行幂等初始化</summary>
        public async ValueTask SeedAsync(PlatformSeedContext context)
        {
            int tid = context.TenantId;
            long uid = context.SystemUserId;

            var (result, existingPermissions) = await PlatformPermissionCRUD.GetListAsync(tid, uid);
            var existingMap = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            if (result.Success && existingPermissions != null)
            {
                foreach (var p in existingPermissions)
                {
                    if (!string.IsNullOrEmpty(p.Code))
                    {
                        existingMap[p.Code] = p.Id;
                    }
                }
            }

            // 将已存在的权限预填到 context 中
            foreach (var kvp in existingMap)
            {
                context.PermissionIdMap[kvp.Key] = kvp.Value;
            }

            var allSeeds = DefaultPermissions.GetDefaultPermissions();

            // 按顺序插入（列表已按层级排列：先父级后子级）
            foreach (var seed in allSeeds)
            {
                var perm = seed.Permission;
                if (existingMap.ContainsKey(perm.Code))
                {
                    context.Log("[Permission] 权限已存在，跳过: " + perm.Code);
                    continue;
                }

                // 解析父级 ID（从 PermissionSeed.ParentCode 获取，不修改实体的 Resource 字段）
                if (!string.IsNullOrEmpty(seed.ParentCode))
                {
                    if (context.PermissionIdMap.TryGetValue(seed.ParentCode, out long parentId))
                    {
                        perm.ParentId = parentId;
                    }
                    else
                    {
                        context.Log("[Permission] 警告: 父级权限未找到: " + seed.ParentCode + " (权限: " + perm.Code + ")");
                    }
                }

                DbInsResult ins = await PlatformPermissionCRUD.InsertAsync(tid, uid, perm);
                if (ins.Success)
                {
                    context.PermissionIdMap[perm.Code] = ins.Id;
                    context.Log("[Permission] 插入权限: " + perm.Code + " => Id=" + ins.Id);
                }
                else
                {
                    context.Log("[Permission] 插入权限失败: " + perm.Code + ", 错误: " + ins.ErrorMessage);
                }
            }

            context.Log("[Permission] 共处理权限: " + allSeeds.Count + ", 映射数: " + context.PermissionIdMap.Count);
        }
    }
}
