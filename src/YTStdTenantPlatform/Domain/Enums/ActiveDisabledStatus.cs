namespace YTStdTenantPlatform.Domain.Enums;

/// <summary>启用/禁用状态（适用于IP白名单、租户Webhook、通知模板、存储策略、限流策略、数据隔离策略等）</summary>
public enum ActiveDisabledStatus
{
    /// <summary>正常</summary>
    Active,

    /// <summary>已禁用</summary>
    Disabled
}
