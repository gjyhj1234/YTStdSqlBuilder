using System;

namespace YTStdEntity;

/// <summary>提供语义化的 NULL 值快捷方式</summary>
public static class DBNULL
{
    public static DbNullable<string> StringValue => DbNullable<string>.NullValue;
    public static DbNullable<int> IntValue => DbNullable<int>.NullValue;
    public static DbNullable<long> LongValue => DbNullable<long>.NullValue;
    public static DbNullable<decimal> DecimalValue => DbNullable<decimal>.NullValue;
    public static DbNullable<DateTime> DateTimeValue => DbNullable<DateTime>.NullValue;
    public static DbNullable<TimeSpan> TimeSpanValue => DbNullable<TimeSpan>.NullValue;
    public static DbNullable<string[]> StringArrayValue => DbNullable<string[]>.NullValue;
    public static DbNullable<int[]> IntArrayValue => DbNullable<int[]>.NullValue;
    public static DbNullable<long[]> LongArrayValue => DbNullable<long[]>.NullValue;
    public static DbNullable<decimal[]> DecimalArrayValue => DbNullable<decimal[]>.NullValue;
    public static DbNullable<DateTime[]> DateTimeArrayValue => DbNullable<DateTime[]>.NullValue;
    public static DbNullable<TimeSpan[]> TimeSpanArrayValue => DbNullable<TimeSpan[]>.NullValue;
}
