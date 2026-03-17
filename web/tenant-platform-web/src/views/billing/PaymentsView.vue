<template>
  <div>
    <div class="page-header">
      <h2>支付单管理</h2>
      <div class="page-header-actions">
        <PageHelpEntry @click="showGuide = true" />
        <DxButton
          v-if="perm.has(BILLING_PAYMENT_VIEW)"
          text="新增支付单"
          icon="add"
          type="default"
          @click="showCreatePopup = true"
        />
      </div>
    </div>

    <FunctionDescriptionCard
      purpose="支付单管理用于记录和跟踪租户的支付交易。每笔支付关联一张账单，支持多种支付渠道。"
      data-scope="全平台所有租户的支付记录。"
      permission-note="需要 billing:payment:view 权限查看列表和创建支付单。"
      risk-note="支付单创建后将进入支付流程，请确认金额和渠道无误。"
      :collapsible="true"
    />

    <div class="card">
      <div class="filter-bar">
        <DxTextBox
          v-model:value="filterKeyword"
          placeholder="搜索订单号 / 租户ID / 第三方交易号"
          :width="300"
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
        <DxColumn data-field="orderNo" caption="订单号" />
        <DxColumn data-field="tenantRefId" caption="租户ID" :width="80" />
        <DxColumn data-field="invoiceId" caption="账单ID" :width="80" />
        <DxColumn data-field="paymentChannel" caption="支付渠道" cell-template="channelCell" :width="100" />
        <DxColumn data-field="paymentStatus" caption="支付状态" cell-template="statusCell" :width="100" />
        <DxColumn data-field="amount" caption="金额" cell-template="amountCell" :width="100" />
        <DxColumn data-field="currencyCode" caption="币种" :width="60" />
        <DxColumn data-field="thirdPartyTxnNo" caption="第三方交易号" />
        <DxColumn data-field="paidAt" caption="支付时间" cell-template="dateTimeCell" />
        <DxColumn data-field="createdAt" caption="创建时间" cell-template="dateTimeCell" />
        <template #statusCell="{ data: cellData }">
          <StatusTag :status="cellData.value" />
        </template>
        <template #channelCell="{ data: cellData }">
          <span>{{ channelMap[cellData.value] || cellData.value }}</span>
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

    <!-- 新增支付单弹窗 -->
    <DxPopup
      :visible="showCreatePopup"
      title="新增支付单"
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
        <DxSimpleItem data-field="invoiceId" editor-type="dxNumberBox">
          <DxLabel text="账单ID" />
        </DxSimpleItem>
        <DxSimpleItem data-field="paymentChannel" editor-type="dxSelectBox"
          :editor-options="{ items: paymentChannels, displayExpr: 'text', valueExpr: 'value' }">
          <DxLabel text="支付渠道" />
        </DxSimpleItem>
        <DxSimpleItem data-field="amount" editor-type="dxNumberBox"
          :editor-options="{ min: 0, format: '#,##0.00' }">
          <DxLabel text="金额" />
        </DxSimpleItem>
        <DxSimpleItem data-field="currencyCode">
          <DxLabel text="币种" />
        </DxSimpleItem>
        <DxButtonItem>
          <DxButtonOptions text="提交" type="default" :use-submit-behavior="false" @click="handleCreate" />
        </DxButtonItem>
      </DxForm>
    </DxPopup>

    <OperationGuideDrawer
      v-model:visible="showGuide"
      title="支付单管理操作指引"
      entry-path="左侧菜单 → 计费管理体系 → 支付单管理"
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
  getPaymentOrders,
  createPaymentOrder,
  type PaymentOrderDto,
  type CreatePaymentOrderRequest,
} from '@/api/billing'
import {
  BILLING_PAYMENT_VIEW,
} from '@/constants/permissions'

const perm = usePermission()
const showGuide = ref(false)
const showCreatePopup = ref(false)
const filterKeyword = ref('')
const filterStatus = ref<string | undefined>(undefined)

const statusOptions = [
  { text: '待支付', value: 'Pending' },
  { text: '已支付', value: 'Paid' },
  { text: '已失败', value: 'Failed' },
  { text: '已关闭', value: 'Closed' },
]

const paymentChannels = [
  { text: '支付宝', value: 'Alipay' },
  { text: '微信支付', value: 'WeChat' },
  { text: '银行转账', value: 'BankTransfer' },
  { text: 'Stripe', value: 'Stripe' },
]

const channelMap: Record<string, string> = {
  Alipay: '支付宝',
  WeChat: '微信支付',
  BankTransfer: '银行转账',
  Stripe: 'Stripe',
}

const gridData = ref<PaymentOrderDto[]>([])

const createForm = reactive<CreatePaymentOrderRequest>({
  tenantRefId: 0,
  invoiceId: 0,
  paymentChannel: 'Alipay',
  amount: 0,
  currencyCode: 'CNY',
})

async function loadData() {
  try {
    const res = await getPaymentOrders({
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
    await createPaymentOrder(createForm)
    showCreatePopup.value = false
    Object.assign(createForm, {
      tenantRefId: 0, invoiceId: 0,
      paymentChannel: 'Alipay', amount: 0,
      currencyCode: 'CNY',
    })
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

const guideSteps = [
  '点击【新增支付单】按钮创建新的支付记录',
  '在列表中搜索或按状态筛选支付单',
  '查看第三方交易号进行对账',
]
const guideFieldNotes = [
  '订单号：系统自动生成的唯一支付订单号',
  '支付渠道：Alipay（支付宝）、WeChat（微信支付）、BankTransfer（银行转账）、Stripe',
  '金额：实际支付金额，需与账单总金额一致',
  '第三方交易号：支付渠道返回的交易流水号',
]
const guideErrorNotes = [
  '同一账单不可重复创建支付单',
  '支付金额必须大于零',
]

onMounted(loadData)
</script>
