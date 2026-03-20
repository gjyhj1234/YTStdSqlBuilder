using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>操作日志列表项</summary>
    public sealed class OperationLogRepDTO
    {
        /// <summary>日志 ID</summary>
        public long Id { get; set; }
        /// <summary>关联租户 ID</summary>
        public long? TenantRefId { get; set; }
        /// <summary>操作者类型</summary>
        public string OperatorType { get; set; } = "";
        /// <summary>操作者 ID</summary>
        public long? OperatorId { get; set; }
        /// <summary>操作动作</summary>
        public string Action { get; set; } = "";
        /// <summary>资源类型</summary>
        public string? ResourceType { get; set; }
        /// <summary>资源 ID</summary>
        public string? ResourceId { get; set; }
        /// <summary>IP 地址</summary>
        public string? IpAddress { get; set; }
        /// <summary>操作结果</summary>
        public string OperationResult { get; set; } = "";
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>审计日志列表项</summary>
    public sealed class AuditLogRepDTO
    {
        /// <summary>日志 ID</summary>
        public long Id { get; set; }
        /// <summary>关联租户 ID</summary>
        public long? TenantRefId { get; set; }
        /// <summary>审计类型</summary>
        public string AuditType { get; set; } = "";
        /// <summary>严重级别</summary>
        public string Severity { get; set; } = "";
        /// <summary>主体类型</summary>
        public string? SubjectType { get; set; }
        /// <summary>主体 ID</summary>
        public string? SubjectId { get; set; }
        /// <summary>合规标签</summary>
        public string? ComplianceTag { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>系统日志列表项</summary>
    public sealed class SystemLogRepDTO
    {
        /// <summary>日志 ID</summary>
        public long Id { get; set; }
        /// <summary>服务名称</summary>
        public string ServiceName { get; set; } = "";
        /// <summary>日志级别</summary>
        public string LogLevel { get; set; } = "";
        /// <summary>链路 ID</summary>
        public string? TraceId { get; set; }
        /// <summary>日志消息</summary>
        public string Message { get; set; } = "";
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }
}
