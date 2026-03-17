<template>
  <div>
    <div class="page-header">
      <h2>通知模板管理</h2>
      <div class="page-header-actions">
        <PageHelpEntry @click="showGuide = true" />
        <DxButton
          v-if="perm.has(NOTIFICATION_TEMPLATE_CREATE)"
          text="新增模板"
          icon="add"
          type="default"
          @click="showCreatePopup = true"
        />
      </div>
    </div>

    <FunctionDescriptionCard
      purpose="通知模板管理用于定义各类通知的内容模板，支持邮件、短信、站内信和推送等渠道。模板支持变量替换。"
      data-scope="全平台通知模板数据。"
      permission-note="需要 notification:template:view 查看，notification:template:create 创建，notification:template:update 编辑。"
      risk-note="禁用模板后，引用该模板的通知将无法发送。"
      :collapsible="true"
    />

    <div class="card">
      <div class="filter-bar">
        <DxTextBox
          v-model:value="filterKeyword"
          placeholder="搜索模板编码 / 模板名称"
          :width="260"
          mode="search"
          value-change-event="input"
        />
        <DxSelectBox
          v-model:value="filterChannel"
          :items="channelOptions"
          display-expr="text"
          value-expr="value"
          placeholder="渠道筛选"
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
        <DxColumn data-field="templateCode" caption="模板编码" />
        <DxColumn data-field="templateName" caption="模板名称" />
        <DxColumn data-field="channel" caption="渠道" :width="100" />
        <DxColumn data-field="subjectTemplate" caption="主题模板" />
        <DxColumn data-field="bodyTemplate" caption="内容模板" cell-template="bodyCell" />
        <DxColumn data-field="status" caption="状态" cell-template="statusCell" :width="100" />
        <DxColumn data-field="createdAt" caption="创建时间" cell-template="dateCell" />
        <DxColumn caption="操作" cell-template="actionCell" :width="200" />
        <template #bodyCell="{ data: cellData }">
          <span>{{ truncate(cellData.value, 40) }}</span>
        </template>
        <template #statusCell="{ data: cellData }">
          <StatusTag :status="cellData.value" />
        </template>
        <template #dateCell="{ data: cellData }">
          <span>{{ formatDateTime(cellData.value) }}</span>
        </template>
        <template #actionCell="{ data: cellData }">
          <DxButton
            v-if="perm.has(NOTIFICATION_TEMPLATE_UPDATE)"
            text="编辑"
            styling-mode="text"
            @click="onEdit(cellData.data)"
          />
          <DxButton
            v-if="cellData.data.status === 'Active' && perm.has(NOTIFICATION_TEMPLATE_UPDATE)"
            text="禁用"
            styling-mode="text"
            type="danger"
            @click="onDisable(cellData.data.id)"
          />
          <DxButton
            v-if="cellData.data.status !== 'Active' && perm.has(NOTIFICATION_TEMPLATE_UPDATE)"
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

    <!-- 新增模板弹窗 -->
    <DxPopup
      :visible="showCreatePopup"
      title="新增通知模板"
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
        <DxSimpleItem data-field="templateCode">
          <DxLabel text="模板编码" />
        </DxSimpleItem>
        <DxSimpleItem data-field="templateName">
          <DxLabel text="模板名称" />
        </DxSimpleItem>
        <DxSimpleItem data-field="channel" editor-type="dxSelectBox"
          :editor-options="{ items: channelOptions, displayExpr: 'text', valueExpr: 'value' }">
          <DxLabel text="渠道" />
        </DxSimpleItem>
        <DxSimpleItem data-field="subjectTemplate" :col-span="2">
          <DxLabel text="主题模板" />
        </DxSimpleItem>
        <DxSimpleItem data-field="bodyTemplate" editor-type="dxTextArea" :col-span="2"
          :editor-options="{ height: 120 }">
          <DxLabel text="内容模板" />
        </DxSimpleItem>
        <DxButtonItem :col-span="2">
          <DxButtonOptions text="提交" type="default" :use-submit-behavior="false" @click="handleCreate" />
        </DxButtonItem>
      </DxForm>
    </DxPopup>

    <!-- 编辑模板弹窗 -->
    <DxPopup
      :visible="showEditPopup"
      title="编辑通知模板"
      :width="560"
      :height="'auto'"
      :show-close-button="true"
      @hiding="showEditPopup = false"
    >
      <DxForm
        :form-data="editForm"
        :col-count="1"
        label-mode="floating"
      >
        <DxSimpleItem data-field="templateName">
          <DxLabel text="模板名称" />
        </DxSimpleItem>
        <DxSimpleItem data-field="subjectTemplate">
          <DxLabel text="主题模板" />
        </DxSimpleItem>
        <DxSimpleItem data-field="bodyTemplate" editor-type="dxTextArea"
          :editor-options="{ height: 120 }">
          <DxLabel text="内容模板" />
        </DxSimpleItem>
        <DxButtonItem>
          <DxButtonOptions text="保存" type="default" :use-submit-behavior="false" @click="handleUpdate" />
        </DxButtonItem>
      </DxForm>
    </DxPopup>

    <OperationGuideDrawer
      v-model:visible="showGuide"
      title="通知模板操作指引"
      entry-path="左侧菜单 → 通知管理 → 通知模板"
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
  getNotificationTemplates,
  createNotificationTemplate,
  updateNotificationTemplate,
  enableNotificationTemplate,
  disableNotificationTemplate,
  type NotificationTemplateDto,
  type CreateNotificationTemplateRequest,
  type UpdateNotificationTemplateRequest,
} from '@/api/notifications'
import {
  NOTIFICATION_TEMPLATE_CREATE,
  NOTIFICATION_TEMPLATE_UPDATE,
} from '@/constants/permissions'

const perm = usePermission()
const showGuide = ref(false)
const showCreatePopup = ref(false)
const showEditPopup = ref(false)
const filterKeyword = ref('')
const filterChannel = ref<string | undefined>(undefined)
const editingId = ref<number | null>(null)

const channelOptions = [
  { text: '邮件', value: 'Email' },
  { text: '短信', value: 'SMS' },
  { text: '站内信', value: 'InApp' },
  { text: '推送', value: 'Push' },
]

const gridData = ref<NotificationTemplateDto[]>([])

const createForm = reactive<CreateNotificationTemplateRequest>({
  templateCode: '',
  templateName: '',
  channel: '',
  subjectTemplate: '',
  bodyTemplate: '',
})

const editForm = reactive<UpdateNotificationTemplateRequest>({
  templateName: '',
  subjectTemplate: '',
  bodyTemplate: '',
})

function truncate(str: string | undefined, maxLen: number): string {
  if (!str) return '-'
  return str.length > maxLen ? str.slice(0, maxLen) + '…' : str
}

async function loadData() {
  try {
    const res = await getNotificationTemplates({
      page: 1,
      pageSize: 20,
      keyword: filterKeyword.value || undefined,
      channel: filterChannel.value || undefined,
    })
    gridData.value = res.data.items
  } catch {
    // 接口未就绪时保持空列表
  }
}

async function handleCreate() {
  try {
    await createNotificationTemplate(createForm)
    showCreatePopup.value = false
    Object.assign(createForm, {
      templateCode: '', templateName: '', channel: '',
      subjectTemplate: '', bodyTemplate: '',
    })
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

function onEdit(row: NotificationTemplateDto) {
  editingId.value = row.id
  Object.assign(editForm, {
    templateName: row.templateName,
    subjectTemplate: row.subjectTemplate,
    bodyTemplate: row.bodyTemplate,
  })
  showEditPopup.value = true
}

async function handleUpdate() {
  if (editingId.value === null) return
  try {
    await updateNotificationTemplate(editingId.value, editForm)
    showEditPopup.value = false
    editingId.value = null
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

async function onEnable(id: number) {
  try {
    await enableNotificationTemplate(id)
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

async function onDisable(id: number) {
  try {
    await disableNotificationTemplate(id)
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

const guideSteps = [
  '点击【新增模板】按钮创建通知模板',
  '填写模板编码、名称、渠道和内容',
  '使用搜索框或渠道筛选查找模板',
  '点击【编辑】修改模板内容',
  '点击【启用/禁用】切换模板状态',
]
const guideFieldNotes = [
  '模板编码：全局唯一，创建后不可修改',
  '渠道：支持 Email（邮件）、SMS（短信）、InApp（站内信）、Push（推送）',
  '内容模板：支持使用 {{变量名}} 格式的变量替换',
]
const guideErrorNotes = [
  '模板编码重复时创建将失败',
  '禁用模板后引用该模板的通知将无法发送',
]

onMounted(loadData)
</script>
