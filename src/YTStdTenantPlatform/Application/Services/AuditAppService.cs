using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YTStdLogger.Core;
using YTStdTenantPlatform.Application.Dtos;
using YTStdTenantPlatform.Entity.TenantPlatform;
using YTStdTenantPlatform.Application.Constants;

namespace YTStdTenantPlatform.Application.Services
{
    /// <summary>日志与审计应用服务（只读）</summary>
    public static class AuditAppService
    {
        // ──────────────────────────────────────────────────────
        // 操作日志
        // ──────────────────────────────────────────────────────

        /// <summary>获取操作日志分页列表</summary>
        public static async ValueTask<PagedResult<OperationLogRepDTO>> GetOperationLogListAsync(
            int tenantId, long operatorId, PagedRequest request)
        {
            var (result, data) = await OperationLogCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null)
                return new PagedResult<OperationLogRepDTO> { Page = request.NormalizedPage, PageSize = request.NormalizedPageSize };

            var filtered = new List<OperationLog>();
            foreach (var log in data)
            {
                if (!string.IsNullOrEmpty(request.Keyword) &&
                    log.Action.IndexOf(request.Keyword, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                filtered.Add(log);
            }

            var items = new List<OperationLogRepDTO>();
            var offset = request.Offset;
            var size = request.NormalizedPageSize;
            for (int i = offset; i < filtered.Count && i < offset + size; i++)
                items.Add(MapOperationLogToDto(filtered[i]));

            return new PagedResult<OperationLogRepDTO>
            {
                Items = items, Total = filtered.Count,
                Page = request.NormalizedPage, PageSize = request.NormalizedPageSize
            };
        }

        /// <summary>获取操作日志详情</summary>
        public static async ValueTask<OperationLogRepDTO?> GetOperationLogByIdAsync(
            int tenantId, long operatorId, long id)
        {
            var (result, data) = await OperationLogCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null) return null;
            foreach (var log in data)
            {
                if (log.Id == id)
                    return MapOperationLogToDto(log);
            }
            return null;
        }

        // ──────────────────────────────────────────────────────
        // 审计日志
        // ──────────────────────────────────────────────────────

        /// <summary>获取审计日志分页列表</summary>
        public static async ValueTask<PagedResult<AuditLogRepDTO>> GetAuditLogListAsync(
            int tenantId, long operatorId, PagedRequest request)
        {
            var (result, data) = await AuditLogCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null)
                return new PagedResult<AuditLogRepDTO> { Page = request.NormalizedPage, PageSize = request.NormalizedPageSize };

            var filtered = new List<AuditLog>();
            foreach (var log in data)
            {
                if (!string.IsNullOrEmpty(request.Status) &&
                    !string.Equals(log.Severity, request.Status, StringComparison.OrdinalIgnoreCase))
                    continue;
                filtered.Add(log);
            }

            var items = new List<AuditLogRepDTO>();
            var offset = request.Offset;
            var size = request.NormalizedPageSize;
            for (int i = offset; i < filtered.Count && i < offset + size; i++)
                items.Add(MapAuditLogToDto(filtered[i]));

            return new PagedResult<AuditLogRepDTO>
            {
                Items = items, Total = filtered.Count,
                Page = request.NormalizedPage, PageSize = request.NormalizedPageSize
            };
        }

        /// <summary>获取审计日志详情</summary>
        public static async ValueTask<AuditLogRepDTO?> GetAuditLogByIdAsync(
            int tenantId, long operatorId, long id)
        {
            var (result, data) = await AuditLogCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null) return null;
            foreach (var log in data)
            {
                if (log.Id == id)
                    return MapAuditLogToDto(log);
            }
            return null;
        }

        // ──────────────────────────────────────────────────────
        // 系统日志
        // ──────────────────────────────────────────────────────

        /// <summary>获取系统日志分页列表</summary>
        public static async ValueTask<PagedResult<SystemLogRepDTO>> GetSystemLogListAsync(
            int tenantId, long operatorId, PagedRequest request)
        {
            var (result, data) = await SystemLogCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null)
                return new PagedResult<SystemLogRepDTO> { Page = request.NormalizedPage, PageSize = request.NormalizedPageSize };

            var filtered = new List<SystemLog>();
            foreach (var log in data)
            {
                if (!string.IsNullOrEmpty(request.Status) &&
                    !string.Equals(log.LogLevel, request.Status, StringComparison.OrdinalIgnoreCase))
                    continue;
                filtered.Add(log);
            }

            var items = new List<SystemLogRepDTO>();
            var offset = request.Offset;
            var size = request.NormalizedPageSize;
            for (int i = offset; i < filtered.Count && i < offset + size; i++)
                items.Add(MapSystemLogToDto(filtered[i]));

            return new PagedResult<SystemLogRepDTO>
            {
                Items = items, Total = filtered.Count,
                Page = request.NormalizedPage, PageSize = request.NormalizedPageSize
            };
        }

        /// <summary>获取系统日志详情</summary>
        public static async ValueTask<SystemLogRepDTO?> GetSystemLogByIdAsync(
            int tenantId, long operatorId, long id)
        {
            var (result, data) = await SystemLogCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null) return null;
            foreach (var log in data)
            {
                if (log.Id == id)
                    return MapSystemLogToDto(log);
            }
            return null;
        }

        // ──────────────────────────────────────────────────────
        // Mapping helpers
        // ──────────────────────────────────────────────────────

        private static OperationLogRepDTO MapOperationLogToDto(OperationLog l) => new OperationLogRepDTO
        {
            Id = l.Id, TenantRefId = l.TenantRefId, OperatorType = l.OperatorType,
            OperatorId = l.OperatorId, Action = l.Action,
            ResourceType = l.ResourceType, ResourceId = l.ResourceId,
            IpAddress = l.IpAddress, OperationResult = l.OperationResult,
            CreatedAt = l.CreatedAt
        };

        private static AuditLogRepDTO MapAuditLogToDto(AuditLog l) => new AuditLogRepDTO
        {
            Id = l.Id, TenantRefId = l.TenantRefId, AuditType = l.AuditType,
            Severity = l.Severity, SubjectType = l.SubjectType,
            SubjectId = l.SubjectId, ComplianceTag = l.ComplianceTag,
            CreatedAt = l.CreatedAt
        };

        private static SystemLogRepDTO MapSystemLogToDto(SystemLog l) => new SystemLogRepDTO
        {
            Id = l.Id, ServiceName = l.ServiceName, LogLevel = l.LogLevel,
            TraceId = l.TraceId, Message = l.Message,
            CreatedAt = l.CreatedAt
        };
    }
}
