using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using YTStdTenantPlatform.Application.Constants;
using YTStdTenantPlatform.Application.Dtos;
using YTStdTenantPlatform.Application.Services;
using YTStdTenantPlatform.Infrastructure.Auth;

namespace YTStdTenantPlatform.Endpoints
{
    /// <summary>API 与集成端点（API 密钥、用量统计、Webhook）</summary>
    public static class ApiIntegrationEndpoints
    {
        /// <summary>注册 API 与集成路由</summary>
        public static void Map(WebApplication app)
        {
            MapApiKeyEndpoints(app);
            MapApiUsageEndpoints(app);
            MapWebhookEventEndpoints(app);
            MapWebhookEndpoints(app);
            MapDeliveryLogEndpoints(app);
        }

        /// <summary>注册 API 密钥路由</summary>
        private static void MapApiKeyEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/tenant-api-keys")
                .WithTags("API 与集成");

            group.MapGet("/", async (HttpContext ctx, int? page, int? pageSize, string? keyword, long tenantRefId) =>
            {
                var user = GetCurrentUser(ctx);
                var req = new PagedRequest { Page = page ?? 1, PageSize = pageSize ?? 20, Keyword = keyword };
                var result = await ApiIntegrationAppService.GetApiKeyListAsync(0, user.UserId, tenantRefId, req);
                await WriteJsonAsync(ctx, ApiResult<PagedResult<TenantApiKeyRepDTO>>.Ok(result));
            }).WithSummary("获取 API 密钥列表");

            group.MapPost("/", async (HttpContext ctx) =>
            {
                var user = GetCurrentUser(ctx);
                var req = await ctx.Request.ReadFromJsonAsync<CreateApiKeyReqDTO>();
                if (req == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.InvalidRequestBody, Messages.InvalidRequestBody), 400); return; }
                var result = await ApiIntegrationAppService.CreateApiKeyAsync(0, user.UserId, req);
                if (result.Code != 0) { await WriteJsonAsync(ctx, ApiResult.Fail(result.Code, result.Message), 400); return; }
                ctx.Response.StatusCode = 201;
                await WriteJsonAsync(ctx, result);
            }).WithSummary("创建 API 密钥");

            group.MapPut("/{id:long}/disable", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var result = await ApiIntegrationAppService.DisableApiKeyAsync(0, user.UserId, id);
                await WriteJsonAsync(ctx, result, result.Code == 0 ? 200 : 400);
            }).WithSummary("禁用 API 密钥");
        }

        /// <summary>注册 API 用量统计路由</summary>
        private static void MapApiUsageEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/tenant-api-usage-stats")
                .WithTags("API 与集成");

            group.MapGet("/", async (HttpContext ctx, int? page, int? pageSize, string? keyword, long tenantRefId) =>
            {
                var user = GetCurrentUser(ctx);
                var req = new PagedRequest { Page = page ?? 1, PageSize = pageSize ?? 20, Keyword = keyword };
                var result = await ApiIntegrationAppService.GetApiUsageStatsAsync(0, user.UserId, tenantRefId, req);
                await WriteJsonAsync(ctx, ApiResult<PagedResult<TenantApiUsageStatRepDTO>>.Ok(result));
            }).WithSummary("获取 API 用量统计");
        }

        /// <summary>注册 Webhook 事件路由</summary>
        private static void MapWebhookEventEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/webhook-events")
                .WithTags("API 与集成");

            group.MapGet("/", async (HttpContext ctx, int? page, int? pageSize, string? keyword) =>
            {
                var user = GetCurrentUser(ctx);
                var req = new PagedRequest { Page = page ?? 1, PageSize = pageSize ?? 20, Keyword = keyword };
                var result = await ApiIntegrationAppService.GetWebhookEventListAsync(0, user.UserId, req);
                await WriteJsonAsync(ctx, ApiResult<PagedResult<WebhookEventRepDTO>>.Ok(result));
            }).WithSummary("获取 Webhook 事件列表");
        }

        /// <summary>注册 Webhook 路由</summary>
        private static void MapWebhookEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/tenant-webhooks")
                .WithTags("API 与集成");

            group.MapGet("/", async (HttpContext ctx, int? page, int? pageSize, string? keyword, long tenantRefId) =>
            {
                var user = GetCurrentUser(ctx);
                var req = new PagedRequest { Page = page ?? 1, PageSize = pageSize ?? 20, Keyword = keyword };
                var result = await ApiIntegrationAppService.GetWebhookListAsync(0, user.UserId, tenantRefId, req);
                await WriteJsonAsync(ctx, ApiResult<PagedResult<TenantWebhookRepDTO>>.Ok(result));
            }).WithSummary("获取 Webhook 列表");

            group.MapPost("/", async (HttpContext ctx) =>
            {
                var user = GetCurrentUser(ctx);
                var req = await ctx.Request.ReadFromJsonAsync<CreateWebhookReqDTO>();
                if (req == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.InvalidRequestBody, Messages.InvalidRequestBody), 400); return; }
                var result = await ApiIntegrationAppService.CreateWebhookAsync(0, user.UserId, req);
                if (result.Code != 0) { await WriteJsonAsync(ctx, ApiResult.Fail(result.Code, result.Message), 400); return; }
                ctx.Response.StatusCode = 201;
                await WriteJsonAsync(ctx, result);
            }).WithSummary("创建 Webhook");

            group.MapPut("/{id:long}", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var req = await ctx.Request.ReadFromJsonAsync<UpdateWebhookReqDTO>();
                if (req == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.InvalidRequestBody, Messages.InvalidRequestBody), 400); return; }
                var result = await ApiIntegrationAppService.UpdateWebhookAsync(0, user.UserId, id, req);
                if (result.Code != 0) { await WriteJsonAsync(ctx, result, 400); return; }
                await WriteJsonAsync(ctx, result);
            }).WithSummary("更新 Webhook");

            group.MapPut("/{id:long}/enable", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var result = await ApiIntegrationAppService.SetWebhookStatusAsync(0, user.UserId, id, "active");
                if (result.Code != 0) { await WriteJsonAsync(ctx, result, 400); return; }
                await WriteJsonAsync(ctx, result);
            }).WithSummary("启用 Webhook");

            group.MapPut("/{id:long}/disable", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var result = await ApiIntegrationAppService.SetWebhookStatusAsync(0, user.UserId, id, "disabled");
                if (result.Code != 0) { await WriteJsonAsync(ctx, result, 400); return; }
                await WriteJsonAsync(ctx, result);
            }).WithSummary("禁用 Webhook");
        }

        /// <summary>注册 Webhook 投递日志路由</summary>
        private static void MapDeliveryLogEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/webhook-delivery-logs")
                .WithTags("API 与集成");

            group.MapGet("/", async (HttpContext ctx, int? page, int? pageSize, string? keyword, long webhookId) =>
            {
                var user = GetCurrentUser(ctx);
                var req = new PagedRequest { Page = page ?? 1, PageSize = pageSize ?? 20, Keyword = keyword };
                var result = await ApiIntegrationAppService.GetDeliveryLogsAsync(0, user.UserId, webhookId, req);
                await WriteJsonAsync(ctx, ApiResult<PagedResult<WebhookDeliveryLogRepDTO>>.Ok(result));
            }).WithSummary("获取 Webhook 投递日志");
        }

        private static CurrentUser GetCurrentUser(HttpContext ctx) =>
            ctx.Items.TryGetValue(CurrentUser.HttpContextKey, out var u) && u is CurrentUser cu ? cu : CurrentUser.Anonymous;

        private static async Task WriteJsonAsync<T>(HttpContext ctx, T data, int statusCode = 200)
        {
            await YTStdTenantPlatform.Infrastructure.Serialization.TenantPlatformJsonResponseWriter.WriteAsync(ctx, data, statusCode);
        }
    }
}
