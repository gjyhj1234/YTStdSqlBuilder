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
    /// <summary>平台运营端点（每日统计、监控指标）</summary>
    public static class PlatformOperationEndpoints
    {
        /// <summary>注册平台运营路由</summary>
        public static void Map(WebApplication app)
        {
            MapDailyStatEndpoints(app);
            MapMonitorMetricEndpoints(app);
        }

        /// <summary>注册租户每日统计路由</summary>
        private static void MapDailyStatEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/tenant-daily-stats")
                .WithTags("平台运营");

            group.MapGet("/", async (HttpContext ctx, int? page, int? pageSize, string? keyword, long tenantRefId) =>
            {
                var user = GetCurrentUser(ctx);
                var req = new PagedRequest { Page = page ?? 1, PageSize = pageSize ?? 20, Keyword = keyword };
                var result = await PlatformOperationAppService.GetDailyStatListAsync(0, user.UserId, tenantRefId, req);
                await WriteJsonAsync(ctx, ApiResult<PagedResult<TenantDailyStatRepDTO>>.Ok(result));
            }).WithSummary("获取租户每日统计列表");
        }

        /// <summary>注册平台监控指标路由</summary>
        private static void MapMonitorMetricEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/platform-monitor-metrics")
                .WithTags("平台运营");

            group.MapGet("/", async (HttpContext ctx, int? page, int? pageSize, string? keyword) =>
            {
                var user = GetCurrentUser(ctx);
                var req = new PagedRequest { Page = page ?? 1, PageSize = pageSize ?? 20, Keyword = keyword };
                var result = await PlatformOperationAppService.GetMonitorMetricListAsync(0, user.UserId, req);
                await WriteJsonAsync(ctx, ApiResult<PagedResult<PlatformMonitorMetricRepDTO>>.Ok(result));
            }).WithSummary("获取平台监控指标列表");
        }

        private static CurrentUser GetCurrentUser(HttpContext ctx) =>
            ctx.Items.TryGetValue(CurrentUser.HttpContextKey, out var u) && u is CurrentUser cu ? cu : CurrentUser.Anonymous;

        private static async Task WriteJsonAsync<T>(HttpContext ctx, T data, int statusCode = 200)
        {
            await YTStdTenantPlatform.Infrastructure.Serialization.TenantPlatformJsonResponseWriter.WriteAsync(ctx, data, statusCode);
        }
    }
}
