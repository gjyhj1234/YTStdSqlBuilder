using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YTStdLogger.Core;
using YTStdTenantPlatform.Application.Dtos;
using YTStdTenantPlatform.Entity.TenantPlatform;

namespace YTStdTenantPlatform.Application.Services
{
    /// <summary>计费与支付应用服务</summary>
    public static class BillingAppService
    {
        // ──────────────────────────────────────────────────────
        // 账单发票
        // ──────────────────────────────────────────────────────

        /// <summary>获取发票分页列表</summary>
        public static async ValueTask<PagedResult<BillingInvoiceDto>> GetInvoiceListAsync(
            int tenantId, long operatorId, PagedRequest request)
        {
            var (result, data) = await BillingInvoiceCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null)
                return new PagedResult<BillingInvoiceDto> { Page = request.NormalizedPage, PageSize = request.NormalizedPageSize };

            var filtered = new List<BillingInvoice>();
            foreach (var inv in data)
            {
                if (!string.IsNullOrEmpty(request.Status) &&
                    !string.Equals(inv.InvoiceStatus, request.Status, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (!string.IsNullOrEmpty(request.Keyword) &&
                    inv.InvoiceNo.IndexOf(request.Keyword, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                filtered.Add(inv);
            }

            var items = new List<BillingInvoiceDto>();
            var offset = request.Offset;
            var size = request.NormalizedPageSize;
            for (int i = offset; i < filtered.Count && i < offset + size; i++)
                items.Add(MapInvoiceToDto(filtered[i]));

            return new PagedResult<BillingInvoiceDto>
            {
                Items = items, Total = filtered.Count,
                Page = request.NormalizedPage, PageSize = request.NormalizedPageSize
            };
        }

        /// <summary>获取发票详情</summary>
        public static async ValueTask<BillingInvoiceDto?> GetInvoiceByIdAsync(
            int tenantId, long operatorId, long id)
        {
            var (result, data) = await BillingInvoiceCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null) return null;
            foreach (var inv in data)
            {
                if (inv.Id == id)
                    return MapInvoiceToDto(inv);
            }
            return null;
        }

        /// <summary>创建发票</summary>
        public static async ValueTask<ApiResult<long>> CreateInvoiceAsync(
            int tenantId, long operatorId, CreateBillingInvoiceRequest req)
        {
            if (req.TenantRefId <= 0)
                return ApiResult<long>.Fail("关联租户 ID 无效");

            var now = DateTime.UtcNow;
            var entity = new BillingInvoice
            {
                InvoiceNo = "INV-" + DateTime.UtcNow.Ticks,
                TenantRefId = req.TenantRefId,
                SubscriptionId = req.SubscriptionId,
                InvoiceStatus = "draft",
                BillingPeriodStart = req.BillingPeriodStart,
                BillingPeriodEnd = req.BillingPeriodEnd,
                CurrencyCode = req.CurrencyCode,
                CreatedAt = now,
                UpdatedAt = now
            };

            var insResult = await BillingInvoiceCRUD.InsertAsync(tenantId, operatorId, entity);
            if (!insResult.Success)
                return ApiResult<long>.Fail("创建发票失败: " + insResult.ErrorMessage);

            Logger.Info(tenantId, operatorId,
                "[BillingAppService] 创建发票: " + entity.InvoiceNo);
            return ApiResult<long>.Ok(insResult.Id);
        }

        /// <summary>作废发票</summary>
        public static async ValueTask<ApiResult> VoidInvoiceAsync(
            int tenantId, long operatorId, long id)
        {
            var (getResult, invoices) = await BillingInvoiceCRUD.GetListAsync(tenantId, operatorId);
            if (!getResult.Success || invoices == null) return ApiResult.Fail("查询发票失败");

            BillingInvoice? target = null;
            foreach (var inv in invoices) { if (inv.Id == id) { target = inv; break; } }
            if (target == null) return ApiResult.Fail("发票不存在");

            target.InvoiceStatus = "voided";
            target.UpdatedAt = DateTime.UtcNow;

            var updResult = await BillingInvoiceCRUD.UpdateAsync(tenantId, operatorId, target);
            if (!updResult.Success) return ApiResult.Fail("作废发票失败");

            Logger.Info(tenantId, operatorId,
                "[BillingAppService] 作废发票: " + target.InvoiceNo);
            return ApiResult.Ok();
        }

        // ──────────────────────────────────────────────────────
        // 发票明细
        // ──────────────────────────────────────────────────────

        /// <summary>获取发票明细列表</summary>
        public static async ValueTask<PagedResult<BillingInvoiceItemDto>> GetInvoiceItemsAsync(
            int tenantId, long operatorId, long invoiceId, PagedRequest request)
        {
            var (result, data) = await BillingInvoiceItemCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null)
                return new PagedResult<BillingInvoiceItemDto> { Page = request.NormalizedPage, PageSize = request.NormalizedPageSize };

            var filtered = new List<BillingInvoiceItem>();
            foreach (var item in data)
            {
                if (item.InvoiceId == invoiceId)
                    filtered.Add(item);
            }

            var items = new List<BillingInvoiceItemDto>();
            var offset = request.Offset;
            var size = request.NormalizedPageSize;
            for (int i = offset; i < filtered.Count && i < offset + size; i++)
            {
                var it = filtered[i];
                items.Add(new BillingInvoiceItemDto
                {
                    Id = it.Id, InvoiceId = it.InvoiceId,
                    ItemType = it.ItemType, ItemName = it.ItemName,
                    Quantity = it.Quantity, UnitPrice = it.UnitPrice,
                    Amount = it.Amount, CreatedAt = it.CreatedAt
                });
            }

            return new PagedResult<BillingInvoiceItemDto>
            {
                Items = items, Total = filtered.Count,
                Page = request.NormalizedPage, PageSize = request.NormalizedPageSize
            };
        }

        // ──────────────────────────────────────────────────────
        // 支付订单
        // ──────────────────────────────────────────────────────

        /// <summary>获取支付订单分页列表</summary>
        public static async ValueTask<PagedResult<PaymentOrderDto>> GetPaymentOrderListAsync(
            int tenantId, long operatorId, PagedRequest request)
        {
            var (result, data) = await PaymentOrderCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null)
                return new PagedResult<PaymentOrderDto> { Page = request.NormalizedPage, PageSize = request.NormalizedPageSize };

            var filtered = new List<PaymentOrder>();
            foreach (var o in data)
            {
                if (!string.IsNullOrEmpty(request.Status) &&
                    !string.Equals(o.PaymentStatus, request.Status, StringComparison.OrdinalIgnoreCase))
                    continue;
                filtered.Add(o);
            }

            var items = new List<PaymentOrderDto>();
            var offset = request.Offset;
            var size = request.NormalizedPageSize;
            for (int i = offset; i < filtered.Count && i < offset + size; i++)
                items.Add(MapPaymentOrderToDto(filtered[i]));

            return new PagedResult<PaymentOrderDto>
            {
                Items = items, Total = filtered.Count,
                Page = request.NormalizedPage, PageSize = request.NormalizedPageSize
            };
        }

        /// <summary>创建支付订单</summary>
        public static async ValueTask<ApiResult<long>> CreatePaymentOrderAsync(
            int tenantId, long operatorId, CreatePaymentOrderRequest req)
        {
            if (req.Amount <= 0)
                return ApiResult<long>.Fail("金额必须大于零");

            var now = DateTime.UtcNow;
            var entity = new PaymentOrder
            {
                OrderNo = "PAY-" + DateTime.UtcNow.Ticks,
                TenantRefId = req.TenantRefId,
                InvoiceId = req.InvoiceId,
                PaymentChannel = req.PaymentChannel,
                PaymentStatus = "pending",
                Amount = req.Amount,
                CurrencyCode = req.CurrencyCode,
                CreatedAt = now,
                UpdatedAt = now
            };

            var insResult = await PaymentOrderCRUD.InsertAsync(tenantId, operatorId, entity);
            if (!insResult.Success)
                return ApiResult<long>.Fail("创建支付订单失败: " + insResult.ErrorMessage);

            Logger.Info(tenantId, operatorId,
                "[BillingAppService] 创建支付订单: " + entity.OrderNo);
            return ApiResult<long>.Ok(insResult.Id);
        }

        // ──────────────────────────────────────────────────────
        // 退款
        // ──────────────────────────────────────────────────────

        /// <summary>获取退款分页列表</summary>
        public static async ValueTask<PagedResult<PaymentRefundDto>> GetRefundListAsync(
            int tenantId, long operatorId, PagedRequest request)
        {
            var (result, data) = await PaymentRefundCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null)
                return new PagedResult<PaymentRefundDto> { Page = request.NormalizedPage, PageSize = request.NormalizedPageSize };

            var items = new List<PaymentRefundDto>();
            var offset = request.Offset;
            var size = request.NormalizedPageSize;
            for (int i = offset; i < data.Count && i < offset + size; i++)
                items.Add(MapRefundToDto(data[i]));

            return new PagedResult<PaymentRefundDto>
            {
                Items = items, Total = data.Count,
                Page = request.NormalizedPage, PageSize = request.NormalizedPageSize
            };
        }

        /// <summary>创建退款</summary>
        public static async ValueTask<ApiResult<long>> CreateRefundAsync(
            int tenantId, long operatorId, CreateRefundRequest req)
        {
            if (req.RefundAmount <= 0)
                return ApiResult<long>.Fail("退款金额必须大于零");

            var now = DateTime.UtcNow;
            var entity = new PaymentRefund
            {
                RefundNo = "REF-" + DateTime.UtcNow.Ticks,
                PaymentOrderId = req.PaymentOrderId,
                RefundStatus = "pending",
                RefundAmount = req.RefundAmount,
                RefundReason = req.RefundReason,
                CreatedAt = now,
                UpdatedAt = now
            };

            var insResult = await PaymentRefundCRUD.InsertAsync(tenantId, operatorId, entity);
            if (!insResult.Success)
                return ApiResult<long>.Fail("创建退款失败: " + insResult.ErrorMessage);

            Logger.Info(tenantId, operatorId,
                "[BillingAppService] 创建退款: " + entity.RefundNo);
            return ApiResult<long>.Ok(insResult.Id);
        }

        // ──────────────────────────────────────────────────────
        // Mapping helpers
        // ──────────────────────────────────────────────────────

        private static BillingInvoiceDto MapInvoiceToDto(BillingInvoice inv) => new BillingInvoiceDto
        {
            Id = inv.Id, InvoiceNo = inv.InvoiceNo, TenantRefId = inv.TenantRefId,
            SubscriptionId = inv.SubscriptionId, InvoiceStatus = inv.InvoiceStatus,
            BillingPeriodStart = inv.BillingPeriodStart, BillingPeriodEnd = inv.BillingPeriodEnd,
            SubtotalAmount = inv.SubtotalAmount, ExtraAmount = inv.ExtraAmount,
            DiscountAmount = inv.DiscountAmount, TotalAmount = inv.TotalAmount,
            CurrencyCode = inv.CurrencyCode, IssuedAt = inv.IssuedAt,
            DueAt = inv.DueAt, PaidAt = inv.PaidAt, CreatedAt = inv.CreatedAt
        };

        private static PaymentOrderDto MapPaymentOrderToDto(PaymentOrder o) => new PaymentOrderDto
        {
            Id = o.Id, OrderNo = o.OrderNo, TenantRefId = o.TenantRefId,
            InvoiceId = o.InvoiceId, PaymentChannel = o.PaymentChannel,
            PaymentStatus = o.PaymentStatus, Amount = o.Amount,
            CurrencyCode = o.CurrencyCode, ThirdPartyTxnNo = o.ThirdPartyTxnNo,
            PaidAt = o.PaidAt, CreatedAt = o.CreatedAt
        };

        private static PaymentRefundDto MapRefundToDto(PaymentRefund r) => new PaymentRefundDto
        {
            Id = r.Id, RefundNo = r.RefundNo, PaymentOrderId = r.PaymentOrderId,
            RefundStatus = r.RefundStatus, RefundAmount = r.RefundAmount,
            RefundReason = r.RefundReason, RefundedAt = r.RefundedAt,
            CreatedAt = r.CreatedAt
        };
    }
}
