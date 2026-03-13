using System;
using System.Runtime.CompilerServices;

namespace YTStdEntity.Attributes;

/// <summary>索引特性，标注在实体类上，可多个</summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class IndexAttribute : Attribute
{
    /// <summary>索引名称</summary>
    public string IndexName { get; }

    /// <summary>索引字段列表</summary>
    public string[] Columns { get; }

    /// <summary>索引类型（默认 Normal）</summary>
    public IndexKind Kind { get; set; } = IndexKind.Normal;

    /// <summary>
    /// 初始化索引特性。
    /// </summary>
    /// <param name="indexName">索引名称</param>
    /// <param name="columns">索引字段列表</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IndexAttribute(string indexName, params string[] columns)
    {
        IndexName = indexName;
        Columns = columns;
    }
}

/// <summary>索引类型</summary>
public enum IndexKind
{
    /// <summary>普通索引</summary>
    Normal,
    /// <summary>唯一索引</summary>
    Unique
}
