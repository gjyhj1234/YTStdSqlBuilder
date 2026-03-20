using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YTStdLogger.Core;
using YTStdTenantPlatform.Application.Dtos;
using YTStdTenantPlatform.Entity.TenantPlatform;
using YTStdTenantPlatform.Application.Constants;

namespace YTStdTenantPlatform.Application.Services
{
    /// <summary>平台运营应用服务（统计与监控）</summary>
    public static class PlatformOperationAppService
    {
        // ──────────────────────────────────────────────────────
        // 租户每日统计
        // ──────────────────────────────────────────────────────

        /// <summary>获取租户每日统计列表</summary>
        public static async ValueTask<PagedResult<TenantDailyStatRepDTO>> GetDailyStatListAsync(
            int tenantId, long operatorId, long tenantRefId, PagedRequest request)
        {
            var (result, data) = await TenantDailyStatCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null)
                return new PagedResult<TenantDailyStatRepDTO> { Page = request.NormalizedPage, PageSize = request.NormalizedPageSize };

            var filtered = new List<TenantDailyStat>();
            foreach (var s in data)
            {
                if (s.TenantRefId == tenantRefId)
                    filtered.Add(s);
            }

            var items = new List<TenantDailyStatRepDTO>();
            var offset = request.Offset;
            var size = request.NormalizedPageSize;
            for (int i = offset; i < filtered.Count && i < offset + size; i++)
            {
                var s = filtered[i];
                items.Add(new TenantDailyStatRepDTO
                {
                    Id = s.Id, TenantRefId = s.TenantRefId, StatDate = s.StatDate,
                    ActiveUserCount = s.ActiveUserCount, NewUserCount = s.NewUserCount,
                    ApiCallCount = s.ApiCallCount, StorageBytes = s.StorageBytes,
                    ResourceScore = s.ResourceScore, CreatedAt = s.CreatedAt
                });
            }

            return new PagedResult<TenantDailyStatRepDTO>
            {
                Items = items, Total = filtered.Count,
                Page = request.NormalizedPage, PageSize = request.NormalizedPageSize
            };
        }

        // ──────────────────────────────────────────────────────
        // 平台监控指标
        // ──────────────────────────────────────────────────────

        /// <summary>获取平台监控指标列表</summary>
        public static async ValueTask<PagedResult<PlatformMonitorMetricRepDTO>> GetMonitorMetricListAsync(
            int tenantId, long operatorId, PagedRequest request)
        {
            var (result, data) = await PlatformMonitorMetricCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null)
                return new PagedResult<PlatformMonitorMetricRepDTO> { Page = request.NormalizedPage, PageSize = request.NormalizedPageSize };

            var filtered = new List<PlatformMonitorMetric>();
            foreach (var m in data)
            {
                if (!string.IsNullOrEmpty(request.Keyword) &&
                    m.MetricType.IndexOf(request.Keyword, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                filtered.Add(m);
            }

            var items = new List<PlatformMonitorMetricRepDTO>();
            var offset = request.Offset;
            var size = request.NormalizedPageSize;
            for (int i = offset; i < filtered.Count && i < offset + size; i++)
            {
                var m = filtered[i];
                items.Add(new PlatformMonitorMetricRepDTO
                {
                    Id = m.Id, ComponentName = m.ComponentName,
                    MetricType = m.MetricType, MetricKey = m.MetricKey,
                    MetricValue = m.MetricValue, MetricUnit = m.MetricUnit,
                    CollectedAt = m.CollectedAt
                });
            }

            return new PagedResult<PlatformMonitorMetricRepDTO>
            {
                Items = items, Total = filtered.Count,
                Page = request.NormalizedPage, PageSize = request.NormalizedPageSize
            };
        }
    }
}
