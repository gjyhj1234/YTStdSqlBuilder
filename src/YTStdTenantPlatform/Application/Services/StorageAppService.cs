using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YTStdLogger.Core;
using YTStdTenantPlatform.Application.Dtos;
using YTStdTenantPlatform.Entity.TenantPlatform;
using YTStdTenantPlatform.Application.Constants;

namespace YTStdTenantPlatform.Application.Services
{
    /// <summary>文件与存储应用服务</summary>
    public static class StorageAppService
    {
        // ──────────────────────────────────────────────────────
        // 存储策略
        // ──────────────────────────────────────────────────────

        /// <summary>获取存储策略分页列表</summary>
        public static async ValueTask<PagedResult<StorageStrategyRepDTO>> GetStrategyListAsync(
            int tenantId, long operatorId, PagedRequest request)
        {
            var (result, data) = await StorageStrategyCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null)
                return new PagedResult<StorageStrategyRepDTO> { Page = request.NormalizedPage, PageSize = request.NormalizedPageSize };

            var filtered = new List<StorageStrategy>();
            foreach (var s in data)
            {
                if (!string.IsNullOrEmpty(request.Keyword) &&
                    s.StrategyName.IndexOf(request.Keyword, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                filtered.Add(s);
            }

            var items = new List<StorageStrategyRepDTO>();
            var offset = request.Offset;
            var size = request.NormalizedPageSize;
            for (int i = offset; i < filtered.Count && i < offset + size; i++)
                items.Add(MapStrategyToDto(filtered[i]));

            return new PagedResult<StorageStrategyRepDTO>
            {
                Items = items, Total = filtered.Count,
                Page = request.NormalizedPage, PageSize = request.NormalizedPageSize
            };
        }

        /// <summary>获取存储策略详情</summary>
        public static async ValueTask<StorageStrategyRepDTO?> GetStrategyByIdAsync(
            int tenantId, long operatorId, long id)
        {
            var (result, data) = await StorageStrategyCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null) return null;
            foreach (var s in data)
            {
                if (s.Id == id)
                    return MapStrategyToDto(s);
            }
            return null;
        }

        /// <summary>创建存储策略</summary>
        public static async ValueTask<ApiResult<long>> CreateStrategyAsync(
            int tenantId, long operatorId, CreateStorageStrategyReqDTO req)
        {
            if (string.IsNullOrWhiteSpace(req.StrategyCode))
                return ApiResult<long>.Fail(ErrorCodes.StorageStrategyNameRequired, Messages.StorageStrategyNameRequired);

            var now = DateTime.UtcNow;
            var entity = new StorageStrategy
            {
                StrategyCode = req.StrategyCode.Trim(),
                StrategyName = req.StrategyName.Trim(),
                ProviderType = req.ProviderType,
                BucketName = req.BucketName,
                BasePath = req.BasePath,
                Status = "active",
                CreatedAt = now,
                UpdatedAt = now
            };

            var insResult = await StorageStrategyCRUD.InsertAsync(tenantId, operatorId, entity);
            if (!insResult.Success)
                return ApiResult<long>.Fail(ErrorCodes.StorageStrategyCreateFailed, Messages.StorageStrategyCreateFailed);

            Logger.Info(tenantId, operatorId,
                "[StorageAppService] 创建存储策略: " + req.StrategyCode);
            return ApiResult<long>.Ok(insResult.Id);
        }

        /// <summary>更新存储策略</summary>
        public static async ValueTask<ApiResult> UpdateStrategyAsync(
            int tenantId, long operatorId, long id, UpdateStorageStrategyReqDTO req)
        {
            var (getResult, strategies) = await StorageStrategyCRUD.GetListAsync(tenantId, operatorId);
            if (!getResult.Success || strategies == null) return ApiResult.Fail(ErrorCodes.StorageStrategyQueryFailed, Messages.StorageStrategyQueryFailed);

            StorageStrategy? target = null;
            foreach (var s in strategies) { if (s.Id == id) { target = s; break; } }
            if (target == null) return ApiResult.Fail(ErrorCodes.StorageStrategyNotFound, Messages.StorageStrategyNotFound);

            if (req.StrategyName != null) target.StrategyName = req.StrategyName;
            if (req.BucketName != null) target.BucketName = req.BucketName;
            if (req.BasePath != null) target.BasePath = req.BasePath;
            target.UpdatedAt = DateTime.UtcNow;

            var updResult = await StorageStrategyCRUD.UpdateAsync(tenantId, operatorId, target);
            if (!updResult.Success) return ApiResult.Fail(ErrorCodes.StorageStrategyUpdateFailed, Messages.StorageStrategyUpdateFailed);

            Logger.Info(tenantId, operatorId,
                "[StorageAppService] 更新存储策略: " + target.StrategyCode);
            return ApiResult.Ok(Messages.OperationSuccess);
        }

        /// <summary>设置存储策略状态（启用/禁用）</summary>
        public static async ValueTask<ApiResult> SetStrategyStatusAsync(
            int tenantId, long operatorId, long id, string status)
        {
            var (getResult, strategies) = await StorageStrategyCRUD.GetListAsync(tenantId, operatorId);
            if (!getResult.Success || strategies == null) return ApiResult.Fail(ErrorCodes.StorageStrategyQueryFailed, Messages.StorageStrategyQueryFailed);

            StorageStrategy? target = null;
            foreach (var s in strategies) { if (s.Id == id) { target = s; break; } }
            if (target == null) return ApiResult.Fail(ErrorCodes.StorageStrategyNotFound, Messages.StorageStrategyNotFound);

            target.Status = status;
            target.UpdatedAt = DateTime.UtcNow;

            var updResult = await StorageStrategyCRUD.UpdateAsync(tenantId, operatorId, target);
            if (!updResult.Success) return ApiResult.Fail(ErrorCodes.StorageStrategyStatusChangeFailed, Messages.StorageStrategyStatusChangeFailed);

            Logger.Info(tenantId, operatorId,
                "[StorageAppService] 存储策略状态变更: " + target.StrategyCode + " → " + status);
            return ApiResult.Ok(Messages.OperationSuccess);
        }

        // ──────────────────────────────────────────────────────
        // 租户文件
        // ──────────────────────────────────────────────────────

        /// <summary>获取文件分页列表</summary>
        public static async ValueTask<PagedResult<TenantFileRepDTO>> GetFileListAsync(
            int tenantId, long operatorId, long tenantRefId, PagedRequest request)
        {
            var (result, data) = await TenantFileCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null)
                return new PagedResult<TenantFileRepDTO> { Page = request.NormalizedPage, PageSize = request.NormalizedPageSize };

            var filtered = new List<TenantFile>();
            foreach (var f in data)
            {
                if (f.TenantRefId != tenantRefId) continue;
                if (!string.IsNullOrEmpty(request.Keyword) &&
                    f.FileName.IndexOf(request.Keyword, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                filtered.Add(f);
            }

            var items = new List<TenantFileRepDTO>();
            var offset = request.Offset;
            var size = request.NormalizedPageSize;
            for (int i = offset; i < filtered.Count && i < offset + size; i++)
                items.Add(MapFileToDto(filtered[i]));

            return new PagedResult<TenantFileRepDTO>
            {
                Items = items, Total = filtered.Count,
                Page = request.NormalizedPage, PageSize = request.NormalizedPageSize
            };
        }

        /// <summary>获取文件详情</summary>
        public static async ValueTask<TenantFileRepDTO?> GetFileByIdAsync(
            int tenantId, long operatorId, long id)
        {
            var (result, data) = await TenantFileCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null) return null;
            foreach (var f in data)
            {
                if (f.Id == id)
                    return MapFileToDto(f);
            }
            return null;
        }

        /// <summary>删除文件</summary>
        public static async ValueTask<ApiResult> DeleteFileAsync(
            int tenantId, long operatorId, long id)
        {
            var delResult = await TenantFileCRUD.DeleteAsync(tenantId, operatorId, id);
            if (!delResult.Success) return ApiResult.Fail(ErrorCodes.FileDeleteFailed, Messages.FileDeleteFailed);

            Logger.Info(tenantId, operatorId, "[StorageAppService] 删除文件: id=" + id);
            return ApiResult.Ok(Messages.OperationSuccess);
        }

        // ──────────────────────────────────────────────────────
        // 文件访问策略
        // ──────────────────────────────────────────────────────

        /// <summary>获取文件访问策略列表</summary>
        public static async ValueTask<PagedResult<FileAccessPolicyRepDTO>> GetFileAccessPoliciesAsync(
            int tenantId, long operatorId, long fileId, PagedRequest request)
        {
            var (result, data) = await FileAccessPolicyCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null)
                return new PagedResult<FileAccessPolicyRepDTO> { Page = request.NormalizedPage, PageSize = request.NormalizedPageSize };

            var filtered = new List<FileAccessPolicy>();
            foreach (var p in data)
            {
                if (p.FileId == fileId)
                    filtered.Add(p);
            }

            var items = new List<FileAccessPolicyRepDTO>();
            var offset = request.Offset;
            var size = request.NormalizedPageSize;
            for (int i = offset; i < filtered.Count && i < offset + size; i++)
            {
                var p = filtered[i];
                items.Add(new FileAccessPolicyRepDTO
                {
                    Id = p.Id, FileId = p.FileId, SubjectType = p.SubjectType,
                    SubjectId = p.SubjectId, PermissionCode = p.PermissionCode,
                    CreatedAt = p.CreatedAt
                });
            }

            return new PagedResult<FileAccessPolicyRepDTO>
            {
                Items = items, Total = filtered.Count,
                Page = request.NormalizedPage, PageSize = request.NormalizedPageSize
            };
        }

        /// <summary>创建/更新文件访问策略</summary>
        public static async ValueTask<ApiResult<long>> SaveFileAccessPolicyAsync(
            int tenantId, long operatorId, SaveFileAccessPolicyReqDTO req)
        {
            if (req.FileId <= 0)
                return ApiResult<long>.Fail(ErrorCodes.InvalidParameter, Messages.InvalidParameter);
            if (string.IsNullOrWhiteSpace(req.SubjectType))
                return ApiResult<long>.Fail(ErrorCodes.InvalidParameter, Messages.InvalidParameter);
            if (string.IsNullOrWhiteSpace(req.PermissionCode))
                return ApiResult<long>.Fail(ErrorCodes.InvalidParameter, Messages.InvalidParameter);

            var entity = new FileAccessPolicy
            {
                FileId = req.FileId,
                SubjectType = req.SubjectType.Trim(),
                SubjectId = req.SubjectId,
                PermissionCode = req.PermissionCode.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            var insResult = await FileAccessPolicyCRUD.InsertAsync(tenantId, operatorId, entity);
            if (!insResult.Success)
                return ApiResult<long>.Fail(ErrorCodes.FileAccessPolicySaveFailed, Messages.FileAccessPolicySaveFailed);

            Logger.Info(tenantId, operatorId,
                "[StorageAppService] 保存文件访问策略: fileId=" + req.FileId);
            return ApiResult<long>.Ok(insResult.Id);
        }

        // ──────────────────────────────────────────────────────
        // Mapping helpers
        // ──────────────────────────────────────────────────────

        private static StorageStrategyRepDTO MapStrategyToDto(StorageStrategy s) => new StorageStrategyRepDTO
        {
            Id = s.Id, StrategyCode = s.StrategyCode, StrategyName = s.StrategyName,
            ProviderType = s.ProviderType, BucketName = s.BucketName,
            BasePath = s.BasePath, Status = s.Status, CreatedAt = s.CreatedAt
        };

        private static TenantFileRepDTO MapFileToDto(TenantFile f) => new TenantFileRepDTO
        {
            Id = f.Id, TenantRefId = f.TenantRefId,
            StorageStrategyId = f.StorageStrategyId, FileName = f.FileName,
            FilePath = f.FilePath, FileExt = f.FileExt, MimeType = f.MimeType,
            FileSize = f.FileSize, UploaderType = f.UploaderType,
            UploaderId = f.UploaderId, Visibility = f.Visibility,
            DownloadCount = f.DownloadCount, CreatedAt = f.CreatedAt
        };
    }
}
