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
    /// <summary>租户配置中心端点（系统配置、功能开关、参数）</summary>
    public static class TenantConfigEndpoints
    {
        /// <summary>注册租户配置路由</summary>
        public static void Map(WebApplication app)
        {
            MapSystemConfigEndpoints(app);
            MapFeatureFlagEndpoints(app);
            MapParameterEndpoints(app);
        }

        /// <summary>注册系统配置路由</summary>
        private static void MapSystemConfigEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/tenant-system-configs")
                .WithTags("租户系统配置");

            group.MapGet("/{tenantRefId:long}", async (HttpContext ctx, long tenantRefId) =>
            {
                var user = GetCurrentUser(ctx);
                var result = await TenantConfigAppService.GetSystemConfigAsync(0, user.UserId, tenantRefId);
                if (result == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.ResourceNotFound, Messages.ResourceNotFound), 404); return; }
                await WriteJsonAsync(ctx, ApiResult<TenantSystemConfigRepDTO>.Ok(result));
            }).WithSummary("获取租户系统配置");

            group.MapPut("/{tenantRefId:long}", async (HttpContext ctx, long tenantRefId) =>
            {
                var user = GetCurrentUser(ctx);
                var req = await ctx.Request.ReadFromJsonAsync<UpdateTenantSystemConfigReqDTO>();
                if (req == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.InvalidRequestBody, Messages.InvalidRequestBody), 400); return; }
                var result = await TenantConfigAppService.UpdateSystemConfigAsync(0, user.UserId, tenantRefId, req);
                await WriteJsonAsync(ctx, result, result.Code == 0 ? 200 : 400);
            }).WithSummary("更新租户系统配置");
        }

        /// <summary>注册功能开关路由</summary>
        private static void MapFeatureFlagEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/tenant-feature-flags")
                .WithTags("租户功能开关");

            group.MapGet("/", async (HttpContext ctx, int? page, int? pageSize, string? keyword, long? tenantRefId) =>
            {
                var user = GetCurrentUser(ctx);
                var req = new PagedRequest { Page = page ?? 1, PageSize = pageSize ?? 20, Keyword = keyword };
                var result = await TenantConfigAppService.GetFeatureFlagListAsync(0, user.UserId, req, tenantRefId);
                await WriteJsonAsync(ctx, ApiResult<PagedResult<TenantFeatureFlagRepDTO>>.Ok(result));
            }).WithSummary("获取功能开关列表");

            group.MapPost("/", async (HttpContext ctx) =>
            {
                var user = GetCurrentUser(ctx);
                var req = await ctx.Request.ReadFromJsonAsync<SaveTenantFeatureFlagReqDTO>();
                if (req == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.InvalidRequestBody, Messages.InvalidRequestBody), 400); return; }
                var result = await TenantConfigAppService.SaveFeatureFlagAsync(0, user.UserId, req);
                if (result.Code != 0) { await WriteJsonAsync(ctx, ApiResult.Fail(result.Code, result.Message), 400); return; }
                ctx.Response.StatusCode = 201;
                await WriteJsonAsync(ctx, result);
            }).WithSummary("创建/更新功能开关");

            group.MapPut("/{id:long}/toggle", async (HttpContext ctx, long id, bool enabled) =>
            {
                var user = GetCurrentUser(ctx);
                var result = await TenantConfigAppService.ToggleFeatureFlagAsync(0, user.UserId, id, enabled);
                await WriteJsonAsync(ctx, result, result.Code == 0 ? 200 : 400);
            }).WithSummary("切换功能开关启停");
        }

        /// <summary>注册租户参数路由</summary>
        private static void MapParameterEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/tenant-parameters")
                .WithTags("租户参数");

            group.MapGet("/", async (HttpContext ctx, int? page, int? pageSize, string? keyword, long? tenantRefId) =>
            {
                var user = GetCurrentUser(ctx);
                var req = new PagedRequest { Page = page ?? 1, PageSize = pageSize ?? 20, Keyword = keyword };
                var result = await TenantConfigAppService.GetParameterListAsync(0, user.UserId, req, tenantRefId);
                await WriteJsonAsync(ctx, ApiResult<PagedResult<TenantParameterRepDTO>>.Ok(result));
            }).WithSummary("获取参数列表");

            group.MapPost("/", async (HttpContext ctx) =>
            {
                var user = GetCurrentUser(ctx);
                var req = await ctx.Request.ReadFromJsonAsync<SaveTenantParameterReqDTO>();
                if (req == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.InvalidRequestBody, Messages.InvalidRequestBody), 400); return; }
                var result = await TenantConfigAppService.SaveParameterAsync(0, user.UserId, req);
                if (result.Code != 0) { await WriteJsonAsync(ctx, ApiResult.Fail(result.Code, result.Message), 400); return; }
                ctx.Response.StatusCode = 201;
                await WriteJsonAsync(ctx, result);
            }).WithSummary("创建/更新参数");

            group.MapDelete("/{id:long}", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var result = await TenantConfigAppService.DeleteParameterAsync(0, user.UserId, id);
                await WriteJsonAsync(ctx, result, result.Code == 0 ? 200 : 400);
            }).WithSummary("删除参数");
        }

        private static CurrentUser GetCurrentUser(HttpContext ctx) =>
            ctx.Items.TryGetValue(CurrentUser.HttpContextKey, out var u) && u is CurrentUser cu ? cu : CurrentUser.Anonymous;

        private static async Task WriteJsonAsync<T>(HttpContext ctx, T data, int statusCode = 200)
        {
            await YTStdTenantPlatform.Infrastructure.Serialization.TenantPlatformJsonResponseWriter.WriteAsync(ctx, data, statusCode);
        }
    }
}
