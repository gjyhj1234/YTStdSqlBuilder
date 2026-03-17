using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>租户参数</summary>
[Entity(TableName = "tenant_parameters", NeedAuditTable = true)]
[Index("uq_tenant_parameters_tenant_param", "tenant_ref_id", "param_key", Kind = IndexKind.Unique)]
public class TenantParameter
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>关联租户 ID</summary>
    public long TenantRefId { get; set; }

    /// <summary>参数键</summary>
    [Column(Length = 128, IsRequired = true)]
    public string ParamKey { get; set; } = "";

    /// <summary>参数名称</summary>
    [Column(Length = 128, IsRequired = true)]
    public string ParamName { get; set; } = "";

    /// <summary>参数类型</summary>
    [Column(Length = 32, IsRequired = true)]
    public string ParamType { get; set; } = "";

    /// <summary>参数值</summary>
    [Column(DbType = "jsonb", IsRequired = true)]
    public string ParamValue { get; set; } = "";

    /// <summary>是否加密</summary>
    public bool IsEncrypted { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; }
}
