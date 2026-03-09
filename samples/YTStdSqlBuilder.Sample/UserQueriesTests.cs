using Xunit;

namespace YTStdSqlBuilder.Sample;

public class UserQueriesTests
{
    [Fact]
    public void GetUserById_GeneratesCorrectSql()
    {
        var result = UserQueries.GetUserById(42);

        Assert.NotNull(result);
        Assert.NotNull(result.Sql);
        Assert.Contains("SELECT", result.Sql);
        Assert.Contains("FROM", result.Sql);
        Assert.Contains("WHERE", result.Sql);
    }
}
