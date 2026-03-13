namespace YTStdSqlBuilder.Expressions;

/// <summary>SQL 字面量表达式</summary>
public sealed class LiteralExpr : SqlExpr
{
    /// <summary>表达式类型</summary>
    public override SqlExprKind Kind => SqlExprKind.Literal;

    /// <summary>SQL 文本表示</summary>
    public string SqlText { get; }
    /// <summary>字面量的原始值</summary>
    public object? Value { get; }

    private LiteralExpr(string sqlText, object? value = null)
    {
        SqlText = sqlText;
        Value = value;
    }

    /// <summary>SQL TRUE 字面量</summary>
    public static readonly LiteralExpr True = new("TRUE", true);
    /// <summary>SQL FALSE 字面量</summary>
    public static readonly LiteralExpr False = new("FALSE", false);
    /// <summary>SQL NULL 字面量</summary>
    public static readonly LiteralExpr Null = new("NULL");

    /// <summary>创建整数字面量</summary>
    /// <param name="value">整数值</param>
    /// <returns>字面量表达式</returns>
    public static LiteralExpr Number(int value) => new(value.ToString(), value);
    /// <summary>创建长整数字面量</summary>
    /// <param name="value">长整数值</param>
    /// <returns>字面量表达式</returns>
    public static LiteralExpr Number(long value) => new(value.ToString(), value);
    /// <summary>创建十进制字面量</summary>
    /// <param name="value">十进制值</param>
    /// <returns>字面量表达式</returns>
    public static LiteralExpr Number(decimal value) => new(value.ToString(), value);
    /// <summary>创建双精度浮点字面量</summary>
    /// <param name="value">双精度浮点值</param>
    /// <returns>字面量表达式</returns>
    public static LiteralExpr Number(double value) => new(value.ToString(), value);
    /// <summary>创建字符串字面量（自动转义单引号）</summary>
    /// <param name="value">字符串值</param>
    /// <returns>字面量表达式</returns>
    public static LiteralExpr String(string value)
    {
        var vsb = new ValueStringBuilder(stackalloc char[64]);
        vsb.Append('\'');
        vsb.Append(value.Replace("'", "''"));
        vsb.Append('\'');
        return new(vsb.ToString(), value);
    }
    /// <summary>创建原始 SQL 文本字面量</summary>
    /// <param name="sqlText">原始 SQL 文本</param>
    /// <returns>字面量表达式</returns>
    public static LiteralExpr Raw(string sqlText) => new(sqlText);
}
