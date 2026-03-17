<template>
  <div>
    <div class="page-header">
      <h2>审计日志</h2>
      <div class="page-header-actions">
        <PageHelpEntry @click="showGuide = true" />
      </div>
    </div>

    <FunctionDescriptionCard
      purpose="审计日志记录平台关键安全事件和合规审计信息，按严重等级分类。用于合规检查和安全分析。"
      data-scope="全平台安全审计事件数据。"
      permission-note="需要 log:audit:view 权限查看列表。"
      risk-note="审计日志为只读数据，不可修改或删除。"
      :collapsible="true"
    />

    <div class="card">
      <div class="filter-bar">
        <DxTextBox
          v-model:value="filterKeyword"
          placeholder="搜索审计类型 / 主体类型 / 合规标签"
          :width="300"
          mode="search"
          value-change-event="input"
        />
        <DxSelectBox
          v-model:value="filterSeverity"
          :items="severityOptions"
          display-expr="text"
          value-expr="value"
          placeholder="严重等级"
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
        <DxColumn data-field="tenantRefId" caption="租户ID" :width="80" />
        <DxColumn data-field="auditType" caption="审计类型" />
        <DxColumn data-field="severity" caption="严重等级" cell-template="severityCell" :width="100" />
        <DxColumn data-field="subjectType" caption="主体类型" />
        <DxColumn data-field="subjectId" caption="主体ID" />
        <DxColumn data-field="complianceTag" caption="合规标签" />
        <DxColumn data-field="createdAt" caption="创建时间" cell-template="dateCell" />
        <template #severityCell="{ data: cellData }">
          <StatusTag :status="cellData.value" :label-map="severityLabelMap" />
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
      title="审计日志详情"
      :width="520"
      :height="'auto'"
      :show-close-button="true"
      @hiding="showDetail = false"
    >
      <div v-if="detailData" class="detail-grid">
        <p><strong>ID：</strong>{{ detailData.id }}</p>
        <p><strong>租户ID：</strong>{{ detailData.tenantRefId }}</p>
        <p><strong>审计类型：</strong>{{ detailData.auditType }}</p>
        <p><strong>严重等级：</strong><StatusTag :status="detailData.severity" :label-map="severityLabelMap" /></p>
        <p><strong>主体类型：</strong>{{ detailData.subjectType }}</p>
        <p><strong>主体ID：</strong>{{ detailData.subjectId }}</p>
        <p><strong>合规标签：</strong>{{ detailData.complianceTag }}</p>
        <p><strong>创建时间：</strong>{{ formatDateTime(detailData.createdAt) }}</p>
      </div>
    </DxPopup>

    <OperationGuideDrawer
      v-model:visible="showGuide"
      title="审计日志操作指引"
      entry-path="左侧菜单 → 日志与审计 → 审计日志"
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
  getAuditLogs,
  getAuditLog,
  type AuditLogDto,
} from '@/api/logs'

const showGuide = ref(false)
const showDetail = ref(false)
const filterKeyword = ref('')
const filterSeverity = ref<string | undefined>(undefined)
const detailData = ref<AuditLogDto | null>(null)

const severityLabelMap: Record<string, string> = {
  Critical: '严重',
  High: '高',
  Medium: '中',
  Low: '低',
}

const severityOptions = [
  { text: '严重', value: 'Critical' },
  { text: '高', value: 'High' },
  { text: '中', value: 'Medium' },
  { text: '低', value: 'Low' },
]

const gridData = ref<AuditLogDto[]>([])

async function loadData() {
  try {
    const res = await getAuditLogs({
      page: 1,
      pageSize: 20,
      keyword: filterKeyword.value || undefined,
      severity: filterSeverity.value || undefined,
    })
    gridData.value = res.data.items
  } catch {
    // 接口未就绪时保持空列表
  }
}

async function onRowClick(e: { data: AuditLogDto }) {
  try {
    const res = await getAuditLog(e.data.id)
    detailData.value = res.data
    showDetail.value = true
  } catch {
    // 错误由 http 层统一处理
  }
}

const guideSteps = [
  '在列表中查看所有审计日志记录',
  '使用搜索框按审计类型、主体类型或合规标签筛选',
  '使用下拉框按严重等级筛选',
  '点击任意行查看审计日志详情',
]
const guideFieldNotes = [
  '严重等级：Critical（严重）> High（高）> Medium（中）> Low（低）',
  '合规标签：标识日志所属的合规标准或审计类别',
  '主体类型：记录被审计操作的目标对象类型',
]
const guideErrorNotes = [
  '审计日志为只读，无法编辑或删除',
]

onMounted(loadData)
</script>
