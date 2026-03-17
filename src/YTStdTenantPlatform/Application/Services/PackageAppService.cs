using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YTStdLogger.Core;
using YTStdTenantPlatform.Application.Dtos;
using YTStdTenantPlatform.Entity.TenantPlatform;

namespace YTStdTenantPlatform.Application.Services
{
    /// <summary>SaaS 套餐应用服务</summary>
    public static class PackageAppService
    {
        // ──────────────────────────────────────────────────────
        // SaaS 套餐
        // ──────────────────────────────────────────────────────

        /// <summary>获取套餐分页列表</summary>
        public static async ValueTask<PagedResult<SaasPackageDto>> GetPackageListAsync(
            int tenantId, long operatorId, PagedRequest request)
        {
            var (result, data) = await SaasPackageCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null)
                return new PagedResult<SaasPackageDto> { Page = request.NormalizedPage, PageSize = request.NormalizedPageSize };

            var filtered = new List<SaasPackage>();
            foreach (var p in data)
            {
                if (!string.IsNullOrEmpty(request.Status) &&
                    !string.Equals(p.Status, request.Status, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (!string.IsNullOrEmpty(request.Keyword) &&
                    p.PackageName.IndexOf(request.Keyword, StringComparison.OrdinalIgnoreCase) < 0 &&
                    p.PackageCode.IndexOf(request.Keyword, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                filtered.Add(p);
            }

            var items = new List<SaasPackageDto>();
            var offset = request.Offset;
            var size = request.NormalizedPageSize;
            for (int i = offset; i < filtered.Count && i < offset + size; i++)
                items.Add(MapPackageToDto(filtered[i]));

            return new PagedResult<SaasPackageDto>
            {
                Items = items, Total = filtered.Count,
                Page = request.NormalizedPage, PageSize = request.NormalizedPageSize
            };
        }

        /// <summary>获取套餐详情</summary>
        public static async ValueTask<SaasPackageDto?> GetPackageByIdAsync(int tenantId, long operatorId, long id)
        {
            var (result, data) = await SaasPackageCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null) return null;
            foreach (var p in data)
            {
                if (p.Id == id)
                    return MapPackageToDto(p);
            }
            return null;
        }

        /// <summary>创建 SaaS 套餐</summary>
        public static async ValueTask<ApiResult<long>> CreatePackageAsync(
            int tenantId, long operatorId, CreateSaasPackageRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.PackageCode))
                return ApiResult<long>.Fail("套餐编码不能为空");

            var now = DateTime.UtcNow;
            var entity = new SaasPackage
            {
                PackageCode = req.PackageCode.Trim(),
                PackageName = req.PackageName.Trim(),
                Description = req.Description,
                Status = "active",
                CreatedBy = operatorId,
                CreatedAt = now,
                UpdatedAt = now
            };

            var insResult = await SaasPackageCRUD.InsertAsync(tenantId, operatorId, entity);
            if (!insResult.Success)
                return ApiResult<long>.Fail("创建套餐失败: " + insResult.ErrorMessage);

            Logger.Info(tenantId, operatorId, "[PackageAppService] 创建套餐: " + req.PackageCode);
            return ApiResult<long>.Ok(insResult.Id);
        }

        /// <summary>更新 SaaS 套餐</summary>
        public static async ValueTask<ApiResult> UpdatePackageAsync(
            int tenantId, long operatorId, long id, UpdateSaasPackageRequest req)
        {
            var (getResult, packages) = await SaasPackageCRUD.GetListAsync(tenantId, operatorId);
            if (!getResult.Success || packages == null) return ApiResult.Fail("查询套餐失败");

            SaasPackage? target = null;
            foreach (var p in packages) { if (p.Id == id) { target = p; break; } }
            if (target == null) return ApiResult.Fail("套餐不存在");

            if (req.PackageName != null) target.PackageName = req.PackageName;
            if (req.Description != null) target.Description = req.Description;
            target.UpdatedAt = DateTime.UtcNow;

            var updResult = await SaasPackageCRUD.UpdateAsync(tenantId, operatorId, target);
            if (!updResult.Success) return ApiResult.Fail("更新套餐失败");

            Logger.Info(tenantId, operatorId, "[PackageAppService] 更新套餐: " + target.PackageCode);
            return ApiResult.Ok();
        }

        /// <summary>设置套餐状态（启用/禁用）</summary>
        public static async ValueTask<ApiResult> SetPackageStatusAsync(
            int tenantId, long operatorId, long id, string status)
        {
            var (getResult, packages) = await SaasPackageCRUD.GetListAsync(tenantId, operatorId);
            if (!getResult.Success || packages == null) return ApiResult.Fail("查询套餐失败");

            SaasPackage? target = null;
            foreach (var p in packages) { if (p.Id == id) { target = p; break; } }
            if (target == null) return ApiResult.Fail("套餐不存在");

            target.Status = status;
            target.UpdatedAt = DateTime.UtcNow;

            var updResult = await SaasPackageCRUD.UpdateAsync(tenantId, operatorId, target);
            if (!updResult.Success) return ApiResult.Fail("状态变更失败");

            Logger.Info(tenantId, operatorId,
                "[PackageAppService] 套餐状态变更: " + target.PackageCode + " → " + status);
            return ApiResult.Ok();
        }

        // ──────────────────────────────────────────────────────
        // SaaS 套餐版本
        // ──────────────────────────────────────────────────────

        /// <summary>获取套餐版本列表</summary>
        public static async ValueTask<PagedResult<SaasPackageVersionDto>> GetVersionListAsync(
            int tenantId, long operatorId, long packageId, PagedRequest request)
        {
            var (result, data) = await SaasPackageVersionCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null)
                return new PagedResult<SaasPackageVersionDto> { Page = request.NormalizedPage, PageSize = request.NormalizedPageSize };

            var filtered = new List<SaasPackageVersion>();
            foreach (var v in data)
            {
                if (v.PackageId != packageId) continue;
                filtered.Add(v);
            }

            var items = new List<SaasPackageVersionDto>();
            var offset = request.Offset;
            var size = request.NormalizedPageSize;
            for (int i = offset; i < filtered.Count && i < offset + size; i++)
                items.Add(MapVersionToDto(filtered[i]));

            return new PagedResult<SaasPackageVersionDto>
            {
                Items = items, Total = filtered.Count,
                Page = request.NormalizedPage, PageSize = request.NormalizedPageSize
            };
        }

        /// <summary>创建套餐版本</summary>
        public static async ValueTask<ApiResult<long>> CreateVersionAsync(
            int tenantId, long operatorId, CreateSaasPackageVersionRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.VersionCode))
                return ApiResult<long>.Fail("版本编码不能为空");

            var now = DateTime.UtcNow;
            var entity = new SaasPackageVersion
            {
                PackageId = req.PackageId,
                VersionCode = req.VersionCode.Trim(),
                VersionName = req.VersionName.Trim(),
                EditionType = req.EditionType,
                BillingCycle = req.BillingCycle,
                Price = req.Price,
                CurrencyCode = req.CurrencyCode,
                TrialDays = req.TrialDays,
                IsDefault = req.IsDefault,
                Enabled = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            var insResult = await SaasPackageVersionCRUD.InsertAsync(tenantId, operatorId, entity);
            if (!insResult.Success)
                return ApiResult<long>.Fail("创建版本失败: " + insResult.ErrorMessage);

            Logger.Info(tenantId, operatorId, "[PackageAppService] 创建版本: " + req.VersionCode);
            return ApiResult<long>.Ok(insResult.Id);
        }

        // ──────────────────────────────────────────────────────
        // SaaS 套餐能力
        // ──────────────────────────────────────────────────────

        /// <summary>获取套餐能力列表</summary>
        public static async ValueTask<PagedResult<SaasPackageCapabilityDto>> GetCapabilityListAsync(
            int tenantId, long operatorId, long packageVersionId, PagedRequest request)
        {
            var (result, data) = await SaasPackageCapabilityCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null)
                return new PagedResult<SaasPackageCapabilityDto> { Page = request.NormalizedPage, PageSize = request.NormalizedPageSize };

            var filtered = new List<SaasPackageCapability>();
            foreach (var c in data)
            {
                if (c.PackageVersionId != packageVersionId) continue;
                filtered.Add(c);
            }

            var items = new List<SaasPackageCapabilityDto>();
            var offset = request.Offset;
            var size = request.NormalizedPageSize;
            for (int i = offset; i < filtered.Count && i < offset + size; i++)
                items.Add(MapCapabilityToDto(filtered[i]));

            return new PagedResult<SaasPackageCapabilityDto>
            {
                Items = items, Total = filtered.Count,
                Page = request.NormalizedPage, PageSize = request.NormalizedPageSize
            };
        }

        /// <summary>创建/更新套餐能力</summary>
        public static async ValueTask<ApiResult<long>> SaveCapabilityAsync(
            int tenantId, long operatorId, SaveSaasPackageCapabilityRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.CapabilityKey))
                return ApiResult<long>.Fail("能力键不能为空");

            var now = DateTime.UtcNow;
            var entity = new SaasPackageCapability
            {
                PackageVersionId = req.PackageVersionId,
                CapabilityKey = req.CapabilityKey.Trim(),
                CapabilityName = req.CapabilityName.Trim(),
                CapabilityType = req.CapabilityType,
                CapabilityValue = req.CapabilityValue,
                CreatedAt = now,
                UpdatedAt = now
            };

            var insResult = await SaasPackageCapabilityCRUD.InsertAsync(tenantId, operatorId, entity);
            if (!insResult.Success)
                return ApiResult<long>.Fail("保存能力失败: " + insResult.ErrorMessage);

            Logger.Info(tenantId, operatorId, "[PackageAppService] 保存能力: " + req.CapabilityKey);
            return ApiResult<long>.Ok(insResult.Id);
        }

        // ──────────────────────────────────────────────────────
        // Mapping helpers
        // ──────────────────────────────────────────────────────

        private static SaasPackageDto MapPackageToDto(SaasPackage p) => new SaasPackageDto
        {
            Id = p.Id, PackageCode = p.PackageCode, PackageName = p.PackageName,
            Description = p.Description, Status = p.Status, CreatedAt = p.CreatedAt
        };

        private static SaasPackageVersionDto MapVersionToDto(SaasPackageVersion v) => new SaasPackageVersionDto
        {
            Id = v.Id, PackageId = v.PackageId, VersionCode = v.VersionCode,
            VersionName = v.VersionName, EditionType = v.EditionType,
            BillingCycle = v.BillingCycle, Price = v.Price,
            CurrencyCode = v.CurrencyCode, TrialDays = v.TrialDays,
            IsDefault = v.IsDefault, Enabled = v.Enabled,
            EffectiveFrom = v.EffectiveFrom, EffectiveTo = v.EffectiveTo,
            CreatedAt = v.CreatedAt
        };

        private static SaasPackageCapabilityDto MapCapabilityToDto(SaasPackageCapability c) => new SaasPackageCapabilityDto
        {
            Id = c.Id, PackageVersionId = c.PackageVersionId,
            CapabilityKey = c.CapabilityKey, CapabilityName = c.CapabilityName,
            CapabilityType = c.CapabilityType, CapabilityValue = c.CapabilityValue,
            CreatedAt = c.CreatedAt
        };
    }
}
