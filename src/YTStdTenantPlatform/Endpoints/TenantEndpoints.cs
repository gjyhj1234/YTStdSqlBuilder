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
    /// <summary>租户管理端点</summary>
    public static class TenantEndpoints
    {
        /// <summary>注册租户管理路由</summary>
        public static void Map(WebApplication app)
        {
            var group = app.MapGroup("/api/tenants")
                .WithTags("租户管理");

            group.MapGet("/", async (HttpContext ctx, int? page, int? pageSize, string? keyword, string? status) =>
            {
                var user = GetCurrentUser(ctx);
                var req = new PagedRequest { Page = page ?? 1, PageSize = pageSize ?? 20, Keyword = keyword, Status = status };
                var result = await TenantLifecycleAppService.GetListAsync(0, user.UserId, req);
                await WriteJsonAsync(ctx, ApiResult<PagedResult<TenantRepDTO>>.Ok(result));
            }).WithSummary("获取租户分页列表");

            group.MapGet("/{id:long}", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var result = await TenantLifecycleAppService.GetByIdAsync(0, user.UserId, id);
                if (result == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.ResourceNotFound, Messages.ResourceNotFound), 404); return; }
                await WriteJsonAsync(ctx, ApiResult<TenantRepDTO>.Ok(result));
            }).WithSummary("获取租户详情");

            group.MapPost("/", async (HttpContext ctx) =>
            {
                var user = GetCurrentUser(ctx);
                var req = await ctx.Request.ReadFromJsonAsync<CreateTenantReqDTO>();
                if (req == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.InvalidRequestBody, Messages.InvalidRequestBody), 400); return; }
                var result = await TenantLifecycleAppService.CreateAsync(0, user.UserId, req);
                if (result.Code != 0) { await WriteJsonAsync(ctx, ApiResult.Fail(result.Code, result.Message), 400); return; }
                ctx.Response.StatusCode = 201;
                await WriteJsonAsync(ctx, result);
            }).WithSummary("创建租户");

            group.MapPut("/{id:long}", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var req = await ctx.Request.ReadFromJsonAsync<UpdateTenantReqDTO>();
                if (req == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.InvalidRequestBody, Messages.InvalidRequestBody), 400); return; }
                var result = await TenantLifecycleAppService.UpdateAsync(0, user.UserId, id, req);
                await WriteJsonAsync(ctx, result, result.Code == 0 ? 200 : 400);
            }).WithSummary("更新租户信息");

            group.MapPut("/{id:long}/status", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var req = await ctx.Request.ReadFromJsonAsync<TenantStatusChangeReqDTO>();
                if (req == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.InvalidRequestBody, Messages.InvalidRequestBody), 400); return; }
                var result = await TenantLifecycleAppService.ChangeStatusAsync(0, user.UserId, id, req);
                await WriteJsonAsync(ctx, result, result.Code == 0 ? 200 : 400);
            }).WithSummary("租户状态流转（启用/暂停/关闭）");

            group.MapGet("/{id:long}/lifecycle-events", async (HttpContext ctx, long id, int? page, int? pageSize) =>
            {
                var user = GetCurrentUser(ctx);
                var req = new PagedRequest { Page = page ?? 1, PageSize = pageSize ?? 20 };
                var result = await TenantLifecycleAppService.GetLifecycleEventsAsync(0, user.UserId, id, req);
                await WriteJsonAsync(ctx, ApiResult<PagedResult<TenantLifecycleEventRepDTO>>.Ok(result));
            }).WithSummary("获取租户生命周期事件列表");
        }

        private static CurrentUser GetCurrentUser(HttpContext ctx) =>
            ctx.Items.TryGetValue(CurrentUser.HttpContextKey, out var u) && u is CurrentUser cu ? cu : CurrentUser.Anonymous;

        private static async Task WriteJsonAsync<T>(HttpContext ctx, T data, int statusCode = 200)
        {
            await YTStdTenantPlatform.Infrastructure.Serialization.TenantPlatformJsonResponseWriter.WriteAsync(ctx, data, statusCode);
        }
    }
}
