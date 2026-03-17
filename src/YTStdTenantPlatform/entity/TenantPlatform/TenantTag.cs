using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>租户标签</summary>
[Entity(TableName = "tenant_tags", NeedAuditTable = true)]
[Index("uq_tenant_tags_key_value", "tag_key", "tag_value", Kind = IndexKind.Unique)]
public class TenantTag
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>标签键</summary>
    [Column(Length = 64, IsRequired = true)]
    public string TagKey { get; set; } = "";

    /// <summary>标签值</summary>
    [Column(Length = 128, IsRequired = true)]
    public string TagValue { get; set; } = "";

    /// <summary>标签类型</summary>
    [Column(Length = 32, IsRequired = true)]
    public string TagType { get; set; } = "";

    /// <summary>描述</summary>
    public string? Description { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }
}
