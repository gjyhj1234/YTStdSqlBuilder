using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>创建存储策略请求参数</summary>
    public sealed class CreateStorageStrategyReqDTO
    {
        /// <summary>策略名称</summary>
        public string StrategyName { get; set; } = "";
        /// <summary>存储类型</summary>
        public string StorageType { get; set; } = "";
        /// <summary>描述</summary>
        public string? Description { get; set; }
    }

    /// <summary>更新存储策略请求参数</summary>
    public sealed class UpdateStorageStrategyReqDTO
    {
        /// <summary>策略名称</summary>
        public string? StrategyName { get; set; }
        /// <summary>描述</summary>
        public string? Description { get; set; }
    }

    /// <summary>保存文件访问策略请求参数</summary>
    public sealed class SaveFileAccessPolicyReqDTO
    {
        /// <summary>租户ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>策略名称</summary>
        public string PolicyName { get; set; } = "";
        /// <summary>策略类型</summary>
        public string PolicyType { get; set; } = "";
        /// <summary>策略值</summary>
        public string? PolicyValue { get; set; }
    }
}
