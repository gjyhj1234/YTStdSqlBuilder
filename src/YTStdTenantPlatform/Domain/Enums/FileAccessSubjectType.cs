namespace YTStdTenantPlatform.Domain.Enums;

/// <summary>文件访问主体类型</summary>
public enum FileAccessSubjectType
{
    /// <summary>租户</summary>
    Tenant,

    /// <summary>用户</summary>
    User,

    /// <summary>角色</summary>
    Role,

    /// <summary>公开访问</summary>
    PublicAccess
}
