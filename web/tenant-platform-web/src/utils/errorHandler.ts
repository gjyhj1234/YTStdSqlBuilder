import { translateText } from '@/locales'
import { AUTH_ACCOUNT_DISABLED, AUTH_ACCOUNT_LOCKED, AUTH_TOKEN_INVALID, FORBIDDEN } from '@/constants/errorCodes'

/** API 错误封装 */
export class ApiError extends Error {
  code: number
  messageKey: string

  constructor(code: number, messageKey: string) {
    super(messageKey)
    this.code = code
    this.messageKey = messageKey
  }

  /** 获取翻译后的消息 */
  get translatedMessage(): string {
    return translateText(this.messageKey)
  }
}

/** 根据错误码执行特殊处理 */
export function handleApiError(error: ApiError): void {
  switch (error.code) {
    case AUTH_ACCOUNT_DISABLED:
    case AUTH_ACCOUNT_LOCKED:
    case AUTH_TOKEN_INVALID:
      localStorage.removeItem('platform_token')
      localStorage.removeItem('platform_user')
      window.location.href = '/login'
      break
    case FORBIDDEN:
      // 权限不足 - 可以在具体视图中处理
      break
    default:
      break
  }
}
