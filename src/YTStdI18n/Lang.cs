namespace YTStdI18n;

/// <summary>
/// 支持的语言枚举。
/// 可按需扩展，新增语言只需添加枚举值 + 对应翻译数组。
/// </summary>
public enum Lang : byte
{
    /// <summary>简体中文（默认，基准语言）</summary>
    ZhCn = 0,

    /// <summary>英语</summary>
    En = 1,

    /// <summary>日语</summary>
    Ja = 2,

    /// <summary>繁体中文</summary>
    ZhTw = 3,

    /// <summary>马来语</summary>
    MsMy = 4,
}
