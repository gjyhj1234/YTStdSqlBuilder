using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>登录请求参数</summary>
    public sealed class LoginReqDTO
    {
        /// <summary>用户名</summary>
        public string Username { get; set; } = "";
        /// <summary>密码</summary>
        public string Password { get; set; } = "";
    }

    /// <summary>刷新令牌请求参数</summary>
    public sealed class RefreshTokenReqDTO
    {
        /// <summary>当前令牌</summary>
        public string Token { get; set; } = "";
    }
}
