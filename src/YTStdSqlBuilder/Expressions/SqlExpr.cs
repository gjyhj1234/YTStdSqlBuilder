namespace YTStdSqlBuilder.Expressions;

/// <summary>SQL 表达式基类</summary>
public abstract class SqlExpr
{
    /// <summary>表达式类型</summary>
    public abstract SqlExprKind Kind { get; }

    /// <summary>为表达式设置别名和 CLR 类型映射</summary>
    /// <typeparam name="T">CLR 类型</typeparam>
    /// <param name="alias">列别名（可选）</param>
    /// <returns>带别名的选择项</returns>
    public SqlSelectItem As<T>(string? alias = null) =>
        new(this, alias, typeof(T));

    /// <summary>为表达式设置别名</summary>
    /// <param name="alias">列别名</param>
    /// <returns>带别名的选择项</returns>
    public SqlSelectItem As(string alias) =>
        new(this, alias);

    /// <summary>创建升序排序项</summary>
    /// <returns>升序排序项</returns>
    public SqlOrderItem Asc() => new(this, descending: false);

    /// <summary>创建降序排序项</summary>
    /// <returns>降序排序项</returns>
    public SqlOrderItem Desc() => new(this, descending: true);
}
