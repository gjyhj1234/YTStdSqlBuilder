using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>平台用户列表项</summary>
    public sealed class PlatformUserRepDTO
    {
        /// <summary>用户 ID</summary>
        public long Id { get; set; }
        /// <summary>用户名</summary>
        public string Username { get; set; } = "";
        /// <summary>邮箱</summary>
        public string Email { get; set; } = "";
        /// <summary>手机号</summary>
        public string? Phone { get; set; }
        /// <summary>显示名称</summary>
        public string DisplayName { get; set; } = "";
        /// <summary>状态（active/disabled/locked）</summary>
        public string Status { get; set; } = "";
        /// <summary>是否启用 MFA</summary>
        public bool MfaEnabled { get; set; }
        /// <summary>最后登录时间</summary>
        public DateTime? LastLoginAt { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }
}
