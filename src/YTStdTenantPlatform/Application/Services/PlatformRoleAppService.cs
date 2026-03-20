using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YTStdLogger.Core;
using YTStdTenantPlatform.Application.Dtos;
using YTStdTenantPlatform.Entity.TenantPlatform;
using YTStdTenantPlatform.Infrastructure.Cache;
using YTStdTenantPlatform.Application.Constants;

namespace YTStdTenantPlatform.Application.Services
{
    /// <summary>平台角色应用服务</summary>
    public static class PlatformRoleAppService
    {
        /// <summary>获取角色分页列表</summary>
        public static async ValueTask<PagedResult<PlatformRoleRepDTO>> GetListAsync(
            int tenantId, long operatorId, PagedRequest request)
        {
            var (result, data) = await PlatformRoleCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null)
                return new PagedResult<PlatformRoleRepDTO> { Page = request.NormalizedPage, PageSize = request.NormalizedPageSize };

            var filtered = new List<PlatformRole>();
            foreach (var r in data)
            {
                if (!string.IsNullOrEmpty(request.Status) &&
                    !string.Equals(r.Status, request.Status, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (!string.IsNullOrEmpty(request.Keyword) &&
                    r.Name.IndexOf(request.Keyword, StringComparison.OrdinalIgnoreCase) < 0 &&
                    r.Code.IndexOf(request.Keyword, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                filtered.Add(r);
            }

            var offset = request.Offset;
            var size = request.NormalizedPageSize;
            var items = new List<PlatformRoleRepDTO>();
            for (int i = offset; i < filtered.Count && i < offset + size; i++)
            {
                var r = filtered[i];
                items.Add(new PlatformRoleRepDTO
                {
                    Id = r.Id, Code = r.Code, Name = r.Name,
                    Description = r.Description, Status = r.Status,
                    CreatedAt = r.CreatedAt
                });
            }

            return new PagedResult<PlatformRoleRepDTO>
            {
                Items = items, Total = filtered.Count,
                Page = request.NormalizedPage, PageSize = request.NormalizedPageSize
            };
        }

        /// <summary>获取角色详情</summary>
        public static async ValueTask<PlatformRoleRepDTO?> GetByIdAsync(int tenantId, long operatorId, long id)
        {
            var (result, data) = await PlatformRoleCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null) return null;
            foreach (var r in data)
            {
                if (r.Id == id)
                    return new PlatformRoleRepDTO
                    {
                        Id = r.Id, Code = r.Code, Name = r.Name,
                        Description = r.Description, Status = r.Status,
                        CreatedAt = r.CreatedAt
                    };
            }
            return null;
        }

        /// <summary>创建角色</summary>
        public static async ValueTask<ApiResult<long>> CreateAsync(
            int tenantId, long operatorId, CreatePlatformRoleReqDTO req)
        {
            if (string.IsNullOrWhiteSpace(req.Code))
                return ApiResult<long>.Fail(ErrorCodes.RoleCodeRequired, Messages.RoleCodeRequired);
            if (string.IsNullOrWhiteSpace(req.Name))
                return ApiResult<long>.Fail(ErrorCodes.RoleNameRequired, Messages.RoleNameRequired);

            var now = DateTime.UtcNow;
            var role = new PlatformRole
            {
                Code = req.Code.Trim(),
                Name = req.Name.Trim(),
                Description = req.Description,
                Status = "active",
                CreatedBy = operatorId,
                UpdatedBy = operatorId,
                CreatedAt = now,
                UpdatedAt = now
            };

            var insResult = await PlatformRoleCRUD.InsertAsync(tenantId, operatorId, role);
            if (!insResult.Success)
                return ApiResult<long>.Fail(ErrorCodes.RoleCreateFailed, Messages.RoleCreateFailed);

            await PlatformCacheCoordinator.InvalidatePermissionsAsync();
            Logger.Info(tenantId, operatorId, "[PlatformRoleAppService] 创建角色: " + req.Code);
            return ApiResult<long>.Ok(insResult.Id);
        }

        /// <summary>更新角色</summary>
        public static async ValueTask<ApiResult> UpdateAsync(
            int tenantId, long operatorId, long id, UpdatePlatformRoleReqDTO req)
        {
            var (getResult, roles) = await PlatformRoleCRUD.GetListAsync(tenantId, operatorId);
            if (!getResult.Success || roles == null) return ApiResult.Fail(ErrorCodes.RoleQueryFailed, Messages.RoleQueryFailed);

            PlatformRole? target = null;
            foreach (var r in roles) { if (r.Id == id) { target = r; break; } }
            if (target == null) return ApiResult.Fail(ErrorCodes.RoleNotFound, Messages.RoleNotFound);

            if (req.Name != null) target.Name = req.Name;
            if (req.Description != null) target.Description = req.Description;
            target.UpdatedBy = operatorId;
            target.UpdatedAt = DateTime.UtcNow;

            var updResult = await PlatformRoleCRUD.UpdateAsync(tenantId, operatorId, target);
            if (!updResult.Success) return ApiResult.Fail(ErrorCodes.RoleUpdateFailed, Messages.RoleUpdateFailed);

            await PlatformCacheCoordinator.InvalidatePermissionsAsync();
            Logger.Info(tenantId, operatorId, "[PlatformRoleAppService] 更新角色: " + target.Code);
            return ApiResult.Ok(Messages.OperationSuccess);
        }

        /// <summary>启用/禁用角色</summary>
        public static async ValueTask<ApiResult> SetStatusAsync(
            int tenantId, long operatorId, long id, string status)
        {
            var (getResult, roles) = await PlatformRoleCRUD.GetListAsync(tenantId, operatorId);
            if (!getResult.Success || roles == null) return ApiResult.Fail(ErrorCodes.RoleQueryFailed, Messages.RoleQueryFailed);

            PlatformRole? target = null;
            foreach (var r in roles) { if (r.Id == id) { target = r; break; } }
            if (target == null) return ApiResult.Fail(ErrorCodes.RoleNotFound, Messages.RoleNotFound);

            target.Status = status;
            target.UpdatedAt = DateTime.UtcNow;
            var updResult = await PlatformRoleCRUD.UpdateAsync(tenantId, operatorId, target);
            if (!updResult.Success) return ApiResult.Fail(ErrorCodes.RoleStatusChangeFailed, Messages.RoleStatusChangeFailed);

            await PlatformCacheCoordinator.InvalidatePermissionsAsync();
            Logger.Info(tenantId, operatorId,
                "[PlatformRoleAppService] 角色状态变更: " + target.Code + " → " + status);
            return ApiResult.Ok(Messages.OperationSuccess);
        }

        /// <summary>角色授权（绑定权限）</summary>
        public static async ValueTask<ApiResult> BindPermissionsAsync(
            int tenantId, long operatorId, long roleId, RolePermissionBindReqDTO req)
        {
            var now = DateTime.UtcNow;
            foreach (var permId in req.PermissionIds)
            {
                var rp = new PlatformRolePermission
                {
                    RoleId = roleId,
                    PermissionId = permId,
                    GrantedBy = operatorId,
                    GrantedAt = now
                };
                await PlatformRolePermissionCRUD.InsertAsync(tenantId, operatorId, rp);
            }

            await PlatformCacheCoordinator.InvalidatePermissionsAsync();
            Logger.Info(tenantId, operatorId,
                "[PlatformRoleAppService] 角色授权: roleId=" + roleId + " 权限数=" + req.PermissionIds.Length);
            return ApiResult.Ok(Messages.OperationSuccess);
        }

        /// <summary>角色成员管理（绑定用户）</summary>
        public static async ValueTask<ApiResult> BindMembersAsync(
            int tenantId, long operatorId, long roleId, RoleMemberBindReqDTO req)
        {
            var now = DateTime.UtcNow;
            foreach (var userId in req.UserIds)
            {
                var rm = new PlatformRoleMember
                {
                    RoleId = roleId,
                    UserId = userId,
                    AssignedBy = operatorId,
                    AssignedAt = now
                };
                await PlatformRoleMemberCRUD.InsertAsync(tenantId, operatorId, rm);
            }

            await PlatformCacheCoordinator.InvalidateUserRolesAsync();
            Logger.Info(tenantId, operatorId,
                "[PlatformRoleAppService] 角色成员: roleId=" + roleId + " 用户数=" + req.UserIds.Length);
            return ApiResult.Ok(Messages.OperationSuccess);
        }
    }
}
