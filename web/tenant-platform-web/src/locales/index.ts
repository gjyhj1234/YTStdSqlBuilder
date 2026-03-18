import { createI18n } from 'vue-i18n'
import { loadMessages, locale as setDevExtremeLocale } from 'devextreme/localization'
import devExtremeEn from 'devextreme/localization/messages/en.json'
import devExtremeZhCn from 'devextreme/localization/messages/zh.json'
import devExtremeZhTw from 'devextreme/localization/messages/zh-tw.json'
import zhCN from './zh-CN.json'
import enUS from './en-US.json'
import msMY from './ms-MY.json'
import zhTW from './zh-TW.json'

export type LocaleCode = 'zh-CN' | 'en-US' | 'ms-MY' | 'zh-TW'

const LOCALE_STORAGE_KEY = 'platform_locale'
const DEFAULT_LOCALE: LocaleCode = 'zh-CN'

const messages = {
  'zh-CN': zhCN,
  'en-US': enUS,
  'ms-MY': msMY,
  'zh-TW': zhTW,
}

loadMessages(devExtremeEn)
loadMessages(devExtremeZhCn)
loadMessages(devExtremeZhTw)

export const i18n = createI18n({
  legacy: false,
  globalInjection: true,
  locale: resolveInitialLocale(),
  fallbackLocale: 'en-US',
  messages,
})

applyLocale(i18n.global.locale.value as LocaleCode)

export const localeOptions: Array<{ value: LocaleCode; labelKey: string }> = [
  { value: 'zh-CN', labelKey: 'languages.zh-CN' },
  { value: 'en-US', labelKey: 'languages.en-US' },
  { value: 'ms-MY', labelKey: 'languages.ms-MY' },
  { value: 'zh-TW', labelKey: 'languages.zh-TW' },
]

function resolveInitialLocale(): LocaleCode {
  return normalizeLocale(localStorage.getItem(LOCALE_STORAGE_KEY))
}

export function normalizeLocale(value?: string | null): LocaleCode {
  switch ((value ?? '').trim()) {
    case 'en':
    case 'en-US':
      return 'en-US'
    case 'ms':
    case 'ms-MY':
      return 'ms-MY'
    case 'zh-TW':
    case 'zh_HK':
    case 'zh-HK':
      return 'zh-TW'
    case 'zh':
    case 'zh-CN':
      return 'zh-CN'
    default:
      return DEFAULT_LOCALE
  }
}

function mapDevExtremeLocale(locale: LocaleCode): string {
  switch (locale) {
    case 'zh-CN':
      return 'zh'
    case 'zh-TW':
      return 'zh-tw'
    case 'ms-MY':
      return 'en'
    case 'en-US':
    default:
      return 'en'
  }
}

function applyLocale(locale: LocaleCode) {
  i18n.global.locale.value = locale
  localStorage.setItem(LOCALE_STORAGE_KEY, locale)
  document.documentElement.lang = locale
  setDevExtremeLocale(mapDevExtremeLocale(locale))
}

export function setLocale(locale: string | null | undefined) {
  applyLocale(normalizeLocale(locale))
}

export function getCurrentLocale(): LocaleCode {
  return normalizeLocale(i18n.global.locale.value as string)
}

export function translateText(text?: string | null): string {
  if (!text) {
    return ''
  }

  const key = text.startsWith('i18n:') ? text.slice(5) : text
  const translated = i18n.global.t(key)
  return translated === key ? text : translated
}

export function translateList(items?: string[] | null): string[] {
  return (items ?? []).map(item => translateText(item))
}

export { LOCALE_STORAGE_KEY }
