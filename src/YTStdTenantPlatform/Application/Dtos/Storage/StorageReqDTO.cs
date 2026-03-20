using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>创建存储策略请求</summary>
    public sealed class CreateStorageStrategyReqDTO
    {
        /// <summary>策略编码</summary>
        public string StrategyCode { get; set; } = "";
        /// <summary>策略名称</summary>
        public string StrategyName { get; set; } = "";
        /// <summary>提供商类型（local/s3/oss/azure）</summary>
        public string ProviderType { get; set; } = "local";
        /// <summary>存储桶名称</summary>
        public string? BucketName { get; set; }
        /// <summary>基础路径</summary>
        public string? BasePath { get; set; }
    }

    /// <summary>更新存储策略请求</summary>
    public sealed class UpdateStorageStrategyReqDTO
    {
        /// <summary>策略名称</summary>
        public string? StrategyName { get; set; }
        /// <summary>存储桶名称</summary>
        public string? BucketName { get; set; }
        /// <summary>基础路径</summary>
        public string? BasePath { get; set; }
    }

    /// <summary>创建/更新文件访问策略请求</summary>
    public sealed class SaveFileAccessPolicyReqDTO
    {
        /// <summary>文件 ID</summary>
        public long FileId { get; set; }
        /// <summary>主体类型</summary>
        public string SubjectType { get; set; } = "";
        /// <summary>主体 ID</summary>
        public string? SubjectId { get; set; }
        /// <summary>权限编码（read/write/delete/admin）</summary>
        public string PermissionCode { get; set; } = "read";
    }
}
