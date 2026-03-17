using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>安全策略</summary>
[Entity(TableName = "platform_security_policies", NeedAuditTable = true)]
public class PlatformSecurityPolicy
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>策略名称</summary>
    [Column(Length = 128, IsRequired = true)]
    public string PolicyName { get; set; } = "";

    /// <summary>是否要求 IP 白名单</summary>
    public bool IpWhitelistRequired { get; set; }

    /// <summary>是否要求多因素认证</summary>
    public bool MfaRequired { get; set; }

    /// <summary>会话超时时间（分钟）</summary>
    public int SessionTimeoutMinutes { get; set; }

    /// <summary>密码策略 ID</summary>
    public long? PasswordPolicyId { get; set; }

    /// <summary>扩展策略</summary>
    [Column(DbType = "jsonb")]
    public string? ExtraPolicy { get; set; }

    /// <summary>是否为默认策略</summary>
    public bool IsDefault { get; set; }

    /// <summary>创建人</summary>
    public long? CreatedBy { get; set; }

    /// <summary>更新人</summary>
    public long? UpdatedBy { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; }
}
