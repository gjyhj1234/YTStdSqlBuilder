using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>租户 API 密钥</summary>
[Entity(TableName = "tenant_api_keys", NeedAuditTable = true)]
[Index("uq_tenant_api_keys_access_key", "access_key", Kind = IndexKind.Unique)]
public class TenantApiKey
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>关联租户 ID</summary>
    public long TenantRefId { get; set; }

    /// <summary>密钥名称</summary>
    [Column(Length = 128, IsRequired = true)]
    public string KeyName { get; set; } = "";

    /// <summary>访问密钥</summary>
    [Column(Length = 128, IsRequired = true)]
    public string AccessKey { get; set; } = "";

    /// <summary>密钥散列</summary>
    [Column(Length = 255, IsRequired = true)]
    public string SecretHash { get; set; } = "";

    /// <summary>状态</summary>
    [Column(Length = 32, IsRequired = true)]
    public string Status { get; set; } = "";

    /// <summary>配额上限</summary>
    public long? QuotaLimit { get; set; }

    /// <summary>速率限制</summary>
    public int? RateLimit { get; set; }

    /// <summary>最后使用时间</summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>过期时间</summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>创建人</summary>
    public long? CreatedBy { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; }
}
