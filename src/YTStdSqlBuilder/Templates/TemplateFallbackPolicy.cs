namespace YTStdSqlBuilder;

public enum TemplateFallbackPolicy
{
    /// <summary>SQL is fully static, no fallback needed.</summary>
    Static,

    /// <summary>SQL falls back to runtime interpreter.</summary>
    RuntimeInterpreter,

    /// <summary>Cannot generate anything.</summary>
    None
}
