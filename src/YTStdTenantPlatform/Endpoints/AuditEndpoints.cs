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
    /// <summary>日志与审计端点（操作日志、审计日志、系统日志）</summary>
    public static class AuditEndpoints
    {
        /// <summary>注册日志与审计路由</summary>
        public static void Map(WebApplication app)
        {
            MapOperationLogEndpoints(app);
            MapAuditLogEndpoints(app);
            MapSystemLogEndpoints(app);
        }

        /// <summary>注册操作日志路由</summary>
        private static void MapOperationLogEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/operation-logs")
                .WithTags("日志与审计");

            group.MapGet("/", async (HttpContext ctx, int? page, int? pageSize, string? keyword, string? status) =>
            {
                var user = GetCurrentUser(ctx);
                var req = new PagedRequest { Page = page ?? 1, PageSize = pageSize ?? 20, Keyword = keyword, Status = status };
                var result = await AuditAppService.GetOperationLogListAsync(0, user.UserId, req);
                await WriteJsonAsync(ctx, ApiResult<PagedResult<OperationLogDto>>.Ok(result));
            }).WithSummary("获取操作日志分页列表");

            group.MapGet("/{id:long}", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var result = await AuditAppService.GetOperationLogByIdAsync(0, user.UserId, id);
                if (result == null) { ctx.Response.StatusCode = 404; return; }
                await WriteJsonAsync(ctx, ApiResult<OperationLogDto>.Ok(result));
            }).WithSummary("获取操作日志详情");
        }

        /// <summary>注册审计日志路由</summary>
        private static void MapAuditLogEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/audit-logs")
                .WithTags("日志与审计");

            group.MapGet("/", async (HttpContext ctx, int? page, int? pageSize, string? keyword, string? status) =>
            {
                var user = GetCurrentUser(ctx);
                var req = new PagedRequest { Page = page ?? 1, PageSize = pageSize ?? 20, Keyword = keyword, Status = status };
                var result = await AuditAppService.GetAuditLogListAsync(0, user.UserId, req);
                await WriteJsonAsync(ctx, ApiResult<PagedResult<AuditLogDto>>.Ok(result));
            }).WithSummary("获取审计日志分页列表");

            group.MapGet("/{id:long}", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var result = await AuditAppService.GetAuditLogByIdAsync(0, user.UserId, id);
                if (result == null) { ctx.Response.StatusCode = 404; return; }
                await WriteJsonAsync(ctx, ApiResult<AuditLogDto>.Ok(result));
            }).WithSummary("获取审计日志详情");
        }

        /// <summary>注册系统日志路由</summary>
        private static void MapSystemLogEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/system-logs")
                .WithTags("日志与审计");

            group.MapGet("/", async (HttpContext ctx, int? page, int? pageSize, string? keyword, string? status) =>
            {
                var user = GetCurrentUser(ctx);
                var req = new PagedRequest { Page = page ?? 1, PageSize = pageSize ?? 20, Keyword = keyword, Status = status };
                var result = await AuditAppService.GetSystemLogListAsync(0, user.UserId, req);
                await WriteJsonAsync(ctx, ApiResult<PagedResult<SystemLogDto>>.Ok(result));
            }).WithSummary("获取系统日志分页列表");

            group.MapGet("/{id:long}", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var result = await AuditAppService.GetSystemLogByIdAsync(0, user.UserId, id);
                if (result == null) { ctx.Response.StatusCode = 404; return; }
                await WriteJsonAsync(ctx, ApiResult<SystemLogDto>.Ok(result));
            }).WithSummary("获取系统日志详情");
        }

        private static CurrentUser GetCurrentUser(HttpContext ctx) =>
            ctx.Items.TryGetValue(CurrentUser.HttpContextKey, out var u) && u is CurrentUser cu ? cu : CurrentUser.Anonymous;

        private static async Task WriteJsonAsync<T>(HttpContext ctx, T data, int statusCode = 200)
        {
            ctx.Response.StatusCode = statusCode;
            ctx.Response.ContentType = "application/json; charset=utf-8";
            await System.Text.Json.JsonSerializer.SerializeAsync(ctx.Response.Body, data);
        }
    }
}
