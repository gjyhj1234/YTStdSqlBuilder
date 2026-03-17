using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>平台登录日志</summary>
[Entity(TableName = "platform_login_logs")]
[Index("idx_platform_login_logs_user_time", "user_id", "occurred_at")]
public class PlatformLoginLog
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>用户 ID</summary>
    public long? UserId { get; set; }

    /// <summary>用户名</summary>
    [Column(Length = 64)]
    public string? Username { get; set; }

    /// <summary>登录类型</summary>
    [Column(Length = 32, IsRequired = true)]
    public string LoginType { get; set; } = "";

    /// <summary>登录状态</summary>
    [Column(Length = 32, IsRequired = true)]
    public string LoginStatus { get; set; } = "";

    /// <summary>IP 地址</summary>
    [Column(Length = 64)]
    public string? IpAddress { get; set; }

    /// <summary>用户代理</summary>
    public string? UserAgent { get; set; }

    /// <summary>失败原因</summary>
    public string? FailureReason { get; set; }

    /// <summary>发生时间</summary>
    public DateTime OccurredAt { get; set; }
}
