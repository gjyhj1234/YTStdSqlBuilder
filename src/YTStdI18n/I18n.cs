using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using YTStdLogger.Core;

namespace YTStdI18n;

/// <summary>
/// 国际化核心状态管理类。
/// 提供语言切换、租户语言偏好的底层实现。
/// <para>
/// 此类为内部实现类，上层用户应通过 Source Generator 生成的 <c>I18n</c> 静态类访问所有功能。
/// <c>I18n</c> 类包装了 <c>I18nCore</c> 并添加了 <c>Register()</c> 方法（自动注册所有语言包）。
/// </para>
/// </summary>
public static class I18nCore
{
    private static volatile Lang _defaultLang;
    private static readonly ConcurrentDictionary<int, Lang> _tenantLangs = new ConcurrentDictionary<int, Lang>();

    /// <summary>
    /// 获取或设置全局默认语言。线程安全（volatile 读写）。
    /// </summary>
    public static Lang DefaultLang
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _defaultLang;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _defaultLang = value;
    }

    /// <summary>
    /// 初始化国际化系统。应用启动时调用一次。
    /// </summary>
    /// <param name="defaultLang">全局默认语言，默认为简体中文。</param>
    public static void Init(Lang defaultLang = Lang.ZhCn)
    {
        _defaultLang = defaultLang;
        _tenantLangs.Clear();
        Logger.Info(0, 0L, () =>
        {
            var vsb = new ValueStringBuilder(64);
            vsb.Append("[I18n] 初始化完成, 默认语言: ");
            vsb.Append(defaultLang.ToString());
            return vsb.ToString();
        });
    }

    /// <summary>
    /// 设置指定租户的语言偏好。
    /// </summary>
    /// <param name="tenantId">租户 ID。</param>
    /// <param name="lang">语言偏好。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetTenantLang(int tenantId, Lang lang)
    {
        _tenantLangs[tenantId] = lang;
        Logger.Debug(tenantId, 0L, () =>
        {
            var vsb = new ValueStringBuilder(64);
            vsb.Append("[I18n] 设置租户语言: tenantId=");
            vsb.Append(tenantId);
            vsb.Append(", lang=");
            vsb.Append(lang.ToString());
            return vsb.ToString();
        });
    }

    /// <summary>
    /// 获取指定租户的语言偏好，未设置则返回全局默认语言。
    /// </summary>
    /// <param name="tenantId">租户 ID。</param>
    /// <returns>租户语言偏好或全局默认语言。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Lang GetTenantLang(int tenantId)
    {
        if (_tenantLangs.TryGetValue(tenantId, out Lang lang))
        {
            return lang;
        }
        return _defaultLang;
    }
}
