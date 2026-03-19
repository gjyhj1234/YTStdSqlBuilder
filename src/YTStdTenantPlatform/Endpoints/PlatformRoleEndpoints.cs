using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using YTStdTenantPlatform.Application.Dtos;
using YTStdTenantPlatform.Application.Services;
using YTStdTenantPlatform.Infrastructure.Auth;

namespace YTStdTenantPlatform.Endpoints
{
    /// <summary>平台角色管理端点</summary>
    public static class PlatformRoleEndpoints
    {
        /// <summary>注册平台角色管理路由</summary>
        public static void Map(WebApplication app)
        {
            var group = app.MapGroup("/api/platform-roles")
                .WithTags("平台角色管理");

            group.MapGet("/", async (HttpContext ctx, int? page, int? pageSize, string? keyword, string? status) =>
            {
                var user = GetCurrentUser(ctx);
                var req = new PagedRequest { Page = page ?? 1, PageSize = pageSize ?? 20, Keyword = keyword, Status = status };
                var result = await PlatformRoleAppService.GetListAsync(0, user.UserId, req);
                await WriteJsonAsync(ctx, ApiResult<PagedResult<PlatformRoleDto>>.Ok(result));
            }).WithSummary("获取角色分页列表");

            group.MapGet("/{id:long}", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var result = await PlatformRoleAppService.GetByIdAsync(0, user.UserId, id);
                if (result == null) { await WriteJsonAsync(ctx, ApiResult.Fail("资源不存在"), 404); return; }
                await WriteJsonAsync(ctx, ApiResult<PlatformRoleDto>.Ok(result));
            }).WithSummary("获取角色详情");

            group.MapPost("/", async (HttpContext ctx) =>
            {
                var user = GetCurrentUser(ctx);
                var req = await ctx.Request.ReadFromJsonAsync<CreatePlatformRoleRequest>();
                if (req == null) { await WriteJsonAsync(ctx, ApiResult.Fail("请求体无效"), 400); return; }
                var result = await PlatformRoleAppService.CreateAsync(0, user.UserId, req);
                if (!result.Success) { await WriteJsonAsync(ctx, ApiResult.Fail(result.Message), 400); return; }
                ctx.Response.StatusCode = 201;
                await WriteJsonAsync(ctx, result);
            }).WithSummary("创建角色");

            group.MapPut("/{id:long}", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var req = await ctx.Request.ReadFromJsonAsync<UpdatePlatformRoleRequest>();
                if (req == null) { await WriteJsonAsync(ctx, ApiResult.Fail("请求体无效"), 400); return; }
                var result = await PlatformRoleAppService.UpdateAsync(0, user.UserId, id, req);
                if (!result.Success) { await WriteJsonAsync(ctx, result, 400); return; }
                await WriteJsonAsync(ctx, result);
            }).WithSummary("更新角色");

            group.MapPut("/{id:long}/enable", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var result = await PlatformRoleAppService.SetStatusAsync(0, user.UserId, id, "active");
                await WriteJsonAsync(ctx, result, result.Success ? 200 : 400);
            }).WithSummary("启用角色");

            group.MapPut("/{id:long}/disable", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var result = await PlatformRoleAppService.SetStatusAsync(0, user.UserId, id, "disabled");
                await WriteJsonAsync(ctx, result, result.Success ? 200 : 400);
            }).WithSummary("禁用角色");

            group.MapPost("/{id:long}/permissions", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var req = await ctx.Request.ReadFromJsonAsync<RolePermissionBindRequest>();
                if (req == null) { await WriteJsonAsync(ctx, ApiResult.Fail("请求体无效"), 400); return; }
                var result = await PlatformRoleAppService.BindPermissionsAsync(0, user.UserId, id, req);
                await WriteJsonAsync(ctx, result, result.Success ? 200 : 400);
            }).WithSummary("角色授权（绑定权限）");

            group.MapPost("/{id:long}/members", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var req = await ctx.Request.ReadFromJsonAsync<RoleMemberBindRequest>();
                if (req == null) { await WriteJsonAsync(ctx, ApiResult.Fail("请求体无效"), 400); return; }
                var result = await PlatformRoleAppService.BindMembersAsync(0, user.UserId, id, req);
                await WriteJsonAsync(ctx, result, result.Success ? 200 : 400);
            }).WithSummary("角色成员管理（绑定用户）");
        }

        private static CurrentUser GetCurrentUser(HttpContext ctx) =>
            ctx.Items.TryGetValue(CurrentUser.HttpContextKey, out var u) && u is CurrentUser cu ? cu : CurrentUser.Anonymous;

        private static async Task WriteJsonAsync<T>(HttpContext ctx, T data, int statusCode = 200)
        {
            await YTStdTenantPlatform.Infrastructure.Serialization.TenantPlatformJsonResponseWriter.WriteAsync(ctx, data, statusCode);
        }
    }
}
