<template>
  <div>
    <div class="page-header">
      <h2>退款管理</h2>
      <div class="page-header-actions">
        <PageHelpEntry @click="showGuide = true" />
        <DxButton
          v-if="perm.has(BILLING_PAYMENT_REFUND)"
          text="新增退款"
          icon="add"
          type="default"
          @click="showCreatePopup = true"
        />
      </div>
    </div>

    <FunctionDescriptionCard
      purpose="退款管理用于处理租户的退款请求。退款关联原始支付单，需要审核并记录退款原因。"
      data-scope="全平台所有租户的退款记录。"
      permission-note="需要 billing:payment:view 权限查看列表，billing:payment:refund 权限创建退款。"
      risk-note="退款操作将触发资金回退流程，请仔细核实退款金额和原因。"
      :collapsible="true"
    />

    <div class="card">
      <div class="filter-bar">
        <DxTextBox
          v-model:value="filterKeyword"
          placeholder="搜索退款单号 / 支付单ID"
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
        <DxColumn data-field="refundNo" caption="退款单号" />
        <DxColumn data-field="paymentOrderId" caption="支付单ID" :width="80" />
        <DxColumn data-field="refundStatus" caption="退款状态" cell-template="statusCell" :width="100" />
        <DxColumn data-field="refundAmount" caption="退款金额" cell-template="amountCell" :width="100" />
        <DxColumn data-field="refundReason" caption="退款原因" />
        <DxColumn data-field="refundedAt" caption="退款时间" cell-template="dateTimeCell" />
        <DxColumn data-field="createdAt" caption="创建时间" cell-template="dateTimeCell" />
        <template #statusCell="{ data: cellData }">
          <StatusTag :status="cellData.value" />
        </template>
        <template #dateTimeCell="{ data: cellData }">
          <span>{{ formatDateTime(cellData.value) }}</span>
        </template>
        <template #amountCell="{ data: cellData }">
          <span>{{ formatAmount(cellData.value) }}</span>
        </template>
        <DxPaging :page-size="20" />
        <DxPager :show-page-size-selector="true" :allowed-page-sizes="[10, 20, 50]" :show-info="true" />
      </DxDataGrid>
    </div>

    <!-- 新增退款弹窗 -->
    <DxPopup
      :visible="showCreatePopup"
      title="新增退款"
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
        <DxSimpleItem data-field="paymentOrderId" editor-type="dxNumberBox">
          <DxLabel text="支付单ID" />
        </DxSimpleItem>
        <DxSimpleItem data-field="refundAmount" editor-type="dxNumberBox"
          :editor-options="{ min: 0, format: '#,##0.00' }">
          <DxLabel text="退款金额" />
        </DxSimpleItem>
        <DxSimpleItem data-field="refundReason" editor-type="dxTextArea">
          <DxLabel text="退款原因" />
        </DxSimpleItem>
        <DxButtonItem>
          <DxButtonOptions text="提交" type="default" :use-submit-behavior="false" @click="handleCreate" />
        </DxButtonItem>
      </DxForm>
    </DxPopup>

    <OperationGuideDrawer
      v-model:visible="showGuide"
      title="退款管理操作指引"
      entry-path="左侧菜单 → 计费管理体系 → 退款管理"
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
import { formatDateTime, formatAmount } from '@/utils/format'
import {
  getPaymentRefunds,
  createRefund,
  type PaymentRefundDto,
  type CreateRefundRequest,
} from '@/api/billing'
import {
  BILLING_PAYMENT_REFUND,
} from '@/constants/permissions'

const perm = usePermission()
const showGuide = ref(false)
const showCreatePopup = ref(false)
const filterKeyword = ref('')
const filterStatus = ref<string | undefined>(undefined)

const statusOptions = [
  { text: '待处理', value: 'Pending' },
  { text: '已退款', value: 'Refunded' },
  { text: '已拒绝', value: 'Rejected' },
  { text: '处理中', value: 'Processing' },
]

const gridData = ref<PaymentRefundDto[]>([])

const createForm = reactive<CreateRefundRequest>({
  paymentOrderId: 0,
  refundAmount: 0,
  refundReason: '',
})

async function loadData() {
  try {
    const res = await getPaymentRefunds({
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
    await createRefund(createForm)
    showCreatePopup.value = false
    Object.assign(createForm, { paymentOrderId: 0, refundAmount: 0, refundReason: '' })
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

const guideSteps = [
  '点击【新增退款】按钮创建退款申请',
  '填写关联的支付单ID、退款金额和退款原因',
  '在列表中搜索或按状态筛选退款记录',
]
const guideFieldNotes = [
  '退款单号：系统自动生成的唯一退款编号',
  '支付单ID：关联的原始支付单标识',
  '退款金额：不能超过原支付金额',
  '退款原因建议：套餐降级差价退还、服务质量问题、重复支付、客户主动取消等',
]
const guideErrorNotes = [
  '退款金额不能超过原支付金额',
  '已退款的支付单不可重复退款',
]

onMounted(loadData)
</script>
