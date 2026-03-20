using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>创建租户分组请求</summary>
    public sealed class CreateTenantGroupReqDTO
    {
        /// <summary>分组编码</summary>
        public string GroupCode { get; set; } = "";
        /// <summary>分组名称</summary>
        public string GroupName { get; set; } = "";
        /// <summary>描述</summary>
        public string? Description { get; set; }
        /// <summary>父级分组 ID</summary>
        public long? ParentId { get; set; }
    }

    /// <summary>创建租户域名请求</summary>
    public sealed class CreateTenantDomainReqDTO
    {
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>域名</summary>
        public string Domain { get; set; } = "";
        /// <summary>域名类型（primary/alias/custom）</summary>
        public string DomainType { get; set; } = "custom";
    }

    /// <summary>创建租户标签请求</summary>
    public sealed class CreateTenantTagReqDTO
    {
        /// <summary>标签键</summary>
        public string TagKey { get; set; } = "";
        /// <summary>标签值</summary>
        public string TagValue { get; set; } = "";
        /// <summary>标签类型</summary>
        public string TagType { get; set; } = "custom";
        /// <summary>描述</summary>
        public string? Description { get; set; }
    }

    /// <summary>标签绑定请求</summary>
    public sealed class TagBindReqDTO
    {
        /// <summary>租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>标签 ID 列表</summary>
        public long[] TagIds { get; set; } = Array.Empty<long>();
    }
}
