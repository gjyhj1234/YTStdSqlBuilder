using System;
using YTStdEntity.Attributes;

namespace YTStdTenantPlatform.Entity.TenantPlatform;

/// <summary>账单发票明细</summary>
[Entity(TableName = "billing_invoice_items")]
[DetailOf(typeof(BillingInvoice), ForeignKey = "InvoiceId")]
public class BillingInvoiceItem
{
    /// <summary>主键</summary>
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>发票 ID</summary>
    public long InvoiceId { get; set; }

    /// <summary>项目类型</summary>
    [Column(Length = 32, IsRequired = true)]
    public string ItemType { get; set; } = "";

    /// <summary>项目名称</summary>
    [Column(Length = 128, IsRequired = true)]
    public string ItemName { get; set; } = "";

    /// <summary>数量</summary>
    [Column(Length = 18, Precision = 4, IsRequired = true)]
    public decimal Quantity { get; set; }

    /// <summary>单价</summary>
    [Column(Length = 18, Precision = 4, IsRequired = true)]
    public decimal UnitPrice { get; set; }

    /// <summary>金额</summary>
    [Column(Length = 18, IsRequired = true)]
    public decimal Amount { get; set; }

    /// <summary>项目元数据</summary>
    [Column(DbType = "jsonb")]
    public string? ItemMetadata { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }
}
