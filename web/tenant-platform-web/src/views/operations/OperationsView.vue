<template>
  <div>
    <div class="page-header">
      <h2>平台运营中心</h2>
      <div class="page-header-actions">
        <PageHelpEntry @click="showGuide = true" />
      </div>
    </div>

    <FunctionDescriptionCard
      purpose="平台运营中心用于查看租户日统计数据和平台监控指标，帮助运营人员了解平台健康状况和资源使用趋势。"
      data-scope="全平台租户日统计和系统监控指标。"
      permission-note="需要 ops:stat:view 权限查看日统计，ops:monitor:view 权限查看监控指标。"
      risk-note="数据为只读统计信息，不支持修改。"
      :collapsible="true"
    />

    <!-- 租户日统计 -->
    <div class="card" style="margin-bottom: 16px;">
      <h3 style="margin: 0 0 12px;">租户日统计</h3>
      <div class="filter-bar">
        <DxNumberBox
          v-model:value="filterTenantRefId"
          placeholder="按租户ID筛选"
          :width="220"
          :show-clear-button="true"
        />
        <DxButton text="查询" icon="search" @click="loadDailyStats" />
      </div>

      <DxDataGrid
        :data-source="dailyStatsData"
        :show-borders="true"
        :column-auto-width="true"
        :hover-state-enabled="true"
        key-expr="id"
      >
        <DxColumn data-field="id" caption="ID" :width="60" />
        <DxColumn data-field="tenantRefId" caption="租户ID" :width="80" />
        <DxColumn data-field="statDate" caption="统计日期" cell-template="dateCellDate" />
        <DxColumn data-field="activeUserCount" caption="活跃用户数" :width="110" />
        <DxColumn data-field="newUserCount" caption="新增用户数" :width="110" />
        <DxColumn data-field="apiCallCount" caption="API 调用数" :width="110" />
        <DxColumn data-field="storageBytes" caption="存储字节数" :width="110" />
        <DxColumn data-field="resourceScore" caption="资源评分" :width="100" />
        <DxColumn data-field="createdAt" caption="创建时间" cell-template="dateCellDateTime" />
        <template #dateCellDate="{ data: cellData }">
          <span>{{ formatDate(cellData.value) }}</span>
        </template>
        <template #dateCellDateTime="{ data: cellData }">
          <span>{{ formatDateTime(cellData.value) }}</span>
        </template>
        <DxPaging :page-size="20" />
        <DxPager :show-page-size-selector="true" :allowed-page-sizes="[10, 20, 50]" :show-info="true" />
      </DxDataGrid>
    </div>

    <!-- 平台监控指标 -->
    <div class="card">
      <h3 style="margin: 0 0 12px;">平台监控指标</h3>
      <DxDataGrid
        :data-source="metricsData"
        :show-borders="true"
        :column-auto-width="true"
        :hover-state-enabled="true"
        key-expr="id"
      >
        <DxColumn data-field="id" caption="ID" :width="60" />
        <DxColumn data-field="componentName" caption="组件名称" />
        <DxColumn data-field="metricType" caption="指标类型" :width="110" />
        <DxColumn data-field="metricKey" caption="指标键" />
        <DxColumn data-field="metricValue" caption="指标值" :width="100" />
        <DxColumn data-field="metricUnit" caption="单位" :width="80" />
        <DxColumn data-field="collectedAt" caption="采集时间" cell-template="dateCell" />
        <template #dateCell="{ data: cellData }">
          <span>{{ formatDateTime(cellData.value) }}</span>
        </template>
        <DxPaging :page-size="20" />
        <DxPager :show-page-size-selector="true" :allowed-page-sizes="[10, 20, 50]" :show-info="true" />
      </DxDataGrid>
    </div>

    <OperationGuideDrawer
      v-model:visible="showGuide"
      title="平台运营中心操作指引"
      entry-path="左侧菜单 → 运营管理 → 运营中心"
      :steps="guideSteps"
      :field-notes="guideFieldNotes"
      :error-notes="guideErrorNotes"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { DxDataGrid, DxColumn, DxPaging, DxPager } from 'devextreme-vue/data-grid'
import { DxButton } from 'devextreme-vue/button'
import { DxNumberBox } from 'devextreme-vue/number-box'
import FunctionDescriptionCard from '@/components/help/FunctionDescriptionCard.vue'
import OperationGuideDrawer from '@/components/help/OperationGuideDrawer.vue'
import PageHelpEntry from '@/components/help/PageHelpEntry.vue'
import { formatDateTime, formatDate } from '@/utils/format'
import {
  getDailyStats,
  getMonitorMetrics,
  type TenantDailyStatDto,
  type PlatformMonitorMetricDto,
} from '@/api/operations'

const showGuide = ref(false)
const filterTenantRefId = ref<number | undefined>(undefined)

const dailyStatsData = ref<TenantDailyStatDto[]>([])
const metricsData = ref<PlatformMonitorMetricDto[]>([])

async function loadDailyStats() {
  try {
    const res = await getDailyStats({
      page: 1,
      pageSize: 20,
      tenantRefId: filterTenantRefId.value,
    })
    dailyStatsData.value = res.data.items
  } catch {
    // 接口未就绪时保持空列表
  }
}

async function loadMetrics() {
  try {
    const res = await getMonitorMetrics({ page: 1, pageSize: 20 })
    metricsData.value = res.data.items
  } catch {
    // 接口未就绪时保持空列表
  }
}

const guideSteps = [
  '在【租户日统计】区域输入租户ID后点击【查询】查看该租户的日统计数据',
  '不填租户ID直接查询可查看所有租户的统计数据',
  '在【平台监控指标】区域查看各组件的实时监控指标',
]
const guideFieldNotes = [
  '活跃用户数：当日有操作记录的用户数量',
  'API 调用数：当日 API 请求总次数',
  '资源评分：综合资源使用情况的评分',
  '指标类型：如 CPU、Memory、Disk 等监控维度',
]
const guideErrorNotes = [
  '统计数据通常有一天延迟，当日数据可能不完整',
  '监控指标数据量较大时建议分页查看',
]

onMounted(() => {
  loadDailyStats()
  loadMetrics()
})
</script>
