using System;
using System.Collections.Generic;
using YTStdTenantPlatform.Entity.TenantPlatform;

namespace YTStdTenantPlatform.Infrastructure.Initialization.SeedData
{
    /// <summary>权限种子数据项，包含权限实体和父级编码</summary>
    public sealed class PermissionSeed
    {
        /// <summary>权限实体</summary>
        public PlatformPermission Permission { get; }

        /// <summary>父级权限编码（用于树形结构解析，不会写入数据库）</summary>
        public string? ParentCode { get; }

        /// <summary>构造权限种子数据项</summary>
        public PermissionSeed(PlatformPermission permission, string? parentCode)
        {
            Permission = permission;
            ParentCode = parentCode;
        }
    }

    /// <summary>默认平台权限种子数据</summary>
    public static class DefaultPermissions
    {
        /// <summary>获取默认权限列表（包含父级编码引用）</summary>
        public static IReadOnlyList<PermissionSeed> GetDefaultPermissions()
        {
            var now = DateTime.UtcNow;
            var list = new List<PermissionSeed>();

            // ── 1. 平台管理 ──
            list.Add(Menu("platform:management", "平台管理", null, now));
            list.Add(Menu("platform:user", "用户管理", "platform:management", now));
            list.Add(Menu("platform:role", "角色管理", "platform:management", now));
            list.Add(Menu("platform:permission", "权限管理", "platform:management", now));
            list.Add(Menu("platform:security", "安全设置", "platform:management", now));

            AddCrud(list, "platform:user", "用户", "/api/platform/users", now);
            AddCrud(list, "platform:role", "角色", "/api/platform/roles", now);
            AddCrud(list, "platform:permission", "权限", "/api/platform/permissions", now);
            AddCrud(list, "platform:security", "安全设置", "/api/platform/security", now);

            list.Add(Operation("platform:user:reset_password", "重置用户密码", "platform:user", now));
            list.Add(Operation("platform:user:lock", "锁定用户", "platform:user", now));
            list.Add(Operation("platform:user:unlock", "解锁用户", "platform:user", now));
            list.Add(Operation("platform:role:assign_permission", "分配角色权限", "platform:role", now));
            list.Add(Operation("platform:role:assign_member", "分配角色成员", "platform:role", now));

            // ── 2. 租户管理 ──
            list.Add(Menu("tenant:management", "租户管理", null, now));
            list.Add(Menu("tenant:list", "租户列表", "tenant:management", now));
            list.Add(Menu("tenant:group", "租户分组", "tenant:management", now));
            list.Add(Menu("tenant:tag", "租户标签", "tenant:management", now));
            list.Add(Menu("tenant:domain", "域名管理", "tenant:management", now));
            list.Add(Menu("tenant:config", "租户配置", "tenant:management", now));

            AddCrud(list, "tenant:list", "租户", "/api/tenants", now);
            AddCrud(list, "tenant:group", "租户分组", "/api/tenants/groups", now);
            AddCrud(list, "tenant:tag", "租户标签", "/api/tenants/tags", now);
            AddCrud(list, "tenant:domain", "域名", "/api/tenants/domains", now);
            AddCrud(list, "tenant:config", "租户配置", "/api/tenants/config", now);

            list.Add(Operation("tenant:list:activate", "激活租户", "tenant:list", now));
            list.Add(Operation("tenant:list:suspend", "暂停租户", "tenant:list", now));
            list.Add(Operation("tenant:list:close", "关闭租户", "tenant:list", now));
            list.Add(Operation("tenant:domain:verify", "验证域名", "tenant:domain", now));

            // ── 3. 套餐管理 ──
            list.Add(Menu("package:management", "套餐管理", null, now));
            list.Add(Menu("package:list", "套餐列表", "package:management", now));
            list.Add(Menu("package:version", "套餐版本", "package:management", now));
            list.Add(Menu("package:capability", "套餐能力", "package:management", now));

            AddCrud(list, "package:list", "套餐", "/api/packages", now);
            AddCrud(list, "package:version", "套餐版本", "/api/packages/versions", now);
            AddCrud(list, "package:capability", "套餐能力", "/api/packages/capabilities", now);

            list.Add(Operation("package:version:publish", "发布版本", "package:version", now));
            list.Add(Operation("package:version:deprecate", "废弃版本", "package:version", now));

            // ── 4. 订阅管理 ──
            list.Add(Menu("subscription:management", "订阅管理", null, now));
            list.Add(Menu("subscription:list", "订阅列表", "subscription:management", now));
            list.Add(Menu("subscription:trial", "试用管理", "subscription:management", now));
            list.Add(Menu("subscription:change", "变更记录", "subscription:management", now));

            AddCrud(list, "subscription:list", "订阅", "/api/subscriptions", now);
            AddCrud(list, "subscription:trial", "试用", "/api/subscriptions/trials", now);
            list.Add(Api("subscription:change:view", "查看变更记录", "/api/subscriptions/changes", "GET", "subscription:change", now));

            list.Add(Operation("subscription:list:renew", "续费订阅", "subscription:list", now));
            list.Add(Operation("subscription:list:cancel", "取消订阅", "subscription:list", now));
            list.Add(Operation("subscription:list:upgrade", "升级订阅", "subscription:list", now));
            list.Add(Operation("subscription:trial:extend", "延长试用", "subscription:trial", now));

            // ── 5. 计费管理 ──
            list.Add(Menu("billing:management", "计费管理", null, now));
            list.Add(Menu("billing:invoice", "账单管理", "billing:management", now));
            list.Add(Menu("billing:payment", "支付记录", "billing:management", now));

            AddCrud(list, "billing:invoice", "账单", "/api/billing/invoices", now);
            list.Add(Api("billing:payment:view", "查看支付记录", "/api/billing/payments", "GET", "billing:payment", now));
            list.Add(Api("billing:payment:detail", "查看支付详情", "/api/billing/payments/{id}", "GET", "billing:payment", now));

            list.Add(Operation("billing:invoice:void", "作废账单", "billing:invoice", now));
            list.Add(Operation("billing:payment:refund", "退款", "billing:payment", now));

            // ── 6. 通知管理 ──
            list.Add(Menu("notification:management", "通知管理", null, now));
            list.Add(Menu("notification:template", "通知模板", "notification:management", now));
            list.Add(Menu("notification:record", "通知记录", "notification:management", now));

            AddCrud(list, "notification:template", "通知模板", "/api/notifications/templates", now);
            list.Add(Api("notification:record:view", "查看通知记录", "/api/notifications/records", "GET", "notification:record", now));
            list.Add(Api("notification:record:detail", "查看通知详情", "/api/notifications/records/{id}", "GET", "notification:record", now));

            list.Add(Operation("notification:template:test", "测试发送", "notification:template", now));
            list.Add(Operation("notification:record:resend", "重新发送", "notification:record", now));

            // ── 7. 日志管理 ──
            list.Add(Menu("log:management", "日志管理", null, now));
            list.Add(Menu("log:operation", "操作日志", "log:management", now));
            list.Add(Menu("log:audit", "审计日志", "log:management", now));
            list.Add(Menu("log:system", "系统日志", "log:management", now));

            list.Add(Api("log:operation:view", "查看操作日志", "/api/logs/operations", "GET", "log:operation", now));
            list.Add(Api("log:audit:view", "查看审计日志", "/api/logs/audits", "GET", "log:audit", now));
            list.Add(Api("log:system:view", "查看系统日志", "/api/logs/system", "GET", "log:system", now));

            list.Add(Operation("log:operation:export", "导出操作日志", "log:operation", now));
            list.Add(Operation("log:audit:export", "导出审计日志", "log:audit", now));

            // ── 8. 基础设施 ──
            list.Add(Menu("infrastructure:management", "基础设施", null, now));
            list.Add(Menu("infra:ratelimit", "限流策略", "infrastructure:management", now));
            list.Add(Menu("infra:isolation", "隔离策略", "infrastructure:management", now));
            list.Add(Menu("infra:component", "组件管理", "infrastructure:management", now));

            AddCrud(list, "infra:ratelimit", "限流策略", "/api/infrastructure/ratelimits", now);
            AddCrud(list, "infra:isolation", "隔离策略", "/api/infrastructure/isolations", now);
            AddCrud(list, "infra:component", "基础设施组件", "/api/infrastructure/components", now);

            list.Add(Operation("infra:component:restart", "重启组件", "infra:component", now));
            list.Add(Operation("infra:component:health_check", "健康检查", "infra:component", now));

            return list;
        }

        /// <summary>创建菜单权限</summary>
        private static PermissionSeed Menu(string code, string name, string? parentCode, DateTime now)
        {
            return new PermissionSeed(
                new PlatformPermission
                {
                    Code = code,
                    Name = name,
                    PermissionType = "menu",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                parentCode);
        }

        /// <summary>创建 API 权限</summary>
        private static PermissionSeed Api(string code, string name, string path, string method, string parentCode, DateTime now)
        {
            return new PermissionSeed(
                new PlatformPermission
                {
                    Code = code,
                    Name = name,
                    PermissionType = "api",
                    Path = path,
                    Method = method,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                parentCode);
        }

        /// <summary>创建操作权限</summary>
        private static PermissionSeed Operation(string code, string name, string parentCode, DateTime now)
        {
            return new PermissionSeed(
                new PlatformPermission
                {
                    Code = code,
                    Name = name,
                    PermissionType = "operation",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                parentCode);
        }

        /// <summary>批量添加模块的 CRUD API 权限</summary>
        private static void AddCrud(List<PermissionSeed> list, string parentCode, string moduleName, string basePath, DateTime now)
        {
            list.Add(Api(parentCode + ":view", "查看" + moduleName, basePath, "GET", parentCode, now));
            list.Add(Api(parentCode + ":create", "创建" + moduleName, basePath, "POST", parentCode, now));
            list.Add(Api(parentCode + ":update", "更新" + moduleName, basePath + "/{id}", "PUT", parentCode, now));
            list.Add(Api(parentCode + ":delete", "删除" + moduleName, basePath + "/{id}", "DELETE", parentCode, now));
        }
    }
}
