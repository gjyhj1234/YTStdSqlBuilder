<template>
  <div>
    <div class="page-header">
      <h2>{{ $t('route.dashboard') }}</h2>
      <div class="page-header-actions">
        <PageHelpEntry @click="showGuide = true" />
      </div>
    </div>

    <FunctionDescriptionCard
      purpose="展示平台核心运营指标概览，包括租户统计、订阅状态、系统健康度等关键数据。"
      data-scope="全平台汇总数据，实时更新。"
      permission-note="仪表盘对所有已登录用户可见。"
      :collapsible="true"
    />

    <div class="dashboard-stats">
      <div class="stat-card">
        <div class="stat-value">{{ stats.totalTenants }}</div>
        <div class="stat-label">{{ $t('总租户数') }}</div>
      </div>
      <div class="stat-card">
        <div class="stat-value">{{ stats.activeTenants }}</div>
        <div class="stat-label">{{ $t('活跃租户') }}</div>
      </div>
      <div class="stat-card">
        <div class="stat-value">{{ stats.totalSubscriptions }}</div>
        <div class="stat-label">{{ $t('有效订阅') }}</div>
      </div>
      <div class="stat-card">
        <div class="stat-value">{{ stats.totalUsers }}</div>
        <div class="stat-label">{{ $t('平台用户') }}</div>
      </div>
      <div class="stat-card">
        <div class="stat-value">{{ stats.expiringTenants }}</div>
        <div class="stat-label">{{ $t('即将到期') }}</div>
      </div>
      <div class="stat-card">
        <div class="stat-value">{{ stats.trialTenants }}</div>
        <div class="stat-label">{{ $t('试用中') }}</div>
      </div>
    </div>

    <div class="card">
      <h3 style="margin-bottom: 12px">{{ $t('快捷入口') }}</h3>
      <div style="display: flex; gap: 12px; flex-wrap: wrap">
        <DxButton :text="$t('menu.tenants')" icon="globe" @click="router.push('/tenants')" />
        <DxButton :text="$t('menu.platformUsers')" icon="group" @click="router.push('/platform-users')" />
        <DxButton :text="$t('menu.packages')" icon="box" @click="router.push('/saas-packages')" />
        <DxButton :text="$t('menu.subscriptionList')" icon="clock" @click="router.push('/subscriptions')" />
        <DxButton :text="$t('menu.operationLogs')" icon="textdocument" @click="router.push('/operation-logs')" />
      </div>
    </div>

    <OperationGuideDrawer
      v-model:visible="showGuide"
      title="仪表盘操作指引"
      entry-path="登录后默认进入仪表盘页面"
      :steps="[
        '查看顶部统计卡片了解平台概况',
        '点击快捷入口快速进入常用功能模块',
        '左侧菜单可访问所有管理模块',
      ]"
      :field-notes="['统计数据实时更新，来源于后端接口']"
      :error-notes="['若统计数据加载失败，请检查网络连接']"
    />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { DxButton } from 'devextreme-vue/button'
import FunctionDescriptionCard from '@/components/help/FunctionDescriptionCard.vue'
import OperationGuideDrawer from '@/components/help/OperationGuideDrawer.vue'
import PageHelpEntry from '@/components/help/PageHelpEntry.vue'

const router = useRouter()
const showGuide = ref(false)

const stats = ref({
  totalTenants: 0,
  activeTenants: 0,
  totalSubscriptions: 0,
  totalUsers: 0,
  expiringTenants: 0,
  trialTenants: 0,
})
</script>
