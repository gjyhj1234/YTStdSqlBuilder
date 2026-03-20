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
    /// <summary>订阅管理端点（订阅、试用、变更记录）</summary>
    public static class SubscriptionEndpoints
    {
        /// <summary>注册订阅管理路由</summary>
        public static void Map(WebApplication app)
        {
            MapSubscriptionEndpoints(app);
            MapTrialEndpoints(app);
            MapSubscriptionChangeEndpoints(app);
        }

        /// <summary>注册租户订阅路由</summary>
        private static void MapSubscriptionEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/tenant-subscriptions")
                .WithTags("订阅管理");

            group.MapGet("/", async (HttpContext ctx, int? page, int? pageSize, string? keyword, string? status) =>
            {
                var user = GetCurrentUser(ctx);
                var req = new PagedRequest { Page = page ?? 1, PageSize = pageSize ?? 20, Keyword = keyword, Status = status };
                var result = await SubscriptionAppService.GetSubscriptionListAsync(0, user.UserId, req);
                await WriteJsonAsync(ctx, ApiResult<PagedResult<TenantSubscriptionRepDTO>>.Ok(result));
            }).WithSummary("获取订阅分页列表");

            group.MapGet("/{id:long}", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var result = await SubscriptionAppService.GetSubscriptionByIdAsync(0, user.UserId, id);
                if (result == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.ResourceNotFound, Messages.ResourceNotFound), 404); return; }
                await WriteJsonAsync(ctx, ApiResult<TenantSubscriptionRepDTO>.Ok(result));
            }).WithSummary("获取订阅详情");

            group.MapPost("/", async (HttpContext ctx) =>
            {
                var user = GetCurrentUser(ctx);
                var req = await ctx.Request.ReadFromJsonAsync<CreateSubscriptionReqDTO>();
                if (req == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.InvalidRequestBody, Messages.InvalidRequestBody), 400); return; }
                var result = await SubscriptionAppService.CreateSubscriptionAsync(0, user.UserId, req);
                if (result.Code != 0) { await WriteJsonAsync(ctx, ApiResult.Fail(result.Code, result.Message), 400); return; }
                ctx.Response.StatusCode = 201;
                await WriteJsonAsync(ctx, result);
            }).WithSummary("创建订阅");

            group.MapPut("/{id:long}/cancel", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var result = await SubscriptionAppService.CancelSubscriptionAsync(0, user.UserId, id);
                await WriteJsonAsync(ctx, result, result.Code == 0 ? 200 : 400);
            }).WithSummary("取消订阅");
        }

        /// <summary>注册试用路由</summary>
        private static void MapTrialEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/tenant-trials")
                .WithTags("订阅管理");

            group.MapGet("/", async (HttpContext ctx, int? page, int? pageSize, string? keyword, string? status) =>
            {
                var user = GetCurrentUser(ctx);
                var req = new PagedRequest { Page = page ?? 1, PageSize = pageSize ?? 20, Keyword = keyword, Status = status };
                var result = await SubscriptionAppService.GetTrialListAsync(0, user.UserId, req);
                await WriteJsonAsync(ctx, ApiResult<PagedResult<TenantTrialRepDTO>>.Ok(result));
            }).WithSummary("获取试用分页列表");

            group.MapPost("/", async (HttpContext ctx) =>
            {
                var user = GetCurrentUser(ctx);
                var req = await ctx.Request.ReadFromJsonAsync<CreateTrialReqDTO>();
                if (req == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.InvalidRequestBody, Messages.InvalidRequestBody), 400); return; }
                var result = await SubscriptionAppService.CreateTrialAsync(0, user.UserId, req);
                if (result.Code != 0) { await WriteJsonAsync(ctx, ApiResult.Fail(result.Code, result.Message), 400); return; }
                ctx.Response.StatusCode = 201;
                await WriteJsonAsync(ctx, result);
            }).WithSummary("创建试用");
        }

        /// <summary>注册订阅变更记录路由</summary>
        private static void MapSubscriptionChangeEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/tenant-subscription-changes")
                .WithTags("订阅管理");

            group.MapGet("/", async (HttpContext ctx, int? page, int? pageSize, string? keyword, long tenantRefId) =>
            {
                var user = GetCurrentUser(ctx);
                var req = new PagedRequest { Page = page ?? 1, PageSize = pageSize ?? 20, Keyword = keyword };
                var result = await SubscriptionAppService.GetSubscriptionChangesAsync(0, user.UserId, tenantRefId, req);
                await WriteJsonAsync(ctx, ApiResult<PagedResult<TenantSubscriptionChangeRepDTO>>.Ok(result));
            }).WithSummary("获取订阅变更记录列表");
        }

        private static CurrentUser GetCurrentUser(HttpContext ctx) =>
            ctx.Items.TryGetValue(CurrentUser.HttpContextKey, out var u) && u is CurrentUser cu ? cu : CurrentUser.Anonymous;

        private static async Task WriteJsonAsync<T>(HttpContext ctx, T data, int statusCode = 200)
        {
            await YTStdTenantPlatform.Infrastructure.Serialization.TenantPlatformJsonResponseWriter.WriteAsync(ctx, data, statusCode);
        }
    }
}
