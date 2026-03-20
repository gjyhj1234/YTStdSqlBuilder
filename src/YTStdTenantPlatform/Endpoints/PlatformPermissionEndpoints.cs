using System;
using System.Collections.Generic;
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
    /// <summary>平台权限端点</summary>
    public static class PlatformPermissionEndpoints
    {
        /// <summary>注册平台权限路由</summary>
        public static void Map(WebApplication app)
        {
            var group = app.MapGroup("/api/platform-permissions")
                .WithTags("平台权限");

            group.MapGet("/tree", async (HttpContext ctx) =>
            {
                var user = GetCurrentUser(ctx);
                var tree = await PlatformPermissionAppService.GetTreeAsync(0, user.UserId);
                await WriteJsonAsync(ctx, ApiResult<List<PlatformPermissionRepDTO>>.Ok(tree));
            }).WithSummary("获取权限树");

            group.MapGet("/", async (HttpContext ctx) =>
            {
                var user = GetCurrentUser(ctx);
                var list = await PlatformPermissionAppService.GetFlatListAsync(0, user.UserId);
                await WriteJsonAsync(ctx, ApiResult<List<PlatformPermissionRepDTO>>.Ok(list));
            }).WithSummary("获取权限平铺列表");

            group.MapGet("/{id:long}", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var result = await PlatformPermissionAppService.GetByIdAsync(0, user.UserId, id);
                if (result == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.ResourceNotFound, Messages.ResourceNotFound), 404); return; }
                await WriteJsonAsync(ctx, ApiResult<PlatformPermissionRepDTO>.Ok(result));
            }).WithSummary("获取权限详情");

            group.MapGet("/code/{code}", (HttpContext ctx, string code) =>
            {
                var result = PlatformPermissionAppService.GetByCode(code);
                if (result == null) { return WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.ResourceNotFound, Messages.ResourceNotFound), 404); }
                return WriteJsonAsync(ctx, ApiResult<PlatformPermissionRepDTO>.Ok(result));
            }).WithSummary("按编码查询权限");
        }

        private static CurrentUser GetCurrentUser(HttpContext ctx) =>
            ctx.Items.TryGetValue(CurrentUser.HttpContextKey, out var u) && u is CurrentUser cu ? cu : CurrentUser.Anonymous;

        private static async Task WriteJsonAsync<T>(HttpContext ctx, T data, int statusCode = 200)
        {
            await YTStdTenantPlatform.Infrastructure.Serialization.TenantPlatformJsonResponseWriter.WriteAsync(ctx, data, statusCode);
        }
    }
}
