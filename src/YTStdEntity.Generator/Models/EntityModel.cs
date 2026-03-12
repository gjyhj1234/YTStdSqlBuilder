using System.Collections.Generic;

namespace YTStdEntity.Generator.Models;

/// <summary>编译期实体模型，从 [Entity] 标注的类解析而来</summary>
internal sealed class EntityModel
{
    /// <summary>实体类名</summary>
    public string ClassName { get; set; } = "";

    /// <summary>数据库表名</summary>
    public string TableName { get; set; } = "";

    /// <summary>命名空间</summary>
    public string Namespace { get; set; } = "";

    /// <summary>视图 SQL（非空时视为视图）</summary>
    public string? ViewSql { get; set; }

    /// <summary>是否需要审计表</summary>
    public bool NeedAuditTable { get; set; }

    /// <summary>是否为租户表（含 TenantId 属性）</summary>
    public bool IsTenantTable { get; set; }

    /// <summary>主键列模型</summary>
    public ColumnModel? PrimaryKey { get; set; }

    /// <summary>所有列模型</summary>
    public List<ColumnModel> Columns { get; set; } = new();

    /// <summary>所有索引模型</summary>
    public List<IndexModel> Indexes { get; set; } = new();

    /// <summary>主从关系（当前实体作为从表时的关系信息）</summary>
    public DetailRelation? DetailRelation { get; set; }

    /// <summary>作为主表时的所有从表关系</summary>
    public List<DetailRelation> DetailTables { get; set; } = new();
}
