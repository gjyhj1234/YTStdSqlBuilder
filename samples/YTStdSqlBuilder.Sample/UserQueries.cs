using YTStdSqlBuilder;
using YTStdSqlBuilder.Conditions;
using YTStdSqlBuilder.Expressions;

namespace YTStdSqlBuilder.Sample;

/// <summary>
/// Sample template class demonstrating static query generation.
/// </summary>
[PgSqlTemplate]
public static partial class UserQueries
{
    /// <summary>
    /// Get user by ID — uses non-generic b.Param() (type inferred from method signature).
    /// </summary>
    [PgSqlQuery]
    public static partial PgSqlRenderResult GetUserById(int userId);

    private static void Define_GetUserById(PgSqlTemplateBuilder b)
    {
        var user = b.Table("users", "u");
        b.Select(
            user.Col("id"),
            user.Col("name"))
         .From(user)
         .Where(user.Col("id"), Op.Eq, b.Param("userId"));
    }

    /// <summary>
    /// Get user by ID — uses generic b.Param&lt;T&gt;() with explicit column types.
    /// </summary>
    [PgSqlQuery]
    public static partial PgSqlRenderResult GetUserByIdTyped(int userId);

    private static void Define_GetUserByIdTyped(PgSqlTemplateBuilder b)
    {
        var user = b.Table("users", "u");
        b.Select(
            user.Col<int>("id"),
            user.Col<string>("name"))
         .From(user)
         .Where(user.Col("id"), Op.Eq, b.Param<int>("userId"));
    }

    /// <summary>
    /// Get all users — no parameters.
    /// </summary>
    [PgSqlQuery]
    public static partial PgSqlRenderResult GetAllUsers();

    private static void Define_GetAllUsers(PgSqlTemplateBuilder b)
    {
        var user = b.Table("users", "u");
        b.Select(
            user.Col<int>("id"),
            user.Col<string>("name"))
         .From(user);
    }

    /// <summary>
    /// Get user details — multiple columns with types.
    /// </summary>
    [PgSqlQuery]
    public static partial PgSqlRenderResult GetUserDetails(int userId);

    private static void Define_GetUserDetails(PgSqlTemplateBuilder b)
    {
        var user = b.Table("users", "u");
        b.Select(
            user.Col<int>("id"),
            user.Col<string>("name"),
            user.Col<string>("email"),
            user.Col<bool>("is_active"))
         .From(user)
         .Where(user.Col("id"), Op.Eq, b.Param<int>("userId"));
    }

    /// <summary>
    /// Search users — dynamic query with WhereIf/AndIf.
    /// Bool parameters with '_condition' suffix control whether each condition is included.
    /// </summary>
    [PgSqlQuery]
    public static partial PgSqlRenderResult SearchUsers(bool name_condition, string name, bool minAge_condition, int minAge);

    private static void Define_SearchUsers(PgSqlTemplateBuilder b)
    {
        var user = b.Table("users", "u");
        b.Select(
            user.Col<int>("id"),
            user.Col<string>("name"))
         .From(user)
         .WhereIf(user.Col("name"), Op.ILike, b.Param<string>("name"))
         .AndIf(user.Col("age"), Op.Gte, b.Param<int>("minAge"));
    }
}
