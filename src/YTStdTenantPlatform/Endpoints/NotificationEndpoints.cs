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
    /// <summary>通知系统端点（通知模板、通知）</summary>
    public static class NotificationEndpoints
    {
        /// <summary>注册通知系统路由</summary>
        public static void Map(WebApplication app)
        {
            MapTemplateEndpoints(app);
            MapNotificationEndpoints(app);
        }

        /// <summary>注册通知模板路由</summary>
        private static void MapTemplateEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/notification-templates")
                .WithTags("通知系统");

            group.MapGet("/", async (HttpContext ctx, int? page, int? pageSize, string? keyword, string? status) =>
            {
                var user = GetCurrentUser(ctx);
                var req = new PagedRequest { Page = page ?? 1, PageSize = pageSize ?? 20, Keyword = keyword, Status = status };
                var result = await NotificationAppService.GetTemplateListAsync(0, user.UserId, req);
                await WriteJsonAsync(ctx, ApiResult<PagedResult<NotificationTemplateRepDTO>>.Ok(result));
            }).WithSummary("获取通知模板分页列表");

            group.MapGet("/{id:long}", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var result = await NotificationAppService.GetTemplateByIdAsync(0, user.UserId, id);
                if (result == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.ResourceNotFound, Messages.ResourceNotFound), 404); return; }
                await WriteJsonAsync(ctx, ApiResult<NotificationTemplateRepDTO>.Ok(result));
            }).WithSummary("获取通知模板详情");

            group.MapPost("/", async (HttpContext ctx) =>
            {
                var user = GetCurrentUser(ctx);
                var req = await ctx.Request.ReadFromJsonAsync<CreateNotificationTemplateReqDTO>();
                if (req == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.InvalidRequestBody, Messages.InvalidRequestBody), 400); return; }
                var result = await NotificationAppService.CreateTemplateAsync(0, user.UserId, req);
                if (result.Code != 0) { await WriteJsonAsync(ctx, ApiResult.Fail(result.Code, result.Message), 400); return; }
                ctx.Response.StatusCode = 201;
                await WriteJsonAsync(ctx, result);
            }).WithSummary("创建通知模板");

            group.MapPut("/{id:long}", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var req = await ctx.Request.ReadFromJsonAsync<UpdateNotificationTemplateReqDTO>();
                if (req == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.InvalidRequestBody, Messages.InvalidRequestBody), 400); return; }
                var result = await NotificationAppService.UpdateTemplateAsync(0, user.UserId, id, req);
                if (result.Code != 0) { await WriteJsonAsync(ctx, result, 400); return; }
                await WriteJsonAsync(ctx, result);
            }).WithSummary("更新通知模板");

            group.MapPut("/{id:long}/enable", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var result = await NotificationAppService.SetTemplateStatusAsync(0, user.UserId, id, "active");
                if (result.Code != 0) { await WriteJsonAsync(ctx, result, 400); return; }
                await WriteJsonAsync(ctx, result);
            }).WithSummary("启用通知模板");

            group.MapPut("/{id:long}/disable", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var result = await NotificationAppService.SetTemplateStatusAsync(0, user.UserId, id, "disabled");
                if (result.Code != 0) { await WriteJsonAsync(ctx, result, 400); return; }
                await WriteJsonAsync(ctx, result);
            }).WithSummary("禁用通知模板");
        }

        /// <summary>注册通知路由</summary>
        private static void MapNotificationEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/notifications")
                .WithTags("通知系统");

            group.MapGet("/", async (HttpContext ctx, int? page, int? pageSize, string? keyword, string? status) =>
            {
                var user = GetCurrentUser(ctx);
                var req = new PagedRequest { Page = page ?? 1, PageSize = pageSize ?? 20, Keyword = keyword, Status = status };
                var result = await NotificationAppService.GetNotificationListAsync(0, user.UserId, req);
                await WriteJsonAsync(ctx, ApiResult<PagedResult<NotificationRepDTO>>.Ok(result));
            }).WithSummary("获取通知分页列表");

            group.MapGet("/{id:long}", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var result = await NotificationAppService.GetNotificationByIdAsync(0, user.UserId, id);
                if (result == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.ResourceNotFound, Messages.ResourceNotFound), 404); return; }
                await WriteJsonAsync(ctx, ApiResult<NotificationRepDTO>.Ok(result));
            }).WithSummary("获取通知详情");

            group.MapPost("/", async (HttpContext ctx) =>
            {
                var user = GetCurrentUser(ctx);
                var req = await ctx.Request.ReadFromJsonAsync<CreateNotificationReqDTO>();
                if (req == null) { await WriteJsonAsync(ctx, ApiResult.Fail(ErrorCodes.InvalidRequestBody, Messages.InvalidRequestBody), 400); return; }
                var result = await NotificationAppService.CreateNotificationAsync(0, user.UserId, req);
                if (result.Code != 0) { await WriteJsonAsync(ctx, ApiResult.Fail(result.Code, result.Message), 400); return; }
                ctx.Response.StatusCode = 201;
                await WriteJsonAsync(ctx, result);
            }).WithSummary("创建通知");

            group.MapPut("/{id:long}/read", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var result = await NotificationAppService.MarkNotificationReadAsync(0, user.UserId, id);
                await WriteJsonAsync(ctx, result, result.Code == 0 ? 200 : 400);
            }).WithSummary("标记通知已读");
        }

        private static CurrentUser GetCurrentUser(HttpContext ctx) =>
            ctx.Items.TryGetValue(CurrentUser.HttpContextKey, out var u) && u is CurrentUser cu ? cu : CurrentUser.Anonymous;

        private static async Task WriteJsonAsync<T>(HttpContext ctx, T data, int statusCode = 200)
        {
            await YTStdTenantPlatform.Infrastructure.Serialization.TenantPlatformJsonResponseWriter.WriteAsync(ctx, data, statusCode);
        }
    }
}
