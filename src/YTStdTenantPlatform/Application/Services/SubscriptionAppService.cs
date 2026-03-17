using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YTStdLogger.Core;
using YTStdTenantPlatform.Application.Dtos;
using YTStdTenantPlatform.Entity.TenantPlatform;

namespace YTStdTenantPlatform.Application.Services
{
    /// <summary>订阅管理应用服务</summary>
    public static class SubscriptionAppService
    {
        // ──────────────────────────────────────────────────────
        // 租户订阅
        // ──────────────────────────────────────────────────────

        /// <summary>获取订阅分页列表</summary>
        public static async ValueTask<PagedResult<TenantSubscriptionDto>> GetSubscriptionListAsync(
            int tenantId, long operatorId, PagedRequest request)
        {
            var (result, data) = await TenantSubscriptionCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null)
                return new PagedResult<TenantSubscriptionDto> { Page = request.NormalizedPage, PageSize = request.NormalizedPageSize };

            var filtered = new List<TenantSubscription>();
            foreach (var s in data)
            {
                if (!string.IsNullOrEmpty(request.Status) &&
                    !string.Equals(s.SubscriptionStatus, request.Status, StringComparison.OrdinalIgnoreCase))
                    continue;
                filtered.Add(s);
            }

            var items = new List<TenantSubscriptionDto>();
            var offset = request.Offset;
            var size = request.NormalizedPageSize;
            for (int i = offset; i < filtered.Count && i < offset + size; i++)
                items.Add(MapSubscriptionToDto(filtered[i]));

            return new PagedResult<TenantSubscriptionDto>
            {
                Items = items, Total = filtered.Count,
                Page = request.NormalizedPage, PageSize = request.NormalizedPageSize
            };
        }

        /// <summary>获取订阅详情</summary>
        public static async ValueTask<TenantSubscriptionDto?> GetSubscriptionByIdAsync(
            int tenantId, long operatorId, long id)
        {
            var (result, data) = await TenantSubscriptionCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null) return null;
            foreach (var s in data)
            {
                if (s.Id == id)
                    return MapSubscriptionToDto(s);
            }
            return null;
        }

        /// <summary>创建订阅</summary>
        public static async ValueTask<ApiResult<long>> CreateSubscriptionAsync(
            int tenantId, long operatorId, CreateSubscriptionRequest req)
        {
            if (req.TenantRefId <= 0)
                return ApiResult<long>.Fail("关联租户 ID 无效");
            if (req.PackageVersionId <= 0)
                return ApiResult<long>.Fail("套餐版本 ID 无效");

            var now = DateTime.UtcNow;
            var entity = new TenantSubscription
            {
                TenantRefId = req.TenantRefId,
                PackageVersionId = req.PackageVersionId,
                SubscriptionStatus = "active",
                SubscriptionType = req.SubscriptionType,
                StartedAt = now,
                ExpiresAt = now.AddYears(1),
                AutoRenew = req.AutoRenew,
                CreatedBy = operatorId,
                CreatedAt = now,
                UpdatedAt = now
            };

            var insResult = await TenantSubscriptionCRUD.InsertAsync(tenantId, operatorId, entity);
            if (!insResult.Success)
                return ApiResult<long>.Fail("创建订阅失败: " + insResult.ErrorMessage);

            Logger.Info(tenantId, operatorId,
                "[SubscriptionAppService] 创建订阅: tenant=" + req.TenantRefId);
            return ApiResult<long>.Ok(insResult.Id);
        }

        /// <summary>取消订阅</summary>
        public static async ValueTask<ApiResult> CancelSubscriptionAsync(
            int tenantId, long operatorId, long id)
        {
            var (getResult, subscriptions) = await TenantSubscriptionCRUD.GetListAsync(tenantId, operatorId);
            if (!getResult.Success || subscriptions == null) return ApiResult.Fail("查询订阅失败");

            TenantSubscription? target = null;
            foreach (var s in subscriptions) { if (s.Id == id) { target = s; break; } }
            if (target == null) return ApiResult.Fail("订阅不存在");

            target.SubscriptionStatus = "cancelled";
            target.CancelledAt = DateTime.UtcNow;
            target.UpdatedAt = DateTime.UtcNow;

            var updResult = await TenantSubscriptionCRUD.UpdateAsync(tenantId, operatorId, target);
            if (!updResult.Success) return ApiResult.Fail("取消订阅失败");

            Logger.Info(tenantId, operatorId,
                "[SubscriptionAppService] 取消订阅: id=" + id);
            return ApiResult.Ok();
        }

        // ──────────────────────────────────────────────────────
        // 租户试用
        // ──────────────────────────────────────────────────────

        /// <summary>获取试用分页列表</summary>
        public static async ValueTask<PagedResult<TenantTrialDto>> GetTrialListAsync(
            int tenantId, long operatorId, PagedRequest request)
        {
            var (result, data) = await TenantTrialCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null)
                return new PagedResult<TenantTrialDto> { Page = request.NormalizedPage, PageSize = request.NormalizedPageSize };

            var items = new List<TenantTrialDto>();
            var offset = request.Offset;
            var size = request.NormalizedPageSize;
            for (int i = offset; i < data.Count && i < offset + size; i++)
                items.Add(MapTrialToDto(data[i]));

            return new PagedResult<TenantTrialDto>
            {
                Items = items, Total = data.Count,
                Page = request.NormalizedPage, PageSize = request.NormalizedPageSize
            };
        }

        /// <summary>创建试用</summary>
        public static async ValueTask<ApiResult<long>> CreateTrialAsync(
            int tenantId, long operatorId, CreateTrialRequest req)
        {
            if (req.TenantRefId <= 0)
                return ApiResult<long>.Fail("关联租户 ID 无效");

            var now = DateTime.UtcNow;
            var entity = new TenantTrial
            {
                TenantRefId = req.TenantRefId,
                PackageVersionId = req.PackageVersionId,
                Status = "active",
                StartedAt = now,
                ExpiresAt = now.AddDays(30),
                CreatedAt = now,
                UpdatedAt = now
            };

            var insResult = await TenantTrialCRUD.InsertAsync(tenantId, operatorId, entity);
            if (!insResult.Success)
                return ApiResult<long>.Fail("创建试用失败: " + insResult.ErrorMessage);

            Logger.Info(tenantId, operatorId,
                "[SubscriptionAppService] 创建试用: tenant=" + req.TenantRefId);
            return ApiResult<long>.Ok(insResult.Id);
        }

        // ──────────────────────────────────────────────────────
        // 订阅变更
        // ──────────────────────────────────────────────────────

        /// <summary>获取订阅变更记录列表</summary>
        public static async ValueTask<PagedResult<TenantSubscriptionChangeDto>> GetSubscriptionChangesAsync(
            int tenantId, long operatorId, long tenantRefId, PagedRequest request)
        {
            var (result, data) = await TenantSubscriptionChangeCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null)
                return new PagedResult<TenantSubscriptionChangeDto> { Page = request.NormalizedPage, PageSize = request.NormalizedPageSize };

            var filtered = new List<TenantSubscriptionChange>();
            foreach (var c in data)
            {
                if (c.TenantRefId == tenantRefId)
                    filtered.Add(c);
            }

            var items = new List<TenantSubscriptionChangeDto>();
            var offset = request.Offset;
            var size = request.NormalizedPageSize;
            for (int i = offset; i < filtered.Count && i < offset + size; i++)
            {
                var c = filtered[i];
                items.Add(new TenantSubscriptionChangeDto
                {
                    Id = c.Id, TenantRefId = c.TenantRefId,
                    SubscriptionId = c.SubscriptionId, ChangeType = c.ChangeType,
                    FromPackageVersionId = c.FromPackageVersionId,
                    ToPackageVersionId = c.ToPackageVersionId,
                    EffectiveAt = c.EffectiveAt, Remark = c.Remark,
                    CreatedAt = c.CreatedAt
                });
            }

            return new PagedResult<TenantSubscriptionChangeDto>
            {
                Items = items, Total = filtered.Count,
                Page = request.NormalizedPage, PageSize = request.NormalizedPageSize
            };
        }

        // ──────────────────────────────────────────────────────
        // Mapping helpers
        // ──────────────────────────────────────────────────────

        private static TenantSubscriptionDto MapSubscriptionToDto(TenantSubscription s) => new TenantSubscriptionDto
        {
            Id = s.Id, TenantRefId = s.TenantRefId, PackageVersionId = s.PackageVersionId,
            SubscriptionStatus = s.SubscriptionStatus, SubscriptionType = s.SubscriptionType,
            StartedAt = s.StartedAt, ExpiresAt = s.ExpiresAt,
            AutoRenew = s.AutoRenew, CancelledAt = s.CancelledAt,
            CreatedAt = s.CreatedAt
        };

        private static TenantTrialDto MapTrialToDto(TenantTrial t) => new TenantTrialDto
        {
            Id = t.Id, TenantRefId = t.TenantRefId, PackageVersionId = t.PackageVersionId,
            Status = t.Status, StartedAt = t.StartedAt, ExpiresAt = t.ExpiresAt,
            ConvertedSubscriptionId = t.ConvertedSubscriptionId, CreatedAt = t.CreatedAt
        };
    }
}
