using Microsoft.CodeAnalysis;

namespace YTStdI18n.Generator;

/// <summary>
/// i18n Source Generator 诊断描述器集合。
/// </summary>
internal static class DiagnosticDescriptors
{
    /// <summary>I18N001: 数组长度不匹配。</summary>
    public static readonly DiagnosticDescriptor ArrayLengthMismatch = new DiagnosticDescriptor(
        id: "I18N001",
        title: "Language pack array length mismatch",
        messageFormat: "Language pack '{0}' category '{1}' array length {2} does not match base language length {3}",
        category: "I18nGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>I18N002: 分类字段缺失。</summary>
    public static readonly DiagnosticDescriptor MissingCategory = new DiagnosticDescriptor(
        id: "I18N002",
        title: "Language pack missing category",
        messageFormat: "Language pack '{0}' is missing category '{1}' which exists in base language '{2}'",
        category: "I18nGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>I18N004: 基准语言数组元素为 null。</summary>
    public static readonly DiagnosticDescriptor BaseLanguageNullElement = new DiagnosticDescriptor(
        id: "I18N004",
        title: "Base language null element",
        messageFormat: "Base language '{0}' category '{1}' index {2} must not be null",
        category: "I18nGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>I18N006: 基准语言数组元素缺少注释常量名。</summary>
    public static readonly DiagnosticDescriptor MissingCommentName = new DiagnosticDescriptor(
        id: "I18N006",
        title: "Missing constant name comment",
        messageFormat: "Missing constant name comment for index {0} in category '{1}'",
        category: "I18nGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>多个基准语言声明。</summary>
    public static readonly DiagnosticDescriptor MultipleBaseLangs = new DiagnosticDescriptor(
        id: "I18N007",
        title: "Multiple base languages",
        messageFormat: "Multiple classes marked with [I18nResource(IsBase = true)]",
        category: "I18nGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
