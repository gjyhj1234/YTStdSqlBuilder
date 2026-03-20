using System;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>创建/更新租户资源配额请求</summary>
    public sealed class SaveTenantResourceQuotaReqDTO
    {
        /// <summary>关联租户 ID</summary>
        public long TenantRefId { get; set; }
        /// <summary>配额类型</summary>
        public string QuotaType { get; set; } = "";
        /// <summary>配额上限</summary>
        public long QuotaLimit { get; set; }
        /// <summary>告警阈值</summary>
        public long? WarningThreshold { get; set; }
        /// <summary>重置周期（daily/monthly/yearly/none）</summary>
        public string? ResetCycle { get; set; }
    }
}
