<template>
  <div>
    <div class="page-header">
      <h2>订阅管理</h2>
      <div class="page-header-actions">
        <PageHelpEntry @click="showGuide = true" />
        <DxButton
          v-if="perm.has(SUBSCRIPTION_LIST_CREATE)"
          text="新增订阅"
          icon="add"
          type="default"
          @click="showCreatePopup = true"
        />
      </div>
    </div>

    <FunctionDescriptionCard
      purpose="订阅管理用于维护租户的套餐订阅关系，包括新订阅、续费、升降级和取消。每个租户在同一时间只有一个有效订阅。"
      data-scope="全平台所有租户的订阅记录。"
      permission-note="需要 subscription:list:view 权限查看列表，subscription:list:create 创建订阅，subscription:list:cancel 取消订阅。"
      risk-note="取消订阅将导致租户无法继续使用对应套餐，请谨慎操作。"
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
        <DxColumn data-field="subscriptionStatus" caption="订阅状态" cell-template="statusCell" :width="100" />
        <DxColumn data-field="subscriptionType" caption="订阅类型" :width="100" />
        <DxColumn data-field="startedAt" caption="开始时间" cell-template="dateTimeCell" />
        <DxColumn data-field="expiresAt" caption="到期时间" cell-template="dateTimeCell" />
        <DxColumn data-field="autoRenew" caption="自动续费" cell-template="booleanCell" :width="80" />
        <DxColumn data-field="cancelledAt" caption="取消时间" cell-template="dateTimeCell" />
        <DxColumn data-field="createdAt" caption="创建时间" cell-template="dateTimeCell" />
        <DxColumn caption="操作" cell-template="actionCell" :width="100" />
        <template #statusCell="{ data: cellData }">
          <StatusTag :status="cellData.value" />
        </template>
        <template #dateTimeCell="{ data: cellData }">
          <span>{{ formatDateTime(cellData.value) }}</span>
        </template>
        <template #booleanCell="{ data: cellData }">
          <span>{{ cellData.value ? '是' : '否' }}</span>
        </template>
        <template #actionCell="{ data: cellData }">
          <DxButton
            v-if="cellData.data.subscriptionStatus === 'Active' && perm.has(SUBSCRIPTION_LIST_CANCEL)"
            text="取消"
            styling-mode="text"
            type="danger"
            @click="onCancel(cellData.data.id)"
          />
        </template>
        <DxPaging :page-size="20" />
        <DxPager :show-page-size-selector="true" :allowed-page-sizes="[10, 20, 50]" :show-info="true" />
      </DxDataGrid>
    </div>

    <!-- 新增订阅弹窗 -->
    <DxPopup
      :visible="showCreatePopup"
      title="新增订阅"
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
        <DxSimpleItem data-field="subscriptionType" editor-type="dxSelectBox"
          :editor-options="{ items: subscriptionTypes, displayExpr: 'text', valueExpr: 'value' }">
          <DxLabel text="订阅类型" />
        </DxSimpleItem>
        <DxSimpleItem data-field="autoRenew" editor-type="dxCheckBox">
          <DxLabel text="自动续费" />
        </DxSimpleItem>
        <DxButtonItem>
          <DxButtonOptions text="提交" type="default" :use-submit-behavior="false" @click="handleCreate" />
        </DxButtonItem>
      </DxForm>
    </DxPopup>

    <OperationGuideDrawer
      v-model:visible="showGuide"
      title="订阅管理操作指引"
      entry-path="左侧菜单 → 订阅管理体系 → 订阅管理"
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
  getSubscriptions,
  createSubscription,
  cancelSubscription,
  type TenantSubscriptionDto,
  type CreateSubscriptionRequest,
} from '@/api/subscriptions'
import {
  SUBSCRIPTION_LIST_CREATE,
  SUBSCRIPTION_LIST_CANCEL,
} from '@/constants/permissions'

const perm = usePermission()
const showGuide = ref(false)
const showCreatePopup = ref(false)
const filterKeyword = ref('')
const filterStatus = ref<string | undefined>(undefined)

const statusOptions = [
  { text: '生效中', value: 'Active' },
  { text: '已过期', value: 'Expired' },
  { text: '已取消', value: 'Cancelled' },
  { text: '待生效', value: 'Pending' },
]

const subscriptionTypes = [
  { text: '新订阅', value: 'New' },
  { text: '升级', value: 'Upgrade' },
  { text: '降级', value: 'Downgrade' },
  { text: '续费', value: 'Renewal' },
]

const gridData = ref<TenantSubscriptionDto[]>([])

const createForm = reactive<CreateSubscriptionRequest>({
  tenantRefId: 0,
  packageVersionId: 0,
  subscriptionType: 'New',
  autoRenew: false,
})

async function loadData() {
  try {
    const res = await getSubscriptions({
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
    await createSubscription(createForm)
    showCreatePopup.value = false
    Object.assign(createForm, { tenantRefId: 0, packageVersionId: 0, subscriptionType: 'New', autoRenew: false })
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

async function onCancel(id: number) {
  try {
    await cancelSubscription(id)
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

const guideSteps = [
  '点击【新增订阅】按钮为租户创建新的套餐订阅',
  '在列表中搜索或按状态筛选订阅记录',
  '点击【取消】终止有效订阅',
]
const guideFieldNotes = [
  '租户ID：关联的租户标识',
  '套餐版本ID：关联的套餐版本标识',
  'SubscriptionType 说明：New（新订阅）、Upgrade（升级）、Downgrade（降级）、Renewal（续费）',
  '自动续费：勾选后到期时自动生成续费订阅',
]
const guideErrorNotes = [
  '同一租户不能同时拥有多个有效订阅',
  '已取消的订阅不可恢复',
]

onMounted(loadData)
</script>
