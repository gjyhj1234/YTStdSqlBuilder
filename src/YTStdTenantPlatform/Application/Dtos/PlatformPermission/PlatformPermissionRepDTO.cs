using System;
using System.Collections.Generic;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>平台权限树节点</summary>
    public sealed class PlatformPermissionRepDTO
    {
        /// <summary>权限 ID</summary>
        public long Id { get; set; }
        /// <summary>权限编码</summary>
        public string Code { get; set; } = "";
        /// <summary>权限名称</summary>
        public string Name { get; set; } = "";
        /// <summary>权限类型（menu/api/action）</summary>
        public string PermissionType { get; set; } = "";
        /// <summary>父级权限 ID</summary>
        public long? ParentId { get; set; }
        /// <summary>路径</summary>
        public string? Path { get; set; }
        /// <summary>HTTP 方法</summary>
        public string? Method { get; set; }
        /// <summary>子节点列表</summary>
        public System.Collections.Generic.List<PlatformPermissionRepDTO>? Children { get; set; }
    }
}
