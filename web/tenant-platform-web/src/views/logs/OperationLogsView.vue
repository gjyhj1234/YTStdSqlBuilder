<template>
  <div>
    <div class="page-header">
      <h2>{{ $t('route.operationLogs') }}</h2>
      <div class="page-header-actions">
        <PageHelpEntry @click="showGuide = true" />
      </div>
    </div>

    <FunctionDescriptionCard
      purpose="操作日志记录平台和租户的所有管理操作，支持按时间、操作类型和结果进行筛选。用于问题追溯和安全审计。"
      data-scope="全平台所有操作记录。"
      permission-note="需要 log:operation:view 权限查看列表。"
      risk-note="操作日志为只读数据，不可修改或删除。"
      :collapsible="true"
    />

    <div class="card">
      <div class="filter-bar">
        <DxTextBox
          v-model:value="filterKeyword"
          :placeholder="$t('搜索操作类型 / 资源类型 / IP')"
          :width="280"
          mode="search"
          value-change-event="input"
        />
        <DxSelectBox
          v-model:value="filterResult"
          :items="resultOptions"
          display-expr="text"
          value-expr="value"
          :placeholder="$t('操作结果')"
          :width="140"
          :show-clear-button="true"
        />
        <DxButton :text="$t('查询')" icon="search" @click="loadData" />
      </div>

      <DxDataGrid
        :data-source="gridData"
        :show-borders="true"
        :column-auto-width="true"
        :hover-state-enabled="true"
        key-expr="id"
        @row-click="onRowClick"
      >
        <DxColumn data-field="id" :caption="$t('common.id')" :width="60" />
        <DxColumn data-field="tenantRefId" :caption="$t('common.tenantId')" :width="80" />
        <DxColumn data-field="operatorType" :caption="$t('操作者类型')" />
        <DxColumn data-field="operatorId" :caption="$t('操作者ID')" :width="90" />
        <DxColumn data-field="action" :caption="$t('common.actions')" />
        <DxColumn data-field="resourceType" :caption="$t('资源类型')" />
        <DxColumn data-field="resourceId" :caption="$t('资源ID')" />
        <DxColumn data-field="ipAddress" :caption="$t('IP 地址')" />
        <DxColumn data-field="operationResult" :caption="$t('操作结果')" cell-template="statusCell" :width="100" />
        <DxColumn data-field="createdAt" :caption="$t('common.createdAt')" cell-template="dateCell" />
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

    <!-- 详情弹窗 -->
    <DxPopup
      :visible="showDetail"
      :title="$t('操作日志详情')"
      :width="520"
      :height="'auto'"
      :show-close-button="true"
      @hiding="showDetail = false"
    >
      <div v-if="detailData" class="detail-grid">
        <p><strong>{{ $t('common.id') }}：</strong>{{ detailData.id }}</p>
        <p><strong>{{ $t('common.tenantId') }}：</strong>{{ detailData.tenantRefId }}</p>
        <p><strong>{{ $t('操作者类型') }}：</strong>{{ detailData.operatorType }}</p>
        <p><strong>{{ $t('操作者ID') }}：</strong>{{ detailData.operatorId }}</p>
        <p><strong>{{ $t('common.actions') }}：</strong>{{ detailData.action }}</p>
        <p><strong>{{ $t('资源类型') }}：</strong>{{ detailData.resourceType }}</p>
        <p><strong>{{ $t('资源ID') }}：</strong>{{ detailData.resourceId }}</p>
        <p><strong>{{ $t('IP 地址') }}：</strong>{{ detailData.ipAddress }}</p>
        <p><strong>{{ $t('操作结果') }}：</strong><StatusTag :status="detailData.operationResult" :label-map="resultLabelMap" /></p>
        <p><strong>{{ $t('common.createdAt') }}：</strong>{{ formatDateTime(detailData.createdAt) }}</p>
      </div>
    </DxPopup>

    <OperationGuideDrawer
      v-model:visible="showGuide"
      title="操作日志操作指引"
      entry-path="左侧菜单 → 日志与审计 → 操作日志"
      :steps="guideSteps"
      :field-notes="guideFieldNotes"
      :error-notes="guideErrorNotes"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { DxDataGrid, DxColumn, DxPaging, DxPager } from 'devextreme-vue/data-grid'
import { DxButton } from 'devextreme-vue/button'
import { DxTextBox } from 'devextreme-vue/text-box'
import { DxSelectBox } from 'devextreme-vue/select-box'
import { DxPopup } from 'devextreme-vue/popup'
import { useI18n } from 'vue-i18n'
import StatusTag from '@/components/StatusTag.vue'
import FunctionDescriptionCard from '@/components/help/FunctionDescriptionCard.vue'
import OperationGuideDrawer from '@/components/help/OperationGuideDrawer.vue'
import PageHelpEntry from '@/components/help/PageHelpEntry.vue'
import { formatDateTime } from '@/utils/format'
import {
  getOperationLogs,
  getOperationLog,
  type OperationLogDto,
} from '@/api/logs'

const showGuide = ref(false)
const showDetail = ref(false)
const filterKeyword = ref('')
const filterResult = ref<string | undefined>(undefined)
const detailData = ref<OperationLogDto | null>(null)
const { t } = useI18n()

const resultOptions = computed(() => [
  { text: t('成功'), value: 'Success' },
  { text: t('失败'), value: 'Failure' },
])

const resultLabelMap = computed(() => ({
  Success: t('成功'),
  Failure: t('失败'),
}))

const gridData = ref<OperationLogDto[]>([])

async function loadData() {
  try {
    const res = await getOperationLogs({
      page: 1,
      pageSize: 20,
      keyword: filterKeyword.value || undefined,
      operationResult: filterResult.value || undefined,
    })
    gridData.value = res.data.items
  } catch {
    // 接口未就绪时保持空列表
  }
}

async function onRowClick(e: { data: OperationLogDto }) {
  try {
    const res = await getOperationLog(e.data.id)
    detailData.value = res.data
    showDetail.value = true
  } catch {
    // 错误由 http 层统一处理
  }
}

const guideSteps = [
  '在列表中查看所有操作日志记录',
  '使用搜索框按操作类型、资源类型或 IP 地址筛选',
  '使用下拉框按操作结果筛选',
  '点击任意行查看日志详情',
]
const guideFieldNotes = [
  '操作者类型：区分平台管理员和租户用户',
  '操作结果：标识操作是否成功',
  'IP 地址：记录操作发起方的网络地址',
]
const guideErrorNotes = [
  '日志数据为只读，无法编辑或删除',
]

onMounted(loadData)
</script>
