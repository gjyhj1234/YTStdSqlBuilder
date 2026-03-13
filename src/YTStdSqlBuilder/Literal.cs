using YTStdSqlBuilder.Expressions;

namespace YTStdSqlBuilder;

/// <summary>提供 SQL 字面量表达式的工厂方法</summary>
public static class Literal
{
    /// <summary>SQL TRUE 字面量</summary>
    public static readonly LiteralExpr True = LiteralExpr.True;

    /// <summary>SQL FALSE 字面量</summary>
    public static readonly LiteralExpr False = LiteralExpr.False;

    /// <summary>SQL NULL 字面量</summary>
    public static readonly LiteralExpr Null = LiteralExpr.Null;

    /// <summary>数字 1 字面量</summary>
    public static readonly LiteralExpr One = LiteralExpr.Number(1);

    /// <summary>数字 0 字面量</summary>
    public static readonly LiteralExpr Zero = LiteralExpr.Number(0);

    /// <summary>创建字符串字面量</summary>
    /// <param name="value">字符串值</param>
    /// <returns>字符串字面量表达式</returns>
    public static LiteralExpr Of(string value) => LiteralExpr.String(value);

    /// <summary>创建整数字面量</summary>
    /// <param name="value">整数值</param>
    /// <returns>数字字面量表达式</returns>
    public static LiteralExpr Of(int value) => LiteralExpr.Number(value);

    /// <summary>创建长整数字面量</summary>
    /// <param name="value">长整数值</param>
    /// <returns>数字字面量表达式</returns>
    public static LiteralExpr Of(long value) => LiteralExpr.Number(value);

    /// <summary>创建十进制字面量</summary>
    /// <param name="value">十进制值</param>
    /// <returns>数字字面量表达式</returns>
    public static LiteralExpr Of(decimal value) => LiteralExpr.Number(value);

    /// <summary>创建双精度浮点字面量</summary>
    /// <param name="value">双精度浮点值</param>
    /// <returns>数字字面量表达式</returns>
    public static LiteralExpr Of(double value) => LiteralExpr.Number(value);

    /// <summary>创建原始 SQL 文本字面量</summary>
    /// <param name="sqlText">原始 SQL 文本</param>
    /// <returns>原始字面量表达式</returns>
    public static LiteralExpr Raw(string sqlText) => LiteralExpr.Raw(sqlText);
}
