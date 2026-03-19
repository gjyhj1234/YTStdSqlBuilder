using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using YTStdAdo;
using YTStdLogger.Core;
using YTStdLogger.Logging;
using YTStdTenantPlatform.Bootstrap;
using YTStdTenantPlatform.Infrastructure.Auth;

// 启动日志
Logger.Init(new YTStdLogger.Logging.LogOptions
{
    MinimumLevel = LogLevel.Debug,
    QueueCapacity = 1024,
    BatchSize = 64,
    BatchMaxDelayMs = 100,
    RetentionMonths = 1,
    MaxLogsPerSecond = 10000,
    FlushEveryBatch = true,
    UseUtcTimestamp = false
});
// 启动数据库连接
DB.Init(new DbOptions
{
    Host = "localhost",
    Port = 5432,
    Database = "test1",
    Username = "postgres",
    Password = "gjwq1234",
    MinPoolSize = 5,
    MaxPoolSize = 20,
    ConnectionTimeoutSeconds = 30,
    RetryCount = 3,
    IdleTimeoutSeconds = 300
});
InitializeAuthSettings();
// ──────────────────────────────────────────────────────────────
// 租户平台单体 WebAPI 主程序入口
// AOT 友好的 Minimal API 架构
// ──────────────────────────────────────────────────────────────

var builder = WebApplication.CreateSlimBuilder(args);
builder.WebHost.UseUrls("http://127.0.0.1:5000");
// 服务注册
ServiceRegistration.ConfigureServices(builder);

var app = builder.Build();

// 中间件管道配置
ServiceRegistration.ConfigureMiddleware(app);

// 路由注册
RouteRegistration.MapRoutes(app);

// 启动初始化（建表、种子数据、缓存预热）
Logger.Info(0, 0, "[Program] 租户平台启动中...");
await StartupInitialization.InitializeAsync();
Logger.Info(0, 0, "[Program] 租户平台启动完成");

app.Run();

static void InitializeAuthSettings()
{
    var secret = Environment.GetEnvironmentVariable("YTSTD_TENANT_PLATFORM_TOKEN_SECRET");
    if (string.IsNullOrWhiteSpace(secret))
    {
        secret = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        Logger.Warn(0, 0, "[Program] 未配置 YTSTD_TENANT_PLATFORM_TOKEN_SECRET，已使用进程内随机密钥");
    }

    PlatformAuthHandler.SetTokenSecret(secret);

    var expiryValue = Environment.GetEnvironmentVariable("YTSTD_TENANT_PLATFORM_TOKEN_EXPIRY_SECONDS");
    if (!string.IsNullOrWhiteSpace(expiryValue) && int.TryParse(expiryValue, out var expirySeconds) && expirySeconds > 0)
    {
        PlatformAuthHandler.SetTokenExpiry(expirySeconds);
    }
}
