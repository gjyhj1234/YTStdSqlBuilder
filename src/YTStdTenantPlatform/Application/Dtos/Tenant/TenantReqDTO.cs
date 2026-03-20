using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>创建租户请求</summary>
    public sealed class CreateTenantReqDTO
    {
        /// <summary>租户编码</summary>
        public string TenantCode { get; set; } = "";
        /// <summary>租户名称</summary>
        public string TenantName { get; set; } = "";
        /// <summary>企业名称</summary>
        public string? EnterpriseName { get; set; }
        /// <summary>联系人姓名</summary>
        public string? ContactName { get; set; }
        /// <summary>联系人手机</summary>
        public string? ContactPhone { get; set; }
        /// <summary>联系人邮箱</summary>
        public string? ContactEmail { get; set; }
        /// <summary>来源类型</summary>
        public string SourceType { get; set; } = "manual";
        /// <summary>隔离模式（shared/schema/database）</summary>
        public string IsolationMode { get; set; } = "shared";
        /// <summary>默认语言</summary>
        public string DefaultLanguage { get; set; } = "zh-CN";
        /// <summary>默认时区</summary>
        public string DefaultTimezone { get; set; } = "Asia/Shanghai";
    }

    /// <summary>更新租户请求</summary>
    public sealed class UpdateTenantReqDTO
    {
        /// <summary>租户名称</summary>
        public string? TenantName { get; set; }
        /// <summary>企业名称</summary>
        public string? EnterpriseName { get; set; }
        /// <summary>联系人姓名</summary>
        public string? ContactName { get; set; }
        /// <summary>联系人手机</summary>
        public string? ContactPhone { get; set; }
        /// <summary>联系人邮箱</summary>
        public string? ContactEmail { get; set; }
    }

    /// <summary>租户状态变更请求</summary>
    public sealed class TenantStatusChangeReqDTO
    {
        /// <summary>目标状态（active/suspended/closed）</summary>
        public string TargetStatus { get; set; } = "";
        /// <summary>变更原因</summary>
        public string? Reason { get; set; }
    }
}
