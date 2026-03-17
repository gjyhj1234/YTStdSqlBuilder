using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>密码策略</summary>
[Entity(TableName = "platform_password_policies", NeedAuditTable = true)]
public class PlatformPasswordPolicy
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>策略名称</summary>
    [Column(Length = 128, IsRequired = true)]
    public string PolicyName { get; set; } = "";

    /// <summary>最小长度</summary>
    public int MinLength { get; set; }

    /// <summary>最大长度</summary>
    public int MaxLength { get; set; }

    /// <summary>是否要求大写字母</summary>
    public bool RequireUppercase { get; set; }

    /// <summary>是否要求小写字母</summary>
    public bool RequireLowercase { get; set; }

    /// <summary>是否要求数字</summary>
    public bool RequireNumber { get; set; }

    /// <summary>是否要求特殊字符</summary>
    public bool RequireSpecial { get; set; }

    /// <summary>密码过期天数</summary>
    public int PasswordExpireDays { get; set; }

    /// <summary>禁止重复使用次数</summary>
    public int PreventReuseCount { get; set; }

    /// <summary>登录失败锁定阈值</summary>
    public int LoginFailLockThreshold { get; set; }

    /// <summary>锁定时长（分钟）</summary>
    public int LockDurationMinutes { get; set; }

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
