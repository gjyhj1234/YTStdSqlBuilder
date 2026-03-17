using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>租户文件</summary>
[Entity(TableName = "tenant_files", NeedAuditTable = true)]
[Index("idx_tenant_files_tenant_visibility", "tenant_ref_id", "visibility")]
public class TenantFile
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>关联租户 ID</summary>
    public long TenantRefId { get; set; }

    /// <summary>存储策略 ID</summary>
    public long? StorageStrategyId { get; set; }

    /// <summary>文件名</summary>
    [Column(Length = 255, IsRequired = true)]
    public string FileName { get; set; } = "";

    /// <summary>文件路径</summary>
    [Column(Length = 500, IsRequired = true)]
    public string FilePath { get; set; } = "";

    /// <summary>文件扩展名</summary>
    [Column(Length = 32)]
    public string? FileExt { get; set; }

    /// <summary>MIME 类型</summary>
    [Column(Length = 128)]
    public string? MimeType { get; set; }

    /// <summary>文件大小</summary>
    public long FileSize { get; set; }

    /// <summary>校验和</summary>
    [Column(Length = 128)]
    public string? Checksum { get; set; }

    /// <summary>上传者类型</summary>
    [Column(Length = 32, IsRequired = true)]
    public string UploaderType { get; set; } = "";

    /// <summary>上传者 ID</summary>
    public long? UploaderId { get; set; }

    /// <summary>可见性</summary>
    [Column(Length = 32, IsRequired = true)]
    public string Visibility { get; set; } = "";

    /// <summary>下载次数</summary>
    public long DownloadCount { get; set; }

    /// <summary>最后下载时间</summary>
    public DateTime? LastDownloadedAt { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; }
}
