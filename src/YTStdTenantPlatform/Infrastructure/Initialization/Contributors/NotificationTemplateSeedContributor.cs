using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YTStdAdo;
using YTStdTenantPlatform.Entity.TenantPlatform;
using YTStdTenantPlatform.Infrastructure.Initialization.SeedData;

namespace YTStdTenantPlatform.Infrastructure.Initialization.Contributors
{
    /// <summary>通知模板种子数据贡献者</summary>
    public sealed class NotificationTemplateSeedContributor : ISeedContributor
    {
        /// <summary>贡献者名称</summary>
        public string Name => "NotificationTemplate";

        /// <summary>执行顺序</summary>
        public int Order => 60;

        /// <summary>执行幂等初始化</summary>
        public async ValueTask SeedAsync(PlatformSeedContext context)
        {
            int tid = context.TenantId;
            long uid = context.SystemUserId;

            var (result, existingTemplates) = await NotificationTemplateCRUD.GetListAsync(tid, uid);
            var existingMap = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            if (result.Success && existingTemplates != null)
            {
                foreach (var t in existingTemplates)
                {
                    if (!string.IsNullOrEmpty(t.TemplateCode))
                    {
                        existingMap[t.TemplateCode] = t.Id;
                    }
                }
            }

            var defaultTemplates = DefaultNotificationTemplates.GetDefaultTemplates();
            foreach (var template in defaultTemplates)
            {
                if (existingMap.ContainsKey(template.TemplateCode))
                {
                    context.Log("[NotificationTemplate] 模板已存在，跳过: " + template.TemplateCode);
                    continue;
                }

                DbInsResult ins = await NotificationTemplateCRUD.InsertAsync(tid, uid, template);
                if (ins.Success)
                {
                    context.Log("[NotificationTemplate] 插入模板: " + template.TemplateCode + " => Id=" + ins.Id);
                }
                else
                {
                    context.Log("[NotificationTemplate] 插入模板失败: " + template.TemplateCode + ", 错误: " + ins.ErrorMessage);
                }
            }
        }
    }
}
