using System;
using System.Runtime.CompilerServices;

namespace YTStdEntity.Attributes;

/// <summary>
/// 标识当前实体为指定主表实体的明细（从表）。
/// Source Generator 据此生成主从表联合审计查询方法。
/// 一个从表仅能属于一个主表。
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class DetailOfAttribute : Attribute
{
    /// <summary>主表实体的类型</summary>
    public Type MasterType { get; }

    /// <summary>当前从表中指向主表主键的外键属性名</summary>
    public string ForeignKey { get; set; } = "";

    /// <summary>
    /// 初始化明细表特性。
    /// </summary>
    /// <param name="masterType">主表实体的类型</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public DetailOfAttribute(Type masterType)
    {
        MasterType = masterType;
    }
}
