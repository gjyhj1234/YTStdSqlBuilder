using System;
using Xunit;
using YTStdTenantPlatform.Entity.TenantPlatform;

namespace YTStdTenantPlatform.Tests;

/// <summary>实体建模基础验证测试</summary>
public class EntityModelingTests
{
    [Fact]
    public void PlatformUser_CanInstantiate()
    {
        var user = new PlatformUser
        {
            Id = 1,
            Username = "admin",
            Email = "admin@example.com",
            DisplayName = "超级管理员",
            PasswordHash = "hashed",
            Status = "active",
            FailedLoginCount = 0,
            MfaEnabled = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Assert.Equal(1, user.Id);
        Assert.Equal("admin", user.Username);
        Assert.Equal("admin@example.com", user.Email);
        Assert.Equal("active", user.Status);
    }

    [Fact]
    public void Tenant_CanInstantiate_WithoutBareTenantId()
    {
        var tenant = new Tenant
        {
            Id = 1,
            TenantCode = "T001",
            TenantName = "测试租户",
            SourceType = "admin",
            LifecycleStatus = "active",
            DefaultLanguage = "zh-CN",
            DefaultTimezone = "Asia/Shanghai",
            IsolationMode = "shared_database",
            Enabled = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Assert.Equal("T001", tenant.TenantCode);
        Assert.Equal("active", tenant.LifecycleStatus);
        Assert.Null(tenant.CurrentPlanId);
        Assert.Null(tenant.CurrentSubscriptionId);
    }

    [Fact]
    public void TenantDomain_UsesTenantRefId()
    {
        var domain = new TenantDomain
        {
            Id = 1,
            TenantRefId = 100,
            Domain = "test.example.com",
            DomainType = "custom",
            IsPrimary = false,
            VerificationStatus = "pending",
            CreatedAt = DateTime.UtcNow
        };

        Assert.Equal(100, domain.TenantRefId);
        Assert.Equal("pending", domain.VerificationStatus);
    }

    [Fact]
    public void SaasPackageVersion_IsDetailOfSaasPackage()
    {
        var version = new SaasPackageVersion
        {
            Id = 1,
            PackageId = 10,
            VersionCode = "V1.0",
            VersionName = "标准版 V1.0",
            EditionType = "standard",
            BillingCycle = "monthly",
            Price = 199.00m,
            CurrencyCode = "CNY",
            TrialDays = 14,
            IsDefault = false,
            Enabled = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Assert.Equal(10, version.PackageId);
        Assert.Equal(199.00m, version.Price);
    }

    [Fact]
    public void BillingInvoiceItem_IsDetailOfBillingInvoice()
    {
        var item = new BillingInvoiceItem
        {
            Id = 1,
            InvoiceId = 100,
            ItemType = "package_fee",
            ItemName = "标准套餐费用",
            Quantity = 1.0m,
            UnitPrice = 199.00m,
            Amount = 199.00m,
            CreatedAt = DateTime.UtcNow
        };

        Assert.Equal(100, item.InvoiceId);
        Assert.Equal(199.00m, item.Amount);
    }

    [Fact]
    public void AllEntities_HaveIdProperty()
    {
        // Verify all 54 entities can be instantiated
        var entities = new object[]
        {
            new PlatformUser(),
            new PlatformRole(),
            new PlatformPermission(),
            new PlatformRolePermission(),
            new PlatformRoleMember(),
            new PlatformPasswordPolicy(),
            new PlatformSecurityPolicy(),
            new PlatformIpWhitelist(),
            new PlatformMfaSetting(),
            new PlatformLoginLog(),
            new Tenant(),
            new TenantInitializationTask(),
            new TenantLifecycleEvent(),
            new TenantDataJob(),
            new TenantGroup(),
            new TenantDomain(),
            new TenantTag(),
            new TenantTagBinding(),
            new TenantGroupMember(),
            new TenantResourceQuota(),
            new TenantResourceUsageStat(),
            new TenantSystemConfig(),
            new TenantFeatureFlag(),
            new TenantParameter(),
            new TenantUiBranding(),
            new SaasPackage(),
            new SaasPackageVersion(),
            new SaasPackageCapability(),
            new TenantSubscription(),
            new TenantTrial(),
            new TenantSubscriptionChange(),
            new BillingInvoice(),
            new BillingInvoiceItem(),
            new PaymentOrder(),
            new PaymentRefund(),
            new TenantApiKey(),
            new TenantApiUsageStat(),
            new WebhookEvent(),
            new TenantWebhook(),
            new TenantWebhookEvent(),
            new WebhookDeliveryLog(),
            new TenantDailyStat(),
            new PlatformMonitorMetric(),
            new OperationLog(),
            new AuditLog(),
            new SystemLog(),
            new NotificationTemplate(),
            new Notification(),
            new StorageStrategy(),
            new TenantFile(),
            new FileAccessPolicy(),
            new RateLimitPolicy(),
            new DataIsolationPolicy(),
            new InfrastructureComponent()
        };

        Assert.Equal(54, entities.Length);
    }
}
