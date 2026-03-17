<template>
  <div class="app-layout">
    <!-- 侧边栏 -->
    <aside class="app-sidebar" :class="{ collapsed: appStore.sidebarCollapsed }">
      <div class="sidebar-logo">
        <template v-if="!appStore.sidebarCollapsed">租户管理平台</template>
        <template v-else>TP</template>
      </div>
      <nav class="sidebar-menu">
        <template v-for="item in visibleMenuItems" :key="item.key">
          <!-- 无子菜单 -->
          <router-link
            v-if="!item.children && item.path"
            :to="item.path"
            class="menu-item"
            :class="{ active: isMenuActive(item) }"
          >
            <i v-if="item.icon" :class="`dx-icon dx-icon-${item.icon}`" />
            <span v-if="!appStore.sidebarCollapsed" class="menu-item-text">{{ item.label }}</span>
          </router-link>

          <!-- 有子菜单 -->
          <template v-else-if="item.children">
            <div
              class="menu-toggle"
              @click="toggleGroup(item.key)"
            >
              <i v-if="item.icon" :class="`dx-icon dx-icon-${item.icon}`" />
              <span v-if="!appStore.sidebarCollapsed" class="menu-item-text">{{ item.label }}</span>
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
                <span class="menu-item-text">{{ child.label }}</span>
              </router-link>
            </div>
          </template>
        </template>
      </nav>
    </aside>

    <!-- 主内容区 -->
    <div class="app-main">
      <header class="app-header">
        <div class="header-left">
          <button class="toggle-btn" @click="appStore.toggleSidebar">
            <i class="dx-icon dx-icon-menu" />
          </button>
          <span class="breadcrumb">{{ currentTitle }}</span>
        </div>
        <div class="header-right">
          <span class="user-info">{{ authStore.displayName }}</span>
          <button class="logout-btn" @click="handleLogout">退出</button>
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
import { useRoute, useRouter } from 'vue-router'
import { useAppStore } from '@/store/app'
import { useAuthStore } from '@/store/auth'
import { menuItems, type MenuItem } from '@/constants/menus'

const appStore = useAppStore()
const authStore = useAuthStore()
const route = useRoute()
const router = useRouter()

const openGroups = ref<Set<string>>(new Set(['platform-management', 'tenant-lifecycle']))

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
  return (route.meta.title as string) || '仪表盘'
})

function handleLogout() {
  authStore.logout()
  router.push('/login')
}
</script>
