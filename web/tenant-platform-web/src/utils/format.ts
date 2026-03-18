import { getCurrentLocale } from '@/locales'

function resolveDate(value: string | Date | null | undefined): Date | null {
  if (!value) return null
  const parsed = typeof value === 'string' ? new Date(value) : value
  return Number.isNaN(parsed.getTime()) ? null : parsed
}

export function formatDate(value: string | Date | null | undefined): string {
  const date = resolveDate(value)
  if (!date) return '-'
  return new Intl.DateTimeFormat(getCurrentLocale(), {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
  }).format(date)
}

/** 日期时间格式化工具 */
export function formatDateTime(value: string | Date | null | undefined): string {
  const date = resolveDate(value)
  if (!date) return '-'
  return new Intl.DateTimeFormat(getCurrentLocale(), {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
  }).format(date)
}

export function formatNumber(value: number | null | undefined): string {
  if (value == null) return '-'
  return new Intl.NumberFormat(getCurrentLocale()).format(value)
}

/** 金额格式化（locale-aware） */
export function formatCurrency(value: number | null | undefined, currency = 'CNY'): string {
  if (value == null) return '-'
  try {
    return new Intl.NumberFormat(getCurrentLocale(), { style: 'currency', currency }).format(value)
  } catch {
    return value.toFixed(2)
  }
}

export function formatAmount(value: number | null | undefined, currency = 'CNY'): string {
  return formatCurrency(value, currency)
}

export function localeCompare(a: string, b: string): number {
  return new Intl.Collator(getCurrentLocale()).compare(a, b)
}
