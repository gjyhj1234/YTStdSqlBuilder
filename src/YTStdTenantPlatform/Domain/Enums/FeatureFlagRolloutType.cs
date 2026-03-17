namespace YTStdTenantPlatform.Domain.Enums;

/// <summary>功能开关发布类型</summary>
public enum FeatureFlagRolloutType
{
    /// <summary>全量发布</summary>
    Full,

    /// <summary>关闭</summary>
    Closed,

    /// <summary>灰度发布</summary>
    Gray
}
