using YTStdSqlBuilder.Internal;

namespace YTStdSqlBuilder;

/// <summary>查询结果的结构描述</summary>
public sealed class QueryResultShape
{
    /// <summary>查询名称</summary>
    public string QueryName { get; }
    /// <summary>结果列列表</summary>
    public List<QueryResultColumn> Columns { get; }

    /// <summary>创建查询结果结构（空列列表）</summary>
    /// <param name="queryName">查询名称</param>
    public QueryResultShape(string queryName)
    {
        QueryName = Guard.NotNullOrEmpty(queryName);
        Columns = new List<QueryResultColumn>();
    }

    /// <summary>创建查询结果结构（指定列列表）</summary>
    /// <param name="queryName">查询名称</param>
    /// <param name="columns">结果列列表</param>
    public QueryResultShape(string queryName, List<QueryResultColumn> columns)
    {
        QueryName = Guard.NotNullOrEmpty(queryName);
        Columns = Guard.NotNull(columns);
    }
}
