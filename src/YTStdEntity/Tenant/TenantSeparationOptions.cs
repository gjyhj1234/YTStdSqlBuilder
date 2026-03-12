using System;

namespace YTStdEntity.Tenant;

/// <summary>租户分离配置</summary>
public sealed class TenantSeparationOptions
{
    /// <summary>目标库连接地址</summary>
    public string TargetConnectionString { get; set; } = "";

    /// <summary>要迁移的租户 ID 列表</summary>
    public int[] TenantIds { get; set; } = Array.Empty<int>();
}
