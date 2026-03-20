namespace YTStdTenantPlatform.Application.Constants;

/// <summary>API 错误码常量定义，用于前端国际化处理</summary>
public static class ErrorCodes
{
    /// <summary>操作成功</summary>
    public const int Success = 0;

    // ── 通用错误 (1xxx) ──
    /// <summary>请求体无效</summary>
    public const int InvalidRequestBody = 1001;
    /// <summary>资源不存在</summary>
    public const int ResourceNotFound = 1002;
    /// <summary>操作失败</summary>
    public const int OperationFailed = 1003;
    /// <summary>服务器内部错误</summary>
    public const int InternalServerError = 1004;
    /// <summary>参数错误</summary>
    public const int InvalidParameter = 1005;
    /// <summary>操作无效</summary>
    public const int InvalidOperation = 1006;
    /// <summary>权限不足</summary>
    public const int Forbidden = 1007;
    /// <summary>系统繁忙</summary>
    public const int SystemBusy = 1008;

    // ── 认证错误 (2xxx) ──
    /// <summary>用户名或密码不能为空</summary>
    public const int AuthCredentialsRequired = 2001;
    /// <summary>用户名或密码错误</summary>
    public const int AuthInvalidCredentials = 2002;
    /// <summary>账户已禁用</summary>
    public const int AuthAccountDisabled = 2003;
    /// <summary>账户已锁定</summary>
    public const int AuthAccountLocked = 2004;
    /// <summary>登录状态更新失败</summary>
    public const int AuthLoginUpdateFailed = 2005;
    /// <summary>令牌无效或已过期</summary>
    public const int AuthTokenInvalid = 2006;

    // ── 平台用户错误 (3xxx) ──
    /// <summary>用户名不能为空</summary>
    public const int UserUsernameRequired = 3001;
    /// <summary>邮箱不能为空</summary>
    public const int UserEmailRequired = 3002;
    /// <summary>密码不能为空</summary>
    public const int UserPasswordRequired = 3003;
    /// <summary>创建用户失败</summary>
    public const int UserCreateFailed = 3004;
    /// <summary>查询用户失败</summary>
    public const int UserQueryFailed = 3005;
    /// <summary>用户不存在</summary>
    public const int UserNotFound = 3006;
    /// <summary>更新用户失败</summary>
    public const int UserUpdateFailed = 3007;
    /// <summary>用户状态变更失败</summary>
    public const int UserStatusChangeFailed = 3008;

    // ── 平台角色错误 (4xxx) ──
    /// <summary>角色编码不能为空</summary>
    public const int RoleCodeRequired = 4001;
    /// <summary>角色名称不能为空</summary>
    public const int RoleNameRequired = 4002;
    /// <summary>创建角色失败</summary>
    public const int RoleCreateFailed = 4003;
    /// <summary>查询角色失败</summary>
    public const int RoleQueryFailed = 4004;
    /// <summary>角色不存在</summary>
    public const int RoleNotFound = 4005;
    /// <summary>更新角色失败</summary>
    public const int RoleUpdateFailed = 4006;
    /// <summary>角色状态变更失败</summary>
    public const int RoleStatusChangeFailed = 4007;
    /// <summary>角色权限绑定失败</summary>
    public const int RolePermissionBindFailed = 4008;
    /// <summary>角色成员绑定失败</summary>
    public const int RoleMemberBindFailed = 4009;

    // ── 权限错误 (5xxx) ──
    // (reserved for future use)

    // ── 租户生命周期错误 (6xxx) ──
    /// <summary>租户编码不能为空</summary>
    public const int TenantCodeRequired = 6001;
    /// <summary>租户名称不能为空</summary>
    public const int TenantNameRequired = 6002;
    /// <summary>创建租户失败</summary>
    public const int TenantCreateFailed = 6003;
    /// <summary>查询租户失败</summary>
    public const int TenantQueryFailed = 6004;
    /// <summary>租户不存在</summary>
    public const int TenantNotFound = 6005;
    /// <summary>更新租户失败</summary>
    public const int TenantUpdateFailed = 6006;
    /// <summary>租户状态变更失败</summary>
    public const int TenantStatusChangeFailed = 6007;
    /// <summary>租户状态流转不允许</summary>
    public const int TenantStatusTransitionDenied = 6008;

    // ── 租户信息错误 (7xxx) ──
    /// <summary>分组编码不能为空</summary>
    public const int GroupCodeRequired = 7001;
    /// <summary>创建分组失败</summary>
    public const int GroupCreateFailed = 7002;
    /// <summary>域名不能为空</summary>
    public const int DomainRequired = 7003;
    /// <summary>创建域名失败</summary>
    public const int DomainCreateFailed = 7004;
    /// <summary>标签键不能为空</summary>
    public const int TagKeyRequired = 7005;
    /// <summary>创建标签失败</summary>
    public const int TagCreateFailed = 7006;
    /// <summary>标签绑定失败</summary>
    public const int TagBindFailed = 7007;

    // ── 租户资源错误 (8xxx) ──
    /// <summary>配额类型不能为空</summary>
    public const int QuotaTypeRequired = 8001;
    /// <summary>配额上限必须大于0</summary>
    public const int QuotaLimitInvalid = 8002;
    /// <summary>保存配额失败</summary>
    public const int QuotaSaveFailed = 8003;

    // ── 租户配置错误 (9xxx) ──
    /// <summary>查询配置失败</summary>
    public const int ConfigQueryFailed = 9001;
    /// <summary>更新配置失败</summary>
    public const int ConfigUpdateFailed = 9002;
    /// <summary>功能键不能为空</summary>
    public const int FeatureKeyRequired = 9003;
    /// <summary>保存功能开关失败</summary>
    public const int FeatureFlagSaveFailed = 9004;
    /// <summary>功能开关不存在</summary>
    public const int FeatureFlagNotFound = 9005;
    /// <summary>功能开关状态变更失败</summary>
    public const int FeatureFlagToggleFailed = 9006;
    /// <summary>参数键不能为空</summary>
    public const int ParamKeyRequired = 9007;
    /// <summary>保存参数失败</summary>
    public const int ParamSaveFailed = 9008;
    /// <summary>删除参数失败</summary>
    public const int ParamDeleteFailed = 9009;

    // ── 套餐错误 (10xxx) ──
    /// <summary>套餐编码不能为空</summary>
    public const int PackageCodeRequired = 10001;
    /// <summary>套餐名称不能为空</summary>
    public const int PackageNameRequired = 10002;
    /// <summary>创建套餐失败</summary>
    public const int PackageCreateFailed = 10003;
    /// <summary>查询套餐失败</summary>
    public const int PackageQueryFailed = 10004;
    /// <summary>套餐不存在</summary>
    public const int PackageNotFound = 10005;
    /// <summary>更新套餐失败</summary>
    public const int PackageUpdateFailed = 10006;
    /// <summary>套餐状态变更失败</summary>
    public const int PackageStatusChangeFailed = 10007;
    /// <summary>版本编码不能为空</summary>
    public const int PackageVersionCodeRequired = 10008;
    /// <summary>版本名称不能为空</summary>
    public const int PackageVersionNameRequired = 10009;
    /// <summary>创建版本失败</summary>
    public const int PackageVersionCreateFailed = 10010;
    /// <summary>能力键不能为空</summary>
    public const int PackageCapabilityKeyRequired = 10011;
    /// <summary>保存能力失败</summary>
    public const int PackageCapabilitySaveFailed = 10012;

    // ── 订阅错误 (11xxx) ──
    /// <summary>创建订阅失败</summary>
    public const int SubscriptionCreateFailed = 11001;
    /// <summary>查询订阅失败</summary>
    public const int SubscriptionQueryFailed = 11002;
    /// <summary>订阅不存在</summary>
    public const int SubscriptionNotFound = 11003;
    /// <summary>取消订阅失败</summary>
    public const int SubscriptionCancelFailed = 11004;
    /// <summary>创建试用失败</summary>
    public const int TrialCreateFailed = 11005;

    // ── 计费错误 (12xxx) ──
    /// <summary>创建发票失败</summary>
    public const int InvoiceCreateFailed = 12001;
    /// <summary>查询发票失败</summary>
    public const int InvoiceQueryFailed = 12002;
    /// <summary>发票不存在</summary>
    public const int InvoiceNotFound = 12003;
    /// <summary>作废发票失败</summary>
    public const int InvoiceVoidFailed = 12004;
    /// <summary>创建支付单失败</summary>
    public const int PaymentOrderCreateFailed = 12005;
    /// <summary>创建退款失败</summary>
    public const int RefundCreateFailed = 12006;

    // ── API集成错误 (13xxx) ──
    /// <summary>创建API密钥失败</summary>
    public const int ApiKeyCreateFailed = 13001;
    /// <summary>禁用API密钥失败</summary>
    public const int ApiKeyDisableFailed = 13002;
    /// <summary>查询API密钥失败</summary>
    public const int ApiKeyQueryFailed = 13003;
    /// <summary>API密钥不存在</summary>
    public const int ApiKeyNotFound = 13004;
    /// <summary>创建Webhook失败</summary>
    public const int WebhookCreateFailed = 13005;
    /// <summary>查询Webhook失败</summary>
    public const int WebhookQueryFailed = 13006;
    /// <summary>Webhook不存在</summary>
    public const int WebhookNotFound = 13007;
    /// <summary>更新Webhook失败</summary>
    public const int WebhookUpdateFailed = 13008;
    /// <summary>Webhook状态变更失败</summary>
    public const int WebhookStatusChangeFailed = 13009;

    // ── 平台运营错误 (14xxx) ──
    // (reserved for future use)

    // ── 审计错误 (15xxx) ──
    // (reserved for future use)

    // ── 通知错误 (16xxx) ──
    /// <summary>模板名称不能为空</summary>
    public const int NotificationTemplateNameRequired = 16001;
    /// <summary>创建通知模板失败</summary>
    public const int NotificationTemplateCreateFailed = 16002;
    /// <summary>查询通知模板失败</summary>
    public const int NotificationTemplateQueryFailed = 16003;
    /// <summary>通知模板不存在</summary>
    public const int NotificationTemplateNotFound = 16004;
    /// <summary>更新通知模板失败</summary>
    public const int NotificationTemplateUpdateFailed = 16005;
    /// <summary>通知模板状态变更失败</summary>
    public const int NotificationTemplateStatusChangeFailed = 16006;
    /// <summary>创建通知失败</summary>
    public const int NotificationCreateFailed = 16007;
    /// <summary>查询通知失败</summary>
    public const int NotificationQueryFailed = 16008;
    /// <summary>通知不存在</summary>
    public const int NotificationNotFound = 16009;
    /// <summary>标记通知已读失败</summary>
    public const int NotificationMarkReadFailed = 16010;

    // ── 存储错误 (17xxx) ──
    /// <summary>策略名称不能为空</summary>
    public const int StorageStrategyNameRequired = 17001;
    /// <summary>创建存储策略失败</summary>
    public const int StorageStrategyCreateFailed = 17002;
    /// <summary>查询存储策略失败</summary>
    public const int StorageStrategyQueryFailed = 17003;
    /// <summary>存储策略不存在</summary>
    public const int StorageStrategyNotFound = 17004;
    /// <summary>更新存储策略失败</summary>
    public const int StorageStrategyUpdateFailed = 17005;
    /// <summary>存储策略状态变更失败</summary>
    public const int StorageStrategyStatusChangeFailed = 17006;
    /// <summary>删除文件失败</summary>
    public const int FileDeleteFailed = 17007;
    /// <summary>保存文件访问策略失败</summary>
    public const int FileAccessPolicySaveFailed = 17008;
}
