using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YTStdLogger.Core;
using YTStdTenantPlatform.Entity.TenantPlatform;

namespace YTStdTenantPlatform.Infrastructure.Cache
{
    /// <summary>平台缓存预热器，在初始化完成后加载核心数据到进程内缓存</summary>
    public static class PlatformCacheWarmer
    {
        /// <summary>权限快照缓存（Code → Permission）</summary>
        private static volatile IReadOnlyDictionary<string, PlatformPermission> _permissionCache =
            new Dictionary<string, PlatformPermission>();

        /// <summary>角色-权限缓存（RoleId → PermissionCode 集合）</summary>
        private static volatile IReadOnlyDictionary<long, IReadOnlyList<string>> _rolePermissionCache =
            new Dictionary<long, IReadOnlyList<string>>();

        /// <summary>用户-角色缓存（UserId → RoleCode 集合）</summary>
        private static volatile IReadOnlyDictionary<long, IReadOnlyList<string>> _userRoleCache =
            new Dictionary<long, IReadOnlyList<string>>();

        /// <summary>功能开关缓存（FeatureKey → Enabled）</summary>
        private static volatile IReadOnlyDictionary<string, bool> _featureFlagCache =
            new Dictionary<string, bool>();

        /// <summary>平台配置缓存（密码策略/安全策略）</summary>
        private static volatile PlatformConfigSnapshot? _configSnapshot;

        /// <summary>获取权限快照缓存</summary>
        public static IReadOnlyDictionary<string, PlatformPermission> PermissionCache => _permissionCache;

        /// <summary>获取角色-权限缓存</summary>
        public static IReadOnlyDictionary<long, IReadOnlyList<string>> RolePermissionCache => _rolePermissionCache;

        /// <summary>获取用户-角色缓存</summary>
        public static IReadOnlyDictionary<long, IReadOnlyList<string>> UserRoleCache => _userRoleCache;

        /// <summary>获取功能开关缓存</summary>
        public static IReadOnlyDictionary<string, bool> FeatureFlagCache => _featureFlagCache;

        /// <summary>获取平台配置快照</summary>
        public static PlatformConfigSnapshot? ConfigSnapshot => _configSnapshot;

        /// <summary>执行全量缓存预热</summary>
        public static async ValueTask WarmUpAsync(int tenantId, long userId)
        {
            Logger.Info(tenantId, userId, "[PlatformCacheWarmer] 开始缓存预热");

            await WarmUpPermissionsAsync(tenantId, userId);
            await WarmUpRolePermissionsAsync(tenantId, userId);
            await WarmUpUserRolesAsync(tenantId, userId);
            await WarmUpFeatureFlagsAsync(tenantId, userId);
            await WarmUpConfigAsync(tenantId, userId);

            Logger.Info(tenantId, userId, "[PlatformCacheWarmer] 缓存预热完成");
        }

        /// <summary>预热权限快照缓存</summary>
        public static async ValueTask WarmUpPermissionsAsync(int tenantId, long userId)
        {
            var (result, data) = await PlatformPermissionCRUD.GetListAsync(tenantId, userId);
            if (result.Success && data != null)
            {
                var dict = new Dictionary<string, PlatformPermission>(data.Count, StringComparer.OrdinalIgnoreCase);
                foreach (var p in data)
                {
                    dict[p.Code] = p;
                }
                _permissionCache = dict;
                Logger.Info(tenantId, userId,
                    "[PlatformCacheWarmer] 权限缓存已加载, 数量=" + data.Count);
            }
        }

        /// <summary>预热角色-权限缓存</summary>
        public static async ValueTask WarmUpRolePermissionsAsync(int tenantId, long userId)
        {
            var (rpResult, rpData) = await PlatformRolePermissionCRUD.GetListAsync(tenantId, userId);
            if (!rpResult.Success || rpData == null) return;

            var permCache = _permissionCache;
            var dict = new Dictionary<long, IReadOnlyList<string>>();

            // 先收集每个 RoleId 的 PermissionId 列表
            var rolePermIds = new Dictionary<long, List<long>>();
            foreach (var rp in rpData)
            {
                if (!rolePermIds.TryGetValue(rp.RoleId, out var list))
                {
                    list = new List<long>();
                    rolePermIds[rp.RoleId] = list;
                }
                list.Add(rp.PermissionId);
            }

            // 需要 Id→Code 反查
            var idToCode = new Dictionary<long, string>();
            foreach (var kvp in permCache)
            {
                idToCode[kvp.Value.Id] = kvp.Key;
            }

            foreach (var kvp in rolePermIds)
            {
                var codes = new List<string>(kvp.Value.Count);
                foreach (var permId in kvp.Value)
                {
                    if (idToCode.TryGetValue(permId, out var code))
                    {
                        codes.Add(code);
                    }
                }
                dict[kvp.Key] = codes;
            }

            _rolePermissionCache = dict;
            Logger.Info(tenantId, userId,
                "[PlatformCacheWarmer] 角色-权限缓存已加载, 角色数=" + dict.Count);
        }

        /// <summary>预热用户-角色缓存</summary>
        public static async ValueTask WarmUpUserRolesAsync(int tenantId, long userId)
        {
            var (rmResult, rmData) = await PlatformRoleMemberCRUD.GetListAsync(tenantId, userId);
            if (!rmResult.Success || rmData == null) return;

            var (roleResult, roleData) = await PlatformRoleCRUD.GetListAsync(tenantId, userId);
            if (!roleResult.Success || roleData == null) return;

            var roleIdToCode = new Dictionary<long, string>();
            foreach (var r in roleData)
            {
                roleIdToCode[r.Id] = r.Code;
            }

            var dict = new Dictionary<long, IReadOnlyList<string>>();
            var userRoleIds = new Dictionary<long, List<long>>();
            foreach (var rm in rmData)
            {
                if (!userRoleIds.TryGetValue(rm.UserId, out var list))
                {
                    list = new List<long>();
                    userRoleIds[rm.UserId] = list;
                }
                list.Add(rm.RoleId);
            }

            foreach (var kvp in userRoleIds)
            {
                var codes = new List<string>(kvp.Value.Count);
                foreach (var roleId in kvp.Value)
                {
                    if (roleIdToCode.TryGetValue(roleId, out var code))
                    {
                        codes.Add(code);
                    }
                }
                dict[kvp.Key] = codes;
            }

            _userRoleCache = dict;
            Logger.Info(tenantId, userId,
                "[PlatformCacheWarmer] 用户-角色缓存已加载, 用户数=" + dict.Count);
        }

        /// <summary>预热功能开关缓存</summary>
        public static async ValueTask WarmUpFeatureFlagsAsync(int tenantId, long userId)
        {
            var (result, data) = await TenantFeatureFlagCRUD.GetListAsync(tenantId, userId);
            if (result.Success && data != null)
            {
                var dict = new Dictionary<string, bool>(data.Count, StringComparer.OrdinalIgnoreCase);
                foreach (var f in data)
                {
                    dict[f.FeatureKey] = f.Enabled;
                }
                _featureFlagCache = dict;
                Logger.Info(tenantId, userId,
                    "[PlatformCacheWarmer] 功能开关缓存已加载, 数量=" + data.Count);
            }
        }

        /// <summary>预热平台配置缓存</summary>
        public static async ValueTask WarmUpConfigAsync(int tenantId, long userId)
        {
            PlatformPasswordPolicy? defaultPasswordPolicy = null;
            PlatformSecurityPolicy? defaultSecurityPolicy = null;

            var (ppResult, ppData) = await PlatformPasswordPolicyCRUD.GetListAsync(tenantId, userId);
            if (ppResult.Success && ppData != null)
            {
                foreach (var p in ppData)
                {
                    if (p.IsDefault)
                    {
                        defaultPasswordPolicy = p;
                        break;
                    }
                }
            }

            var (spResult, spData) = await PlatformSecurityPolicyCRUD.GetListAsync(tenantId, userId);
            if (spResult.Success && spData != null)
            {
                foreach (var s in spData)
                {
                    if (s.IsDefault)
                    {
                        defaultSecurityPolicy = s;
                        break;
                    }
                }
            }

            _configSnapshot = new PlatformConfigSnapshot(defaultPasswordPolicy, defaultSecurityPolicy);
            Logger.Info(tenantId, userId, "[PlatformCacheWarmer] 平台配置缓存已加载");
        }

        /// <summary>清除所有缓存（用于测试）</summary>
        public static void ClearAll()
        {
            _permissionCache = new Dictionary<string, PlatformPermission>();
            _rolePermissionCache = new Dictionary<long, IReadOnlyList<string>>();
            _userRoleCache = new Dictionary<long, IReadOnlyList<string>>();
            _featureFlagCache = new Dictionary<string, bool>();
            _configSnapshot = null;
        }
    }

    /// <summary>平台配置快照</summary>
    public sealed class PlatformConfigSnapshot
    {
        /// <summary>默认密码策略</summary>
        public PlatformPasswordPolicy? DefaultPasswordPolicy { get; }

        /// <summary>默认安全策略</summary>
        public PlatformSecurityPolicy? DefaultSecurityPolicy { get; }

        /// <summary>构造平台配置快照</summary>
        public PlatformConfigSnapshot(
            PlatformPasswordPolicy? defaultPasswordPolicy,
            PlatformSecurityPolicy? defaultSecurityPolicy)
        {
            DefaultPasswordPolicy = defaultPasswordPolicy;
            DefaultSecurityPolicy = defaultSecurityPolicy;
        }
    }
}
