using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>创建平台角色请求</summary>
    public sealed class CreatePlatformRoleReqDTO
    {
        /// <summary>角色编码</summary>
        public string Code { get; set; } = "";
        /// <summary>角色名称</summary>
        public string Name { get; set; } = "";
        /// <summary>描述</summary>
        public string? Description { get; set; }
    }

    /// <summary>更新平台角色请求</summary>
    public sealed class UpdatePlatformRoleReqDTO
    {
        /// <summary>角色名称</summary>
        public string? Name { get; set; }
        /// <summary>描述</summary>
        public string? Description { get; set; }
    }

    /// <summary>角色授权请求（批量绑定权限）</summary>
    public sealed class RolePermissionBindReqDTO
    {
        /// <summary>权限 ID 列表</summary>
        public long[] PermissionIds { get; set; } = Array.Empty<long>();
    }

    /// <summary>角色成员请求（批量绑定用户）</summary>
    public sealed class RoleMemberBindReqDTO
    {
        /// <summary>用户 ID 列表</summary>
        public long[] UserIds { get; set; } = Array.Empty<long>();
    }
}
