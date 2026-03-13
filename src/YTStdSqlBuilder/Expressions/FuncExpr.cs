using YTStdSqlBuilder.Internal;

namespace YTStdSqlBuilder.Expressions;

/// <summary>SQL 函数调用表达式</summary>
public sealed class FuncExpr : SqlExpr
{
    /// <summary>表达式类型</summary>
    public override SqlExprKind Kind => SqlExprKind.Function;

    /// <summary>函数名称</summary>
    public string FunctionName { get; }
    /// <summary>函数参数列表</summary>
    public SqlExpr[] Arguments { get; }

    /// <summary>创建 SQL 函数调用表达式</summary>
    /// <param name="functionName">函数名称</param>
    /// <param name="arguments">函数参数</param>
    public FuncExpr(string functionName, params SqlExpr[] arguments)
    {
        FunctionName = Guard.NotNullOrEmpty(functionName);
        Arguments = arguments ?? Array.Empty<SqlExpr>();
    }
}
