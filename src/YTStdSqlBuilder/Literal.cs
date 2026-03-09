using YTStdSqlBuilder.Expressions;

namespace YTStdSqlBuilder;

public static class Literal
{
    public static readonly LiteralExpr True = LiteralExpr.True;
    public static readonly LiteralExpr False = LiteralExpr.False;
    public static readonly LiteralExpr Null = LiteralExpr.Null;
    public static readonly LiteralExpr One = LiteralExpr.Number(1);
    public static readonly LiteralExpr Zero = LiteralExpr.Number(0);

    public static LiteralExpr Of(string value) => LiteralExpr.String(value);
    public static LiteralExpr Of(int value) => LiteralExpr.Number(value);
    public static LiteralExpr Of(long value) => LiteralExpr.Number(value);
    public static LiteralExpr Of(decimal value) => LiteralExpr.Number(value);
    public static LiteralExpr Of(double value) => LiteralExpr.Number(value);
    public static LiteralExpr Raw(string sqlText) => LiteralExpr.Raw(sqlText);
}
