<template>
  <div>
    <div class="page-header">
      <h2>套餐管理</h2>
      <div class="page-header-actions">
        <PageHelpEntry @click="showGuide = true" />
        <DxButton
          v-if="perm.has(PACKAGE_LIST_CREATE)"
          text="新增套餐"
          icon="add"
          type="default"
          @click="showCreatePopup = true"
        />
      </div>
    </div>

    <FunctionDescriptionCard
      purpose="套餐管理用于维护 SaaS 平台的产品套餐定义，包括套餐的创建、编辑、启用与禁用。套餐是订阅和计费的基础。"
      data-scope="全平台所有 SaaS 套餐记录。"
      permission-note="需要 package:list:view 权限查看列表，package:list:create 创建套餐，package:list:update 编辑套餐。"
      risk-note="禁用套餐后，已关联的订阅不受影响，但新订阅将无法选择该套餐。"
      :collapsible="true"
    />

    <div class="card">
      <div class="filter-bar">
        <DxTextBox
          v-model:value="filterKeyword"
          placeholder="搜索套餐编码 / 套餐名称"
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
        <DxColumn data-field="packageCode" caption="套餐编码" :width="120" />
        <DxColumn data-field="packageName" caption="套餐名称" :width="160" />
        <DxColumn data-field="description" caption="描述" />
        <DxColumn data-field="status" caption="状态" cell-template="statusCell" :width="100" />
        <DxColumn data-field="createdAt" caption="创建时间" cell-template="dateTimeCell" :width="160" />
        <DxColumn caption="操作" cell-template="actionCell" :width="160" />
        <template #statusCell="{ data: cellData }">
          <StatusTag :status="cellData.value" />
        </template>
        <template #dateTimeCell="{ data: cellData }">
          <span>{{ formatDateTime(cellData.value) }}</span>
        </template>
        <template #actionCell="{ data: cellData }">
          <DxButton
            v-if="perm.has(PACKAGE_LIST_UPDATE)"
            text="编辑"
            styling-mode="text"
            type="default"
            @click="openEdit(cellData.data)"
          />
          <DxButton
            v-if="cellData.data.status === 'Active' && perm.has(PACKAGE_LIST_UPDATE)"
            text="禁用"
            styling-mode="text"
            type="danger"
            @click="onDisable(cellData.data.id)"
          />
          <DxButton
            v-if="cellData.data.status === 'Disabled' && perm.has(PACKAGE_LIST_UPDATE)"
            text="启用"
            styling-mode="text"
            type="success"
            @click="onEnable(cellData.data.id)"
          />
        </template>
        <DxPaging :page-size="20" />
        <DxPager :show-page-size-selector="true" :allowed-page-sizes="[10, 20, 50]" :show-info="true" />
      </DxDataGrid>
    </div>

    <!-- 新增套餐弹窗 -->
    <DxPopup
      :visible="showCreatePopup"
      title="新增套餐"
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
        <DxSimpleItem data-field="packageCode" editor-type="dxTextBox">
          <DxLabel text="套餐编码" />
        </DxSimpleItem>
        <DxSimpleItem data-field="packageName" editor-type="dxTextBox">
          <DxLabel text="套餐名称" />
        </DxSimpleItem>
        <DxSimpleItem data-field="description" editor-type="dxTextArea">
          <DxLabel text="描述" />
        </DxSimpleItem>
        <DxButtonItem>
          <DxButtonOptions text="提交" type="default" :use-submit-behavior="false" @click="handleCreate" />
        </DxButtonItem>
      </DxForm>
    </DxPopup>

    <!-- 编辑套餐弹窗 -->
    <DxPopup
      :visible="showEditPopup"
      title="编辑套餐"
      :width="480"
      :height="'auto'"
      :show-close-button="true"
      @hiding="showEditPopup = false"
    >
      <DxForm
        :form-data="editForm"
        :col-count="1"
        label-mode="floating"
      >
        <DxSimpleItem data-field="packageName" editor-type="dxTextBox">
          <DxLabel text="套餐名称" />
        </DxSimpleItem>
        <DxSimpleItem data-field="description" editor-type="dxTextArea">
          <DxLabel text="描述" />
        </DxSimpleItem>
        <DxButtonItem>
          <DxButtonOptions text="保存" type="default" :use-submit-behavior="false" @click="handleEdit" />
        </DxButtonItem>
      </DxForm>
    </DxPopup>

    <OperationGuideDrawer
      v-model:visible="showGuide"
      title="套餐管理操作指引"
      entry-path="左侧菜单 → SaaS 套餐系统 → 套餐管理"
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
import StatusTag from '@/components/StatusTag.vue'
import FunctionDescriptionCard from '@/components/help/FunctionDescriptionCard.vue'
import OperationGuideDrawer from '@/components/help/OperationGuideDrawer.vue'
import PageHelpEntry from '@/components/help/PageHelpEntry.vue'
import { usePermission } from '@/composables/usePermission'
import { formatDateTime } from '@/utils/format'
import {
  getPackages,
  createPackage,
  updatePackage,
  enablePackage,
  disablePackage,
  type SaasPackageDto,
  type CreateSaasPackageRequest,
  type UpdateSaasPackageRequest,
} from '@/api/packages'
import {
  PACKAGE_LIST_CREATE,
  PACKAGE_LIST_UPDATE,
} from '@/constants/permissions'

const perm = usePermission()
const showGuide = ref(false)
const showCreatePopup = ref(false)
const showEditPopup = ref(false)
const filterKeyword = ref('')
const editingId = ref(0)

const gridData = ref<SaasPackageDto[]>([])

const createForm = reactive<CreateSaasPackageRequest>({
  packageCode: '',
  packageName: '',
  description: '',
})

const editForm = reactive<UpdateSaasPackageRequest>({
  packageName: '',
  description: '',
})

async function loadData() {
  try {
    const res = await getPackages({
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
    await createPackage(createForm)
    showCreatePopup.value = false
    Object.assign(createForm, { packageCode: '', packageName: '', description: '' })
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

function openEdit(row: SaasPackageDto) {
  editingId.value = row.id
  editForm.packageName = row.packageName
  editForm.description = row.description
  showEditPopup.value = true
}

async function handleEdit() {
  try {
    await updatePackage(editingId.value, editForm)
    showEditPopup.value = false
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

async function onEnable(id: number) {
  try {
    await enablePackage(id)
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

async function onDisable(id: number) {
  try {
    await disablePackage(id)
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

const guideSteps = [
  '点击【新增套餐】按钮创建新的 SaaS 套餐',
  '在列表中搜索套餐编码或名称',
  '点击【编辑】修改套餐信息',
  '点击【启用/禁用】切换套餐状态',
]
const guideFieldNotes = [
  '套餐编码：业务唯一标识，创建后不可修改',
  '套餐名称：用于显示的名称',
  '描述：套餐的详细说明',
  '状态：Active（正常）/ Disabled（已禁用）',
]
const guideErrorNotes = [
  '套餐编码不能与已有套餐重复',
  '禁用套餐不影响已有订阅，但新订阅将无法选择该套餐',
]

onMounted(loadData)
</script>
