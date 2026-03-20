using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YTStdLogger.Core;
using YTStdTenantPlatform.Application.Dtos;
using YTStdTenantPlatform.Entity.TenantPlatform;
using YTStdTenantPlatform.Application.Constants;

namespace YTStdTenantPlatform.Application.Services
{
    /// <summary>租户生命周期应用服务</summary>
    public static class TenantLifecycleAppService
    {
        /// <summary>获取租户分页列表</summary>
        public static async ValueTask<PagedResult<TenantRepDTO>> GetListAsync(
            int tenantId, long operatorId, PagedRequest request)
        {
            var (result, data) = await TenantCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null)
                return new PagedResult<TenantRepDTO> { Page = request.NormalizedPage, PageSize = request.NormalizedPageSize };

            var filtered = new List<Tenant>();
            foreach (var t in data)
            {
                if (t.DeletedAt != null) continue;
                if (!string.IsNullOrEmpty(request.Status) &&
                    !string.Equals(t.LifecycleStatus, request.Status, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (!string.IsNullOrEmpty(request.Keyword) &&
                    t.TenantName.IndexOf(request.Keyword, StringComparison.OrdinalIgnoreCase) < 0 &&
                    t.TenantCode.IndexOf(request.Keyword, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                filtered.Add(t);
            }

            var items = new List<TenantRepDTO>();
            var offset = request.Offset;
            var size = request.NormalizedPageSize;
            for (int i = offset; i < filtered.Count && i < offset + size; i++)
                items.Add(MapToDto(filtered[i]));

            return new PagedResult<TenantRepDTO>
            {
                Items = items, Total = filtered.Count,
                Page = request.NormalizedPage, PageSize = request.NormalizedPageSize
            };
        }

        /// <summary>获取租户详情</summary>
        public static async ValueTask<TenantRepDTO?> GetByIdAsync(int tenantId, long operatorId, long id)
        {
            var (result, data) = await TenantCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null) return null;
            foreach (var t in data)
            {
                if (t.Id == id && t.DeletedAt == null)
                    return MapToDto(t);
            }
            return null;
        }

        /// <summary>创建租户</summary>
        public static async ValueTask<ApiResult<long>> CreateAsync(
            int tenantId, long operatorId, CreateTenantReqDTO req)
        {
            if (string.IsNullOrWhiteSpace(req.TenantCode))
                return ApiResult<long>.Fail(ErrorCodes.TenantCodeRequired, Messages.TenantCodeRequired);
            if (string.IsNullOrWhiteSpace(req.TenantName))
                return ApiResult<long>.Fail(ErrorCodes.TenantNameRequired, Messages.TenantNameRequired);

            var now = DateTime.UtcNow;
            var tenant = new Tenant
            {
                TenantCode = req.TenantCode.Trim(),
                TenantName = req.TenantName.Trim(),
                EnterpriseName = req.EnterpriseName,
                ContactName = req.ContactName,
                ContactPhone = req.ContactPhone,
                ContactEmail = req.ContactEmail,
                SourceType = req.SourceType,
                LifecycleStatus = "pending",
                IsolationMode = req.IsolationMode,
                DefaultLanguage = req.DefaultLanguage,
                DefaultTimezone = req.DefaultTimezone,
                Enabled = false,
                CreatedBy = operatorId,
                CreatedAt = now,
                UpdatedAt = now
            };

            var insResult = await TenantCRUD.InsertAsync(tenantId, operatorId, tenant);
            if (!insResult.Success)
                return ApiResult<long>.Fail(ErrorCodes.TenantCreateFailed, Messages.TenantCreateFailed);

            // 记录生命周期事件
            await RecordLifecycleEventAsync(tenantId, operatorId, insResult.Id, "created", null, "pending", "新建租户", operatorId);

            Logger.Info(tenantId, operatorId, "[TenantLifecycleAppService] 创建租户: " + req.TenantCode);
            return ApiResult<long>.Ok(insResult.Id);
        }

        /// <summary>更新租户信息</summary>
        public static async ValueTask<ApiResult> UpdateAsync(
            int tenantId, long operatorId, long id, UpdateTenantReqDTO req)
        {
            var (getResult, tenants) = await TenantCRUD.GetListAsync(tenantId, operatorId);
            if (!getResult.Success || tenants == null) return ApiResult.Fail(ErrorCodes.TenantQueryFailed, Messages.TenantQueryFailed);

            Tenant? target = null;
            foreach (var t in tenants) { if (t.Id == id && t.DeletedAt == null) { target = t; break; } }
            if (target == null) return ApiResult.Fail(ErrorCodes.TenantNotFound, Messages.TenantNotFound);

            if (req.TenantName != null) target.TenantName = req.TenantName;
            if (req.EnterpriseName != null) target.EnterpriseName = req.EnterpriseName;
            if (req.ContactName != null) target.ContactName = req.ContactName;
            if (req.ContactPhone != null) target.ContactPhone = req.ContactPhone;
            if (req.ContactEmail != null) target.ContactEmail = req.ContactEmail;
            target.UpdatedAt = DateTime.UtcNow;

            var updResult = await TenantCRUD.UpdateAsync(tenantId, operatorId, target);
            if (!updResult.Success) return ApiResult.Fail(ErrorCodes.TenantUpdateFailed, Messages.TenantUpdateFailed);

            Logger.Info(tenantId, operatorId, "[TenantLifecycleAppService] 更新租户: " + target.TenantCode);
            return ApiResult.Ok(Messages.OperationSuccess);
        }

        /// <summary>租户状态流转</summary>
        public static async ValueTask<ApiResult> ChangeStatusAsync(
            int tenantId, long operatorId, long id, TenantStatusChangeReqDTO req)
        {
            var (getResult, tenants) = await TenantCRUD.GetListAsync(tenantId, operatorId);
            if (!getResult.Success || tenants == null) return ApiResult.Fail(ErrorCodes.TenantQueryFailed, Messages.TenantQueryFailed);

            Tenant? target = null;
            foreach (var t in tenants) { if (t.Id == id && t.DeletedAt == null) { target = t; break; } }
            if (target == null) return ApiResult.Fail(ErrorCodes.TenantNotFound, Messages.TenantNotFound);

            var fromStatus = target.LifecycleStatus;
            var toStatus = req.TargetStatus;

            // 状态流转校验
            if (!IsValidTransition(fromStatus, toStatus))
                return ApiResult.Fail(ErrorCodes.TenantStatusTransitionDenied, Messages.TenantStatusTransitionDenied);

            target.LifecycleStatus = toStatus;
            target.UpdatedAt = DateTime.UtcNow;

            if (string.Equals(toStatus, "active", StringComparison.OrdinalIgnoreCase))
            {
                target.Enabled = true;
                if (target.ActivatedAt == null) target.ActivatedAt = DateTime.UtcNow;
            }
            else if (string.Equals(toStatus, "suspended", StringComparison.OrdinalIgnoreCase))
            {
                target.Enabled = false;
                target.SuspendedAt = DateTime.UtcNow;
            }
            else if (string.Equals(toStatus, "closed", StringComparison.OrdinalIgnoreCase))
            {
                target.Enabled = false;
                target.ClosedAt = DateTime.UtcNow;
            }

            var updResult = await TenantCRUD.UpdateAsync(tenantId, operatorId, target);
            if (!updResult.Success) return ApiResult.Fail(ErrorCodes.TenantStatusChangeFailed, Messages.TenantStatusChangeFailed);

            await RecordLifecycleEventAsync(tenantId, operatorId, id, "status_changed",
                fromStatus, toStatus, req.Reason, operatorId);

            Logger.Info(tenantId, operatorId,
                "[TenantLifecycleAppService] 租户状态变更: " + target.TenantCode + " " + fromStatus + " → " + toStatus);
            return ApiResult.Ok(Messages.OperationSuccess);
        }

        /// <summary>获取租户生命周期事件列表</summary>
        public static async ValueTask<PagedResult<TenantLifecycleEventRepDTO>> GetLifecycleEventsAsync(
            int tenantId, long operatorId, long tenantRefId, PagedRequest request)
        {
            var (result, data) = await TenantLifecycleEventCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null)
                return new PagedResult<TenantLifecycleEventRepDTO> { Page = request.NormalizedPage, PageSize = request.NormalizedPageSize };

            var filtered = new List<TenantLifecycleEvent>();
            foreach (var e in data)
            {
                if (e.TenantRefId == tenantRefId)
                    filtered.Add(e);
            }

            var items = new List<TenantLifecycleEventRepDTO>();
            var offset = request.Offset;
            var size = request.NormalizedPageSize;
            for (int i = offset; i < filtered.Count && i < offset + size; i++)
            {
                var e = filtered[i];
                items.Add(new TenantLifecycleEventRepDTO
                {
                    Id = e.Id, TenantRefId = e.TenantRefId,
                    EventType = e.EventType, FromStatus = e.FromStatus,
                    ToStatus = e.ToStatus, Reason = e.Reason,
                    OccurredAt = e.OccurredAt
                });
            }

            return new PagedResult<TenantLifecycleEventRepDTO>
            {
                Items = items, Total = filtered.Count,
                Page = request.NormalizedPage, PageSize = request.NormalizedPageSize
            };
        }

        /// <summary>校验状态流转是否合法</summary>
        private static bool IsValidTransition(string from, string to)
        {
            // pending → active
            // active → suspended / closed
            // suspended → active / closed
            if (string.Equals(from, "pending", StringComparison.OrdinalIgnoreCase))
                return string.Equals(to, "active", StringComparison.OrdinalIgnoreCase);
            if (string.Equals(from, "active", StringComparison.OrdinalIgnoreCase))
                return string.Equals(to, "suspended", StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(to, "closed", StringComparison.OrdinalIgnoreCase);
            if (string.Equals(from, "suspended", StringComparison.OrdinalIgnoreCase))
                return string.Equals(to, "active", StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(to, "closed", StringComparison.OrdinalIgnoreCase);
            return false;
        }

        /// <summary>记录生命周期事件</summary>
        private static async ValueTask RecordLifecycleEventAsync(
            int tenantId, long operatorId, long tenantRefId,
            string eventType, string? fromStatus, string? toStatus,
            string? reason, long? opId)
        {
            var evt = new TenantLifecycleEvent
            {
                TenantRefId = tenantRefId,
                EventType = eventType,
                FromStatus = fromStatus,
                ToStatus = toStatus,
                Reason = reason,
                OperatorId = opId,
                OccurredAt = DateTime.UtcNow
            };
            await TenantLifecycleEventCRUD.InsertAsync(tenantId, operatorId, evt);
        }

        /// <summary>映射实体到 DTO</summary>
        private static TenantRepDTO MapToDto(Tenant t)
        {
            return new TenantRepDTO
            {
                Id = t.Id, TenantCode = t.TenantCode, TenantName = t.TenantName,
                EnterpriseName = t.EnterpriseName, ContactName = t.ContactName,
                ContactEmail = t.ContactEmail, LifecycleStatus = t.LifecycleStatus,
                IsolationMode = t.IsolationMode, Enabled = t.Enabled,
                OpenedAt = t.OpenedAt, ExpiresAt = t.ExpiresAt, CreatedAt = t.CreatedAt
            };
        }
    }
}
