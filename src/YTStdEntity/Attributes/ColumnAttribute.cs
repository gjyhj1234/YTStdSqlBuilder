using System;

namespace YTStdEntity.Attributes;

/// <summary>列级特性，标注在实体属性上</summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class ColumnAttribute : Attribute
{
    /// <summary>数据库列名。为空时使用属性名</summary>
    public string? ColumnName { get; set; }

    /// <summary>显示标题。为空时使用属性名</summary>
    public string? Title { get; set; }

    /// <summary>是否为主键（默认 false）。系统禁止联合主键</summary>
    public bool IsPrimaryKey { get; set; }

    /// <summary>
    /// 字段长度：
    /// - string 类型：设置后使用 varchar(Length)，不设置使用 text
    /// - decimal 类型：设置后使用该值，不设置默认 12
    /// </summary>
    public int Length { get; set; }

    /// <summary>decimal 精度（默认 2）</summary>
    public int Precision { get; set; } = 2;

    /// <summary>是否必填 / NOT NULL（默认 false）</summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// 显式指定数据库字段类型（PostgreSQL 类型字符串）。
    /// 为空时根据 CLR 类型自动映射。
    /// </summary>
    public string? DbType { get; set; }
}
