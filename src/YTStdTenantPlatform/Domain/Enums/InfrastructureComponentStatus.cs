namespace YTStdTenantPlatform.Domain.Enums;

/// <summary>基础设施组件状态</summary>
public enum InfrastructureComponentStatus
{
    /// <summary>正常</summary>
    Active,

    /// <summary>已禁用</summary>
    Disabled,

    /// <summary>降级</summary>
    Degraded
}
