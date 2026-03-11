namespace YTStdLogger.Logging;

/// <summary>
/// 日志等级，数值越大表示越详细。
/// 过滤规则：仅当 <c>level &lt;= MinimumLevel</c> 时写入。
/// </summary>
public enum LogLevel : byte
{
    /// <summary>
    /// 致命错误。
    /// </summary>
    Fatal = 0,

    /// <summary>
    /// 错误。
    /// </summary>
    Error = 1,

    /// <summary>
    /// 警告。
    /// </summary>
    Warn = 2,

    /// <summary>
    /// 信息。
    /// </summary>
    Infor = 3,

    /// <summary>
    /// 调试。
    /// </summary>
    Debug = 4
}
