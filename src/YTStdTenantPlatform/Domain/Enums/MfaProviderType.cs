namespace YTStdTenantPlatform.Domain.Enums;

/// <summary>多因素认证提供者类型</summary>
public enum MfaProviderType
{
    /// <summary>基于时间的一次性密码</summary>
    Totp,

    /// <summary>短信验证</summary>
    Sms,

    /// <summary>邮箱验证</summary>
    Email,

    /// <summary>应用验证</summary>
    App
}
