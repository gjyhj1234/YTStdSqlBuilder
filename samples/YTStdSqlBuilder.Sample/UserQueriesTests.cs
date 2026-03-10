using Xunit;

namespace YTStdSqlBuilder.Sample;

/// <summary>
/// Tests verifying that the source generator produces correct, runnable code.
/// These tests exercise the actual generated partial method implementations.
/// </summary>
public class UserQueriesTests
{
    [Fact]
    public void GetUserById_NonGenericParam_GeneratesCorrectSql()
    {
        var result = UserQueries.GetUserById(42);

        Assert.Equal(
            "SELECT \"u\".\"id\", \"u\".\"name\" FROM \"users\" AS \"u\" WHERE \"u\".\"id\" = @p0",
            result.Sql);
        Assert.Single(result.Params);
        Assert.Equal("@p0", result.Params[0].Name);
        Assert.Equal(42, result.Params[0].Value);
    }

    [Fact]
    public void GetUserByIdTyped_GenericParam_GeneratesCorrectSql()
    {
        var result = UserQueries.GetUserByIdTyped(99);

        Assert.Equal(
            "SELECT \"u\".\"id\", \"u\".\"name\" FROM \"users\" AS \"u\" WHERE \"u\".\"id\" = @p0",
            result.Sql);
        Assert.Single(result.Params);
        Assert.Equal("@p0", result.Params[0].Name);
        Assert.Equal(99, result.Params[0].Value);
    }

    [Fact]
    public void GetAllUsers_NoParams_GeneratesCorrectSql()
    {
        var result = UserQueries.GetAllUsers();

        Assert.Equal(
            "SELECT \"u\".\"id\", \"u\".\"name\" FROM \"users\" AS \"u\"",
            result.Sql);
        Assert.Empty(result.Params);
    }

    [Fact]
    public void GetUserDetails_MultipleColumns_GeneratesCorrectSql()
    {
        var result = UserQueries.GetUserDetails(1);

        Assert.Contains("\"u\".\"id\"", result.Sql);
        Assert.Contains("\"u\".\"name\"", result.Sql);
        Assert.Contains("\"u\".\"email\"", result.Sql);
        Assert.Contains("\"u\".\"is_active\"", result.Sql);
        Assert.Contains("WHERE", result.Sql);
        Assert.Single(result.Params);
        Assert.Equal(1, result.Params[0].Value);
    }

    [Fact]
    public void SearchUsers_DynamicQuery_WithAllParams()
    {
        var result = UserQueries.SearchUsers(true, "alice", true, 18);

        Assert.Contains("SELECT", result.Sql);
        Assert.Contains("FROM", result.Sql);
    }

    [Fact]
    public void SearchUsers_DynamicQuery_WithNullParams()
    {
        var result = UserQueries.SearchUsers(false, "", false, 0);

        // With false condition params, dynamic conditions should be skipped
        Assert.Contains("SELECT", result.Sql);
        Assert.Contains("FROM", result.Sql);
    }

    [Fact]
    public void GetUserById_SqlConstant_IsAccessible()
    {
        // The generator emits a public const string for static queries
        Assert.NotNull(UserQueries.GetUserById_Sql);
        Assert.Contains("SELECT", UserQueries.GetUserById_Sql);
    }

    [Fact]
    public void GetUserByIdTyped_SqlConstant_MatchesNonGeneric()
    {
        // Both forms should produce identical SQL
        Assert.Equal(UserQueries.GetUserById_Sql, UserQueries.GetUserByIdTyped_Sql);
    }

    [Fact]
    public void GeneratedSql_MatchesRuntimeBuilder()
    {
        // Verify that the generated static SQL matches what the runtime builder produces
        var user = Table.Def("users").As("u");
        var runtimeResult = PgSql
            .Select(user["id"], user["name"])
            .From(user)
            .Where(user["id"], Op.Eq, Param.Value(42))
            .Build();

        Assert.Equal(UserQueries.GetUserById_Sql, runtimeResult.Sql);
    }
}
