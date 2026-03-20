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
    /// <summary>租户资源管理端点</summary>
    public static class TenantResourceEndpoints
    {
        /// <summary>注册租户资源管理路由</summary>
        public static void Map(WebApplication app)
        {
            var group = app.MapGroup("/api/tenant-resource-quotas")
                .WithTags("租户资源管理");

            group.MapGet("/", async (HttpContext ctx, int? page, int? pageSize, string? keyword, long? tenantRefId) =>
            {
                var user = GetCurrentUser(ctx);
                var req = new PagedRequest { Page = page ?? 1, PageSize = pageSize ?? 20, Keyword = keyword };
                var result = await TenantResourceAppService.GetListAsync(0, user.UserId, req, tenantRefId);
                await WriteJsonAsync(ctx, ApiResult<PagedResult<TenantResourceQuotaRepDTO>>.Ok(result));
            }).WithSummary("获取资源配额列表");

            group.MapGet("/{id:long}", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var result = await TenantResourceAppService.GetByIdAsync(0, user.UserId, id);
                if (result == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.ResourceNotFound, Messages.ResourceNotFound), 404); return; }
                await WriteJsonAsync(ctx, ApiResult<TenantResourceQuotaRepDTO>.Ok(result));
            }).WithSummary("获取配额详情");

            group.MapPost("/", async (HttpContext ctx) =>
            {
                var user = GetCurrentUser(ctx);
                var req = await ctx.Request.ReadFromJsonAsync<SaveTenantResourceQuotaReqDTO>();
                if (req == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.InvalidRequestBody, Messages.InvalidRequestBody), 400); return; }
                var result = await TenantResourceAppService.SaveAsync(0, user.UserId, req);
                if (result.Code != 0) { await WriteJsonAsync(ctx, ApiResult.Fail(result.Code, result.Message), 400); return; }
                ctx.Response.StatusCode = 201;
                await WriteJsonAsync(ctx, result);
            }).WithSummary("创建/更新资源配额");
        }

        private static CurrentUser GetCurrentUser(HttpContext ctx) =>
            ctx.Items.TryGetValue(CurrentUser.HttpContextKey, out var u) && u is CurrentUser cu ? cu : CurrentUser.Anonymous;

        private static async Task WriteJsonAsync<T>(HttpContext ctx, T data, int statusCode = 200)
        {
            await YTStdTenantPlatform.Infrastructure.Serialization.TenantPlatformJsonResponseWriter.WriteAsync(ctx, data, statusCode);
        }
    }
}
