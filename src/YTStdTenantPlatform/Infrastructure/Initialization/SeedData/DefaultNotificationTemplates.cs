using System;
using System.Collections.Generic;
using YTStdTenantPlatform.Entity.TenantPlatform;

namespace YTStdTenantPlatform.Infrastructure.Initialization.SeedData
{
    /// <summary>默认通知模板种子数据</summary>
    public static class DefaultNotificationTemplates
    {
        /// <summary>获取默认通知模板列表</summary>
        public static IReadOnlyList<NotificationTemplate> GetDefaultTemplates()
        {
            var now = DateTime.UtcNow;
            return new[]
            {
                new NotificationTemplate
                {
                    TemplateCode = "tenant_welcome",
                    TemplateName = "租户欢迎邮件",
                    Channel = "email",
                    SubjectTemplate = "欢迎使用 {{platform_name}}",
                    BodyTemplate = "尊敬的 {{contact_name}}，\n\n"
                        + "欢迎加入 {{platform_name}}！您的租户 {{tenant_name}}（编码：{{tenant_code}}）已创建成功。\n\n"
                        + "请登录管理后台完成初始配置。\n\n"
                        + "如有任何问题，请联系平台客服。",
                    Variables = "[\"platform_name\",\"contact_name\",\"tenant_name\",\"tenant_code\"]",
                    Status = "active",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new NotificationTemplate
                {
                    TemplateCode = "subscription_expire_warning",
                    TemplateName = "订阅即将到期提醒",
                    Channel = "email",
                    SubjectTemplate = "订阅即将到期提醒 - {{package_name}}",
                    BodyTemplate = "尊敬的 {{contact_name}}，\n\n"
                        + "您的租户 {{tenant_name}} 订阅的 {{package_name}} 将于 {{expire_date}} 到期。\n\n"
                        + "为避免服务中断，请及时续费。\n\n"
                        + "感谢您的使用。",
                    Variables = "[\"contact_name\",\"tenant_name\",\"package_name\",\"expire_date\"]",
                    Status = "active",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new NotificationTemplate
                {
                    TemplateCode = "payment_success",
                    TemplateName = "支付成功通知",
                    Channel = "email",
                    SubjectTemplate = "支付成功 - 订单 {{order_no}}",
                    BodyTemplate = "尊敬的 {{contact_name}}，\n\n"
                        + "您的订单 {{order_no}} 已支付成功。\n\n"
                        + "支付金额：{{amount}} {{currency}}\n"
                        + "套餐名称：{{package_name}}\n"
                        + "有效期至：{{expire_date}}\n\n"
                        + "感谢您的支持。",
                    Variables = "[\"contact_name\",\"order_no\",\"amount\",\"currency\",\"package_name\",\"expire_date\"]",
                    Status = "active",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new NotificationTemplate
                {
                    TemplateCode = "password_reset",
                    TemplateName = "密码重置通知",
                    Channel = "email",
                    SubjectTemplate = "密码重置请求",
                    BodyTemplate = "尊敬的 {{username}}，\n\n"
                        + "我们收到了您的密码重置请求。请使用以下链接重置密码：\n\n"
                        + "{{reset_link}}\n\n"
                        + "此链接将在 {{expire_minutes}} 分钟后失效。\n\n"
                        + "如果您未发起此操作，请忽略本邮件并确认账户安全。",
                    Variables = "[\"username\",\"reset_link\",\"expire_minutes\"]",
                    Status = "active",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new NotificationTemplate
                {
                    TemplateCode = "system_maintenance",
                    TemplateName = "系统维护通知",
                    Channel = "site_message",
                    SubjectTemplate = "系统维护通知",
                    BodyTemplate = "尊敬的用户，\n\n"
                        + "平台将于 {{start_time}} 至 {{end_time}} 进行系统维护。\n\n"
                        + "维护期间服务可能暂时不可用，请提前做好准备。\n\n"
                        + "维护内容：{{maintenance_desc}}\n\n"
                        + "给您带来的不便，敬请谅解。",
                    Variables = "[\"start_time\",\"end_time\",\"maintenance_desc\"]",
                    Status = "active",
                    CreatedAt = now,
                    UpdatedAt = now
                }
            };
        }
    }
}
