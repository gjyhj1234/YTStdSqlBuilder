<template>
  <div>
    <div class="page-header">
      <h2>Webhook 管理</h2>
      <div class="page-header-actions">
        <PageHelpEntry @click="showGuide = true" />
        <DxButton
          v-if="perm.has(INFRA_WEBHOOK_CREATE)"
          text="创建 Webhook"
          icon="add"
          type="default"
          @click="showCreatePopup = true"
        />
      </div>
    </div>

    <FunctionDescriptionCard
      purpose="Webhook管理用于配置租户的事件推送端点。当平台事件发生时将向配置的URL推送通知。"
      data-scope="全平台所有租户的 Webhook 配置。"
      permission-note="需要 infra:webhook:view 权限查看列表，infra:webhook:create 创建，infra:webhook:update 编辑和启禁用。"
      risk-note="禁用 Webhook 后，事件通知将不再推送到目标地址。"
      :collapsible="true"
    />

    <div class="card">
      <DxDataGrid
        :data-source="gridData"
        :show-borders="true"
        :column-auto-width="true"
        :hover-state-enabled="true"
        key-expr="id"
      >
        <DxColumn data-field="id" caption="ID" :width="60" />
        <DxColumn data-field="tenantRefId" caption="租户ID" :width="80" />
        <DxColumn data-field="webhookName" caption="Webhook 名称" />
        <DxColumn data-field="targetUrl" caption="目标 URL" />
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
            v-if="perm.has(INFRA_WEBHOOK_UPDATE)"
            text="编辑"
            styling-mode="text"
            @click="onEdit(cellData.data)"
          />
          <DxButton
            v-if="cellData.data.status === 'Active' && perm.has(INFRA_WEBHOOK_UPDATE)"
            text="禁用"
            styling-mode="text"
            type="danger"
            @click="onDisable(cellData.data.id)"
          />
          <DxButton
            v-if="cellData.data.status !== 'Active' && perm.has(INFRA_WEBHOOK_UPDATE)"
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

    <!-- 创建 Webhook 弹窗 -->
    <DxPopup
      :visible="showCreatePopup"
      title="创建 Webhook"
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
        <DxSimpleItem data-field="webhookName">
          <DxLabel text="Webhook 名称" />
        </DxSimpleItem>
        <DxSimpleItem data-field="targetUrl">
          <DxLabel text="目标 URL" />
        </DxSimpleItem>
        <DxButtonItem>
          <DxButtonOptions text="提交" type="default" :use-submit-behavior="false" @click="handleCreate" />
        </DxButtonItem>
      </DxForm>
    </DxPopup>

    <!-- 编辑 Webhook 弹窗 -->
    <DxPopup
      :visible="showEditPopup"
      title="编辑 Webhook"
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
        <DxSimpleItem data-field="webhookName">
          <DxLabel text="Webhook 名称" />
        </DxSimpleItem>
        <DxSimpleItem data-field="targetUrl">
          <DxLabel text="目标 URL" />
        </DxSimpleItem>
        <DxButtonItem>
          <DxButtonOptions text="保存" type="default" :use-submit-behavior="false" @click="handleEdit" />
        </DxButtonItem>
      </DxForm>
    </DxPopup>

    <OperationGuideDrawer
      v-model:visible="showGuide"
      title="Webhook 管理操作指引"
      entry-path="左侧菜单 → API 集成管理 → Webhook 管理"
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
import { DxPopup } from 'devextreme-vue/popup'
import { DxForm, DxSimpleItem, DxLabel, DxButtonItem, DxButtonOptions } from 'devextreme-vue/form'
import StatusTag from '@/components/StatusTag.vue'
import FunctionDescriptionCard from '@/components/help/FunctionDescriptionCard.vue'
import OperationGuideDrawer from '@/components/help/OperationGuideDrawer.vue'
import PageHelpEntry from '@/components/help/PageHelpEntry.vue'
import { usePermission } from '@/composables/usePermission'
import { formatDateTime } from '@/utils/format'
import {
  getWebhooks,
  createWebhook,
  updateWebhook,
  enableWebhook,
  disableWebhook,
  type TenantWebhookDto,
  type CreateWebhookRequest,
  type UpdateWebhookRequest,
} from '@/api/apiIntegration'
import {
  INFRA_WEBHOOK_CREATE,
  INFRA_WEBHOOK_UPDATE,
} from '@/constants/permissions'

const perm = usePermission()
const showGuide = ref(false)
const showCreatePopup = ref(false)
const showEditPopup = ref(false)
const editingId = ref<number>(0)

const gridData = ref<TenantWebhookDto[]>([])

const createForm = reactive<CreateWebhookRequest>({
  tenantRefId: 0,
  webhookName: '',
  targetUrl: '',
})

const editForm = reactive<UpdateWebhookRequest>({
  webhookName: '',
  targetUrl: '',
})

async function loadData() {
  try {
    const res = await getWebhooks({ page: 1, pageSize: 20 })
    gridData.value = res.data.items
  } catch {
    // 接口未就绪时保持空列表
  }
}

async function handleCreate() {
  try {
    await createWebhook(createForm)
    showCreatePopup.value = false
    Object.assign(createForm, { tenantRefId: 0, webhookName: '', targetUrl: '' })
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

function onEdit(webhook: TenantWebhookDto) {
  editingId.value = webhook.id
  Object.assign(editForm, { webhookName: webhook.webhookName, targetUrl: webhook.targetUrl })
  showEditPopup.value = true
}

async function handleEdit() {
  try {
    await updateWebhook(editingId.value, editForm)
    showEditPopup.value = false
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

async function onEnable(id: number) {
  try {
    await enableWebhook(id)
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

async function onDisable(id: number) {
  try {
    await disableWebhook(id)
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

const guideSteps = [
  '点击【创建 Webhook】按钮，填写租户ID、名称和目标 URL',
  '在列表中查看 Webhook 配置和状态',
  '点击【编辑】修改 Webhook 名称或目标 URL',
  '点击【禁用/启用】切换 Webhook 推送状态',
]
const guideFieldNotes = [
  '租户ID：目标租户的系统ID',
  'Webhook 名称：用于标识推送端点的用途',
  '目标 URL：接收事件推送的 HTTP(S) 地址',
]
const guideErrorNotes = [
  '目标 URL 不可达时推送将失败并记录到投递日志',
  '禁用后事件将不再推送，启用后恢复推送',
]

onMounted(loadData)
</script>
