using System;
using System.Runtime.CompilerServices;

namespace YTStdEntity;

/// <summary>提供语义化的 NULL 值快捷方式，用于 Update 操作中显式设置字段为 NULL。</summary>
public static class DBNULL
{
    /// <summary>字符串类型的 NULL 值</summary>
    public static DbNullable<string> StringValue { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => DbNullable<string>.NullValue; }
    /// <summary>整数类型的 NULL 值</summary>
    public static DbNullable<int> IntValue { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => DbNullable<int>.NullValue; }
    /// <summary>长整数类型的 NULL 值</summary>
    public static DbNullable<long> LongValue { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => DbNullable<long>.NullValue; }
    /// <summary>十进制类型的 NULL 值</summary>
    public static DbNullable<decimal> DecimalValue { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => DbNullable<decimal>.NullValue; }
    /// <summary>日期时间类型的 NULL 值</summary>
    public static DbNullable<DateTime> DateTimeValue { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => DbNullable<DateTime>.NullValue; }
    /// <summary>时间跨度类型的 NULL 值</summary>
    public static DbNullable<TimeSpan> TimeSpanValue { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => DbNullable<TimeSpan>.NullValue; }
    /// <summary>字符串数组类型的 NULL 值</summary>
    public static DbNullable<string[]> StringArrayValue { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => DbNullable<string[]>.NullValue; }
    /// <summary>整数数组类型的 NULL 值</summary>
    public static DbNullable<int[]> IntArrayValue { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => DbNullable<int[]>.NullValue; }
    /// <summary>长整数数组类型的 NULL 值</summary>
    public static DbNullable<long[]> LongArrayValue { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => DbNullable<long[]>.NullValue; }
    /// <summary>十进制数组类型的 NULL 值</summary>
    public static DbNullable<decimal[]> DecimalArrayValue { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => DbNullable<decimal[]>.NullValue; }
    /// <summary>日期时间数组类型的 NULL 值</summary>
    public static DbNullable<DateTime[]> DateTimeArrayValue { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => DbNullable<DateTime[]>.NullValue; }
    /// <summary>时间跨度数组类型的 NULL 值</summary>
    public static DbNullable<TimeSpan[]> TimeSpanArrayValue { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => DbNullable<TimeSpan[]>.NullValue; }
}
