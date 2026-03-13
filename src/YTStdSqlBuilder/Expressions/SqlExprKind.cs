namespace YTStdSqlBuilder.Expressions;

/// <summary>SQL 表达式类型枚举</summary>
public enum SqlExprKind
{
    /// <summary>列引用</summary>
    Column,
    /// <summary>参数</summary>
    Param,
    /// <summary>字面量</summary>
    Literal,
    /// <summary>函数调用</summary>
    Function,
    /// <summary>子查询</summary>
    SubQuery,
    /// <summary>原始 SQL</summary>
    Raw,
    /// <summary>通配符</summary>
    All,
    /// <summary>CASE 表达式</summary>
    Case,
    /// <summary>二元运算</summary>
    Binary
}
