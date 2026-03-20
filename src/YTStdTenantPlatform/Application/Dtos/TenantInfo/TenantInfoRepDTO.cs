using System;
using System.Collections.Generic;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>租户分组列表项</summary>
    public sealed class TenantGroupRepDTO
    {
        /// <summary>分组 ID</summary>
        public long Id { get; set; }
        /// <summary>分组编码</summary>
        public string GroupCode { get; set; } = "";
        /// <summary>分组名称</summary>
        public string GroupName { get; set; } = "";
        /// <summary>描述</summary>
        public string? Description { get; set; }
        /// <summary>父级分组 ID</summary>
        public long? ParentId { get; set; }
        /// <summary>子节点列表</summary>
        public List<TenantGroupRepDTO>? Children { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>租户域名列表项</summary>
    public sealed class TenantDomainRepDTO
    {
        /// <summary>域名 ID</summary>
        public long Id { get; set; }
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>域名</summary>
        public string Domain { get; set; } = "";
        /// <summary>域名类型</summary>
        public string DomainType { get; set; } = "";
        /// <summary>是否为主域名</summary>
        public bool IsPrimary { get; set; }
        /// <summary>验证状态</summary>
        public string VerificationStatus { get; set; } = "";
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>租户标签列表项</summary>
    public sealed class TenantTagRepDTO
    {
        /// <summary>标签 ID</summary>
        public long Id { get; set; }
        /// <summary>标签键</summary>
        public string TagKey { get; set; } = "";
        /// <summary>标签值</summary>
        public string TagValue { get; set; } = "";
        /// <summary>标签类型</summary>
        public string TagType { get; set; } = "";
        /// <summary>描述</summary>
        public string? Description { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }
}
