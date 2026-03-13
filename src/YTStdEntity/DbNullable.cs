using System.Runtime.CompilerServices;

namespace YTStdEntity;

/// <summary>
/// 三态可空结构体：区分"未设置"、"设置为具体值"和"设置为 NULL"。
/// 用于 Update 操作中精确控制字段更新行为。
/// </summary>
public readonly struct DbNullable<T>
{
    /// <summary>标记是否显式设置了值（包括设置为 null）</summary>
    public bool IsSet { get; }

    /// <summary>实际的值。如果 IsSet 为 false，此值无意义</summary>
    public T? Value { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private DbNullable(T? value, bool isSet)
    {
        Value = value;
        IsSet = isSet;
    }

    /// <summary>公开构造函数：传入值即表示"设置"</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public DbNullable(T? value)
    {
        Value = value;
        IsSet = true;
    }

    /// <summary>明确表示"设置为 NULL"</summary>
    public static DbNullable<T> NullValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(default, true);
    }

    /// <summary>明确表示"未设置"（默认状态）</summary>
    public static DbNullable<T> Unset
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(default, false);
    }

    /// <summary>
    /// 隐式转换：允许直接赋值 T 或 T? 给 DbNullable&lt;T&gt;
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator DbNullable<T>(T? value) => new(value, true);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
    {
        if (!IsSet) return "[Unset]";
        if (Value == null) return "[Set: null]";
        var vsb = new ValueStringBuilder(stackalloc char[64]);
        vsb.Append("[Set: ");
        vsb.Append(Value.ToString());
        vsb.Append(']');
        return vsb.ToString();
    }
}
