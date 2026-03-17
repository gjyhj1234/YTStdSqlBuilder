<template>
  <div>
    <div class="page-header">
      <h2>存储策略管理</h2>
      <div class="page-header-actions">
        <PageHelpEntry @click="showGuide = true" />
        <DxButton
          v-if="perm.has(INFRA_COMPONENT_CREATE)"
          text="新增策略"
          icon="add"
          type="default"
          @click="showCreatePopup = true"
        />
      </div>
    </div>

    <FunctionDescriptionCard
      purpose="存储策略管理用于配置文件存储的后端提供者，支持本地存储、S3、Azure Blob等多种存储引擎。每种策略定义存储桶和路径。"
      data-scope="全平台存储策略配置数据。"
      permission-note="需要 infra:component:view 查看，infra:component:create 创建，infra:component:update 编辑。"
      risk-note="禁用存储策略后，使用该策略的文件上传将不可用。"
      :collapsible="true"
    />

    <div class="card">
      <div class="filter-bar">
        <DxTextBox
          v-model:value="filterKeyword"
          placeholder="搜索策略编码 / 策略名称"
          :width="260"
          mode="search"
          value-change-event="input"
        />
        <DxSelectBox
          v-model:value="filterProvider"
          :items="providerOptions"
          display-expr="text"
          value-expr="value"
          placeholder="提供者类型"
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
        <DxColumn data-field="strategyCode" caption="策略编码" />
        <DxColumn data-field="strategyName" caption="策略名称" />
        <DxColumn data-field="providerType" caption="提供者类型" :width="120" />
        <DxColumn data-field="bucketName" caption="存储桶" />
        <DxColumn data-field="basePath" caption="基础路径" />
        <DxColumn data-field="status" caption="状态" cell-template="statusCell" :width="100" />
        <DxColumn data-field="createdAt" caption="创建时间" cell-template="dateCell" />
        <DxColumn caption="操作" cell-template="actionCell" :width="200" />
        <template #statusCell="{ data: cellData }">
          <StatusTag :status="cellData.value" />
        </template>
        <template #dateCell="{ data: cellData }">
          <span>{{ formatDateTime(cellData.value) }}</span>
        </template>
        <template #actionCell="{ data: cellData }">
          <DxButton
            v-if="perm.has(INFRA_COMPONENT_UPDATE)"
            text="编辑"
            styling-mode="text"
            @click="onEdit(cellData.data)"
          />
          <DxButton
            v-if="cellData.data.status === 'Active' && perm.has(INFRA_COMPONENT_UPDATE)"
            text="禁用"
            styling-mode="text"
            type="danger"
            @click="onDisable(cellData.data.id)"
          />
          <DxButton
            v-if="cellData.data.status !== 'Active' && perm.has(INFRA_COMPONENT_UPDATE)"
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

    <!-- 新增策略弹窗 -->
    <DxPopup
      :visible="showCreatePopup"
      title="新增存储策略"
      :width="560"
      :height="'auto'"
      :show-close-button="true"
      @hiding="showCreatePopup = false"
    >
      <DxForm
        :form-data="createForm"
        :col-count="2"
        label-mode="floating"
      >
        <DxSimpleItem data-field="strategyCode">
          <DxLabel text="策略编码" />
        </DxSimpleItem>
        <DxSimpleItem data-field="strategyName">
          <DxLabel text="策略名称" />
        </DxSimpleItem>
        <DxSimpleItem data-field="providerType" editor-type="dxSelectBox"
          :editor-options="{ items: providerOptions, displayExpr: 'text', valueExpr: 'value' }">
          <DxLabel text="提供者类型" />
        </DxSimpleItem>
        <DxSimpleItem data-field="bucketName">
          <DxLabel text="存储桶名称" />
        </DxSimpleItem>
        <DxSimpleItem data-field="basePath" :col-span="2">
          <DxLabel text="基础路径" />
        </DxSimpleItem>
        <DxButtonItem :col-span="2">
          <DxButtonOptions text="提交" type="default" :use-submit-behavior="false" @click="handleCreate" />
        </DxButtonItem>
      </DxForm>
    </DxPopup>

    <!-- 编辑策略弹窗 -->
    <DxPopup
      :visible="showEditPopup"
      title="编辑存储策略"
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
        <DxSimpleItem data-field="strategyName">
          <DxLabel text="策略名称" />
        </DxSimpleItem>
        <DxSimpleItem data-field="bucketName">
          <DxLabel text="存储桶名称" />
        </DxSimpleItem>
        <DxSimpleItem data-field="basePath">
          <DxLabel text="基础路径" />
        </DxSimpleItem>
        <DxButtonItem>
          <DxButtonOptions text="保存" type="default" :use-submit-behavior="false" @click="handleUpdate" />
        </DxButtonItem>
      </DxForm>
    </DxPopup>

    <OperationGuideDrawer
      v-model:visible="showGuide"
      title="存储策略操作指引"
      entry-path="左侧菜单 → 技术基础设施 → 存储策略"
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
  getStorageStrategies,
  createStorageStrategy,
  updateStorageStrategy,
  enableStorageStrategy,
  disableStorageStrategy,
  type StorageStrategyDto,
  type CreateStorageStrategyRequest,
  type UpdateStorageStrategyRequest,
} from '@/api/storage'
import {
  INFRA_COMPONENT_CREATE,
  INFRA_COMPONENT_UPDATE,
} from '@/constants/permissions'

const perm = usePermission()
const showGuide = ref(false)
const showCreatePopup = ref(false)
const showEditPopup = ref(false)
const filterKeyword = ref('')
const filterProvider = ref<string | undefined>(undefined)
const editingId = ref<number | null>(null)

const providerOptions = [
  { text: '本地存储', value: 'Local' },
  { text: 'S3', value: 'S3' },
  { text: 'Azure Blob', value: 'Azure' },
  { text: '阿里云 OSS', value: 'Aliyun' },
  { text: 'MinIO', value: 'Minio' },
]

const gridData = ref<StorageStrategyDto[]>([])

const createForm = reactive<CreateStorageStrategyRequest>({
  strategyCode: '',
  strategyName: '',
  providerType: '',
  bucketName: '',
  basePath: '',
})

const editForm = reactive<UpdateStorageStrategyRequest>({
  strategyName: '',
  bucketName: '',
  basePath: '',
})

async function loadData() {
  try {
    const res = await getStorageStrategies({
      page: 1,
      pageSize: 20,
      keyword: filterKeyword.value || undefined,
      providerType: filterProvider.value || undefined,
    })
    gridData.value = res.data.items
  } catch {
    // 接口未就绪时保持空列表
  }
}

async function handleCreate() {
  try {
    await createStorageStrategy(createForm)
    showCreatePopup.value = false
    Object.assign(createForm, {
      strategyCode: '', strategyName: '', providerType: '',
      bucketName: '', basePath: '',
    })
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

function onEdit(row: StorageStrategyDto) {
  editingId.value = row.id
  Object.assign(editForm, {
    strategyName: row.strategyName,
    bucketName: row.bucketName,
    basePath: row.basePath,
  })
  showEditPopup.value = true
}

async function handleUpdate() {
  if (editingId.value === null) return
  try {
    await updateStorageStrategy(editingId.value, editForm)
    showEditPopup.value = false
    editingId.value = null
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

async function onEnable(id: number) {
  try {
    await enableStorageStrategy(id)
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

async function onDisable(id: number) {
  try {
    await disableStorageStrategy(id)
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

const guideSteps = [
  '点击【新增策略】按钮创建存储策略',
  '填写策略编码、名称、提供者类型、存储桶和路径',
  '使用搜索框或提供者类型筛选查找策略',
  '点击【编辑】修改策略名称、存储桶或路径',
  '点击【启用/禁用】切换策略状态',
]
const guideFieldNotes = [
  '策略编码：全局唯一，创建后不可修改',
  '提供者类型：Local（本地）、S3、Azure、Aliyun（阿里云）、Minio',
  '存储桶：对应存储服务的 Bucket 名称',
  '基础路径：文件存储的根目录路径',
]
const guideErrorNotes = [
  '策略编码重复时创建将失败',
  '禁用策略后使用该策略的文件上传不可用',
]

onMounted(loadData)
</script>
