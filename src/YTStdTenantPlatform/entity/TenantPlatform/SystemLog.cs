using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>系统日志</summary>
[Entity(TableName = "system_logs")]
public class SystemLog
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>服务名称</summary>
    [Column(Length = 128, IsRequired = true)]
    public string ServiceName { get; set; } = "";

    /// <summary>日志级别</summary>
    [Column(Length = 16, IsRequired = true)]
    public string LogLevel { get; set; } = "";

    /// <summary>链路 ID</summary>
    [Column(Length = 128)]
    public string? TraceId { get; set; }

    /// <summary>日志消息</summary>
    [Column(IsRequired = true)]
    public string Message { get; set; } = "";

    /// <summary>上下文</summary>
    [Column(DbType = "jsonb")]
    public string? Context { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }
}
