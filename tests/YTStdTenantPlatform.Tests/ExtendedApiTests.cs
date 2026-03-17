using System;
using Xunit;
using YTStdTenantPlatform.Application.Dtos;

namespace YTStdTenantPlatform.Tests
{
    /// <summary>扩展 API DTO 与请求测试</summary>
    public class ExtendedApiTests
    {
        // ──────────────────────────────────────────────────────────
        // SaaS 套餐系统
        // ──────────────────────────────────────────────────────────

        [Fact]
        public void SaasPackageDto_HasExpectedProperties()
        {
            var dto = new SaasPackageDto
            {
                Id = 1,
                PackageCode = "basic",
                PackageName = "基础版",
                Status = "active",
                CreatedAt = DateTime.UtcNow
            };
            Assert.Equal(1, dto.Id);
            Assert.Equal("basic", dto.PackageCode);
            Assert.Equal("基础版", dto.PackageName);
            Assert.Equal("active", dto.Status);
        }

        [Fact]
        public void CreateSaasPackageRequest_DefaultValues()
        {
            var req = new CreateSaasPackageRequest();
            Assert.Equal("", req.PackageCode);
            Assert.Equal("", req.PackageName);
            Assert.Null(req.Description);
        }

        [Fact]
        public void UpdateSaasPackageRequest_NullableProperties()
        {
            var req = new UpdateSaasPackageRequest();
            Assert.Null(req.PackageName);
            Assert.Null(req.Description);
        }

        [Fact]
        public void SaasPackageVersionDto_HasExpectedProperties()
        {
            var dto = new SaasPackageVersionDto
            {
                VersionCode = "v1.0",
                EditionType = "standard",
                BillingCycle = "monthly",
                Price = 99.00m,
                CurrencyCode = "CNY",
                TrialDays = 30
            };
            Assert.Equal("v1.0", dto.VersionCode);
            Assert.Equal("standard", dto.EditionType);
            Assert.Equal("monthly", dto.BillingCycle);
            Assert.Equal(99.00m, dto.Price);
            Assert.Equal("CNY", dto.CurrencyCode);
            Assert.Equal(30, dto.TrialDays);
        }

        [Fact]
        public void CreateSaasPackageVersionRequest_DefaultValues()
        {
            var req = new CreateSaasPackageVersionRequest();
            Assert.Equal("monthly", req.BillingCycle);
            Assert.Equal("CNY", req.CurrencyCode);
            Assert.Equal(0, req.TrialDays);
            Assert.False(req.IsDefault);
        }

        [Fact]
        public void SaasPackageCapabilityDto_HasExpectedProperties()
        {
            var dto = new SaasPackageCapabilityDto
            {
                CapabilityKey = "max_users",
                CapabilityType = "limit",
                CapabilityValue = "{\"value\":100}"
            };
            Assert.Equal("max_users", dto.CapabilityKey);
            Assert.Equal("limit", dto.CapabilityType);
            Assert.Equal("{\"value\":100}", dto.CapabilityValue);
        }

        [Fact]
        public void SaveSaasPackageCapabilityRequest_DefaultValues()
        {
            var req = new SaveSaasPackageCapabilityRequest();
            Assert.Equal("", req.CapabilityKey);
            Assert.Equal("", req.CapabilityName);
        }

        // ──────────────────────────────────────────────────────────
        // 订阅系统
        // ──────────────────────────────────────────────────────────

        [Fact]
        public void TenantSubscriptionDto_HasExpectedProperties()
        {
            var dto = new TenantSubscriptionDto
            {
                SubscriptionStatus = "active",
                SubscriptionType = "standard",
                AutoRenew = true
            };
            Assert.Equal("active", dto.SubscriptionStatus);
            Assert.Equal("standard", dto.SubscriptionType);
            Assert.True(dto.AutoRenew);
        }

        [Fact]
        public void CreateSubscriptionRequest_DefaultValues()
        {
            var req = new CreateSubscriptionRequest();
            Assert.Equal("standard", req.SubscriptionType);
            Assert.False(req.AutoRenew);
        }

        [Fact]
        public void TenantTrialDto_HasExpectedProperties()
        {
            var dto = new TenantTrialDto
            {
                Status = "active",
                ConvertedSubscriptionId = null
            };
            Assert.Equal("active", dto.Status);
            Assert.Null(dto.ConvertedSubscriptionId);
        }

        [Fact]
        public void CreateTrialRequest_DefaultValues()
        {
            var req = new CreateTrialRequest();
            Assert.Equal(0, req.TenantRefId);
            Assert.Equal(0, req.PackageVersionId);
        }

        [Fact]
        public void TenantSubscriptionChangeDto_HasExpectedProperties()
        {
            var dto = new TenantSubscriptionChangeDto
            {
                ChangeType = "upgrade"
            };
            Assert.Equal("upgrade", dto.ChangeType);
        }

        // ──────────────────────────────────────────────────────────
        // 计费系统
        // ──────────────────────────────────────────────────────────

        [Fact]
        public void BillingInvoiceDto_HasExpectedProperties()
        {
            var dto = new BillingInvoiceDto
            {
                InvoiceNo = "INV-001",
                InvoiceStatus = "draft",
                TotalAmount = 199.00m,
                CurrencyCode = "CNY"
            };
            Assert.Equal("INV-001", dto.InvoiceNo);
            Assert.Equal("draft", dto.InvoiceStatus);
            Assert.Equal(199.00m, dto.TotalAmount);
            Assert.Equal("CNY", dto.CurrencyCode);
        }

        [Fact]
        public void CreateBillingInvoiceRequest_DefaultValues()
        {
            var req = new CreateBillingInvoiceRequest();
            Assert.Equal("CNY", req.CurrencyCode);
        }

        [Fact]
        public void BillingInvoiceItemDto_HasExpectedProperties()
        {
            var dto = new BillingInvoiceItemDto
            {
                ItemType = "subscription",
                Amount = 100.00m
            };
            Assert.Equal("subscription", dto.ItemType);
            Assert.Equal(100.00m, dto.Amount);
        }

        [Fact]
        public void PaymentOrderDto_HasExpectedProperties()
        {
            var dto = new PaymentOrderDto
            {
                OrderNo = "PAY-001",
                PaymentChannel = "alipay",
                PaymentStatus = "pending",
                Amount = 199.00m
            };
            Assert.Equal("PAY-001", dto.OrderNo);
            Assert.Equal("alipay", dto.PaymentChannel);
            Assert.Equal("pending", dto.PaymentStatus);
            Assert.Equal(199.00m, dto.Amount);
        }

        [Fact]
        public void CreatePaymentOrderRequest_DefaultValues()
        {
            var req = new CreatePaymentOrderRequest();
            Assert.Equal("manual", req.PaymentChannel);
            Assert.Equal("CNY", req.CurrencyCode);
        }

        [Fact]
        public void PaymentRefundDto_HasExpectedProperties()
        {
            var dto = new PaymentRefundDto
            {
                RefundNo = "REF-001",
                RefundStatus = "pending",
                RefundAmount = 50.00m
            };
            Assert.Equal("REF-001", dto.RefundNo);
            Assert.Equal("pending", dto.RefundStatus);
            Assert.Equal(50.00m, dto.RefundAmount);
        }

        [Fact]
        public void CreateRefundRequest_DefaultValues()
        {
            var req = new CreateRefundRequest();
            Assert.Equal(0, req.RefundAmount);
            Assert.Null(req.RefundReason);
        }

        // ──────────────────────────────────────────────────────────
        // API 与集成平台
        // ──────────────────────────────────────────────────────────

        [Fact]
        public void TenantApiKeyDto_HasExpectedProperties()
        {
            var dto = new TenantApiKeyDto
            {
                KeyName = "Test Key",
                AccessKey = "ak_123",
                Status = "active"
            };
            Assert.Equal("Test Key", dto.KeyName);
            Assert.Equal("ak_123", dto.AccessKey);
            Assert.Equal("active", dto.Status);
        }

        [Fact]
        public void CreateApiKeyRequest_DefaultValues()
        {
            var req = new CreateApiKeyRequest();
            Assert.Equal("", req.KeyName);
            Assert.Null(req.ExpiresAt);
        }

        [Fact]
        public void ApiKeyCreatedResult_HasExpectedProperties()
        {
            var result = new ApiKeyCreatedResult
            {
                Id = 1,
                AccessKey = "ak_abc123",
                SecretKey = "secret123"
            };
            Assert.Equal(1, result.Id);
            Assert.Equal("ak_abc123", result.AccessKey);
            Assert.Equal("secret123", result.SecretKey);
        }

        [Fact]
        public void TenantApiUsageStatDto_HasExpectedProperties()
        {
            var dto = new TenantApiUsageStatDto
            {
                RequestCount = 1000,
                SuccessCount = 950,
                ErrorCount = 50
            };
            Assert.Equal(1000, dto.RequestCount);
            Assert.Equal(950, dto.SuccessCount);
            Assert.Equal(50, dto.ErrorCount);
        }

        [Fact]
        public void WebhookEventDto_HasExpectedProperties()
        {
            var dto = new WebhookEventDto
            {
                EventCode = "tenant.created",
                EventName = "租户创建"
            };
            Assert.Equal("tenant.created", dto.EventCode);
            Assert.Equal("租户创建", dto.EventName);
        }

        [Fact]
        public void TenantWebhookDto_HasExpectedProperties()
        {
            var dto = new TenantWebhookDto
            {
                WebhookName = "通知",
                TargetUrl = "https://example.com/hook",
                Status = "active"
            };
            Assert.Equal("通知", dto.WebhookName);
            Assert.Equal("https://example.com/hook", dto.TargetUrl);
            Assert.Equal("active", dto.Status);
        }

        [Fact]
        public void CreateWebhookRequest_DefaultValues()
        {
            var req = new CreateWebhookRequest();
            Assert.Equal("", req.WebhookName);
            Assert.Equal("", req.TargetUrl);
        }

        [Fact]
        public void UpdateWebhookRequest_NullableProperties()
        {
            var req = new UpdateWebhookRequest();
            Assert.Null(req.WebhookName);
            Assert.Null(req.TargetUrl);
        }

        [Fact]
        public void WebhookDeliveryLogDto_HasExpectedProperties()
        {
            var dto = new WebhookDeliveryLogDto
            {
                DeliveryStatus = "success",
                RetryCount = 0
            };
            Assert.Equal("success", dto.DeliveryStatus);
            Assert.Equal(0, dto.RetryCount);
        }

        // ──────────────────────────────────────────────────────────
        // 平台运营
        // ──────────────────────────────────────────────────────────

        [Fact]
        public void TenantDailyStatDto_HasExpectedProperties()
        {
            var dto = new TenantDailyStatDto
            {
                ActiveUserCount = 100,
                ApiCallCount = 50000,
                StorageBytes = 1073741824
            };
            Assert.Equal(100, dto.ActiveUserCount);
            Assert.Equal(50000, dto.ApiCallCount);
            Assert.Equal(1073741824, dto.StorageBytes);
        }

        [Fact]
        public void PlatformMonitorMetricDto_HasExpectedProperties()
        {
            var dto = new PlatformMonitorMetricDto
            {
                MetricType = "cpu",
                MetricKey = "usage",
                MetricValue = 75.5m,
                MetricUnit = "%"
            };
            Assert.Equal("cpu", dto.MetricType);
            Assert.Equal("usage", dto.MetricKey);
            Assert.Equal(75.5m, dto.MetricValue);
            Assert.Equal("%", dto.MetricUnit);
        }

        // ──────────────────────────────────────────────────────────
        // 日志与审计
        // ──────────────────────────────────────────────────────────

        [Fact]
        public void OperationLogDto_HasExpectedProperties()
        {
            var dto = new OperationLogDto
            {
                OperatorType = "platform",
                Action = "create_tenant",
                OperationResult = "success"
            };
            Assert.Equal("platform", dto.OperatorType);
            Assert.Equal("create_tenant", dto.Action);
            Assert.Equal("success", dto.OperationResult);
        }

        [Fact]
        public void AuditLogDto_HasExpectedProperties()
        {
            var dto = new AuditLogDto
            {
                AuditType = "data_change",
                Severity = "high"
            };
            Assert.Equal("data_change", dto.AuditType);
            Assert.Equal("high", dto.Severity);
        }

        [Fact]
        public void SystemLogDto_HasExpectedProperties()
        {
            var dto = new SystemLogDto
            {
                ServiceName = "TenantPlatform",
                LogLevel = "error",
                Message = "测试日志"
            };
            Assert.Equal("TenantPlatform", dto.ServiceName);
            Assert.Equal("error", dto.LogLevel);
            Assert.Equal("测试日志", dto.Message);
        }

        // ──────────────────────────────────────────────────────────
        // 通知系统
        // ──────────────────────────────────────────────────────────

        [Fact]
        public void NotificationTemplateDto_HasExpectedProperties()
        {
            var dto = new NotificationTemplateDto
            {
                TemplateCode = "welcome",
                Channel = "email",
                BodyTemplate = "欢迎",
                Status = "active"
            };
            Assert.Equal("welcome", dto.TemplateCode);
            Assert.Equal("email", dto.Channel);
            Assert.Equal("欢迎", dto.BodyTemplate);
            Assert.Equal("active", dto.Status);
        }

        [Fact]
        public void CreateNotificationTemplateRequest_DefaultValues()
        {
            var req = new CreateNotificationTemplateRequest();
            Assert.Equal("email", req.Channel);
            Assert.Equal("", req.TemplateCode);
            Assert.Equal("", req.TemplateName);
        }

        [Fact]
        public void UpdateNotificationTemplateRequest_NullableProperties()
        {
            var req = new UpdateNotificationTemplateRequest();
            Assert.Null(req.TemplateName);
            Assert.Null(req.SubjectTemplate);
            Assert.Null(req.BodyTemplate);
        }

        [Fact]
        public void NotificationDto_HasExpectedProperties()
        {
            var dto = new NotificationDto
            {
                Channel = "email",
                Recipient = "test@test.com",
                SendStatus = "pending"
            };
            Assert.Equal("email", dto.Channel);
            Assert.Equal("test@test.com", dto.Recipient);
            Assert.Equal("pending", dto.SendStatus);
        }

        [Fact]
        public void CreateNotificationRequest_DefaultValues()
        {
            var req = new CreateNotificationRequest();
            Assert.Equal("email", req.Channel);
            Assert.Equal("", req.Recipient);
        }

        // ──────────────────────────────────────────────────────────
        // 文件与存储
        // ──────────────────────────────────────────────────────────

        [Fact]
        public void StorageStrategyDto_HasExpectedProperties()
        {
            var dto = new StorageStrategyDto
            {
                StrategyCode = "default",
                ProviderType = "local",
                Status = "active"
            };
            Assert.Equal("default", dto.StrategyCode);
            Assert.Equal("local", dto.ProviderType);
            Assert.Equal("active", dto.Status);
        }

        [Fact]
        public void CreateStorageStrategyRequest_DefaultValues()
        {
            var req = new CreateStorageStrategyRequest();
            Assert.Equal("local", req.ProviderType);
        }

        [Fact]
        public void UpdateStorageStrategyRequest_NullableProperties()
        {
            var req = new UpdateStorageStrategyRequest();
            Assert.Null(req.StrategyName);
            Assert.Null(req.BucketName);
            Assert.Null(req.BasePath);
        }

        [Fact]
        public void TenantFileDto_HasExpectedProperties()
        {
            var dto = new TenantFileDto
            {
                FileName = "test.pdf",
                FilePath = "/files/test.pdf",
                FileSize = 1024,
                UploaderType = "platform",
                Visibility = "private"
            };
            Assert.Equal("test.pdf", dto.FileName);
            Assert.Equal("/files/test.pdf", dto.FilePath);
            Assert.Equal(1024, dto.FileSize);
            Assert.Equal("platform", dto.UploaderType);
            Assert.Equal("private", dto.Visibility);
        }

        [Fact]
        public void FileAccessPolicyDto_HasExpectedProperties()
        {
            var dto = new FileAccessPolicyDto
            {
                SubjectType = "user",
                PermissionCode = "read"
            };
            Assert.Equal("user", dto.SubjectType);
            Assert.Equal("read", dto.PermissionCode);
        }

        [Fact]
        public void SaveFileAccessPolicyRequest_DefaultValues()
        {
            var req = new SaveFileAccessPolicyRequest();
            Assert.Equal("read", req.PermissionCode);
        }
    }
}
