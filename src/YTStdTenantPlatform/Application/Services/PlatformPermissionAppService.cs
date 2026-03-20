using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YTStdTenantPlatform.Application.Dtos;
using YTStdTenantPlatform.Entity.TenantPlatform;
using YTStdTenantPlatform.Infrastructure.Cache;
using YTStdTenantPlatform.Application.Constants;

namespace YTStdTenantPlatform.Application.Services
{
    /// <summary>平台权限应用服务</summary>
    public static class PlatformPermissionAppService
    {
        /// <summary>获取权限树</summary>
        public static async ValueTask<List<PlatformPermissionRepDTO>> GetTreeAsync(int tenantId, long operatorId)
        {
            var (result, data) = await PlatformPermissionCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null)
                return new List<PlatformPermissionRepDTO>();

            return BuildTree(data);
        }

        /// <summary>获取权限平铺列表</summary>
        public static async ValueTask<List<PlatformPermissionRepDTO>> GetFlatListAsync(int tenantId, long operatorId)
        {
            var (result, data) = await PlatformPermissionCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null)
                return new List<PlatformPermissionRepDTO>();

            var list = new List<PlatformPermissionRepDTO>(data.Count);
            foreach (var p in data)
                list.Add(MapToDto(p));
            return list;
        }

        /// <summary>获取权限详情</summary>
        public static async ValueTask<PlatformPermissionRepDTO?> GetByIdAsync(int tenantId, long operatorId, long id)
        {
            var cache = PlatformCacheWarmer.PermissionCache;
            foreach (var kvp in cache)
            {
                if (kvp.Value.Id == id)
                    return MapToDto(kvp.Value);
            }

            var (result, data) = await PlatformPermissionCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null) return null;
            foreach (var p in data)
            {
                if (p.Id == id)
                    return MapToDto(p);
            }
            return null;
        }

        /// <summary>按权限编码查询</summary>
        public static PlatformPermissionRepDTO? GetByCode(string code)
        {
            var cache = PlatformCacheWarmer.PermissionCache;
            if (cache.TryGetValue(code, out var perm))
                return MapToDto(perm);
            return null;
        }

        /// <summary>构建权限树</summary>
        private static List<PlatformPermissionRepDTO> BuildTree(IReadOnlyList<PlatformPermission> data)
        {
            var allDtos = new Dictionary<long, PlatformPermissionRepDTO>(data.Count);
            foreach (var p in data)
            {
                var dto = MapToDto(p);
                dto.Children = new List<PlatformPermissionRepDTO>();
                allDtos[p.Id] = dto;
            }

            var roots = new List<PlatformPermissionRepDTO>();
            foreach (var p in data)
            {
                var dto = allDtos[p.Id];
                if (p.ParentId == null || p.ParentId == 0)
                {
                    roots.Add(dto);
                }
                else if (allDtos.TryGetValue(p.ParentId.Value, out var parent))
                {
                    parent.Children!.Add(dto);
                }
            }
            return roots;
        }

        /// <summary>映射实体到 DTO</summary>
        private static PlatformPermissionRepDTO MapToDto(PlatformPermission p)
        {
            return new PlatformPermissionRepDTO
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                PermissionType = p.PermissionType,
                ParentId = p.ParentId,
                Path = p.Path,
                Method = p.Method
            };
        }
    }
}
