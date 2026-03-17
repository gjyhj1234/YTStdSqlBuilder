using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>基础设施组件</summary>
[Entity(TableName = "infrastructure_components", NeedAuditTable = true)]
[Index("uq_infrastructure_components_type_name", "component_type", "component_name", Kind = IndexKind.Unique)]
public class InfrastructureComponent
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>组件类型</summary>
    [Column(Length = 32, IsRequired = true)]
    public string ComponentType { get; set; } = "";

    /// <summary>组件名称</summary>
    [Column(Length = 128, IsRequired = true)]
    public string ComponentName { get; set; } = "";

    /// <summary>端点地址</summary>
    [Column(Length = 255)]
    public string? Endpoint { get; set; }

    /// <summary>状态</summary>
    [Column(Length = 32, IsRequired = true)]
    public string Status { get; set; } = "";

    /// <summary>组件配置</summary>
    [Column(DbType = "jsonb")]
    public string? ComponentConfig { get; set; }

    /// <summary>最后心跳时间</summary>
    public DateTime? LastHeartbeatAt { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; }
}
