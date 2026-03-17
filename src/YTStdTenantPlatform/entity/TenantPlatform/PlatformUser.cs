using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>平台用户</summary>
[Entity(TableName = "platform_users", NeedAuditTable = true)]
[Index("uq_platform_users_username", "username", Kind = IndexKind.Unique)]
[Index("uq_platform_users_email", "email", Kind = IndexKind.Unique)]
public class PlatformUser
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>用户名</summary>
    [Column(Length = 64, IsRequired = true)]
    public string Username { get; set; } = "";

    /// <summary>邮箱</summary>
    [Column(Length = 128, IsRequired = true)]
    public string Email { get; set; } = "";

    /// <summary>手机号</summary>
    [Column(Length = 32)]
    public string? Phone { get; set; }

    /// <summary>显示名称</summary>
    [Column(Length = 128, IsRequired = true)]
    public string DisplayName { get; set; } = "";

    /// <summary>密码散列</summary>
    [Column(Length = 255, IsRequired = true)]
    public string PasswordHash { get; set; } = "";

    /// <summary>密码盐</summary>
    [Column(Length = 128)]
    public string? PasswordSalt { get; set; }

    /// <summary>用户状态</summary>
    [Column(Length = 32, IsRequired = true)]
    public string Status { get; set; } = "";

    /// <summary>密码过期时间</summary>
    public DateTime? PasswordExpiresAt { get; set; }

    /// <summary>最后登录时间</summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>最后登录 IP</summary>
    [Column(Length = 64)]
    public string? LastLoginIp { get; set; }

    /// <summary>登录失败次数</summary>
    public int FailedLoginCount { get; set; }

    /// <summary>锁定截止时间</summary>
    public DateTime? LockedUntil { get; set; }

    /// <summary>是否启用多因素认证</summary>
    public bool MfaEnabled { get; set; }

    /// <summary>备注</summary>
    public string? Remark { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>删除时间</summary>
    public DateTime? DeletedAt { get; set; }
}
