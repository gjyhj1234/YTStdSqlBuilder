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
    /// <summary>文件与存储端点（存储策略、租户文件、访问策略）</summary>
    public static class StorageEndpoints
    {
        /// <summary>注册文件与存储路由</summary>
        public static void Map(WebApplication app)
        {
            MapStrategyEndpoints(app);
            MapFileEndpoints(app);
            MapFileAccessPolicyEndpoints(app);
        }

        /// <summary>注册存储策略路由</summary>
        private static void MapStrategyEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/storage-strategies")
                .WithTags("文件与存储");

            group.MapGet("/", async (HttpContext ctx, int? page, int? pageSize, string? keyword, string? status) =>
            {
                var user = GetCurrentUser(ctx);
                var req = new PagedRequest { Page = page ?? 1, PageSize = pageSize ?? 20, Keyword = keyword, Status = status };
                var result = await StorageAppService.GetStrategyListAsync(0, user.UserId, req);
                await WriteJsonAsync(ctx, ApiResult<PagedResult<StorageStrategyRepDTO>>.Ok(result));
            }).WithSummary("获取存储策略分页列表");

            group.MapGet("/{id:long}", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var result = await StorageAppService.GetStrategyByIdAsync(0, user.UserId, id);
                if (result == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.ResourceNotFound, Messages.ResourceNotFound), 404); return; }
                await WriteJsonAsync(ctx, ApiResult<StorageStrategyRepDTO>.Ok(result));
            }).WithSummary("获取存储策略详情");

            group.MapPost("/", async (HttpContext ctx) =>
            {
                var user = GetCurrentUser(ctx);
                var req = await ctx.Request.ReadFromJsonAsync<CreateStorageStrategyReqDTO>();
                if (req == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.InvalidRequestBody, Messages.InvalidRequestBody), 400); return; }
                var result = await StorageAppService.CreateStrategyAsync(0, user.UserId, req);
                if (result.Code != 0) { await WriteJsonAsync(ctx, ApiResult.Fail(result.Code, result.Message), 400); return; }
                ctx.Response.StatusCode = 201;
                await WriteJsonAsync(ctx, result);
            }).WithSummary("创建存储策略");

            group.MapPut("/{id:long}", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var req = await ctx.Request.ReadFromJsonAsync<UpdateStorageStrategyReqDTO>();
                if (req == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.InvalidRequestBody, Messages.InvalidRequestBody), 400); return; }
                var result = await StorageAppService.UpdateStrategyAsync(0, user.UserId, id, req);
                if (result.Code != 0) { await WriteJsonAsync(ctx, result, 400); return; }
                await WriteJsonAsync(ctx, result);
            }).WithSummary("更新存储策略");

            group.MapPut("/{id:long}/enable", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var result = await StorageAppService.SetStrategyStatusAsync(0, user.UserId, id, "active");
                if (result.Code != 0) { await WriteJsonAsync(ctx, result, 400); return; }
                await WriteJsonAsync(ctx, result);
            }).WithSummary("启用存储策略");

            group.MapPut("/{id:long}/disable", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var result = await StorageAppService.SetStrategyStatusAsync(0, user.UserId, id, "disabled");
                if (result.Code != 0) { await WriteJsonAsync(ctx, result, 400); return; }
                await WriteJsonAsync(ctx, result);
            }).WithSummary("禁用存储策略");
        }

        /// <summary>注册租户文件路由</summary>
        private static void MapFileEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/tenant-files")
                .WithTags("文件与存储");

            group.MapGet("/", async (HttpContext ctx, int? page, int? pageSize, string? keyword, long tenantRefId) =>
            {
                var user = GetCurrentUser(ctx);
                var req = new PagedRequest { Page = page ?? 1, PageSize = pageSize ?? 20, Keyword = keyword };
                var result = await StorageAppService.GetFileListAsync(0, user.UserId, tenantRefId, req);
                await WriteJsonAsync(ctx, ApiResult<PagedResult<TenantFileRepDTO>>.Ok(result));
            }).WithSummary("获取租户文件列表");

            group.MapGet("/{id:long}", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var result = await StorageAppService.GetFileByIdAsync(0, user.UserId, id);
                if (result == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.ResourceNotFound, Messages.ResourceNotFound), 404); return; }
                await WriteJsonAsync(ctx, ApiResult<TenantFileRepDTO>.Ok(result));
            }).WithSummary("获取文件详情");

            group.MapDelete("/{id:long}", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var result = await StorageAppService.DeleteFileAsync(0, user.UserId, id);
                await WriteJsonAsync(ctx, result, result.Code == 0 ? 200 : 400);
            }).WithSummary("删除文件");
        }

        /// <summary>注册文件访问策略路由</summary>
        private static void MapFileAccessPolicyEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/file-access-policies")
                .WithTags("文件与存储");

            group.MapGet("/", async (HttpContext ctx, int? page, int? pageSize, string? keyword, long fileId) =>
            {
                var user = GetCurrentUser(ctx);
                var req = new PagedRequest { Page = page ?? 1, PageSize = pageSize ?? 20, Keyword = keyword };
                var result = await StorageAppService.GetFileAccessPoliciesAsync(0, user.UserId, fileId, req);
                await WriteJsonAsync(ctx, ApiResult<PagedResult<FileAccessPolicyRepDTO>>.Ok(result));
            }).WithSummary("获取文件访问策略列表");

            group.MapPost("/", async (HttpContext ctx) =>
            {
                var user = GetCurrentUser(ctx);
                var req = await ctx.Request.ReadFromJsonAsync<SaveFileAccessPolicyReqDTO>();
                if (req == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.InvalidRequestBody, Messages.InvalidRequestBody), 400); return; }
                var result = await StorageAppService.SaveFileAccessPolicyAsync(0, user.UserId, req);
                if (result.Code != 0) { await WriteJsonAsync(ctx, ApiResult.Fail(result.Code, result.Message), 400); return; }
                ctx.Response.StatusCode = 201;
                await WriteJsonAsync(ctx, result);
            }).WithSummary("创建/更新文件访问策略");
        }

        private static CurrentUser GetCurrentUser(HttpContext ctx) =>
            ctx.Items.TryGetValue(CurrentUser.HttpContextKey, out var u) && u is CurrentUser cu ? cu : CurrentUser.Anonymous;

        private static async Task WriteJsonAsync<T>(HttpContext ctx, T data, int statusCode = 200)
        {
            await YTStdTenantPlatform.Infrastructure.Serialization.TenantPlatformJsonResponseWriter.WriteAsync(ctx, data, statusCode);
        }
    }
}
