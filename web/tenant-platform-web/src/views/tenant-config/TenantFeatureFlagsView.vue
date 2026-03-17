<template>
  <div>
    <div class="page-header">
      <h2>功能开关管理</h2>
      <div class="page-header-actions">
        <PageHelpEntry @click="showGuide = true" />
        <DxButton
          v-if="perm.has(TENANT_CONFIG_UPDATE)"
          text="新增功能开关"
          icon="add"
          type="default"
          @click="showCreatePopup = true"
        />
      </div>
    </div>

    <FunctionDescriptionCard
      purpose="功能开关管理用于控制租户级别的功能模块启停，支持全量发布、灰度发布等策略。可以安全地进行功能上线和回退。"
      data-scope="按租户隔离的功能开关列表。"
      permission-note="需要 tenant:config:view 权限查看列表，tenant:config:update 权限创建和切换开关。"
      risk-note="关闭功能开关将导致对应租户无法使用该功能模块。"
      :collapsible="true"
    />

    <div class="card">
      <div class="filter-bar">
        <DxNumberBox
          v-model:value="filterTenantRefId"
          placeholder="租户ID"
          :width="160"
          :show-spin-buttons="true"
          :min="1"
        />
        <DxTextBox
          v-model:value="filterKeyword"
          placeholder="搜索功能键 / 功能名"
          :width="240"
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
        <DxColumn data-field="featureKey" caption="功能键" />
        <DxColumn data-field="featureName" caption="功能名称" />
        <DxColumn data-field="enabled" caption="启用状态" cell-template="enabledCell" :width="100" />
        <DxColumn data-field="rolloutType" caption="发布策略" :width="120" />
        <DxColumn data-field="updatedAt" caption="更新时间" cell-template="dateCell" />
        <DxColumn caption="操作" cell-template="actionCell" :width="140" />
        <template #enabledCell="{ data: cellData }">
          <StatusTag :status="cellData.value ? 'Active' : 'Disabled'" />
        </template>
        <template #dateCell="{ data: cellData }">
          <span>{{ formatDateTime(cellData.value) }}</span>
        </template>
        <template #actionCell="{ data: cellData }">
          <DxButton
            v-if="perm.has(TENANT_CONFIG_UPDATE)"
            :text="cellData.data.enabled ? '禁用' : '启用'"
            styling-mode="text"
            :type="cellData.data.enabled ? 'danger' : 'success'"
            @click="onToggle(cellData.data)"
          />
          <DxButton
            v-if="perm.has(TENANT_CONFIG_UPDATE)"
            text="编辑"
            styling-mode="text"
            @click="onEdit(cellData.data)"
          />
        </template>
        <DxPaging :page-size="20" />
        <DxPager :show-page-size-selector="true" :allowed-page-sizes="[10, 20, 50]" :show-info="true" />
      </DxDataGrid>
    </div>

    <!-- 新增 / 编辑功能开关弹窗 -->
    <DxPopup
      :visible="showCreatePopup"
      :title="isEditing ? '编辑功能开关' : '新增功能开关'"
      :width="480"
      :height="'auto'"
      :show-close-button="true"
      @hiding="onPopupHiding"
    >
      <DxForm
        :form-data="createForm"
        :col-count="1"
        label-mode="floating"
      >
        <DxSimpleItem data-field="tenantRefId" editor-type="dxNumberBox"
          :editor-options="{ min: 1, showSpinButtons: true }">
          <DxLabel text="租户ID" />
        </DxSimpleItem>
        <DxSimpleItem data-field="featureKey">
          <DxLabel text="功能键" />
        </DxSimpleItem>
        <DxSimpleItem data-field="featureName">
          <DxLabel text="功能名称" />
        </DxSimpleItem>
        <DxSimpleItem data-field="enabled" editor-type="dxCheckBox">
          <DxLabel text="启用" />
        </DxSimpleItem>
        <DxSimpleItem data-field="rolloutType" editor-type="dxSelectBox"
          :editor-options="{ items: rolloutTypes, displayExpr: 'text', valueExpr: 'value' }">
          <DxLabel text="发布策略" />
        </DxSimpleItem>
        <DxButtonItem>
          <DxButtonOptions text="提交" type="default" :use-submit-behavior="false" @click="handleSave" />
        </DxButtonItem>
      </DxForm>
    </DxPopup>

    <OperationGuideDrawer
      v-model:visible="showGuide"
      title="功能开关操作指引"
      entry-path="左侧菜单 → 租户配置中心 → 功能开关"
      :steps="guideSteps"
      :field-notes="guideFieldNotes"
      :error-notes="guideErrorNotes"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { DxDataGrid, DxColumn, DxPaging, DxPager } from 'devextreme-vue/data-grid'
import { DxButton } from 'devextreme-vue/button'
import { DxTextBox } from 'devextreme-vue/text-box'
import { DxNumberBox } from 'devextreme-vue/number-box'
import { DxPopup } from 'devextreme-vue/popup'
import { DxForm, DxSimpleItem, DxLabel, DxButtonItem, DxButtonOptions } from 'devextreme-vue/form'
import StatusTag from '@/components/StatusTag.vue'
import FunctionDescriptionCard from '@/components/help/FunctionDescriptionCard.vue'
import OperationGuideDrawer from '@/components/help/OperationGuideDrawer.vue'
import PageHelpEntry from '@/components/help/PageHelpEntry.vue'
import { usePermission } from '@/composables/usePermission'
import { formatDateTime } from '@/utils/format'
import {
  getTenantFeatureFlags,
  saveTenantFeatureFlag,
  toggleFeatureFlag,
  type TenantFeatureFlagDto,
  type SaveTenantFeatureFlagRequest,
} from '@/api/tenantConfig'
import {
  TENANT_CONFIG_UPDATE,
} from '@/constants/permissions'

const perm = usePermission()
const showGuide = ref(false)
const showCreatePopup = ref(false)
const isEditing = ref(false)
const filterTenantRefId = ref<number | undefined>(undefined)
const filterKeyword = ref('')

const rolloutTypes = [
  { text: '全量发布', value: 'FullRelease' },
  { text: '灰度发布', value: 'GrayRelease' },
  { text: '白名单', value: 'Whitelist' },
  { text: '百分比', value: 'Percentage' },
]

const gridData = ref<TenantFeatureFlagDto[]>([])

const defaultForm: SaveTenantFeatureFlagRequest = {
  tenantRefId: 0,
  featureKey: '',
  featureName: '',
  enabled: true,
  rolloutType: 'FullRelease',
}

const createForm = reactive<SaveTenantFeatureFlagRequest>({ ...defaultForm })

async function loadData() {
  try {
    const res = await getTenantFeatureFlags({
      page: 1,
      pageSize: 20,
      tenantRefId: filterTenantRefId.value,
      keyword: filterKeyword.value || undefined,
    })
    gridData.value = res.data.items
  } catch {
    // 接口未就绪时保持空列表
  }
}

async function handleSave() {
  try {
    await saveTenantFeatureFlag(createForm)
    showCreatePopup.value = false
    isEditing.value = false
    Object.assign(createForm, { ...defaultForm })
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

function onEdit(row: TenantFeatureFlagDto) {
  Object.assign(createForm, {
    tenantRefId: row.tenantRefId,
    featureKey: row.featureKey,
    featureName: row.featureName,
    enabled: row.enabled,
    rolloutType: row.rolloutType,
  })
  isEditing.value = true
  showCreatePopup.value = true
}

async function onToggle(row: TenantFeatureFlagDto) {
  try {
    await toggleFeatureFlag(row.id, !row.enabled)
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

function onPopupHiding() {
  showCreatePopup.value = false
  isEditing.value = false
  Object.assign(createForm, { ...defaultForm })
}

const guideSteps = [
  '点击【新增功能开关】创建新的功能开关',
  '输入租户ID并点击查询，筛选指定租户的功能开关',
  '点击【启用/禁用】按钮快速切换功能状态',
  '点击【编辑】修改功能开关的发布策略',
]
const guideFieldNotes = [
  '功能键：全局唯一标识符，如 module.feature_name',
  '功能名称：可读的功能描述',
  '发布策略：FullRelease=全量发布，GrayRelease=灰度发布，Whitelist=白名单，Percentage=按百分比',
]
const guideErrorNotes = [
  '同一租户下功能键不可重复',
  '禁用核心功能开关可能影响租户业务',
]
</script>
