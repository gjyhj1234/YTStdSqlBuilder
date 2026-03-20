using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>租户列表项</summary>
    public sealed class TenantRepDTO
    {
        /// <summary>租户 ID</summary>
        public long Id { get; set; }
        /// <summary>租户编码</summary>
        public string TenantCode { get; set; } = "";
        /// <summary>租户名称</summary>
        public string TenantName { get; set; } = "";
        /// <summary>企业名称</summary>
        public string? EnterpriseName { get; set; }
        /// <summary>联系人姓名</summary>
        public string? ContactName { get; set; }
        /// <summary>联系人邮箱</summary>
        public string? ContactEmail { get; set; }
        /// <summary>生命周期状态</summary>
        public string LifecycleStatus { get; set; } = "";
        /// <summary>隔离模式</summary>
        public string IsolationMode { get; set; } = "";
        /// <summary>是否启用</summary>
        public bool Enabled { get; set; }
        /// <summary>开通时间</summary>
        public DateTime? OpenedAt { get; set; }
        /// <summary>到期时间</summary>
        public DateTime? ExpiresAt { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>租户生命周期事件列表项</summary>
    public sealed class TenantLifecycleEventRepDTO
    {
        /// <summary>事件 ID</summary>
        public long Id { get; set; }
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>事件类型</summary>
        public string EventType { get; set; } = "";
        /// <summary>原状态</summary>
        public string? FromStatus { get; set; }
        /// <summary>目标状态</summary>
        public string? ToStatus { get; set; }
        /// <summary>原因</summary>
        public string? Reason { get; set; }
        /// <summary>发生时间</summary>
        public DateTime OccurredAt { get; set; }
    }
}
