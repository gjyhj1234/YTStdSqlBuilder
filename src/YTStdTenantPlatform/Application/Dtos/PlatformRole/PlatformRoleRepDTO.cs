using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>平台角色列表项</summary>
    public sealed class PlatformRoleRepDTO
    {
        /// <summary>角色 ID</summary>
        public long Id { get; set; }
        /// <summary>角色编码</summary>
        public string Code { get; set; } = "";
        /// <summary>角色名称</summary>
        public string Name { get; set; } = "";
        /// <summary>描述</summary>
        public string? Description { get; set; }
        /// <summary>状态</summary>
        public string Status { get; set; } = "";
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }
}
