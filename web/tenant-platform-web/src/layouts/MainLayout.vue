<template>
  <div class="app-layout">
    <aside class="app-sidebar" :class="{ collapsed: appStore.sidebarCollapsed }">
      <div class="sidebar-logo">
        <template v-if="!appStore.sidebarCollapsed">{{ t('app.title') }}</template>
        <template v-else>{{ t('app.shortTitle') }}</template>
      </div>
      <nav class="sidebar-menu">
        <template v-for="item in visibleMenuItems" :key="item.key">
          <router-link
            v-if="!item.children && item.path"
            :to="item.path"
            class="menu-item"
            :class="{ active: isMenuActive(item) }"
          >
            <i v-if="item.icon" :class="`dx-icon dx-icon-${item.icon}`" />
            <span v-if="!appStore.sidebarCollapsed" class="menu-item-text">{{ t(item.label) }}</span>
          </router-link>

          <template v-else-if="item.children">
            <div
              class="menu-toggle"
              @click="toggleGroup(item.key)"
            >
              <i v-if="item.icon" :class="`dx-icon dx-icon-${item.icon}`" />
              <span v-if="!appStore.sidebarCollapsed" class="menu-item-text">{{ t(item.label) }}</span>
              <span
                v-if="!appStore.sidebarCollapsed"
                class="menu-toggle-arrow"
                :class="{ open: openGroups.has(item.key) }"
              >▸</span>
            </div>
            <div v-if="openGroups.has(item.key) && !appStore.sidebarCollapsed" class="menu-children">
              <router-link
                v-for="child in getVisibleChildren(item)"
                :key="child.key"
                :to="child.path!"
                class="menu-item"
                :class="{ active: isMenuActive(child) }"
              >
                <span class="menu-item-text">{{ t(child.label) }}</span>
              </router-link>
            </div>
          </template>
        </template>
      </nav>
    </aside>

    <div class="app-main">
      <header class="app-header">
        <div class="header-left">
          <button class="toggle-btn" @click="appStore.toggleSidebar">
            <i class="dx-icon dx-icon-menu" />
          </button>
          <span class="breadcrumb">{{ currentTitle }}</span>
        </div>
        <div class="header-right">
          <DxSelectBox
            :items="languageOptions"
            display-expr="label"
            value-expr="value"
            :value="appStore.locale"
            :width="140"
            :input-attr="{ 'aria-label': t('app.language') }"
            @value-changed="onLocaleChange"
          />
          <span class="user-info">{{ authStore.displayName }}</span>
          <button class="logout-btn" @click="handleLogout">{{ t('app.logout') }}</button>
        </div>
      </header>
      <main class="app-content">
        <router-view />
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { DxSelectBox } from 'devextreme-vue/select-box'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useAppStore } from '@/store/app'
import { useAuthStore } from '@/store/auth'
import { menuItems, type MenuItem } from '@/constants/menus'
import { localeOptions, type LocaleCode } from '@/locales'

const appStore = useAppStore()
const authStore = useAuthStore()
const route = useRoute()
const router = useRouter()
const { t, locale } = useI18n()

const openGroups = ref<Set<string>>(new Set(['platform-management', 'tenant-lifecycle']))

const languageOptions = computed(() => {
  locale.value
  return localeOptions.map(item => ({
    value: item.value,
    label: t(item.labelKey),
  }))
})

function toggleGroup(key: string) {
  if (openGroups.value.has(key)) {
    openGroups.value.delete(key)
  } else {
    openGroups.value.add(key)
  }
}

/** 根据权限过滤可见菜单 */
const visibleMenuItems = computed(() => {
  return menuItems.filter(item => {
    if (item.children) {
      return getVisibleChildren(item).length > 0
    }
    return isItemVisible(item)
  })
})

function getVisibleChildren(item: MenuItem): MenuItem[] {
  return (item.children ?? []).filter(child => isItemVisible(child))
}

function isItemVisible(item: MenuItem): boolean {
  if (!item.permissions || item.permissions.length === 0) return true
  return authStore.hasAnyPermission(item.permissions)
}

function isMenuActive(item: MenuItem): boolean {
  return item.path ? route.path === item.path : false
}

const currentTitle = computed(() => {
  return t((route.meta.title as string) || 'route.dashboard')
})

function onLocaleChange(event: { value?: unknown }) {
  if (event.value) {
    appStore.updateLocale(event.value as LocaleCode)
  }
}

function handleLogout() {
  authStore.logout()
  router.push('/login')
}
</script>
