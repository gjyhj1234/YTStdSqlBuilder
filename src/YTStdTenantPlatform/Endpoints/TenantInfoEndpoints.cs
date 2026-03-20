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
    /// <summary>租户信息端点（分组、域名、标签）</summary>
    public static class TenantInfoEndpoints
    {
        /// <summary>注册租户信息路由</summary>
        public static void Map(WebApplication app)
        {
            MapGroupEndpoints(app);
            MapDomainEndpoints(app);
            MapTagEndpoints(app);
        }

        /// <summary>注册租户分组路由</summary>
        private static void MapGroupEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/tenant-groups")
                .WithTags("租户分组");

            group.MapGet("/tree", async (HttpContext ctx) =>
            {
                var user = GetCurrentUser(ctx);
                var tree = await TenantInfoAppService.GetGroupTreeAsync(0, user.UserId);
                await WriteJsonAsync(ctx, ApiResult<List<TenantGroupRepDTO>>.Ok(tree));
            }).WithSummary("获取分组树");

            group.MapGet("/", async (HttpContext ctx) =>
            {
                var user = GetCurrentUser(ctx);
                var list = await TenantInfoAppService.GetGroupListAsync(0, user.UserId);
                await WriteJsonAsync(ctx, ApiResult<List<TenantGroupRepDTO>>.Ok(list));
            }).WithSummary("获取分组平铺列表");

            group.MapPost("/", async (HttpContext ctx) =>
            {
                var user = GetCurrentUser(ctx);
                var req = await ctx.Request.ReadFromJsonAsync<CreateTenantGroupReqDTO>();
                if (req == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.InvalidRequestBody, Messages.InvalidRequestBody), 400); return; }
                var result = await TenantInfoAppService.CreateGroupAsync(0, user.UserId, req);
                if (result.Code != 0) { await WriteJsonAsync(ctx, ApiResult.Fail(result.Code, result.Message), 400); return; }
                ctx.Response.StatusCode = 201;
                await WriteJsonAsync(ctx, result);
            }).WithSummary("创建租户分组");
        }

        /// <summary>注册租户域名路由</summary>
        private static void MapDomainEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/tenant-domains")
                .WithTags("租户域名");

            group.MapGet("/", async (HttpContext ctx, long tenantRefId) =>
            {
                var user = GetCurrentUser(ctx);
                var list = await TenantInfoAppService.GetDomainsAsync(0, user.UserId, tenantRefId);
                await WriteJsonAsync(ctx, ApiResult<List<TenantDomainRepDTO>>.Ok(list));
            }).WithSummary("获取租户域名列表");

            group.MapPost("/", async (HttpContext ctx) =>
            {
                var user = GetCurrentUser(ctx);
                var req = await ctx.Request.ReadFromJsonAsync<CreateTenantDomainReqDTO>();
                if (req == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.InvalidRequestBody, Messages.InvalidRequestBody), 400); return; }
                var result = await TenantInfoAppService.CreateDomainAsync(0, user.UserId, req);
                if (result.Code != 0) { await WriteJsonAsync(ctx, ApiResult.Fail(result.Code, result.Message), 400); return; }
                ctx.Response.StatusCode = 201;
                await WriteJsonAsync(ctx, result);
            }).WithSummary("创建租户域名");
        }

        /// <summary>注册租户标签路由</summary>
        private static void MapTagEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/tenant-tags")
                .WithTags("租户标签");

            group.MapGet("/", async (HttpContext ctx, int? page, int? pageSize, string? keyword) =>
            {
                var user = GetCurrentUser(ctx);
                var req = new PagedRequest { Page = page ?? 1, PageSize = pageSize ?? 20, Keyword = keyword };
                var result = await TenantInfoAppService.GetTagListAsync(0, user.UserId, req);
                await WriteJsonAsync(ctx, ApiResult<PagedResult<TenantTagRepDTO>>.Ok(result));
            }).WithSummary("获取标签列表");

            group.MapPost("/", async (HttpContext ctx) =>
            {
                var user = GetCurrentUser(ctx);
                var req = await ctx.Request.ReadFromJsonAsync<CreateTenantTagReqDTO>();
                if (req == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.InvalidRequestBody, Messages.InvalidRequestBody), 400); return; }
                var result = await TenantInfoAppService.CreateTagAsync(0, user.UserId, req);
                if (result.Code != 0) { await WriteJsonAsync(ctx, ApiResult.Fail(result.Code, result.Message), 400); return; }
                ctx.Response.StatusCode = 201;
                await WriteJsonAsync(ctx, result);
            }).WithSummary("创建标签");

            group.MapPost("/bind", async (HttpContext ctx) =>
            {
                var user = GetCurrentUser(ctx);
                var req = await ctx.Request.ReadFromJsonAsync<TagBindReqDTO>();
                if (req == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.InvalidRequestBody, Messages.InvalidRequestBody), 400); return; }
                var result = await TenantInfoAppService.BindTagsAsync(0, user.UserId, req);
                await WriteJsonAsync(ctx, result, result.Code == 0 ? 200 : 400);
            }).WithSummary("批量绑定标签");
        }

        private static CurrentUser GetCurrentUser(HttpContext ctx) =>
            ctx.Items.TryGetValue(CurrentUser.HttpContextKey, out var u) && u is CurrentUser cu ? cu : CurrentUser.Anonymous;

        private static async Task WriteJsonAsync<T>(HttpContext ctx, T data, int statusCode = 200)
        {
            await YTStdTenantPlatform.Infrastructure.Serialization.TenantPlatformJsonResponseWriter.WriteAsync(ctx, data, statusCode);
        }
    }
}
