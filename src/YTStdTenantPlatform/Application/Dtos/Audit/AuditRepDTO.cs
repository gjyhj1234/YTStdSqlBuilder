using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>操作日志响应数据</summary>
    public sealed class OperationLogRepDTO
    {
        /// <summary>日志ID</summary>
        public long Id { get; set; }
        /// <summary>操作用户ID</summary>
        public long? OperatorId { get; set; }
        /// <summary>操作用户名</summary>
        public string? OperatorName { get; set; }
        /// <summary>操作类型</summary>
        public string OperationType { get; set; } = "";
        /// <summary>操作描述</summary>
        public string? OperationDesc { get; set; }
        /// <summary>IP地址</summary>
        public string? IpAddress { get; set; }
        /// <summary>操作时间</summary>
        public DateTime OccurredAt { get; set; }
    }

    /// <summary>审计日志响应数据</summary>
    public sealed class AuditLogRepDTO
    {
        /// <summary>日志ID</summary>
        public long Id { get; set; }
        /// <summary>操作用户ID</summary>
        public long? OperatorId { get; set; }
        /// <summary>操作用户名</summary>
        public string? OperatorName { get; set; }
        /// <summary>审计类型</summary>
        public string AuditType { get; set; } = "";
        /// <summary>目标实体</summary>
        public string? TargetEntity { get; set; }
        /// <summary>目标ID</summary>
        public long? TargetId { get; set; }
        /// <summary>审计描述</summary>
        public string? AuditDesc { get; set; }
        /// <summary>发生时间</summary>
        public DateTime OccurredAt { get; set; }
    }

    /// <summary>系统日志响应数据</summary>
    public sealed class SystemLogRepDTO
    {
        /// <summary>日志ID</summary>
        public long Id { get; set; }
        /// <summary>日志级别</summary>
        public string LogLevel { get; set; } = "";
        /// <summary>日志消息</summary>
        public string Message { get; set; } = "";
        /// <summary>来源</summary>
        public string? Source { get; set; }
        /// <summary>异常信息</summary>
        public string? Exception { get; set; }
        /// <summary>发生时间</summary>
        public DateTime OccurredAt { get; set; }
    }
}
