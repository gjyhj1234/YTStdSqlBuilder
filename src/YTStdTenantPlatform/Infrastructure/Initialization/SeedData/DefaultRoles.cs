using System;
using System.Collections.Generic;
using System.Linq;
using YTStdTenantPlatform.Entity.TenantPlatform;

namespace YTStdTenantPlatform.Infrastructure.Initialization.SeedData
{
    /// <summary>默认平台角色种子数据</summary>
    public static class DefaultRoles
    {
        /// <summary>超级管理员角色编码</summary>
        public const string SuperAdminCode = "super_admin";

        /// <summary>平台管理员角色编码</summary>
        public const string PlatformAdminCode = "platform_admin";

        /// <summary>平台只读角色编码</summary>
        public const string PlatformViewerCode = "platform_viewer";

        /// <summary>获取默认角色列表</summary>
        public static IReadOnlyList<PlatformRole> GetDefaultRoles()
        {
            var now = DateTime.UtcNow;
            return new[]
            {
                new PlatformRole
                {
                    Code = SuperAdminCode,
                    Name = "超级管理员",
                    Description = "拥有平台全部权限的超级管理员角色",
                    Status = "active",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new PlatformRole
                {
                    Code = PlatformAdminCode,
                    Name = "平台管理员",
                    Description = "拥有平台日常管理权限，不含安全策略配置",
                    Status = "active",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new PlatformRole
                {
                    Code = PlatformViewerCode,
                    Name = "平台只读",
                    Description = "仅拥有平台只读查看权限",
                    Status = "active",
                    CreatedAt = now,
                    UpdatedAt = now
                }
            };
        }

        /// <summary>安全相关权限编码前缀集合，平台管理员角色不包含这些权限</summary>
        private static readonly HashSet<string> SecurityPermissionPrefixes = new HashSet<string>
        {
            "platform:security"
        };

        /// <summary>
        /// 获取角色与权限的绑定关系。
        /// 使用权限编码列表，由调用方在运行时解析为实际 ID。
        /// </summary>
        public static IReadOnlyList<RolePermissionBinding> GetRolePermissionBindings(IReadOnlyList<PermissionSeed> allPermissions)
        {
            var bindings = new List<RolePermissionBinding>();

            // 超级管理员: 拥有全部权限
            foreach (var seed in allPermissions)
            {
                bindings.Add(new RolePermissionBinding { RoleCode = SuperAdminCode, PermissionCode = seed.Permission.Code });
            }

            // 平台管理员: 全部权限，排除安全相关
            foreach (var seed in allPermissions)
            {
                if (!IsSecurityPermission(seed.Permission.Code))
                {
                    bindings.Add(new RolePermissionBinding { RoleCode = PlatformAdminCode, PermissionCode = seed.Permission.Code });
                }
            }

            // 平台只读: 仅查看类 API 权限和菜单权限
            foreach (var seed in allPermissions)
            {
                if (seed.Permission.PermissionType == "menu" || seed.Permission.Code.EndsWith(":view"))
                {
                    bindings.Add(new RolePermissionBinding { RoleCode = PlatformViewerCode, PermissionCode = seed.Permission.Code });
                }
            }

            return bindings;
        }

        /// <summary>
        /// 获取角色成员绑定关系。
        /// 默认将 admin 用户分配到超级管理员角色。
        /// </summary>
        public static IReadOnlyList<RoleMemberBinding> GetRoleMemberBindings()
        {
            return new[]
            {
                new RoleMemberBinding { RoleCode = SuperAdminCode, Username = "admin" }
            };
        }

        /// <summary>判断权限编码是否属于安全相关权限</summary>
        private static bool IsSecurityPermission(string code)
        {
            foreach (var prefix in SecurityPermissionPrefixes)
            {
                if (code.StartsWith(prefix, StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>角色-权限绑定描述</summary>
        public class RolePermissionBinding
        {
            /// <summary>角色编码</summary>
            public string RoleCode { get; set; } = "";

            /// <summary>权限编码</summary>
            public string PermissionCode { get; set; } = "";
        }

        /// <summary>角色-成员绑定描述</summary>
        public class RoleMemberBinding
        {
            /// <summary>角色编码</summary>
            public string RoleCode { get; set; } = "";

            /// <summary>用户名</summary>
            public string Username { get; set; } = "";
        }
    }
}
