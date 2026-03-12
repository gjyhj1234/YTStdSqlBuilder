using System;
using YTStdEntity.Attributes;

namespace YTStdEntity.Sample;

/// <summary>示例实体：系统用户</summary>
[Entity(TableName = "sys_user", NeedAuditTable = true)]
[Index("idx_user_name", "name")]
[Index("idx_user_email", "email", Kind = IndexKind.Unique)]
public class SysUser
{
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    public int TenantId { get; set; }

    [Column(Title = "用户名", Length = 50, IsRequired = true)]
    public string Name { get; set; } = "";

    [Column(Title = "邮箱", Length = 200)]
    public string? Email { get; set; }

    [Column(Precision = 4)]
    public decimal? Balance { get; set; }

    public DateTime CreatedAt { get; set; }

    public string[]? Tags { get; set; }
}

/// <summary>示例实体：订单（主表）</summary>
[Entity(TableName = "order", NeedAuditTable = true)]
[Index("idx_order_user", "user_id")]
public class Order
{
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    public int TenantId { get; set; }

    public long UserId { get; set; }

    [Column(Precision = 2)]
    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }
}

/// <summary>示例实体：订单明细（从表）</summary>
[Entity(TableName = "order_item", NeedAuditTable = true)]
[DetailOf(typeof(Order), ForeignKey = "OrderId")]
[Index("idx_order_item_order", "order_id")]
public class OrderItem
{
    [Column(IsPrimaryKey = true)]
    public long Id { get; set; }

    public int TenantId { get; set; }

    public long OrderId { get; set; }

    [Column(Length = 100)]
    public string ProductName { get; set; } = "";

    public int Quantity { get; set; }

    [Column(Precision = 2)]
    public decimal UnitPrice { get; set; }
}
