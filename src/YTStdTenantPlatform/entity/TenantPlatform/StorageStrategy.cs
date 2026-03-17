using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>存储策略</summary>
[Entity(TableName = "storage_strategies", NeedAuditTable = true)]
[Index("uq_storage_strategies_strategy_code", "strategy_code", Kind = IndexKind.Unique)]
public class StorageStrategy
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>策略编码</summary>
    [Column(Length = 64, IsRequired = true)]
    public string StrategyCode { get; set; } = "";

    /// <summary>策略名称</summary>
    [Column(Length = 128, IsRequired = true)]
    public string StrategyName { get; set; } = "";

    /// <summary>提供商类型</summary>
    [Column(Length = 32, IsRequired = true)]
    public string ProviderType { get; set; } = "";

    /// <summary>存储桶名称</summary>
    [Column(Length = 255)]
    public string? BucketName { get; set; }

    /// <summary>基础路径</summary>
    [Column(Length = 255)]
    public string? BasePath { get; set; }

    /// <summary>配置</summary>
    [Column(DbType = "jsonb")]
    public string? Config { get; set; }

    /// <summary>状态</summary>
    [Column(Length = 32, IsRequired = true)]
    public string Status { get; set; } = "";

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; }
}
