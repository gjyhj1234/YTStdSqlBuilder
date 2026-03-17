<template>
  <div>
    <div class="page-header">
      <h2>通知记录管理</h2>
      <div class="page-header-actions">
        <PageHelpEntry @click="showGuide = true" />
        <DxButton
          v-if="perm.has(NOTIFICATION_RECORD_RESEND)"
          text="发送通知"
          icon="add"
          type="default"
          @click="showCreatePopup = true"
        />
      </div>
    </div>

    <FunctionDescriptionCard
      purpose="通知记录管理用于查看和发送通知消息。支持按渠道、发送状态筛选，以及标记为已读操作。"
      data-scope="全平台通知发送记录。"
      permission-note="需要 notification:record:view 查看列表，notification:record:resend 发送通知。"
      risk-note="发送通知后不可撤回，请确认收件信息无误。"
      :collapsible="true"
    />

    <div class="card">
      <div class="filter-bar">
        <DxTextBox
          v-model:value="filterKeyword"
          placeholder="搜索收件人 / 主题"
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
        <DxSelectBox
          v-model:value="filterStatus"
          :items="sendStatusOptions"
          display-expr="text"
          value-expr="value"
          placeholder="发送状态"
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
        <DxColumn data-field="templateId" caption="模板ID" :width="80" />
        <DxColumn data-field="channel" caption="渠道" :width="80" />
        <DxColumn data-field="recipient" caption="收件人" />
        <DxColumn data-field="subject" caption="主题" />
        <DxColumn data-field="sendStatus" caption="发送状态" cell-template="statusCell" :width="100" />
        <DxColumn data-field="sentAt" caption="发送时间" cell-template="dateCell" />
        <DxColumn data-field="readAt" caption="已读时间" cell-template="dateCell" />
        <DxColumn data-field="createdAt" caption="创建时间" cell-template="dateCell" />
        <DxColumn caption="操作" cell-template="actionCell" :width="100" />
        <template #statusCell="{ data: cellData }">
          <StatusTag :status="cellData.value" />
        </template>
        <template #dateCell="{ data: cellData }">
          <span>{{ formatDateTime(cellData.value) }}</span>
        </template>
        <template #actionCell="{ data: cellData }">
          <DxButton
            v-if="!cellData.data.readAt && perm.has(NOTIFICATION_RECORD_VIEW)"
            text="标记已读"
            styling-mode="text"
            @click="onMarkRead(cellData.data.id)"
          />
        </template>
        <DxPaging :page-size="20" />
        <DxPager :show-page-size-selector="true" :allowed-page-sizes="[10, 20, 50]" :show-info="true" />
      </DxDataGrid>
    </div>

    <!-- 发送通知弹窗 -->
    <DxPopup
      :visible="showCreatePopup"
      title="发送通知"
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
        <DxSimpleItem data-field="tenantRefId" editor-type="dxNumberBox">
          <DxLabel text="租户ID" />
        </DxSimpleItem>
        <DxSimpleItem data-field="templateId" editor-type="dxNumberBox">
          <DxLabel text="模板ID" />
        </DxSimpleItem>
        <DxSimpleItem data-field="channel" editor-type="dxSelectBox"
          :editor-options="{ items: channelOptions, displayExpr: 'text', valueExpr: 'value' }">
          <DxLabel text="渠道" />
        </DxSimpleItem>
        <DxSimpleItem data-field="recipient">
          <DxLabel text="收件人" />
        </DxSimpleItem>
        <DxSimpleItem data-field="subject" :col-span="2">
          <DxLabel text="主题" />
        </DxSimpleItem>
        <DxSimpleItem data-field="body" editor-type="dxTextArea" :col-span="2"
          :editor-options="{ height: 120 }">
          <DxLabel text="内容" />
        </DxSimpleItem>
        <DxButtonItem :col-span="2">
          <DxButtonOptions text="发送" type="default" :use-submit-behavior="false" @click="handleCreate" />
        </DxButtonItem>
      </DxForm>
    </DxPopup>

    <OperationGuideDrawer
      v-model:visible="showGuide"
      title="通知记录操作指引"
      entry-path="左侧菜单 → 通知管理 → 通知记录"
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
  getNotifications,
  createNotification,
  markNotificationRead,
  type NotificationDto,
  type CreateNotificationRequest,
} from '@/api/notifications'
import {
  NOTIFICATION_RECORD_VIEW,
  NOTIFICATION_RECORD_RESEND,
} from '@/constants/permissions'

const perm = usePermission()
const showGuide = ref(false)
const showCreatePopup = ref(false)
const filterKeyword = ref('')
const filterChannel = ref<string | undefined>(undefined)
const filterStatus = ref<string | undefined>(undefined)

const channelOptions = [
  { text: '邮件', value: 'Email' },
  { text: '短信', value: 'SMS' },
  { text: '站内信', value: 'InApp' },
  { text: '推送', value: 'Push' },
]

const sendStatusOptions = [
  { text: '待发送', value: 'Pending' },
  { text: '已发送', value: 'Sent' },
  { text: '发送失败', value: 'Failed' },
]

const gridData = ref<NotificationDto[]>([])

const createForm = reactive<CreateNotificationRequest>({
  tenantRefId: 0,
  templateId: 0,
  channel: '',
  recipient: '',
  subject: '',
  body: '',
})

async function loadData() {
  try {
    const res = await getNotifications({
      page: 1,
      pageSize: 20,
      keyword: filterKeyword.value || undefined,
      channel: filterChannel.value || undefined,
      sendStatus: filterStatus.value || undefined,
    })
    gridData.value = res.data.items
  } catch {
    // 接口未就绪时保持空列表
  }
}

async function handleCreate() {
  try {
    await createNotification(createForm)
    showCreatePopup.value = false
    Object.assign(createForm, {
      tenantRefId: 0, templateId: 0, channel: '',
      recipient: '', subject: '', body: '',
    })
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

async function onMarkRead(id: number) {
  try {
    await markNotificationRead(id)
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

const guideSteps = [
  '点击【发送通知】按钮创建并发送新通知',
  '填写租户ID、模板ID、渠道、收件人等信息',
  '使用搜索框按收件人或主题筛选',
  '使用下拉框按渠道或发送状态筛选',
  '点击【标记已读】将通知标记为已读',
]
const guideFieldNotes = [
  '租户ID：通知所属的租户编号',
  '模板ID：关联的通知模板编号',
  '渠道：支持 Email、SMS、InApp、Push',
  '发送状态：Pending（待发送）、Sent（已发送）、Failed（失败）',
]
const guideErrorNotes = [
  '发送通知后不可撤回',
  '模板ID 无效时发送可能失败',
]

onMounted(loadData)
</script>
