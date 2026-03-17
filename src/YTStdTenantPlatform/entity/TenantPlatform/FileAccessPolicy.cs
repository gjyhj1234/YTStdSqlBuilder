using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>文件访问策略</summary>
[Entity(TableName = "file_access_policies", NeedAuditTable = true)]
public class FileAccessPolicy
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>文件 ID</summary>
    public long FileId { get; set; }

    /// <summary>主体类型</summary>
    [Column(Length = 32, IsRequired = true)]
    public string SubjectType { get; set; } = "";

    /// <summary>主体 ID</summary>
    [Column(Length = 128)]
    public string? SubjectId { get; set; }

    /// <summary>权限编码</summary>
    [Column(Length = 32, IsRequired = true)]
    public string PermissionCode { get; set; } = "";

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }
}
