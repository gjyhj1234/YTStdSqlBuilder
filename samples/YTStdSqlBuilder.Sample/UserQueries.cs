using YTStdSqlBuilder;
using YTStdSqlBuilder.Conditions;
using YTStdSqlBuilder.Expressions;

namespace YTStdSqlBuilder.Sample;

[PgSqlTemplate]
public static partial class UserQueries
{
    [PgSqlQuery]
    public static partial PgSqlRenderResult GetUserById(int userId);

    private static void Define_GetUserById(PgSqlTemplateBuilder b)
    {
        var user = b.Table("users", "u");
        b.Select(
            user.Col<int>("id"),
            user.Col<string>("name"))
         .From(user)
         .Where(user.Col("id"), Op.Eq, b.Param<int>("userId"));
    }
}
