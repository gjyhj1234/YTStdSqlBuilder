<template>
  <div>
    <div class="page-header">
      <h2>系统日志</h2>
      <div class="page-header-actions">
        <PageHelpEntry @click="showGuide = true" />
      </div>
    </div>

    <FunctionDescriptionCard
      purpose="系统日志记录平台各服务组件的运行日志，包含错误、警告和信息级别。用于故障排查和系统监控。"
      data-scope="全平台服务运行日志数据。"
      permission-note="需要 log:system:view 权限查看列表。"
      risk-note="系统日志为只读数据，不可修改或删除。"
      :collapsible="true"
    />

    <div class="card">
      <div class="filter-bar">
        <DxTextBox
          v-model:value="filterKeyword"
          placeholder="搜索服务名称 / TraceId / 消息内容"
          :width="300"
          mode="search"
          value-change-event="input"
        />
        <DxSelectBox
          v-model:value="filterLevel"
          :items="levelOptions"
          display-expr="text"
          value-expr="value"
          placeholder="日志等级"
          :width="140"
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
        @row-click="onRowClick"
      >
        <DxColumn data-field="id" caption="ID" :width="60" />
        <DxColumn data-field="serviceName" caption="服务名称" />
        <DxColumn data-field="logLevel" caption="日志等级" cell-template="levelCell" :width="100" />
        <DxColumn data-field="traceId" caption="TraceId" />
        <DxColumn data-field="message" caption="消息" />
        <DxColumn data-field="createdAt" caption="创建时间" cell-template="dateCell" />
        <template #levelCell="{ data: cellData }">
          <StatusTag :status="cellData.value" :label-map="levelLabelMap" />
        </template>
        <template #dateCell="{ data: cellData }">
          <span>{{ formatDateTime(cellData.value) }}</span>
        </template>
        <DxPaging :page-size="20" />
        <DxPager :show-page-size-selector="true" :allowed-page-sizes="[10, 20, 50]" :show-info="true" />
      </DxDataGrid>
    </div>

    <!-- 详情弹窗 -->
    <DxPopup
      :visible="showDetail"
      title="系统日志详情"
      :width="600"
      :height="'auto'"
      :show-close-button="true"
      @hiding="showDetail = false"
    >
      <div v-if="detailData" class="detail-grid">
        <p><strong>ID：</strong>{{ detailData.id }}</p>
        <p><strong>服务名称：</strong>{{ detailData.serviceName }}</p>
        <p><strong>日志等级：</strong><StatusTag :status="detailData.logLevel" :label-map="levelLabelMap" /></p>
        <p><strong>TraceId：</strong>{{ detailData.traceId }}</p>
        <p><strong>消息：</strong>{{ detailData.message }}</p>
        <p><strong>创建时间：</strong>{{ formatDateTime(detailData.createdAt) }}</p>
      </div>
    </DxPopup>

    <OperationGuideDrawer
      v-model:visible="showGuide"
      title="系统日志操作指引"
      entry-path="左侧菜单 → 日志与审计 → 系统日志"
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
import { DxTextBox } from 'devextreme-vue/text-box'
import { DxSelectBox } from 'devextreme-vue/select-box'
import { DxPopup } from 'devextreme-vue/popup'
import StatusTag from '@/components/StatusTag.vue'
import FunctionDescriptionCard from '@/components/help/FunctionDescriptionCard.vue'
import OperationGuideDrawer from '@/components/help/OperationGuideDrawer.vue'
import PageHelpEntry from '@/components/help/PageHelpEntry.vue'
import { formatDateTime } from '@/utils/format'
import {
  getSystemLogs,
  getSystemLog,
  type SystemLogDto,
} from '@/api/logs'

const showGuide = ref(false)
const showDetail = ref(false)
const filterKeyword = ref('')
const filterLevel = ref<string | undefined>(undefined)
const detailData = ref<SystemLogDto | null>(null)

const levelLabelMap: Record<string, string> = {
  Error: '错误',
  Warning: '警告',
  Info: '信息',
  Debug: '调试',
}

const levelOptions = [
  { text: '错误', value: 'Error' },
  { text: '警告', value: 'Warning' },
  { text: '信息', value: 'Info' },
  { text: '调试', value: 'Debug' },
]

const gridData = ref<SystemLogDto[]>([])

async function loadData() {
  try {
    const res = await getSystemLogs({
      page: 1,
      pageSize: 20,
      keyword: filterKeyword.value || undefined,
      logLevel: filterLevel.value || undefined,
    })
    gridData.value = res.data.items
  } catch {
    // 接口未就绪时保持空列表
  }
}

async function onRowClick(e: { data: SystemLogDto }) {
  try {
    const res = await getSystemLog(e.data.id)
    detailData.value = res.data
    showDetail.value = true
  } catch {
    // 错误由 http 层统一处理
  }
}

const guideSteps = [
  '在列表中查看所有系统日志记录',
  '使用搜索框按服务名称、TraceId 或消息内容筛选',
  '使用下拉框按日志等级筛选',
  '点击任意行查看日志详情',
]
const guideFieldNotes = [
  '日志等级：Error（错误）> Warning（警告）> Info（信息）> Debug（调试）',
  'TraceId：用于跨服务链路追踪的唯一标识',
  '服务名称：产生该日志的服务组件名称',
]
const guideErrorNotes = [
  '系统日志为只读，无法编辑或删除',
]

onMounted(loadData)
</script>
