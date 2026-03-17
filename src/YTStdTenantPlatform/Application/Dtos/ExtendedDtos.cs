using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    // ──────────────────────────────────────────────────────────
    // Module 6: SaaS 套餐系统 DTO
    // ──────────────────────────────────────────────────────────

    /// <summary>SaaS 套餐列表项</summary>
    public sealed class SaasPackageDto
    {
        /// <summary>套餐 ID</summary>
        public long Id { get; set; }
        /// <summary>套餐编码</summary>
        public string PackageCode { get; set; } = "";
        /// <summary>套餐名称</summary>
        public string PackageName { get; set; } = "";
        /// <summary>描述</summary>
        public string? Description { get; set; }
        /// <summary>状态</summary>
        public string Status { get; set; } = "";
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>创建 SaaS 套餐请求</summary>
    public sealed class CreateSaasPackageRequest
    {
        /// <summary>套餐编码</summary>
        public string PackageCode { get; set; } = "";
        /// <summary>套餐名称</summary>
        public string PackageName { get; set; } = "";
        /// <summary>描述</summary>
        public string? Description { get; set; }
    }

    /// <summary>更新 SaaS 套餐请求</summary>
    public sealed class UpdateSaasPackageRequest
    {
        /// <summary>套餐名称</summary>
        public string? PackageName { get; set; }
        /// <summary>描述</summary>
        public string? Description { get; set; }
    }

    /// <summary>SaaS 套餐版本列表项</summary>
    public sealed class SaasPackageVersionDto
    {
        /// <summary>版本 ID</summary>
        public long Id { get; set; }
        /// <summary>套餐 ID</summary>
        public long PackageId { get; set; }
        /// <summary>版本编码</summary>
        public string VersionCode { get; set; } = "";
        /// <summary>版本名称</summary>
        public string VersionName { get; set; } = "";
        /// <summary>版本类型</summary>
        public string EditionType { get; set; } = "";
        /// <summary>计费周期</summary>
        public string BillingCycle { get; set; } = "";
        /// <summary>价格</summary>
        public decimal Price { get; set; }
        /// <summary>货币编码</summary>
        public string CurrencyCode { get; set; } = "";
        /// <summary>试用天数</summary>
        public int TrialDays { get; set; }
        /// <summary>是否默认</summary>
        public bool IsDefault { get; set; }
        /// <summary>是否启用</summary>
        public bool Enabled { get; set; }
        /// <summary>生效开始时间</summary>
        public DateTime? EffectiveFrom { get; set; }
        /// <summary>生效结束时间</summary>
        public DateTime? EffectiveTo { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>创建 SaaS 套餐版本请求</summary>
    public sealed class CreateSaasPackageVersionRequest
    {
        /// <summary>套餐 ID</summary>
        public long PackageId { get; set; }
        /// <summary>版本编码</summary>
        public string VersionCode { get; set; } = "";
        /// <summary>版本名称</summary>
        public string VersionName { get; set; } = "";
        /// <summary>版本类型</summary>
        public string EditionType { get; set; } = "";
        /// <summary>计费周期（monthly/quarterly/yearly）</summary>
        public string BillingCycle { get; set; } = "monthly";
        /// <summary>价格</summary>
        public decimal Price { get; set; }
        /// <summary>货币编码</summary>
        public string CurrencyCode { get; set; } = "CNY";
        /// <summary>试用天数</summary>
        public int TrialDays { get; set; } = 0;
        /// <summary>是否默认</summary>
        public bool IsDefault { get; set; } = false;
    }

    /// <summary>SaaS 套餐能力列表项</summary>
    public sealed class SaasPackageCapabilityDto
    {
        /// <summary>能力 ID</summary>
        public long Id { get; set; }
        /// <summary>套餐版本 ID</summary>
        public long PackageVersionId { get; set; }
        /// <summary>能力键</summary>
        public string CapabilityKey { get; set; } = "";
        /// <summary>能力名称</summary>
        public string CapabilityName { get; set; } = "";
        /// <summary>能力类型</summary>
        public string CapabilityType { get; set; } = "";
        /// <summary>能力值</summary>
        public string CapabilityValue { get; set; } = "";
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>创建/更新 SaaS 套餐能力请求</summary>
    public sealed class SaveSaasPackageCapabilityRequest
    {
        /// <summary>套餐版本 ID</summary>
        public long PackageVersionId { get; set; }
        /// <summary>能力键</summary>
        public string CapabilityKey { get; set; } = "";
        /// <summary>能力名称</summary>
        public string CapabilityName { get; set; } = "";
        /// <summary>能力类型</summary>
        public string CapabilityType { get; set; } = "";
        /// <summary>能力值</summary>
        public string CapabilityValue { get; set; } = "";
    }

    // ──────────────────────────────────────────────────────────
    // Module 7: 订阅系统 DTO
    // ──────────────────────────────────────────────────────────

    /// <summary>租户订阅列表项</summary>
    public sealed class TenantSubscriptionDto
    {
        /// <summary>订阅 ID</summary>
        public long Id { get; set; }
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>套餐版本 ID</summary>
        public long PackageVersionId { get; set; }
        /// <summary>订阅状态</summary>
        public string SubscriptionStatus { get; set; } = "";
        /// <summary>订阅类型</summary>
        public string SubscriptionType { get; set; } = "";
        /// <summary>开始时间</summary>
        public DateTime StartedAt { get; set; }
        /// <summary>到期时间</summary>
        public DateTime ExpiresAt { get; set; }
        /// <summary>是否自动续费</summary>
        public bool AutoRenew { get; set; }
        /// <summary>取消时间</summary>
        public DateTime? CancelledAt { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>创建订阅请求</summary>
    public sealed class CreateSubscriptionRequest
    {
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>套餐版本 ID</summary>
        public long PackageVersionId { get; set; }
        /// <summary>订阅类型（standard/enterprise/custom）</summary>
        public string SubscriptionType { get; set; } = "standard";
        /// <summary>是否自动续费</summary>
        public bool AutoRenew { get; set; } = false;
    }

    /// <summary>租户试用列表项</summary>
    public sealed class TenantTrialDto
    {
        /// <summary>试用 ID</summary>
        public long Id { get; set; }
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>套餐版本 ID</summary>
        public long? PackageVersionId { get; set; }
        /// <summary>状态</summary>
        public string Status { get; set; } = "";
        /// <summary>开始时间</summary>
        public DateTime StartedAt { get; set; }
        /// <summary>到期时间</summary>
        public DateTime ExpiresAt { get; set; }
        /// <summary>转化订阅 ID</summary>
        public long? ConvertedSubscriptionId { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>创建试用请求</summary>
    public sealed class CreateTrialRequest
    {
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>套餐版本 ID</summary>
        public long PackageVersionId { get; set; }
    }

    /// <summary>租户订阅变更列表项</summary>
    public sealed class TenantSubscriptionChangeDto
    {
        /// <summary>变更 ID</summary>
        public long Id { get; set; }
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>订阅 ID</summary>
        public long? SubscriptionId { get; set; }
        /// <summary>变更类型</summary>
        public string ChangeType { get; set; } = "";
        /// <summary>原套餐版本 ID</summary>
        public long? FromPackageVersionId { get; set; }
        /// <summary>目标套餐版本 ID</summary>
        public long? ToPackageVersionId { get; set; }
        /// <summary>生效时间</summary>
        public DateTime EffectiveAt { get; set; }
        /// <summary>备注</summary>
        public string? Remark { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    // ──────────────────────────────────────────────────────────
    // Module 8: 计费系统 DTO
    // ──────────────────────────────────────────────────────────

    /// <summary>账单发票列表项</summary>
    public sealed class BillingInvoiceDto
    {
        /// <summary>发票 ID</summary>
        public long Id { get; set; }
        /// <summary>发票编号</summary>
        public string InvoiceNo { get; set; } = "";
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>订阅 ID</summary>
        public long? SubscriptionId { get; set; }
        /// <summary>发票状态</summary>
        public string InvoiceStatus { get; set; } = "";
        /// <summary>账期开始</summary>
        public DateTime BillingPeriodStart { get; set; }
        /// <summary>账期结束</summary>
        public DateTime BillingPeriodEnd { get; set; }
        /// <summary>小计金额</summary>
        public decimal SubtotalAmount { get; set; }
        /// <summary>附加金额</summary>
        public decimal ExtraAmount { get; set; }
        /// <summary>折扣金额</summary>
        public decimal DiscountAmount { get; set; }
        /// <summary>总金额</summary>
        public decimal TotalAmount { get; set; }
        /// <summary>货币编码</summary>
        public string CurrencyCode { get; set; } = "";
        /// <summary>开票时间</summary>
        public DateTime? IssuedAt { get; set; }
        /// <summary>到期时间</summary>
        public DateTime? DueAt { get; set; }
        /// <summary>支付时间</summary>
        public DateTime? PaidAt { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>创建账单发票请求</summary>
    public sealed class CreateBillingInvoiceRequest
    {
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>订阅 ID</summary>
        public long? SubscriptionId { get; set; }
        /// <summary>账期开始</summary>
        public DateTime BillingPeriodStart { get; set; }
        /// <summary>账期结束</summary>
        public DateTime BillingPeriodEnd { get; set; }
        /// <summary>货币编码</summary>
        public string CurrencyCode { get; set; } = "CNY";
    }

    /// <summary>账单发票明细列表项</summary>
    public sealed class BillingInvoiceItemDto
    {
        /// <summary>明细 ID</summary>
        public long Id { get; set; }
        /// <summary>发票 ID</summary>
        public long InvoiceId { get; set; }
        /// <summary>项目类型</summary>
        public string ItemType { get; set; } = "";
        /// <summary>项目名称</summary>
        public string ItemName { get; set; } = "";
        /// <summary>数量</summary>
        public decimal Quantity { get; set; }
        /// <summary>单价</summary>
        public decimal UnitPrice { get; set; }
        /// <summary>金额</summary>
        public decimal Amount { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>支付订单列表项</summary>
    public sealed class PaymentOrderDto
    {
        /// <summary>订单 ID</summary>
        public long Id { get; set; }
        /// <summary>订单号</summary>
        public string OrderNo { get; set; } = "";
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>发票 ID</summary>
        public long? InvoiceId { get; set; }
        /// <summary>支付渠道</summary>
        public string PaymentChannel { get; set; } = "";
        /// <summary>支付状态</summary>
        public string PaymentStatus { get; set; } = "";
        /// <summary>金额</summary>
        public decimal Amount { get; set; }
        /// <summary>货币编码</summary>
        public string CurrencyCode { get; set; } = "";
        /// <summary>第三方交易号</summary>
        public string? ThirdPartyTxnNo { get; set; }
        /// <summary>支付时间</summary>
        public DateTime? PaidAt { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>创建支付订单请求</summary>
    public sealed class CreatePaymentOrderRequest
    {
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>发票 ID</summary>
        public long? InvoiceId { get; set; }
        /// <summary>支付渠道（manual/alipay/wechat/bank）</summary>
        public string PaymentChannel { get; set; } = "manual";
        /// <summary>金额</summary>
        public decimal Amount { get; set; }
        /// <summary>货币编码</summary>
        public string CurrencyCode { get; set; } = "CNY";
    }

    /// <summary>支付退款列表项</summary>
    public sealed class PaymentRefundDto
    {
        /// <summary>退款 ID</summary>
        public long Id { get; set; }
        /// <summary>退款编号</summary>
        public string RefundNo { get; set; } = "";
        /// <summary>支付订单 ID</summary>
        public long PaymentOrderId { get; set; }
        /// <summary>退款状态</summary>
        public string RefundStatus { get; set; } = "";
        /// <summary>退款金额</summary>
        public decimal RefundAmount { get; set; }
        /// <summary>退款原因</summary>
        public string? RefundReason { get; set; }
        /// <summary>退款时间</summary>
        public DateTime? RefundedAt { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>创建退款请求</summary>
    public sealed class CreateRefundRequest
    {
        /// <summary>支付订单 ID</summary>
        public long PaymentOrderId { get; set; }
        /// <summary>退款金额</summary>
        public decimal RefundAmount { get; set; }
        /// <summary>退款原因</summary>
        public string? RefundReason { get; set; }
    }

    // ──────────────────────────────────────────────────────────
    // Module 9: API 与集成平台 DTO
    // ──────────────────────────────────────────────────────────

    /// <summary>租户 API 密钥列表项</summary>
    public sealed class TenantApiKeyDto
    {
        /// <summary>密钥 ID</summary>
        public long Id { get; set; }
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>密钥名称</summary>
        public string KeyName { get; set; } = "";
        /// <summary>访问密钥</summary>
        public string AccessKey { get; set; } = "";
        /// <summary>状态</summary>
        public string Status { get; set; } = "";
        /// <summary>配额上限</summary>
        public long? QuotaLimit { get; set; }
        /// <summary>速率限制</summary>
        public int? RateLimit { get; set; }
        /// <summary>最后使用时间</summary>
        public DateTime? LastUsedAt { get; set; }
        /// <summary>过期时间</summary>
        public DateTime? ExpiresAt { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>创建 API 密钥请求</summary>
    public sealed class CreateApiKeyRequest
    {
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>密钥名称</summary>
        public string KeyName { get; set; } = "";
        /// <summary>过期时间</summary>
        public DateTime? ExpiresAt { get; set; }
    }

    /// <summary>API 密钥创建结果（包含仅在创建时返回的密钥明文）</summary>
    public sealed class ApiKeyCreatedResult
    {
        /// <summary>密钥 ID</summary>
        public long Id { get; set; }
        /// <summary>Access Key</summary>
        public string AccessKey { get; set; } = "";
        /// <summary>Secret Key（仅创建时返回一次，后续不可查看）</summary>
        public string SecretKey { get; set; } = "";
    }

    /// <summary>租户 API 用量统计列表项</summary>
    public sealed class TenantApiUsageStatDto
    {
        /// <summary>统计 ID</summary>
        public long Id { get; set; }
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>API 密钥 ID</summary>
        public long? ApiKeyId { get; set; }
        /// <summary>统计日期</summary>
        public DateTime StatDate { get; set; }
        /// <summary>API 路径</summary>
        public string ApiPath { get; set; } = "";
        /// <summary>请求次数</summary>
        public long RequestCount { get; set; }
        /// <summary>成功次数</summary>
        public long SuccessCount { get; set; }
        /// <summary>错误次数</summary>
        public long ErrorCount { get; set; }
        /// <summary>平均延迟（毫秒）</summary>
        public int AverageLatencyMs { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>Webhook 事件列表项</summary>
    public sealed class WebhookEventDto
    {
        /// <summary>事件 ID</summary>
        public long Id { get; set; }
        /// <summary>事件编码</summary>
        public string EventCode { get; set; } = "";
        /// <summary>事件名称</summary>
        public string EventName { get; set; } = "";
        /// <summary>描述</summary>
        public string? Description { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>租户 Webhook 列表项</summary>
    public sealed class TenantWebhookDto
    {
        /// <summary>Webhook ID</summary>
        public long Id { get; set; }
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>Webhook 名称</summary>
        public string WebhookName { get; set; } = "";
        /// <summary>目标地址</summary>
        public string TargetUrl { get; set; } = "";
        /// <summary>状态</summary>
        public string Status { get; set; } = "";
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>创建 Webhook 请求</summary>
    public sealed class CreateWebhookRequest
    {
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>Webhook 名称</summary>
        public string WebhookName { get; set; } = "";
        /// <summary>目标地址</summary>
        public string TargetUrl { get; set; } = "";
    }

    /// <summary>更新 Webhook 请求</summary>
    public sealed class UpdateWebhookRequest
    {
        /// <summary>Webhook 名称</summary>
        public string? WebhookName { get; set; }
        /// <summary>目标地址</summary>
        public string? TargetUrl { get; set; }
    }

    /// <summary>Webhook 投递日志列表项</summary>
    public sealed class WebhookDeliveryLogDto
    {
        /// <summary>日志 ID</summary>
        public long Id { get; set; }
        /// <summary>Webhook ID</summary>
        public long WebhookId { get; set; }
        /// <summary>事件 ID</summary>
        public long? EventId { get; set; }
        /// <summary>投递状态</summary>
        public string DeliveryStatus { get; set; } = "";
        /// <summary>响应状态码</summary>
        public int? ResponseStatusCode { get; set; }
        /// <summary>重试次数</summary>
        public int RetryCount { get; set; }
        /// <summary>投递时间</summary>
        public DateTime? DeliveredAt { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    // ──────────────────────────────────────────────────────────
    // Module 10: 平台运营 DTO
    // ──────────────────────────────────────────────────────────

    /// <summary>租户每日统计列表项</summary>
    public sealed class TenantDailyStatDto
    {
        /// <summary>统计 ID</summary>
        public long Id { get; set; }
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>统计日期</summary>
        public DateTime StatDate { get; set; }
        /// <summary>活跃用户数</summary>
        public int ActiveUserCount { get; set; }
        /// <summary>新增用户数</summary>
        public int NewUserCount { get; set; }
        /// <summary>API 调用次数</summary>
        public long ApiCallCount { get; set; }
        /// <summary>存储字节数</summary>
        public long StorageBytes { get; set; }
        /// <summary>资源评分</summary>
        public decimal ResourceScore { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>平台监控指标列表项</summary>
    public sealed class PlatformMonitorMetricDto
    {
        /// <summary>指标 ID</summary>
        public long Id { get; set; }
        /// <summary>组件名称</summary>
        public string ComponentName { get; set; } = "";
        /// <summary>指标类型</summary>
        public string MetricType { get; set; } = "";
        /// <summary>指标键</summary>
        public string MetricKey { get; set; } = "";
        /// <summary>指标值</summary>
        public decimal MetricValue { get; set; }
        /// <summary>指标单位</summary>
        public string? MetricUnit { get; set; }
        /// <summary>采集时间</summary>
        public DateTime CollectedAt { get; set; }
    }

    // ──────────────────────────────────────────────────────────
    // Module 11: 日志与审计 DTO
    // ──────────────────────────────────────────────────────────

    /// <summary>操作日志列表项</summary>
    public sealed class OperationLogDto
    {
        /// <summary>日志 ID</summary>
        public long Id { get; set; }
        /// <summary>关联租户 ID</summary>
        public long? TenantRefId { get; set; }
        /// <summary>操作者类型</summary>
        public string OperatorType { get; set; } = "";
        /// <summary>操作者 ID</summary>
        public long? OperatorId { get; set; }
        /// <summary>操作动作</summary>
        public string Action { get; set; } = "";
        /// <summary>资源类型</summary>
        public string? ResourceType { get; set; }
        /// <summary>资源 ID</summary>
        public string? ResourceId { get; set; }
        /// <summary>IP 地址</summary>
        public string? IpAddress { get; set; }
        /// <summary>操作结果</summary>
        public string OperationResult { get; set; } = "";
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>审计日志列表项</summary>
    public sealed class AuditLogDto
    {
        /// <summary>日志 ID</summary>
        public long Id { get; set; }
        /// <summary>关联租户 ID</summary>
        public long? TenantRefId { get; set; }
        /// <summary>审计类型</summary>
        public string AuditType { get; set; } = "";
        /// <summary>严重级别</summary>
        public string Severity { get; set; } = "";
        /// <summary>主体类型</summary>
        public string? SubjectType { get; set; }
        /// <summary>主体 ID</summary>
        public string? SubjectId { get; set; }
        /// <summary>合规标签</summary>
        public string? ComplianceTag { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>系统日志列表项</summary>
    public sealed class SystemLogDto
    {
        /// <summary>日志 ID</summary>
        public long Id { get; set; }
        /// <summary>服务名称</summary>
        public string ServiceName { get; set; } = "";
        /// <summary>日志级别</summary>
        public string LogLevel { get; set; } = "";
        /// <summary>链路 ID</summary>
        public string? TraceId { get; set; }
        /// <summary>日志消息</summary>
        public string Message { get; set; } = "";
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    // ──────────────────────────────────────────────────────────
    // Module 12: 通知系统 DTO
    // ──────────────────────────────────────────────────────────

    /// <summary>通知模板列表项</summary>
    public sealed class NotificationTemplateDto
    {
        /// <summary>模板 ID</summary>
        public long Id { get; set; }
        /// <summary>模板编码</summary>
        public string TemplateCode { get; set; } = "";
        /// <summary>模板名称</summary>
        public string TemplateName { get; set; } = "";
        /// <summary>通知渠道</summary>
        public string Channel { get; set; } = "";
        /// <summary>主题模板</summary>
        public string? SubjectTemplate { get; set; }
        /// <summary>正文模板</summary>
        public string BodyTemplate { get; set; } = "";
        /// <summary>状态</summary>
        public string Status { get; set; } = "";
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>创建通知模板请求</summary>
    public sealed class CreateNotificationTemplateRequest
    {
        /// <summary>模板编码</summary>
        public string TemplateCode { get; set; } = "";
        /// <summary>模板名称</summary>
        public string TemplateName { get; set; } = "";
        /// <summary>通知渠道（email/sms/webhook/in_app）</summary>
        public string Channel { get; set; } = "email";
        /// <summary>主题模板</summary>
        public string? SubjectTemplate { get; set; }
        /// <summary>正文模板</summary>
        public string BodyTemplate { get; set; } = "";
    }

    /// <summary>更新通知模板请求</summary>
    public sealed class UpdateNotificationTemplateRequest
    {
        /// <summary>模板名称</summary>
        public string? TemplateName { get; set; }
        /// <summary>主题模板</summary>
        public string? SubjectTemplate { get; set; }
        /// <summary>正文模板</summary>
        public string? BodyTemplate { get; set; }
    }

    /// <summary>通知列表项</summary>
    public sealed class NotificationDto
    {
        /// <summary>通知 ID</summary>
        public long Id { get; set; }
        /// <summary>关联租户 ID</summary>
        public long? TenantRefId { get; set; }
        /// <summary>模板 ID</summary>
        public long? TemplateId { get; set; }
        /// <summary>通知渠道</summary>
        public string Channel { get; set; } = "";
        /// <summary>接收人</summary>
        public string Recipient { get; set; } = "";
        /// <summary>主题</summary>
        public string? Subject { get; set; }
        /// <summary>正文</summary>
        public string Body { get; set; } = "";
        /// <summary>发送状态</summary>
        public string SendStatus { get; set; } = "";
        /// <summary>发送时间</summary>
        public DateTime? SentAt { get; set; }
        /// <summary>阅读时间</summary>
        public DateTime? ReadAt { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>创建通知请求</summary>
    public sealed class CreateNotificationRequest
    {
        /// <summary>关联租户 ID</summary>
        public long? TenantRefId { get; set; }
        /// <summary>模板 ID</summary>
        public long? TemplateId { get; set; }
        /// <summary>通知渠道（email/sms/webhook/in_app）</summary>
        public string Channel { get; set; } = "email";
        /// <summary>接收人</summary>
        public string Recipient { get; set; } = "";
        /// <summary>主题</summary>
        public string? Subject { get; set; }
        /// <summary>正文</summary>
        public string Body { get; set; } = "";
    }

    // ──────────────────────────────────────────────────────────
    // Module 13: 文件与存储 DTO
    // ──────────────────────────────────────────────────────────

    /// <summary>存储策略列表项</summary>
    public sealed class StorageStrategyDto
    {
        /// <summary>策略 ID</summary>
        public long Id { get; set; }
        /// <summary>策略编码</summary>
        public string StrategyCode { get; set; } = "";
        /// <summary>策略名称</summary>
        public string StrategyName { get; set; } = "";
        /// <summary>提供商类型</summary>
        public string ProviderType { get; set; } = "";
        /// <summary>存储桶名称</summary>
        public string? BucketName { get; set; }
        /// <summary>基础路径</summary>
        public string? BasePath { get; set; }
        /// <summary>状态</summary>
        public string Status { get; set; } = "";
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>创建存储策略请求</summary>
    public sealed class CreateStorageStrategyRequest
    {
        /// <summary>策略编码</summary>
        public string StrategyCode { get; set; } = "";
        /// <summary>策略名称</summary>
        public string StrategyName { get; set; } = "";
        /// <summary>提供商类型（local/s3/oss/azure）</summary>
        public string ProviderType { get; set; } = "local";
        /// <summary>存储桶名称</summary>
        public string? BucketName { get; set; }
        /// <summary>基础路径</summary>
        public string? BasePath { get; set; }
    }

    /// <summary>更新存储策略请求</summary>
    public sealed class UpdateStorageStrategyRequest
    {
        /// <summary>策略名称</summary>
        public string? StrategyName { get; set; }
        /// <summary>存储桶名称</summary>
        public string? BucketName { get; set; }
        /// <summary>基础路径</summary>
        public string? BasePath { get; set; }
    }

    /// <summary>租户文件列表项</summary>
    public sealed class TenantFileDto
    {
        /// <summary>文件 ID</summary>
        public long Id { get; set; }
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>存储策略 ID</summary>
        public long? StorageStrategyId { get; set; }
        /// <summary>文件名</summary>
        public string FileName { get; set; } = "";
        /// <summary>文件路径</summary>
        public string FilePath { get; set; } = "";
        /// <summary>文件扩展名</summary>
        public string? FileExt { get; set; }
        /// <summary>MIME 类型</summary>
        public string? MimeType { get; set; }
        /// <summary>文件大小</summary>
        public long FileSize { get; set; }
        /// <summary>上传者类型</summary>
        public string UploaderType { get; set; } = "";
        /// <summary>上传者 ID</summary>
        public long? UploaderId { get; set; }
        /// <summary>可见性</summary>
        public string Visibility { get; set; } = "";
        /// <summary>下载次数</summary>
        public long DownloadCount { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>文件访问策略列表项</summary>
    public sealed class FileAccessPolicyDto
    {
        /// <summary>策略 ID</summary>
        public long Id { get; set; }
        /// <summary>文件 ID</summary>
        public long FileId { get; set; }
        /// <summary>主体类型</summary>
        public string SubjectType { get; set; } = "";
        /// <summary>主体 ID</summary>
        public string? SubjectId { get; set; }
        /// <summary>权限编码</summary>
        public string PermissionCode { get; set; } = "";
        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>创建/更新文件访问策略请求</summary>
    public sealed class SaveFileAccessPolicyRequest
    {
        /// <summary>文件 ID</summary>
        public long FileId { get; set; }
        /// <summary>主体类型</summary>
        public string SubjectType { get; set; } = "";
        /// <summary>主体 ID</summary>
        public string? SubjectId { get; set; }
        /// <summary>权限编码（read/write/delete/admin）</summary>
        public string PermissionCode { get; set; } = "read";
    }
}
