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
    /// <summary>SaaS 套餐管理端点（套餐、版本、能力）</summary>
    public static class PackageEndpoints
    {
        /// <summary>注册 SaaS 套餐管理路由</summary>
        public static void Map(WebApplication app)
        {
            MapPackageEndpoints(app);
            MapVersionEndpoints(app);
            MapCapabilityEndpoints(app);
        }

        /// <summary>注册套餐路由</summary>
        private static void MapPackageEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/saas-packages")
                .WithTags("SaaS 套餐管理");

            group.MapGet("/", async (HttpContext ctx, int? page, int? pageSize, string? keyword, string? status) =>
            {
                var user = GetCurrentUser(ctx);
                var req = new PagedRequest { Page = page ?? 1, PageSize = pageSize ?? 20, Keyword = keyword, Status = status };
                var result = await PackageAppService.GetPackageListAsync(0, user.UserId, req);
                await WriteJsonAsync(ctx, ApiResult<PagedResult<SaasPackageRepDTO>>.Ok(result));
            }).WithSummary("获取套餐分页列表");

            group.MapGet("/{id:long}", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var result = await PackageAppService.GetPackageByIdAsync(0, user.UserId, id);
                if (result == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.ResourceNotFound, Messages.ResourceNotFound), 404); return; }
                await WriteJsonAsync(ctx, ApiResult<SaasPackageRepDTO>.Ok(result));
            }).WithSummary("获取套餐详情");

            group.MapPost("/", async (HttpContext ctx) =>
            {
                var user = GetCurrentUser(ctx);
                var req = await ctx.Request.ReadFromJsonAsync<CreateSaasPackageReqDTO>();
                if (req == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.InvalidRequestBody, Messages.InvalidRequestBody), 400); return; }
                var result = await PackageAppService.CreatePackageAsync(0, user.UserId, req);
                if (result.Code != 0) { await WriteJsonAsync(ctx, ApiResult.Fail(result.Code, result.Message), 400); return; }
                ctx.Response.StatusCode = 201;
                await WriteJsonAsync(ctx, result);
            }).WithSummary("创建套餐");

            group.MapPut("/{id:long}", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var req = await ctx.Request.ReadFromJsonAsync<UpdateSaasPackageReqDTO>();
                if (req == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.InvalidRequestBody, Messages.InvalidRequestBody), 400); return; }
                var result = await PackageAppService.UpdatePackageAsync(0, user.UserId, id, req);
                if (result.Code != 0) { await WriteJsonAsync(ctx, result, 400); return; }
                await WriteJsonAsync(ctx, result);
            }).WithSummary("更新套餐");

            group.MapPut("/{id:long}/enable", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var result = await PackageAppService.SetPackageStatusAsync(0, user.UserId, id, "active");
                if (result.Code != 0) { await WriteJsonAsync(ctx, result, 400); return; }
                await WriteJsonAsync(ctx, result);
            }).WithSummary("启用套餐");

            group.MapPut("/{id:long}/disable", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var result = await PackageAppService.SetPackageStatusAsync(0, user.UserId, id, "disabled");
                if (result.Code != 0) { await WriteJsonAsync(ctx, result, 400); return; }
                await WriteJsonAsync(ctx, result);
            }).WithSummary("禁用套餐");
        }

        /// <summary>注册套餐版本路由</summary>
        private static void MapVersionEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/saas-package-versions")
                .WithTags("SaaS 套餐管理");

            group.MapGet("/{packageId:long}", async (HttpContext ctx, long packageId, int? page, int? pageSize, string? keyword) =>
            {
                var user = GetCurrentUser(ctx);
                var req = new PagedRequest { Page = page ?? 1, PageSize = pageSize ?? 20, Keyword = keyword };
                var result = await PackageAppService.GetVersionListAsync(0, user.UserId, packageId, req);
                await WriteJsonAsync(ctx, ApiResult<PagedResult<SaasPackageVersionRepDTO>>.Ok(result));
            }).WithSummary("获取套餐版本列表");

            group.MapPost("/{packageId:long}", async (HttpContext ctx, long packageId) =>
            {
                var user = GetCurrentUser(ctx);
                var req = await ctx.Request.ReadFromJsonAsync<CreateSaasPackageVersionReqDTO>();
                if (req == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.InvalidRequestBody, Messages.InvalidRequestBody), 400); return; }
                req.PackageId = packageId;
                var result = await PackageAppService.CreateVersionAsync(0, user.UserId, req);
                if (result.Code != 0) { await WriteJsonAsync(ctx, ApiResult.Fail(result.Code, result.Message), 400); return; }
                ctx.Response.StatusCode = 201;
                await WriteJsonAsync(ctx, result);
            }).WithSummary("创建套餐版本");
        }

        /// <summary>注册套餐能力路由</summary>
        private static void MapCapabilityEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/saas-package-capabilities")
                .WithTags("SaaS 套餐管理");

            group.MapGet("/{packageVersionId:long}", async (HttpContext ctx, long packageVersionId, int? page, int? pageSize, string? keyword) =>
            {
                var user = GetCurrentUser(ctx);
                var req = new PagedRequest { Page = page ?? 1, PageSize = pageSize ?? 20, Keyword = keyword };
                var result = await PackageAppService.GetCapabilityListAsync(0, user.UserId, packageVersionId, req);
                await WriteJsonAsync(ctx, ApiResult<PagedResult<SaasPackageCapabilityRepDTO>>.Ok(result));
            }).WithSummary("获取套餐能力列表");

            group.MapPost("/{packageVersionId:long}", async (HttpContext ctx, long packageVersionId) =>
            {
                var user = GetCurrentUser(ctx);
                var req = await ctx.Request.ReadFromJsonAsync<SaveSaasPackageCapabilityReqDTO>();
                if (req == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.InvalidRequestBody, Messages.InvalidRequestBody), 400); return; }
                req.PackageVersionId = packageVersionId;
                var result = await PackageAppService.SaveCapabilityAsync(0, user.UserId, req);
                if (result.Code != 0) { await WriteJsonAsync(ctx, ApiResult.Fail(result.Code, result.Message), 400); return; }
                ctx.Response.StatusCode = 201;
                await WriteJsonAsync(ctx, result);
            }).WithSummary("创建/更新套餐能力");
        }

        private static CurrentUser GetCurrentUser(HttpContext ctx) =>
            ctx.Items.TryGetValue(CurrentUser.HttpContextKey, out var u) && u is CurrentUser cu ? cu : CurrentUser.Anonymous;

        private static async Task WriteJsonAsync<T>(HttpContext ctx, T data, int statusCode = 200)
        {
            await YTStdTenantPlatform.Infrastructure.Serialization.TenantPlatformJsonResponseWriter.WriteAsync(ctx, data, statusCode);
        }
    }
}
