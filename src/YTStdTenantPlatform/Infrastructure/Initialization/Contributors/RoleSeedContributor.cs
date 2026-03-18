using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YTStdAdo;
using YTStdTenantPlatform.Entity.TenantPlatform;
using YTStdTenantPlatform.Infrastructure.Initialization.SeedData;

namespace YTStdTenantPlatform.Infrastructure.Initialization.Contributors
{
    /// <summary>角色种子数据贡献者</summary>
    public sealed class RoleSeedContributor : ISeedContributor
    {
        /// <summary>贡献者名称</summary>
        public string Name => "Role";

        /// <summary>执行顺序</summary>
        public int Order => 40;

        /// <summary>执行幂等初始化</summary>
        public async ValueTask SeedAsync(PlatformSeedContext context)
        {
            int tid = context.TenantId;
            long uid = context.SystemUserId;

            // ── 1. 插入角色 ──
            var (roleResult, existingRoles) = await PlatformRoleCRUD.GetListAsync(tid, uid);
            var existingRoleMap = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            if (roleResult.Success && existingRoles != null)
            {
                foreach (var r in existingRoles)
                {
                    if (!string.IsNullOrEmpty(r.Code))
                    {
                        existingRoleMap[r.Code] = r.Id;
                    }
                }
            }

            var defaultRoles = DefaultRoles.GetDefaultRoles();
            foreach (var role in defaultRoles)
            {
                if (existingRoleMap.TryGetValue(role.Code, out long existingId))
                {
                    context.RoleIdMap[role.Code] = existingId;
                    context.Log("[Role] 角色已存在，跳过: " + role.Code);
                    continue;
                }

                role.Id = await context.GetNextLongIdAsync();
                DbInsResult ins = await PlatformRoleCRUD.InsertAsync(tid, uid, role);
                if (ins.Success)
                {
                    context.RoleIdMap[role.Code] = ins.Id;
                    context.Log("[Role] 插入角色: " + role.Code + " => Id=" + ins.Id);
                }
                else
                {
                    context.Log("[Role] 插入角色失败: " + role.Code + ", 错误: " + ins.ErrorMessage);
                }
            }

            // ── 2. 绑定角色权限 ──
            var allPermissions = DefaultPermissions.GetDefaultPermissions();
            var bindings = DefaultRoles.GetRolePermissionBindings(allPermissions);

            var (rpResult, existingRPs) = await PlatformRolePermissionCRUD.GetListAsync(tid, uid);
            var existingRPSet = new HashSet<string>(StringComparer.Ordinal);
            if (rpResult.Success && existingRPs != null)
            {
                foreach (var rp in existingRPs)
                {
                    existingRPSet.Add(rp.RoleId + ":" + rp.PermissionId);
                }
            }

            foreach (var binding in bindings)
            {
                if (!context.RoleIdMap.TryGetValue(binding.RoleCode, out long roleId))
                {
                    continue;
                }
                if (!context.PermissionIdMap.TryGetValue(binding.PermissionCode, out long permId))
                {
                    continue;
                }

                string key = roleId + ":" + permId;
                if (existingRPSet.Contains(key))
                {
                    continue;
                }

                var rpEntity = new PlatformRolePermission
                {
                    Id = await context.GetNextLongIdAsync(),
                    RoleId = roleId,
                    PermissionId = permId,
                    GrantedBy = uid,
                    GrantedAt = DateTime.UtcNow
                };

                DbInsResult ins = await PlatformRolePermissionCRUD.InsertAsync(tid, uid, rpEntity);
                if (ins.Success)
                {
                    existingRPSet.Add(key);
                }
                else
                {
                    context.Log("[Role] 绑定角色权限失败: " + binding.RoleCode + " -> " + binding.PermissionCode + ", 错误: " + ins.ErrorMessage);
                }
            }

            context.Log("[Role] 角色权限绑定完成");

            // ── 3. 绑定角色成员 ──
            var memberBindings = DefaultRoles.GetRoleMemberBindings();

            var (rmResult, existingRMs) = await PlatformRoleMemberCRUD.GetListAsync(tid, uid);
            var existingRMSet = new HashSet<string>(StringComparer.Ordinal);
            if (rmResult.Success && existingRMs != null)
            {
                foreach (var rm in existingRMs)
                {
                    existingRMSet.Add(rm.RoleId + ":" + rm.UserId);
                }
            }

            foreach (var mb in memberBindings)
            {
                if (!context.RoleIdMap.TryGetValue(mb.RoleCode, out long roleId))
                {
                    context.Log("[Role] 角色成员绑定跳过，角色未找到: " + mb.RoleCode);
                    continue;
                }
                if (!context.UserIdMap.TryGetValue(mb.Username, out long userId))
                {
                    context.Log("[Role] 角色成员绑定跳过，用户未找到: " + mb.Username);
                    continue;
                }

                string key = roleId + ":" + userId;
                if (existingRMSet.Contains(key))
                {
                    context.Log("[Role] 角色成员已绑定，跳过: " + mb.RoleCode + " -> " + mb.Username);
                    continue;
                }

                var rmEntity = new PlatformRoleMember
                {
                    Id = await context.GetNextLongIdAsync(),
                    RoleId = roleId,
                    UserId = userId,
                    AssignedBy = uid,
                    AssignedAt = DateTime.UtcNow
                };

                DbInsResult ins = await PlatformRoleMemberCRUD.InsertAsync(tid, uid, rmEntity);
                if (ins.Success)
                {
                    existingRMSet.Add(key);
                    context.Log("[Role] 绑定角色成员: " + mb.RoleCode + " -> " + mb.Username);
                }
                else
                {
                    context.Log("[Role] 绑定角色成员失败: " + mb.RoleCode + " -> " + mb.Username + ", 错误: " + ins.ErrorMessage);
                }
            }
        }
    }
}
