using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>多因素认证设置</summary>
[Entity(TableName = "platform_mfa_settings", NeedAuditTable = true)]
public class PlatformMfaSetting
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>平台用户 ID</summary>
    public long UserId { get; set; }

    /// <summary>认证提供方类型</summary>
    [Column(Length = 32, IsRequired = true)]
    public string ProviderType { get; set; } = "";

    /// <summary>密钥密文</summary>
    public string? SecretCiphertext { get; set; }

    /// <summary>手机号</summary>
    [Column(Length = 32)]
    public string? Phone { get; set; }

    /// <summary>邮箱</summary>
    [Column(Length = 128)]
    public string? Email { get; set; }

    /// <summary>是否为主要方式</summary>
    public bool IsPrimary { get; set; }

    /// <summary>状态</summary>
    [Column(Length = 32, IsRequired = true)]
    public string Status { get; set; } = "";

    /// <summary>验证时间</summary>
    public DateTime? VerifiedAt { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; }
}
