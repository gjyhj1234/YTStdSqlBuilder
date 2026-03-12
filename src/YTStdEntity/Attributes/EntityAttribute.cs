using System;

namespace YTStdEntity.Attributes;

/// <summary>实体级特性，标注在实体类上</summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class EntityAttribute : Attribute
{
    /// <summary>物理表名或视图名。为空时使用类名</summary>
    public string? TableName { get; set; }

    /// <summary>视图 SQL。设置后视为视图而非表</summary>
    public string? ViewSql { get; set; }

    /// <summary>是否需要审计表（默认 false）</summary>
    public bool NeedAuditTable { get; set; }
}
