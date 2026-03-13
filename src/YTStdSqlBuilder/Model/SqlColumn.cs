using YTStdSqlBuilder.Expressions;
using YTStdSqlBuilder.Internal;

namespace YTStdSqlBuilder;

/// <summary>SQL 列定义</summary>
public sealed class SqlColumn
{
    /// <summary>所属表源</summary>
    public SqlTableSource TableSource { get; }
    /// <summary>列名</summary>
    public string Name { get; }
    /// <summary>CLR 类型（可选）</summary>
    public Type? ClrType { get; }

    /// <summary>创建 SQL 列定义</summary>
    /// <param name="tableSource">所属表源</param>
    /// <param name="name">列名</param>
    /// <param name="clrType">CLR 类型（可选）</param>
    public SqlColumn(SqlTableSource tableSource, string name, Type? clrType = null)
    {
        TableSource = Guard.NotNull(tableSource);
        Name = Guard.NotNullOrEmpty(name);
        ClrType = clrType;
    }

    /// <summary>创建带 CLR 类型映射的选择项</summary>
    /// <typeparam name="T">CLR 类型</typeparam>
    /// <param name="alias">列别名（可选，默认使用列名）</param>
    /// <returns>带别名和类型映射的选择项</returns>
    public SqlSelectItem As<T>(string? alias = null) =>
        new(new ColumnExpr(this), alias ?? Name, typeof(T));

    /// <summary>创建带别名的选择项</summary>
    /// <param name="alias">列别名</param>
    /// <returns>带别名的选择项</returns>
    public SqlSelectItem As(string alias) =>
        new(new ColumnExpr(this), alias, ClrType);
}
