<template>
  <div>
    <div class="page-header">
      <h2>资源配额管理</h2>
      <div class="page-header-actions">
        <PageHelpEntry @click="showGuide = true" />
        <DxButton
          v-if="perm.has(TENANT_RESOURCE_CREATE)"
          text="新增配额"
          icon="add"
          type="default"
          @click="showCreatePopup = true"
        />
      </div>
    </div>

    <FunctionDescriptionCard
      purpose="资源配额管理用于设置每个租户的资源使用上限，包括存储、API调用、用户数等。超过配额将触发告警或限制。"
      data-scope="全平台所有租户资源配额数据。"
      permission-note="需要 tenant:resource:view 权限查看列表，tenant:resource:create 创建配额。"
      risk-note="配额设置过低可能影响租户正常使用，请根据实际需求合理配置。"
      :collapsible="true"
    />

    <div class="card">
      <div class="filter-bar">
        <DxTextBox
          v-model:value="filterKeyword"
          placeholder="搜索配额类型"
          :width="260"
          mode="search"
          value-change-event="input"
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
        <DxColumn data-field="tenantRefId" caption="租户ID" :width="80" />
        <DxColumn data-field="quotaType" caption="配额类型" />
        <DxColumn data-field="quotaLimit" caption="配额上限" :width="100" />
        <DxColumn data-field="warningThreshold" caption="告警阈值" :width="100" />
        <DxColumn data-field="resetCycle" caption="重置周期" :width="100" />
        <DxColumn data-field="createdAt" caption="创建时间" cell-template="dateCell" />
        <DxColumn caption="操作" cell-template="actionCell" :width="120" />
        <template #dateCell="{ data: cellData }">
          <span>{{ formatDateTime(cellData.value) }}</span>
        </template>
        <template #actionCell="{ data: cellData }">
          <DxButton
            v-if="perm.has(TENANT_RESOURCE_UPDATE)"
            text="编辑"
            styling-mode="text"
            @click="onEdit(cellData.data)"
          />
          <DxButton
            v-if="perm.has(TENANT_RESOURCE_DELETE)"
            text="删除"
            styling-mode="text"
            type="danger"
            @click="onDelete(cellData.data.id)"
          />
        </template>
        <DxPaging :page-size="20" />
        <DxPager :show-page-size-selector="true" :allowed-page-sizes="[10, 20, 50]" :show-info="true" />
      </DxDataGrid>
    </div>

    <!-- 新增/编辑配额弹窗 -->
    <DxPopup
      :visible="showCreatePopup"
      title="新增资源配额"
      :width="480"
      :height="'auto'"
      :show-close-button="true"
      @hiding="showCreatePopup = false"
    >
      <DxForm
        :form-data="createForm"
        :col-count="1"
        label-mode="floating"
      >
        <DxSimpleItem data-field="tenantRefId" editor-type="dxNumberBox">
          <DxLabel text="租户ID" />
        </DxSimpleItem>
        <DxSimpleItem data-field="quotaType" editor-type="dxSelectBox"
          :editor-options="{ items: quotaTypes, displayExpr: 'text', valueExpr: 'value' }">
          <DxLabel text="配额类型" />
        </DxSimpleItem>
        <DxSimpleItem data-field="quotaLimit" editor-type="dxNumberBox"
          :editor-options="{ min: 0 }">
          <DxLabel text="配额上限" />
        </DxSimpleItem>
        <DxSimpleItem data-field="warningThreshold" editor-type="dxNumberBox"
          :editor-options="{ min: 0 }">
          <DxLabel text="告警阈值" />
        </DxSimpleItem>
        <DxSimpleItem data-field="resetCycle" editor-type="dxSelectBox"
          :editor-options="{ items: resetCycles, displayExpr: 'text', valueExpr: 'value' }">
          <DxLabel text="重置周期" />
        </DxSimpleItem>
        <DxButtonItem>
          <DxButtonOptions text="提交" type="default" :use-submit-behavior="false" @click="handleCreate" />
        </DxButtonItem>
      </DxForm>
    </DxPopup>

    <OperationGuideDrawer
      v-model:visible="showGuide"
      title="资源配额操作指引"
      entry-path="左侧菜单 → 租户信息体系 → 资源配额"
      :steps="guideSteps"
      :field-notes="guideFieldNotes"
      :error-notes="guideErrorNotes"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { DxDataGrid, DxColumn, DxPaging, DxPager } from 'devextreme-vue/data-grid'
import { DxButton } from 'devextreme-vue/button'
import { DxTextBox } from 'devextreme-vue/text-box'
import { DxPopup } from 'devextreme-vue/popup'
import { DxForm, DxSimpleItem, DxLabel, DxButtonItem, DxButtonOptions } from 'devextreme-vue/form'
import FunctionDescriptionCard from '@/components/help/FunctionDescriptionCard.vue'
import OperationGuideDrawer from '@/components/help/OperationGuideDrawer.vue'
import PageHelpEntry from '@/components/help/PageHelpEntry.vue'
import { usePermission } from '@/composables/usePermission'
import { formatDateTime } from '@/utils/format'
import {
  getTenantResourceQuotas,
  saveTenantResourceQuota,
  type TenantResourceQuotaDto,
  type SaveTenantResourceQuotaRequest,
} from '@/api/tenantResources'
import {
  TENANT_RESOURCE_CREATE,
  TENANT_RESOURCE_UPDATE,
  TENANT_RESOURCE_DELETE,
} from '@/constants/permissions'

const perm = usePermission()
const showGuide = ref(false)
const showCreatePopup = ref(false)
const filterKeyword = ref('')

const quotaTypes = [
  { text: '存储空间', value: 'Storage' },
  { text: 'API 调用次数', value: 'ApiCalls' },
  { text: '用户数', value: 'UserCount' },
  { text: '并发连接数', value: 'Concurrency' },
  { text: '带宽', value: 'Bandwidth' },
]

const resetCycles = [
  { text: '不重置', value: 'None' },
  { text: '每日', value: 'Daily' },
  { text: '每周', value: 'Weekly' },
  { text: '每月', value: 'Monthly' },
  { text: '每年', value: 'Yearly' },
]

const gridData = ref<TenantResourceQuotaDto[]>([])

const createForm = reactive<SaveTenantResourceQuotaRequest>({
  tenantRefId: 0,
  quotaType: 'Storage',
  quotaLimit: 0,
  warningThreshold: 0,
  resetCycle: 'Monthly',
})

async function loadData() {
  try {
    const res = await getTenantResourceQuotas({
      page: 1,
      pageSize: 20,
      keyword: filterKeyword.value || undefined,
    })
    gridData.value = res.data.items
  } catch {
    // 接口未就绪时保持空列表
  }
}

async function handleCreate() {
  try {
    await saveTenantResourceQuota(createForm)
    showCreatePopup.value = false
    Object.assign(createForm, {
      tenantRefId: 0, quotaType: 'Storage', quotaLimit: 0,
      warningThreshold: 0, resetCycle: 'Monthly',
    })
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

function onEdit(_quota: TenantResourceQuotaDto) {
  // 后续阶段完善编辑功能
}

function onDelete(_id: number) {
  // 后续阶段完善删除功能
}

const guideSteps = [
  '点击【新增配额】按钮为租户设置资源配额',
  '选择租户、配额类型并填写上限和告警阈值',
  '选择重置周期确定配额计量方式',
  '使用列表中的【编辑】按钮调整已有配额',
]
const guideFieldNotes = [
  '配额类型：Storage（存储）、ApiCalls（API调用）、UserCount（用户数）、Concurrency（并发）、Bandwidth（带宽）',
  '告警阈值：当使用量达到此值时触发告警通知',
  '重置周期：None 表示永不重置，Monthly 为最常用的按月重置',
]
const guideErrorNotes = [
  '同一租户同一配额类型只能设置一条记录',
  '告警阈值不应超过配额上限',
]

onMounted(loadData)
</script>
