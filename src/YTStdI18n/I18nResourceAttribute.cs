using System;

namespace YTStdI18n;

/// <summary>
/// 标记一个静态类为国际化语言资源类。
/// Source Generator 将扫描此特性以生成常量键、扩展方法和注册代码。
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class I18nResourceAttribute : Attribute
{
    /// <summary>
    /// 是否为基准语言（默认 false）。
    /// 基准语言用于定义所有分类和键的顺序，其他语言包必须严格遵循。
    /// </summary>
    public bool IsBase { get; set; }
}
