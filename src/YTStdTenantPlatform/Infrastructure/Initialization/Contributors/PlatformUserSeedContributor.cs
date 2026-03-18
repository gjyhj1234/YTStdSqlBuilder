using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YTStdAdo;
using YTStdTenantPlatform.Entity.TenantPlatform;
using YTStdTenantPlatform.Infrastructure.Initialization.SeedData;

namespace YTStdTenantPlatform.Infrastructure.Initialization.Contributors
{
    /// <summary>平台用户种子数据贡献者</summary>
    public sealed class PlatformUserSeedContributor : ISeedContributor
    {
        /// <summary>贡献者名称</summary>
        public string Name => "PlatformUser";

        /// <summary>执行顺序</summary>
        public int Order => 10;

        /// <summary>执行幂等初始化</summary>
        public async ValueTask SeedAsync(PlatformSeedContext context)
        {
            int tid = context.TenantId;
            long uid = context.SystemUserId;

            var (result, existingUsers) = await PlatformUserCRUD.GetListAsync(tid, uid);
            var existingMap = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            if (result.Success && existingUsers != null)
            {
                foreach (var u in existingUsers)
                {
                    if (!string.IsNullOrEmpty(u.Username))
                    {
                        existingMap[u.Username] = u.Id;
                    }
                }
            }

            var defaultUsers = DefaultPlatformUsers.GetDefaultUsers();
            foreach (var user in defaultUsers)
            {
                if (existingMap.TryGetValue(user.Username, out long existingId))
                {
                    context.UserIdMap[user.Username] = existingId;
                    context.Log("[PlatformUser] 用户已存在，跳过: " + user.Username);
                    continue;
                }

                user.Id = await context.GetNextLongIdAsync();
                DbInsResult ins = await PlatformUserCRUD.InsertAsync(tid, uid, user);
                if (ins.Success)
                {
                    context.UserIdMap[user.Username] = ins.Id;
                    context.Log("[PlatformUser] 插入用户: " + user.Username + " => Id=" + ins.Id);
                }
                else
                {
                    context.Log("[PlatformUser] 插入用户失败: " + user.Username + ", 错误: " + ins.ErrorMessage);
                }
            }
        }
    }
}
