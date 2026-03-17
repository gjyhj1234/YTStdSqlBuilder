using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>平台权限</summary>
[Entity(TableName = "platform_permissions", NeedAuditTable = true)]
[Index("uq_platform_permissions_code", "code", Kind = IndexKind.Unique)]
public class PlatformPermission
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>权限编码</summary>
    [Column(Length = 128, IsRequired = true)]
    public string Code { get; set; } = "";

    /// <summary>权限名称</summary>
    [Column(Length = 128, IsRequired = true)]
    public string Name { get; set; } = "";

    /// <summary>权限类型</summary>
    [Column(Length = 32, IsRequired = true)]
    public string PermissionType { get; set; } = "";

    /// <summary>父级权限 ID</summary>
    public long? ParentId { get; set; }

    /// <summary>资源标识</summary>
    [Column(Length = 255)]
    public string? Resource { get; set; }

    /// <summary>操作标识</summary>
    [Column(Length = 64)]
    public string? Action { get; set; }

    /// <summary>路径</summary>
    [Column(Length = 255)]
    public string? Path { get; set; }

    /// <summary>HTTP 方法</summary>
    [Column(Length = 16)]
    public string? Method { get; set; }

    /// <summary>数据范围规则</summary>
    [Column(DbType = "jsonb")]
    public string? DataScopeRule { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; }
}
