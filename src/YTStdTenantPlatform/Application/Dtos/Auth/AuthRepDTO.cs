using System;
using System.Collections.Generic;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>登录响应数据</summary>
    public sealed class LoginRepDTO
    {
        /// <summary>访问令牌</summary>
        public string? Token { get; set; }
        /// <summary>令牌过期时间（秒）</summary>
        public int ExpiresIn { get; set; }
        /// <summary>用户ID</summary>
        public long UserId { get; set; }
        /// <summary>用户名</summary>
        public string Username { get; set; } = "";
        /// <summary>显示名称</summary>
        public string DisplayName { get; set; } = "";
        /// <summary>是否需要重置密码</summary>
        public bool RequirePasswordReset { get; set; }
        /// <summary>角色列表</summary>
        public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
        /// <summary>权限列表</summary>
        public IReadOnlyList<string> Permissions { get; set; } = Array.Empty<string>();
        /// <summary>是否超级管理员</summary>
        public bool IsSuperAdmin { get; set; }
    }

    /// <summary>当前用户信息响应数据</summary>
    public sealed class CurrentUserRepDTO
    {
        /// <summary>用户ID</summary>
        public long UserId { get; set; }
        /// <summary>用户名</summary>
        public string Username { get; set; } = "";
        /// <summary>显示名称</summary>
        public string DisplayName { get; set; } = "";
        /// <summary>是否超级管理员</summary>
        public bool IsSuperAdmin { get; set; }
    }
}
