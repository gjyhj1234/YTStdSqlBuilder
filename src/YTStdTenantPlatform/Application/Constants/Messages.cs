namespace YTStdTenantPlatform.Application.Constants;

/// <summary>国际化消息键常量，后端返回给前端的所有提示信息使用消息键而非硬编码文本</summary>
public static class Messages
{
    /// <summary>操作成功</summary>
    public const string OperationSuccess = "operation.success";
    /// <summary>操作失败</summary>
    public const string OperationFailed = "operation.failed";

    // ── 通用消息 ──
    /// <summary>请求体无效</summary>
    public const string InvalidRequestBody = "common.invalid_request_body";
    /// <summary>资源不存在</summary>
    public const string ResourceNotFound = "common.resource_not_found";
    /// <summary>服务器内部错误</summary>
    public const string InternalServerError = "common.internal_server_error";
    /// <summary>请联系管理员或稍后重试</summary>
    public const string ContactAdminOrRetry = "common.contact_admin_or_retry";
    /// <summary>参数错误</summary>
    public const string InvalidParameter = "common.invalid_parameter";
    /// <summary>操作无效</summary>
    public const string InvalidOperation = "common.invalid_operation";
    /// <summary>权限不足</summary>
    public const string Forbidden = "common.forbidden";
    /// <summary>系统繁忙，请稍后再试</summary>
    public const string SystemBusy = "common.system_busy";

    // ── 认证消息 ──
    /// <summary>用户名或密码不能为空</summary>
    public const string AuthCredentialsRequired = "auth.credentials_required";
    /// <summary>用户名或密码错误</summary>
    public const string AuthInvalidCredentials = "auth.invalid_credentials";
    /// <summary>账户已禁用</summary>
    public const string AuthAccountDisabled = "auth.account_disabled";
    /// <summary>账户已锁定</summary>
    public const string AuthAccountLocked = "auth.account_locked";
    /// <summary>登录状态更新失败</summary>
    public const string AuthLoginUpdateFailed = "auth.login_update_failed";
    /// <summary>令牌无效或已过期</summary>
    public const string AuthTokenInvalid = "auth.token_invalid";
    /// <summary>登录成功</summary>
    public const string AuthLoginSuccess = "auth.login_success";
    /// <summary>登录成功，需尽快修改密码</summary>
    public const string AuthLoginSuccessPasswordExpired = "auth.login_success_password_expired";
    /// <summary>刷新成功</summary>
    public const string AuthRefreshSuccess = "auth.refresh_success";

    // ── 平台用户消息 ──
    /// <summary>用户名不能为空</summary>
    public const string UserUsernameRequired = "user.username_required";
    /// <summary>邮箱不能为空</summary>
    public const string UserEmailRequired = "user.email_required";
    /// <summary>密码不能为空</summary>
    public const string UserPasswordRequired = "user.password_required";
    /// <summary>创建用户失败</summary>
    public const string UserCreateFailed = "user.create_failed";
    /// <summary>查询用户失败</summary>
    public const string UserQueryFailed = "user.query_failed";
    /// <summary>用户不存在</summary>
    public const string UserNotFound = "user.not_found";
    /// <summary>更新用户失败</summary>
    public const string UserUpdateFailed = "user.update_failed";
    /// <summary>用户状态变更失败</summary>
    public const string UserStatusChangeFailed = "user.status_change_failed";

    // ── 平台角色消息 ──
    /// <summary>角色编码不能为空</summary>
    public const string RoleCodeRequired = "role.code_required";
    /// <summary>角色名称不能为空</summary>
    public const string RoleNameRequired = "role.name_required";
    /// <summary>创建角色失败</summary>
    public const string RoleCreateFailed = "role.create_failed";
    /// <summary>查询角色失败</summary>
    public const string RoleQueryFailed = "role.query_failed";
    /// <summary>角色不存在</summary>
    public const string RoleNotFound = "role.not_found";
    /// <summary>更新角色失败</summary>
    public const string RoleUpdateFailed = "role.update_failed";
    /// <summary>角色状态变更失败</summary>
    public const string RoleStatusChangeFailed = "role.status_change_failed";

    // ── 租户生命周期消息 ──
    /// <summary>租户编码不能为空</summary>
    public const string TenantCodeRequired = "tenant.code_required";
    /// <summary>租户名称不能为空</summary>
    public const string TenantNameRequired = "tenant.name_required";
    /// <summary>创建租户失败</summary>
    public const string TenantCreateFailed = "tenant.create_failed";
    /// <summary>查询租户失败</summary>
    public const string TenantQueryFailed = "tenant.query_failed";
    /// <summary>租户不存在</summary>
    public const string TenantNotFound = "tenant.not_found";
    /// <summary>更新租户失败</summary>
    public const string TenantUpdateFailed = "tenant.update_failed";
    /// <summary>租户状态变更失败</summary>
    public const string TenantStatusChangeFailed = "tenant.status_change_failed";
    /// <summary>租户状态流转不允许</summary>
    public const string TenantStatusTransitionDenied = "tenant.status_transition_denied";

    // ── 租户信息消息 ──
    /// <summary>分组编码不能为空</summary>
    public const string GroupCodeRequired = "group.code_required";
    /// <summary>创建分组失败</summary>
    public const string GroupCreateFailed = "group.create_failed";
    /// <summary>域名不能为空</summary>
    public const string DomainRequired = "domain.required";
    /// <summary>创建域名失败</summary>
    public const string DomainCreateFailed = "domain.create_failed";
    /// <summary>标签键不能为空</summary>
    public const string TagKeyRequired = "tag.key_required";
    /// <summary>创建标签失败</summary>
    public const string TagCreateFailed = "tag.create_failed";
    /// <summary>标签绑定失败</summary>
    public const string TagBindFailed = "tag.bind_failed";

    // ── 租户资源消息 ──
    /// <summary>配额类型不能为空</summary>
    public const string QuotaTypeRequired = "quota.type_required";
    /// <summary>配额上限必须大于0</summary>
    public const string QuotaLimitInvalid = "quota.limit_invalid";
    /// <summary>保存配额失败</summary>
    public const string QuotaSaveFailed = "quota.save_failed";

    // ── 租户配置消息 ──
    /// <summary>查询配置失败</summary>
    public const string ConfigQueryFailed = "config.query_failed";
    /// <summary>更新配置失败</summary>
    public const string ConfigUpdateFailed = "config.update_failed";
    /// <summary>功能键不能为空</summary>
    public const string FeatureKeyRequired = "feature.key_required";
    /// <summary>保存功能开关失败</summary>
    public const string FeatureFlagSaveFailed = "feature.save_failed";
    /// <summary>功能开关不存在</summary>
    public const string FeatureFlagNotFound = "feature.not_found";
    /// <summary>功能开关状态变更失败</summary>
    public const string FeatureFlagToggleFailed = "feature.toggle_failed";
    /// <summary>参数键不能为空</summary>
    public const string ParamKeyRequired = "param.key_required";
    /// <summary>保存参数失败</summary>
    public const string ParamSaveFailed = "param.save_failed";
    /// <summary>删除参数失败</summary>
    public const string ParamDeleteFailed = "param.delete_failed";

    // ── 套餐消息 ──
    /// <summary>套餐编码不能为空</summary>
    public const string PackageCodeRequired = "package.code_required";
    /// <summary>套餐名称不能为空</summary>
    public const string PackageNameRequired = "package.name_required";
    /// <summary>创建套餐失败</summary>
    public const string PackageCreateFailed = "package.create_failed";
    /// <summary>查询套餐失败</summary>
    public const string PackageQueryFailed = "package.query_failed";
    /// <summary>套餐不存在</summary>
    public const string PackageNotFound = "package.not_found";
    /// <summary>更新套餐失败</summary>
    public const string PackageUpdateFailed = "package.update_failed";
    /// <summary>套餐状态变更失败</summary>
    public const string PackageStatusChangeFailed = "package.status_change_failed";
    /// <summary>版本编码不能为空</summary>
    public const string PackageVersionCodeRequired = "package.version_code_required";
    /// <summary>版本名称不能为空</summary>
    public const string PackageVersionNameRequired = "package.version_name_required";
    /// <summary>创建版本失败</summary>
    public const string PackageVersionCreateFailed = "package.version_create_failed";
    /// <summary>能力键不能为空</summary>
    public const string PackageCapabilityKeyRequired = "package.capability_key_required";
    /// <summary>保存能力失败</summary>
    public const string PackageCapabilitySaveFailed = "package.capability_save_failed";

    // ── 订阅消息 ──
    /// <summary>创建订阅失败</summary>
    public const string SubscriptionCreateFailed = "subscription.create_failed";
    /// <summary>查询订阅失败</summary>
    public const string SubscriptionQueryFailed = "subscription.query_failed";
    /// <summary>订阅不存在</summary>
    public const string SubscriptionNotFound = "subscription.not_found";
    /// <summary>取消订阅失败</summary>
    public const string SubscriptionCancelFailed = "subscription.cancel_failed";
    /// <summary>创建试用失败</summary>
    public const string TrialCreateFailed = "subscription.trial_create_failed";

    // ── 计费消息 ──
    /// <summary>创建发票失败</summary>
    public const string InvoiceCreateFailed = "billing.invoice_create_failed";
    /// <summary>查询发票失败</summary>
    public const string InvoiceQueryFailed = "billing.invoice_query_failed";
    /// <summary>发票不存在</summary>
    public const string InvoiceNotFound = "billing.invoice_not_found";
    /// <summary>作废发票失败</summary>
    public const string InvoiceVoidFailed = "billing.invoice_void_failed";
    /// <summary>创建支付单失败</summary>
    public const string PaymentOrderCreateFailed = "billing.payment_create_failed";
    /// <summary>创建退款失败</summary>
    public const string RefundCreateFailed = "billing.refund_create_failed";

    // ── API集成消息 ──
    /// <summary>创建API密钥失败</summary>
    public const string ApiKeyCreateFailed = "api.key_create_failed";
    /// <summary>禁用API密钥失败</summary>
    public const string ApiKeyDisableFailed = "api.key_disable_failed";
    /// <summary>查询API密钥失败</summary>
    public const string ApiKeyQueryFailed = "api.key_query_failed";
    /// <summary>API密钥不存在</summary>
    public const string ApiKeyNotFound = "api.key_not_found";
    /// <summary>创建Webhook失败</summary>
    public const string WebhookCreateFailed = "api.webhook_create_failed";
    /// <summary>查询Webhook失败</summary>
    public const string WebhookQueryFailed = "api.webhook_query_failed";
    /// <summary>Webhook不存在</summary>
    public const string WebhookNotFound = "api.webhook_not_found";
    /// <summary>更新Webhook失败</summary>
    public const string WebhookUpdateFailed = "api.webhook_update_failed";
    /// <summary>Webhook状态变更失败</summary>
    public const string WebhookStatusChangeFailed = "api.webhook_status_change_failed";

    // ── 通知消息 ──
    /// <summary>模板名称不能为空</summary>
    public const string NotificationTemplateNameRequired = "notification.template_name_required";
    /// <summary>创建通知模板失败</summary>
    public const string NotificationTemplateCreateFailed = "notification.template_create_failed";
    /// <summary>查询通知模板失败</summary>
    public const string NotificationTemplateQueryFailed = "notification.template_query_failed";
    /// <summary>通知模板不存在</summary>
    public const string NotificationTemplateNotFound = "notification.template_not_found";
    /// <summary>更新通知模板失败</summary>
    public const string NotificationTemplateUpdateFailed = "notification.template_update_failed";
    /// <summary>通知模板状态变更失败</summary>
    public const string NotificationTemplateStatusChangeFailed = "notification.template_status_change_failed";
    /// <summary>创建通知失败</summary>
    public const string NotificationCreateFailed = "notification.create_failed";
    /// <summary>查询通知失败</summary>
    public const string NotificationQueryFailed = "notification.query_failed";
    /// <summary>通知不存在</summary>
    public const string NotificationNotFound = "notification.not_found";
    /// <summary>标记通知已读失败</summary>
    public const string NotificationMarkReadFailed = "notification.mark_read_failed";

    // ── 存储消息 ──
    /// <summary>策略名称不能为空</summary>
    public const string StorageStrategyNameRequired = "storage.strategy_name_required";
    /// <summary>创建存储策略失败</summary>
    public const string StorageStrategyCreateFailed = "storage.strategy_create_failed";
    /// <summary>查询存储策略失败</summary>
    public const string StorageStrategyQueryFailed = "storage.strategy_query_failed";
    /// <summary>存储策略不存在</summary>
    public const string StorageStrategyNotFound = "storage.strategy_not_found";
    /// <summary>更新存储策略失败</summary>
    public const string StorageStrategyUpdateFailed = "storage.strategy_update_failed";
    /// <summary>存储策略状态变更失败</summary>
    public const string StorageStrategyStatusChangeFailed = "storage.strategy_status_change_failed";
    /// <summary>删除文件失败</summary>
    public const string FileDeleteFailed = "storage.file_delete_failed";
    /// <summary>保存文件访问策略失败</summary>
    public const string FileAccessPolicySaveFailed = "storage.file_access_policy_save_failed";
}
