using System.Globalization;

namespace YTStdSqlBuilder;

internal static class PgSqlLiteralFormatter
{
    public static string Format(object? value)
    {
        return value switch
        {
            null => "NULL",
            bool b => b ? "TRUE" : "FALSE",
            string s => $"'{s.Replace("'", "''")}'",
            char c => $"'{c}'",
            int i => i.ToString(CultureInfo.InvariantCulture),
            long l => l.ToString(CultureInfo.InvariantCulture),
            decimal d => d.ToString(CultureInfo.InvariantCulture),
            double d => d.ToString(CultureInfo.InvariantCulture),
            float f => f.ToString(CultureInfo.InvariantCulture),
            short s => s.ToString(CultureInfo.InvariantCulture),
            byte b => b.ToString(CultureInfo.InvariantCulture),
            DateTime dt => $"'{dt.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture)}'",
            DateTimeOffset dto => $"'{dto.ToString("o", CultureInfo.InvariantCulture)}'",
            Guid g => $"'{g}'::uuid",
            Enum e => Convert.ToInt64(e, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture),
            _ => $"'{value}'"
        };
    }

    public static IEnumerable<string> FormatEnumerable(System.Collections.IEnumerable values)
    {
        foreach (var item in values)
            yield return Format(item);
    }
}
