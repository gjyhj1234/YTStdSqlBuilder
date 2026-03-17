<template>
  <div>
    <div class="page-header">
      <h2>试用管理</h2>
      <div class="page-header-actions">
        <PageHelpEntry @click="showGuide = true" />
        <DxButton
          v-if="perm.has(SUBSCRIPTION_TRIAL_CREATE)"
          text="新增试用"
          icon="add"
          type="default"
          @click="showCreatePopup = true"
        />
      </div>
    </div>

    <FunctionDescriptionCard
      purpose="试用管理用于管理租户的试用期记录。试用到期后可转为正式订阅或自动过期。"
      data-scope="全平台所有租户的试用记录。"
      permission-note="需要 subscription:trial:view 权限查看列表，subscription:trial:create 创建试用。"
      risk-note="试用到期后未转正将自动失效，租户将无法继续使用对应套餐功能。"
      :collapsible="true"
    />

    <div class="card">
      <div class="filter-bar">
        <DxTextBox
          v-model:value="filterKeyword"
          placeholder="搜索租户ID / 套餐版本ID"
          :width="260"
          mode="search"
          value-change-event="input"
        />
        <DxSelectBox
          v-model:value="filterStatus"
          :items="statusOptions"
          display-expr="text"
          value-expr="value"
          placeholder="状态筛选"
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
      >
        <DxColumn data-field="id" caption="ID" :width="60" />
        <DxColumn data-field="tenantRefId" caption="租户ID" :width="80" />
        <DxColumn data-field="packageVersionId" caption="套餐版本ID" :width="100" />
        <DxColumn data-field="status" caption="状态" cell-template="statusCell" :width="100" />
        <DxColumn data-field="startedAt" caption="开始时间" cell-template="dateTimeCell" />
        <DxColumn data-field="expiresAt" caption="到期时间" cell-template="dateTimeCell" />
        <DxColumn data-field="convertedSubscriptionId" caption="转正订阅ID" :width="100" />
        <DxColumn data-field="createdAt" caption="创建时间" cell-template="dateTimeCell" />
        <template #statusCell="{ data: cellData }">
          <StatusTag :status="cellData.value" />
        </template>
        <template #dateTimeCell="{ data: cellData }">
          <span>{{ formatDateTime(cellData.value) }}</span>
        </template>
        <DxPaging :page-size="20" />
        <DxPager :show-page-size-selector="true" :allowed-page-sizes="[10, 20, 50]" :show-info="true" />
      </DxDataGrid>
    </div>

    <!-- 新增试用弹窗 -->
    <DxPopup
      :visible="showCreatePopup"
      title="新增试用"
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
        <DxSimpleItem data-field="packageVersionId" editor-type="dxNumberBox">
          <DxLabel text="套餐版本ID" />
        </DxSimpleItem>
        <DxButtonItem>
          <DxButtonOptions text="提交" type="default" :use-submit-behavior="false" @click="handleCreate" />
        </DxButtonItem>
      </DxForm>
    </DxPopup>

    <OperationGuideDrawer
      v-model:visible="showGuide"
      title="试用管理操作指引"
      entry-path="左侧菜单 → 订阅管理体系 → 试用管理"
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
import { DxSelectBox } from 'devextreme-vue/select-box'
import { DxPopup } from 'devextreme-vue/popup'
import { DxForm, DxSimpleItem, DxLabel, DxButtonItem, DxButtonOptions } from 'devextreme-vue/form'
import StatusTag from '@/components/StatusTag.vue'
import FunctionDescriptionCard from '@/components/help/FunctionDescriptionCard.vue'
import OperationGuideDrawer from '@/components/help/OperationGuideDrawer.vue'
import PageHelpEntry from '@/components/help/PageHelpEntry.vue'
import { usePermission } from '@/composables/usePermission'
import { formatDateTime } from '@/utils/format'
import {
  getTrials,
  createTrial,
  type TenantTrialDto,
  type CreateTrialRequest,
} from '@/api/subscriptions'
import {
  SUBSCRIPTION_TRIAL_CREATE,
} from '@/constants/permissions'

const perm = usePermission()
const showGuide = ref(false)
const showCreatePopup = ref(false)
const filterKeyword = ref('')
const filterStatus = ref<string | undefined>(undefined)

const statusOptions = [
  { text: '试用中', value: 'Active' },
  { text: '已过期', value: 'Expired' },
  { text: '已转正', value: 'Converted' },
]

const gridData = ref<TenantTrialDto[]>([])

const createForm = reactive<CreateTrialRequest>({
  tenantRefId: 0,
  packageVersionId: 0,
})

async function loadData() {
  try {
    const res = await getTrials({
      page: 1,
      pageSize: 20,
      keyword: filterKeyword.value || undefined,
      status: filterStatus.value || undefined,
    })
    gridData.value = res.data.items
  } catch {
    // 接口未就绪时保持空列表
  }
}

async function handleCreate() {
  try {
    await createTrial(createForm)
    showCreatePopup.value = false
    Object.assign(createForm, { tenantRefId: 0, packageVersionId: 0 })
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

const guideSteps = [
  '点击【新增试用】按钮为租户创建试用记录',
  '在列表中搜索或按状态筛选试用记录',
  '试用到期后可通过订阅管理创建正式订阅完成转正',
]
const guideFieldNotes = [
  '租户ID：关联的租户标识',
  '套餐版本ID：试用的套餐版本标识',
  '转正订阅ID：试用转正后关联的正式订阅ID，未转正时为空',
  '试用转正说明：试用到期前在订阅管理中为该租户创建正式订阅即可完成转正',
]
const guideErrorNotes = [
  '同一租户同一套餐不能重复创建试用',
  '试用到期后未转正将自动标记为过期',
]

onMounted(loadData)
</script>
