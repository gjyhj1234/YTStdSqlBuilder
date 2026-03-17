<template>
  <div>
    <div class="page-header">
      <h2>租户参数管理</h2>
      <div class="page-header-actions">
        <PageHelpEntry @click="showGuide = true" />
        <DxButton
          v-if="perm.has(TENANT_CONFIG_UPDATE)"
          text="新增参数"
          icon="add"
          type="default"
          @click="showCreatePopup = true"
        />
      </div>
    </div>

    <FunctionDescriptionCard
      purpose="参数管理用于维护租户级别的动态配置参数，支持字符串、数字、布尔等类型。适合存储业务规则和运营参数。"
      data-scope="按租户隔离的参数列表。"
      permission-note="需要 tenant:config:view 权限查看列表，tenant:config:update 权限新增、编辑和删除参数。"
      risk-note="删除参数可能导致依赖该参数的功能异常，请确认后操作。"
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
          placeholder="搜索参数键 / 参数名"
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
        <DxColumn data-field="paramKey" caption="参数键" />
        <DxColumn data-field="paramName" caption="参数名称" />
        <DxColumn data-field="paramType" caption="参数类型" :width="100" />
        <DxColumn data-field="paramValue" caption="参数值" />
        <DxColumn data-field="updatedAt" caption="更新时间" cell-template="dateCell" />
        <DxColumn caption="操作" cell-template="actionCell" :width="140" />
        <template #dateCell="{ data: cellData }">
          <span>{{ formatDateTime(cellData.value) }}</span>
        </template>
        <template #actionCell="{ data: cellData }">
          <DxButton
            v-if="perm.has(TENANT_CONFIG_UPDATE)"
            text="编辑"
            styling-mode="text"
            @click="onEdit(cellData.data)"
          />
          <DxButton
            v-if="perm.has(TENANT_CONFIG_DELETE)"
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

    <!-- 新增 / 编辑参数弹窗 -->
    <DxPopup
      :visible="showCreatePopup"
      :title="isEditing ? '编辑参数' : '新增参数'"
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
        <DxSimpleItem data-field="paramKey">
          <DxLabel text="参数键" />
        </DxSimpleItem>
        <DxSimpleItem data-field="paramName">
          <DxLabel text="参数名称" />
        </DxSimpleItem>
        <DxSimpleItem data-field="paramType" editor-type="dxSelectBox"
          :editor-options="{ items: paramTypes, displayExpr: 'text', valueExpr: 'value' }">
          <DxLabel text="参数类型" />
        </DxSimpleItem>
        <DxSimpleItem data-field="paramValue">
          <DxLabel text="参数值" />
        </DxSimpleItem>
        <DxButtonItem>
          <DxButtonOptions text="提交" type="default" :use-submit-behavior="false" @click="handleSave" />
        </DxButtonItem>
      </DxForm>
    </DxPopup>

    <OperationGuideDrawer
      v-model:visible="showGuide"
      title="参数管理操作指引"
      entry-path="左侧菜单 → 租户配置中心 → 参数管理"
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
import FunctionDescriptionCard from '@/components/help/FunctionDescriptionCard.vue'
import OperationGuideDrawer from '@/components/help/OperationGuideDrawer.vue'
import PageHelpEntry from '@/components/help/PageHelpEntry.vue'
import { usePermission } from '@/composables/usePermission'
import { formatDateTime } from '@/utils/format'
import {
  getTenantParameters,
  saveTenantParameter,
  deleteTenantParameter,
  type TenantParameterDto,
  type SaveTenantParameterRequest,
} from '@/api/tenantConfig'
import {
  TENANT_CONFIG_UPDATE,
  TENANT_CONFIG_DELETE,
} from '@/constants/permissions'

const perm = usePermission()
const showGuide = ref(false)
const showCreatePopup = ref(false)
const isEditing = ref(false)
const filterTenantRefId = ref<number | undefined>(undefined)
const filterKeyword = ref('')

const paramTypes = [
  { text: '字符串', value: 'String' },
  { text: '数字', value: 'Number' },
  { text: '布尔', value: 'Boolean' },
  { text: 'JSON', value: 'Json' },
]

const gridData = ref<TenantParameterDto[]>([])

const defaultForm: SaveTenantParameterRequest = {
  tenantRefId: 0,
  paramKey: '',
  paramName: '',
  paramType: 'String',
  paramValue: '',
}

const createForm = reactive<SaveTenantParameterRequest>({ ...defaultForm })

async function loadData() {
  try {
    const res = await getTenantParameters({
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
    await saveTenantParameter(createForm)
    showCreatePopup.value = false
    isEditing.value = false
    Object.assign(createForm, { ...defaultForm })
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

function onEdit(row: TenantParameterDto) {
  Object.assign(createForm, {
    tenantRefId: row.tenantRefId,
    paramKey: row.paramKey,
    paramName: row.paramName,
    paramType: row.paramType,
    paramValue: row.paramValue,
  })
  isEditing.value = true
  showCreatePopup.value = true
}

async function onDelete(id: number) {
  try {
    await deleteTenantParameter(id)
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
  '点击【新增参数】创建新的配置参数',
  '输入租户ID和关键词筛选参数列表',
  '点击【编辑】修改参数名称、类型或值',
  '点击【删除】移除不再使用的参数',
]
const guideFieldNotes = [
  '参数键：全局唯一标识符，如 max_users、enable_export',
  '参数类型：String=字符串，Number=数字，Boolean=布尔值，Json=JSON对象',
  '参数值：根据参数类型填写对应格式的值',
]
const guideErrorNotes = [
  '同一租户下参数键不可重复',
  '删除参数后无法恢复，请确认该参数无业务依赖',
  '修改参数类型后请确保参数值格式匹配',
]
</script>
