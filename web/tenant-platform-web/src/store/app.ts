/** 应用全局状态 */
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { getCurrentLocale, setLocale, type LocaleCode } from '@/locales'

export const useAppStore = defineStore('app', () => {
  const sidebarCollapsed = ref(false)
  const locale = ref<LocaleCode>(getCurrentLocale())

  function toggleSidebar() {
    sidebarCollapsed.value = !sidebarCollapsed.value
  }

  function updateLocale(nextLocale: LocaleCode) {
    setLocale(nextLocale)
    locale.value = getCurrentLocale()
  }

  return { sidebarCollapsed, locale, toggleSidebar, updateLocale }
})
