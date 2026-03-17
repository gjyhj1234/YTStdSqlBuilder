using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>数据隔离策略</summary>
[Entity(TableName = "data_isolation_policies", NeedAuditTable = true)]
public class DataIsolationPolicy
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>关联租户 ID</summary>
    public long? TenantRefId { get; set; }

    /// <summary>隔离类型</summary>
    [Column(Length = 32, IsRequired = true)]
    public string IsolationType { get; set; } = "";

    /// <summary>策略名称</summary>
    [Column(Length = 128, IsRequired = true)]
    public string PolicyName { get; set; } = "";

    /// <summary>策略配置</summary>
    [Column(DbType = "jsonb", IsRequired = true)]
    public string PolicyConfig { get; set; } = "";

    /// <summary>状态</summary>
    [Column(Length = 32, IsRequired = true)]
    public string Status { get; set; } = "";

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; }
}
