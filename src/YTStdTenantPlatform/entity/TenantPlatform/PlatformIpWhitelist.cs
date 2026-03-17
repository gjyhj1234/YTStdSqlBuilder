using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>IP 白名单</summary>
[Entity(TableName = "platform_ip_whitelists", NeedAuditTable = true)]
public class PlatformIpWhitelist
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>适用对象类型</summary>
    [Column(Length = 32, IsRequired = true)]
    public string SubjectType { get; set; } = "";

    /// <summary>适用对象 ID</summary>
    public long? SubjectId { get; set; }

    /// <summary>IP 地址</summary>
    [Column(Length = 64, IsRequired = true)]
    public string IpAddress { get; set; } = "";

    /// <summary>IP CIDR 段</summary>
    [Column(Length = 64)]
    public string? IpCidr { get; set; }

    /// <summary>描述</summary>
    public string? Description { get; set; }

    /// <summary>状态</summary>
    [Column(Length = 32, IsRequired = true)]
    public string Status { get; set; } = "";

    /// <summary>生效开始时间</summary>
    public DateTime? EffectiveFrom { get; set; }

    /// <summary>生效结束时间</summary>
    public DateTime? EffectiveTo { get; set; }

    /// <summary>创建人</summary>
    public long? CreatedBy { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }
}
