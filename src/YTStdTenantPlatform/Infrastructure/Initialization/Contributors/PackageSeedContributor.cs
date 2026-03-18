using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YTStdAdo;
using YTStdTenantPlatform.Entity.TenantPlatform;
using YTStdTenantPlatform.Infrastructure.Initialization.SeedData;

namespace YTStdTenantPlatform.Infrastructure.Initialization.Contributors
{
    /// <summary>套餐种子数据贡献者</summary>
    public sealed class PackageSeedContributor : ISeedContributor
    {
        /// <summary>贡献者名称</summary>
        public string Name => "Package";

        /// <summary>执行顺序</summary>
        public int Order => 50;

        /// <summary>执行幂等初始化</summary>
        public async ValueTask SeedAsync(PlatformSeedContext context)
        {
            int tid = context.TenantId;
            long uid = context.SystemUserId;

            // ── 1. 插入套餐 ──
            var (pkgResult, existingPkgs) = await SaasPackageCRUD.GetListAsync(tid, uid);
            var existingPkgMap = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            if (pkgResult.Success && existingPkgs != null)
            {
                foreach (var p in existingPkgs)
                {
                    if (!string.IsNullOrEmpty(p.PackageCode))
                    {
                        existingPkgMap[p.PackageCode] = p.Id;
                    }
                }
            }

            var defaultPackages = DefaultPackages.GetDefaultPackages();
            foreach (var pkg in defaultPackages)
            {
                if (existingPkgMap.TryGetValue(pkg.PackageCode, out long existingId))
                {
                    context.PackageIdMap[pkg.PackageCode] = existingId;
                    context.Log("[Package] 套餐已存在，跳过: " + pkg.PackageCode);
                    continue;
                }

                pkg.Id = await context.GetNextLongIdAsync();
                DbInsResult ins = await SaasPackageCRUD.InsertAsync(tid, uid, pkg);
                if (ins.Success)
                {
                    context.PackageIdMap[pkg.PackageCode] = ins.Id;
                    context.Log("[Package] 插入套餐: " + pkg.PackageCode + " => Id=" + ins.Id);
                }
                else
                {
                    context.Log("[Package] 插入套餐失败: " + pkg.PackageCode + ", 错误: " + ins.ErrorMessage);
                }
            }

            // ── 2. 插入套餐版本 ──
            var (verResult, existingVers) = await SaasPackageVersionCRUD.GetListAsync(tid, uid);
            var existingVerMap = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            if (verResult.Success && existingVers != null)
            {
                foreach (var v in existingVers)
                {
                    if (!string.IsNullOrEmpty(v.VersionCode))
                    {
                        string verKey = v.PackageId + ":" + v.VersionCode;
                        existingVerMap[verKey] = v.Id;
                        // 同时按 VersionCode 存入 context 映射
                        context.PackageVersionIdMap[v.VersionCode] = v.Id;
                    }
                }
            }

            var defaultVersions = DefaultPackages.GetDefaultVersions();
            foreach (var vSeed in defaultVersions)
            {
                if (!context.PackageIdMap.TryGetValue(vSeed.PackageCode, out long packageId))
                {
                    context.Log("[Package] 版本关联的套餐未找到: " + vSeed.PackageCode);
                    continue;
                }

                string verKey = packageId + ":" + vSeed.Version.VersionCode;
                if (existingVerMap.ContainsKey(verKey))
                {
                    context.Log("[Package] 版本已存在，跳过: " + vSeed.Version.VersionCode);
                    continue;
                }

                vSeed.Version.PackageId = packageId;
                vSeed.Version.Id = await context.GetNextLongIdAsync();
                DbInsResult ins = await SaasPackageVersionCRUD.InsertAsync(tid, uid, vSeed.Version);
                if (ins.Success)
                {
                    context.PackageVersionIdMap[vSeed.Version.VersionCode] = ins.Id;
                    context.Log("[Package] 插入版本: " + vSeed.Version.VersionCode + " => Id=" + ins.Id);
                }
                else
                {
                    context.Log("[Package] 插入版本失败: " + vSeed.Version.VersionCode + ", 错误: " + ins.ErrorMessage);
                }
            }

            // ── 3. 插入套餐能力 ──
            var (capResult, existingCaps) = await SaasPackageCapabilityCRUD.GetListAsync(tid, uid);
            var existingCapSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (capResult.Success && existingCaps != null)
            {
                foreach (var c in existingCaps)
                {
                    if (!string.IsNullOrEmpty(c.CapabilityKey))
                    {
                        existingCapSet.Add(c.PackageVersionId + ":" + c.CapabilityKey);
                    }
                }
            }

            var defaultCapabilities = DefaultPackages.GetDefaultCapabilities();
            foreach (var cSeed in defaultCapabilities)
            {
                if (!context.PackageVersionIdMap.TryGetValue(cSeed.VersionCode, out long versionId))
                {
                    context.Log("[Package] 能力关联的版本未找到: " + cSeed.VersionCode);
                    continue;
                }

                string capKey = versionId + ":" + cSeed.Capability.CapabilityKey;
                if (existingCapSet.Contains(capKey))
                {
                    continue;
                }

                cSeed.Capability.PackageVersionId = versionId;
                cSeed.Capability.Id = await context.GetNextLongIdAsync();
                DbInsResult ins = await SaasPackageCapabilityCRUD.InsertAsync(tid, uid, cSeed.Capability);
                if (ins.Success)
                {
                    existingCapSet.Add(capKey);
                    context.Log("[Package] 插入能力: " + cSeed.VersionCode + "/" + cSeed.Capability.CapabilityKey + " => Id=" + ins.Id);
                }
                else
                {
                    context.Log("[Package] 插入能力失败: " + cSeed.Capability.CapabilityKey + ", 错误: " + ins.ErrorMessage);
                }
            }
        }
    }
}
