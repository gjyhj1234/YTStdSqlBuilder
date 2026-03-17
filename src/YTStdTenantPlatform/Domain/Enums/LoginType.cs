namespace YTStdTenantPlatform.Domain.Enums;

/// <summary>登录方式</summary>
public enum LoginType
{
    /// <summary>密码登录</summary>
    Password,

    /// <summary>多因素认证登录</summary>
    Mfa,

    /// <summary>API密钥登录</summary>
    Api,

    /// <summary>单点登录</summary>
    Sso
}
