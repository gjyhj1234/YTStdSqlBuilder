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
    /// <summary>计费与账单端点（发票、支付订单、退款）</summary>
    public static class BillingEndpoints
    {
        /// <summary>注册计费与账单路由</summary>
        public static void Map(WebApplication app)
        {
            MapInvoiceEndpoints(app);
            MapPaymentOrderEndpoints(app);
            MapRefundEndpoints(app);
        }

        /// <summary>注册发票路由</summary>
        private static void MapInvoiceEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/billing-invoices")
                .WithTags("计费与账单");

            group.MapGet("/", async (HttpContext ctx, int? page, int? pageSize, string? keyword, string? status) =>
            {
                var user = GetCurrentUser(ctx);
                var req = new PagedRequest { Page = page ?? 1, PageSize = pageSize ?? 20, Keyword = keyword, Status = status };
                var result = await BillingAppService.GetInvoiceListAsync(0, user.UserId, req);
                await WriteJsonAsync(ctx, ApiResult<PagedResult<BillingInvoiceDto>>.Ok(result));
            }).WithSummary("获取发票分页列表");

            group.MapGet("/{id:long}", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var result = await BillingAppService.GetInvoiceByIdAsync(0, user.UserId, id);
                if (result == null) { ctx.Response.StatusCode = 404; return; }
                await WriteJsonAsync(ctx, ApiResult<BillingInvoiceDto>.Ok(result));
            }).WithSummary("获取发票详情");

            group.MapPost("/", async (HttpContext ctx) =>
            {
                var user = GetCurrentUser(ctx);
                var req = await ctx.Request.ReadFromJsonAsync<CreateBillingInvoiceRequest>();
                if (req == null) { await WriteJsonAsync(ctx, ApiResult.Fail("请求体无效"), 400); return; }
                var result = await BillingAppService.CreateInvoiceAsync(0, user.UserId, req);
                if (!result.Success) { await WriteJsonAsync(ctx, ApiResult.Fail(result.Message), 400); return; }
                ctx.Response.StatusCode = 201;
                await WriteJsonAsync(ctx, result);
            }).WithSummary("创建发票");

            group.MapPut("/{id:long}/void", async (HttpContext ctx, long id) =>
            {
                var user = GetCurrentUser(ctx);
                var result = await BillingAppService.VoidInvoiceAsync(0, user.UserId, id);
                await WriteJsonAsync(ctx, result, result.Success ? 200 : 400);
            }).WithSummary("作废发票");

            group.MapGet("/{invoiceId:long}/items", async (HttpContext ctx, long invoiceId, int? page, int? pageSize) =>
            {
                var user = GetCurrentUser(ctx);
                var req = new PagedRequest { Page = page ?? 1, PageSize = pageSize ?? 20 };
                var result = await BillingAppService.GetInvoiceItemsAsync(0, user.UserId, invoiceId, req);
                await WriteJsonAsync(ctx, ApiResult<PagedResult<BillingInvoiceItemDto>>.Ok(result));
            }).WithSummary("获取发票明细列表");
        }

        /// <summary>注册支付订单路由</summary>
        private static void MapPaymentOrderEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/payment-orders")
                .WithTags("计费与账单");

            group.MapGet("/", async (HttpContext ctx, int? page, int? pageSize, string? keyword, string? status) =>
            {
                var user = GetCurrentUser(ctx);
                var req = new PagedRequest { Page = page ?? 1, PageSize = pageSize ?? 20, Keyword = keyword, Status = status };
                var result = await BillingAppService.GetPaymentOrderListAsync(0, user.UserId, req);
                await WriteJsonAsync(ctx, ApiResult<PagedResult<PaymentOrderDto>>.Ok(result));
            }).WithSummary("获取支付订单分页列表");

            group.MapPost("/", async (HttpContext ctx) =>
            {
                var user = GetCurrentUser(ctx);
                var req = await ctx.Request.ReadFromJsonAsync<CreatePaymentOrderRequest>();
                if (req == null) { await WriteJsonAsync(ctx, ApiResult.Fail("请求体无效"), 400); return; }
                var result = await BillingAppService.CreatePaymentOrderAsync(0, user.UserId, req);
                if (!result.Success) { await WriteJsonAsync(ctx, ApiResult.Fail(result.Message), 400); return; }
                ctx.Response.StatusCode = 201;
                await WriteJsonAsync(ctx, result);
            }).WithSummary("创建支付订单");
        }

        /// <summary>注册退款路由</summary>
        private static void MapRefundEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/payment-refunds")
                .WithTags("计费与账单");

            group.MapGet("/", async (HttpContext ctx, int? page, int? pageSize, string? keyword, string? status) =>
            {
                var user = GetCurrentUser(ctx);
                var req = new PagedRequest { Page = page ?? 1, PageSize = pageSize ?? 20, Keyword = keyword, Status = status };
                var result = await BillingAppService.GetRefundListAsync(0, user.UserId, req);
                await WriteJsonAsync(ctx, ApiResult<PagedResult<PaymentRefundDto>>.Ok(result));
            }).WithSummary("获取退款分页列表");

            group.MapPost("/", async (HttpContext ctx) =>
            {
                var user = GetCurrentUser(ctx);
                var req = await ctx.Request.ReadFromJsonAsync<CreateRefundRequest>();
                if (req == null) { await WriteJsonAsync(ctx, ApiResult.Fail("请求体无效"), 400); return; }
                var result = await BillingAppService.CreateRefundAsync(0, user.UserId, req);
                if (!result.Success) { await WriteJsonAsync(ctx, ApiResult.Fail(result.Message), 400); return; }
                ctx.Response.StatusCode = 201;
                await WriteJsonAsync(ctx, result);
            }).WithSummary("创建退款");
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
