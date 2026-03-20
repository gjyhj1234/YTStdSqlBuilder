using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>存储策略列表项</summary>
    public sealed class StorageStrategyRepDTO
    {
        /// <summary>策略 ID</summary>
        public long Id { get; set; }
        /// <summary>策略编码</summary>
        public string StrategyCode { get; set; } = "";
        /// <summary>策略名称</summary>
        public string StrategyName { get; set; } = "";
        /// <summary>提供商类型</summary>
        public string ProviderType { get; set; } = "";
        /// <summary>存储桶名称</summary>
        public string? BucketName { get; set; }
        /// <summary>基础路径</summary>
        public string? BasePath { get; set; }
        /// <summary>状态</summary>
        public string Status { get; set; } = "";
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>租户文件列表项</summary>
    public sealed class TenantFileRepDTO
    {
        /// <summary>文件 ID</summary>
        public long Id { get; set; }
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>存储策略 ID</summary>
        public long? StorageStrategyId { get; set; }
        /// <summary>文件名</summary>
        public string FileName { get; set; } = "";
        /// <summary>文件路径</summary>
        public string FilePath { get; set; } = "";
        /// <summary>文件扩展名</summary>
        public string? FileExt { get; set; }
        /// <summary>MIME 类型</summary>
        public string? MimeType { get; set; }
        /// <summary>文件大小</summary>
        public long FileSize { get; set; }
        /// <summary>上传者类型</summary>
        public string UploaderType { get; set; } = "";
        /// <summary>上传者 ID</summary>
        public long? UploaderId { get; set; }
        /// <summary>可见性</summary>
        public string Visibility { get; set; } = "";
        /// <summary>下载次数</summary>
        public long DownloadCount { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>文件访问策略列表项</summary>
    public sealed class FileAccessPolicyRepDTO
    {
        /// <summary>策略 ID</summary>
        public long Id { get; set; }
        /// <summary>文件 ID</summary>
        public long FileId { get; set; }
        /// <summary>主体类型</summary>
        public string SubjectType { get; set; } = "";
        /// <summary>主体 ID</summary>
        public string? SubjectId { get; set; }
        /// <summary>权限编码</summary>
        public string PermissionCode { get; set; } = "";
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }
}
