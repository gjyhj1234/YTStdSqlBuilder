<template>
  <div>
    <div class="page-header">
      <h2>Webhook 推送日志</h2>
      <div class="page-header-actions">
        <PageHelpEntry @click="showGuide = true" />
      </div>
    </div>

    <FunctionDescriptionCard
      purpose="Webhook推送日志用于查看每次Webhook事件投递的详细记录，包括投递状态、响应码和重试次数。"
      data-scope="全平台所有 Webhook 的投递日志。"
      permission-note="需要 infra:webhook:view 权限查看日志。"
      risk-note="日志为只读数据，不支持修改或删除。"
      :collapsible="true"
    />

    <div class="card">
      <div class="filter-bar">
        <DxNumberBox
          v-model:value="filterWebhookId"
          placeholder="按 Webhook ID 筛选"
          :width="220"
          :show-clear-button="true"
        />
        <DxButton text="查询" icon="search" @click="loadData" />
      </div>

      <DxDataGrid
        :data-source="gridData"
        :show-borders="true"
        :column-auto-width="true"
        :hover-state-enabled="true"
        key-expr="id"
      >
        <DxColumn data-field="id" caption="ID" :width="60" />
        <DxColumn data-field="webhookId" caption="Webhook ID" :width="110" />
        <DxColumn data-field="eventId" caption="事件ID" :width="80" />
        <DxColumn data-field="deliveryStatus" caption="投递状态" cell-template="statusCell" :width="110" />
        <DxColumn data-field="responseStatusCode" caption="响应码" :width="90" />
        <DxColumn data-field="retryCount" caption="重试次数" :width="90" />
        <DxColumn data-field="deliveredAt" caption="投递时间" cell-template="dateCell" />
        <DxColumn data-field="createdAt" caption="创建时间" cell-template="dateCell" />
        <template #statusCell="{ data: cellData }">
          <StatusTag :status="cellData.value" />
        </template>
        <template #dateCell="{ data: cellData }">
          <span>{{ formatDateTime(cellData.value) }}</span>
        </template>
        <DxPaging :page-size="20" />
        <DxPager :show-page-size-selector="true" :allowed-page-sizes="[10, 20, 50]" :show-info="true" />
      </DxDataGrid>
    </div>

    <OperationGuideDrawer
      v-model:visible="showGuide"
      title="Webhook 推送日志操作指引"
      entry-path="左侧菜单 → API 集成管理 → 推送日志"
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
import StatusTag from '@/components/StatusTag.vue'
import FunctionDescriptionCard from '@/components/help/FunctionDescriptionCard.vue'
import OperationGuideDrawer from '@/components/help/OperationGuideDrawer.vue'
import PageHelpEntry from '@/components/help/PageHelpEntry.vue'
import { formatDateTime } from '@/utils/format'
import {
  getWebhookDeliveryLogs,
  type WebhookDeliveryLogDto,
} from '@/api/apiIntegration'

const showGuide = ref(false)
const filterWebhookId = ref<number | undefined>(undefined)

const gridData = ref<WebhookDeliveryLogDto[]>([])

async function loadData() {
  try {
    const res = await getWebhookDeliveryLogs({
      page: 1,
      pageSize: 20,
      webhookId: filterWebhookId.value,
    })
    gridData.value = res.data.items
  } catch {
    // 接口未就绪时保持空列表
  }
}

const guideSteps = [
  '在筛选栏输入 Webhook ID 后点击【查询】按钮',
  '在列表中查看投递状态、响应码和重试次数',
  '根据投递状态判断推送是否成功',
]
const guideFieldNotes = [
  'Webhook ID：需要查询日志的 Webhook 的系统ID',
  '投递状态：Success 表示推送成功，Failed 表示推送失败',
  '响应码：目标服务器返回的 HTTP 状态码',
  '重试次数：推送失败后的自动重试次数',
]
const guideErrorNotes = [
  '日志为只读，如需排查问题请根据响应码和状态分析',
  '多次重试仍失败的推送建议检查目标 URL 可用性',
]

onMounted(loadData)
</script>
