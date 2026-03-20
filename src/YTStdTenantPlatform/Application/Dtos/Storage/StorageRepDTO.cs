using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>存储策略响应数据</summary>
    public sealed class StorageStrategyRepDTO
    {
        /// <summary>策略ID</summary>
        public long Id { get; set; }
        /// <summary>策略名称</summary>
        public string StrategyName { get; set; } = "";
        /// <summary>存储类型</summary>
        public string StorageType { get; set; } = "";
        /// <summary>描述</summary>
        public string? Description { get; set; }
        /// <summary>状态</summary>
        public string Status { get; set; } = "";
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>租户文件响应数据</summary>
    public sealed class TenantFileRepDTO
    {
        /// <summary>文件ID</summary>
        public long Id { get; set; }
        /// <summary>租户ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>文件名</summary>
        public string FileName { get; set; } = "";
        /// <summary>文件大小（字节）</summary>
        public long FileSize { get; set; }
        /// <summary>MIME类型</summary>
        public string? MimeType { get; set; }
        /// <summary>存储路径</summary>
        public string? StoragePath { get; set; }
        /// <summary>上传时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>文件访问策略响应数据</summary>
    public sealed class FileAccessPolicyRepDTO
    {
        /// <summary>策略ID</summary>
        public long Id { get; set; }
        /// <summary>租户ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>策略名称</summary>
        public string PolicyName { get; set; } = "";
        /// <summary>策略类型</summary>
        public string PolicyType { get; set; } = "";
        /// <summary>策略值</summary>
        public string? PolicyValue { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }
}
