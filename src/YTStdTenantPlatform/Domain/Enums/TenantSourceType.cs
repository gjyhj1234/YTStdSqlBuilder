namespace YTStdTenantPlatform.Domain.Enums;

/// <summary>租户来源类型</summary>
public enum TenantSourceType
{
    /// <summary>自助注册</summary>
    SelfService,

    /// <summary>管理员创建</summary>
    Admin,

    /// <summary>API创建</summary>
    Api
}
