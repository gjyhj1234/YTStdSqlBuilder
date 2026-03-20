using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YTStdLogger.Core;
using YTStdTenantPlatform.Application.Dtos;
using YTStdTenantPlatform.Entity.TenantPlatform;
using YTStdTenantPlatform.Application.Constants;

namespace YTStdTenantPlatform.Application.Services
{
    /// <summary>租户资源管理应用服务</summary>
    public static class TenantResourceAppService
    {
        /// <summary>获取资源配额列表</summary>
        public static async ValueTask<PagedResult<TenantResourceQuotaRepDTO>> GetListAsync(
            int tenantId, long operatorId, PagedRequest request, long? tenantRefId = null)
        {
            var (result, data) = await TenantResourceQuotaCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null)
                return new PagedResult<TenantResourceQuotaRepDTO> { Page = request.NormalizedPage, PageSize = request.NormalizedPageSize };

            var filtered = new List<TenantResourceQuota>();
            foreach (var q in data)
            {
                if (tenantRefId.HasValue && q.TenantRefId != tenantRefId.Value) continue;
                if (!string.IsNullOrEmpty(request.Keyword) &&
                    q.QuotaType.IndexOf(request.Keyword, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                filtered.Add(q);
            }

            var items = new List<TenantResourceQuotaRepDTO>();
            var offset = request.Offset;
            var size = request.NormalizedPageSize;
            for (int i = offset; i < filtered.Count && i < offset + size; i++)
                items.Add(MapToDto(filtered[i]));

            return new PagedResult<TenantResourceQuotaRepDTO>
            {
                Items = items, Total = filtered.Count,
                Page = request.NormalizedPage, PageSize = request.NormalizedPageSize
            };
        }

        /// <summary>获取配额详情</summary>
        public static async ValueTask<TenantResourceQuotaRepDTO?> GetByIdAsync(
            int tenantId, long operatorId, long id)
        {
            var (result, data) = await TenantResourceQuotaCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null) return null;
            foreach (var q in data)
            {
                if (q.Id == id)
                    return MapToDto(q);
            }
            return null;
        }

        /// <summary>创建或更新资源配额</summary>
        public static async ValueTask<ApiResult<long>> SaveAsync(
            int tenantId, long operatorId, SaveTenantResourceQuotaReqDTO req)
        {
            if (string.IsNullOrWhiteSpace(req.QuotaType))
                return ApiResult<long>.Fail(ErrorCodes.QuotaTypeRequired, Messages.QuotaTypeRequired);
            if (req.QuotaLimit <= 0)
                return ApiResult<long>.Fail(ErrorCodes.QuotaLimitInvalid, Messages.QuotaLimitInvalid);

            var now = DateTime.UtcNow;
            var quota = new TenantResourceQuota
            {
                TenantRefId = req.TenantRefId,
                QuotaType = req.QuotaType.Trim(),
                QuotaLimit = req.QuotaLimit,
                WarningThreshold = req.WarningThreshold,
                ResetCycle = req.ResetCycle,
                CreatedAt = now,
                UpdatedAt = now
            };

            var insResult = await TenantResourceQuotaCRUD.InsertAsync(tenantId, operatorId, quota);
            if (!insResult.Success)
                return ApiResult<long>.Fail(ErrorCodes.QuotaSaveFailed, Messages.QuotaSaveFailed);

            Logger.Info(tenantId, operatorId,
                "[TenantResourceAppService] 保存配额: tenant=" + req.TenantRefId + " type=" + req.QuotaType);
            return ApiResult<long>.Ok(insResult.Id);
        }

        private static TenantResourceQuotaRepDTO MapToDto(TenantResourceQuota q)
        {
            return new TenantResourceQuotaRepDTO
            {
                Id = q.Id, TenantRefId = q.TenantRefId, QuotaType = q.QuotaType,
                QuotaLimit = q.QuotaLimit, WarningThreshold = q.WarningThreshold,
                ResetCycle = q.ResetCycle, CreatedAt = q.CreatedAt
            };
        }
    }
}
