using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using YTStdAdo;
using YTStdLogger.Core;
using YTStdTenantPlatform.Bootstrap;

// 启动日志
Logger.Init(new YTStdLogger.Logging.LogOptions
{
    RootPath = root,
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
// ──────────────────────────────────────────────────────────────
// 租户平台单体 WebAPI 主程序入口
// AOT 友好的 Minimal API 架构
// ──────────────────────────────────────────────────────────────

var builder = WebApplication.CreateSlimBuilder(args);

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
