<template>
  <div>
    <div class="page-header">
      <h2>账单管理</h2>
      <div class="page-header-actions">
        <PageHelpEntry @click="showGuide = true" />
        <DxButton
          v-if="perm.has(BILLING_INVOICE_CREATE)"
          text="新增账单"
          icon="add"
          type="default"
          @click="showCreatePopup = true"
        />
      </div>
    </div>

    <FunctionDescriptionCard
      purpose="账单管理用于生成和管理租户的计费账单。每张账单关联一个订阅周期，包含金额明细和支付状态。"
      data-scope="全平台所有租户的账单数据。"
      permission-note="需要 billing:invoice:view 权限查看列表，billing:invoice:create 创建账单，billing:invoice:void 作废账单。"
      risk-note="作废账单后不可恢复，请确认操作无误。"
      :collapsible="true"
    />

    <div class="card">
      <div class="filter-bar">
        <DxTextBox
          v-model:value="filterKeyword"
          placeholder="搜索账单号 / 租户ID"
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
        <DxColumn data-field="invoiceNo" caption="账单号" />
        <DxColumn data-field="tenantRefId" caption="租户ID" :width="80" />
        <DxColumn data-field="subscriptionId" caption="订阅ID" :width="80" />
        <DxColumn data-field="invoiceStatus" caption="账单状态" cell-template="statusCell" :width="100" />
        <DxColumn data-field="billingPeriodStart" caption="账期开始" cell-template="dateCell" />
        <DxColumn data-field="billingPeriodEnd" caption="账期结束" cell-template="dateCell" />
        <DxColumn data-field="subtotalAmount" caption="小计金额" cell-template="amountCell" :width="100" />
        <DxColumn data-field="extraAmount" caption="附加金额" cell-template="amountCell" :width="100" />
        <DxColumn data-field="discountAmount" caption="折扣金额" cell-template="amountCell" :width="100" />
        <DxColumn data-field="totalAmount" caption="总金额" cell-template="amountCell" :width="100" />
        <DxColumn data-field="currencyCode" caption="币种" :width="60" />
        <DxColumn data-field="issuedAt" caption="开具时间" cell-template="dateTimeCell" />
        <DxColumn data-field="dueAt" caption="到期时间" cell-template="dateTimeCell" />
        <DxColumn data-field="paidAt" caption="支付时间" cell-template="dateTimeCell" />
        <DxColumn data-field="createdAt" caption="创建时间" cell-template="dateTimeCell" />
        <DxColumn caption="操作" cell-template="actionCell" :width="100" />
        <template #statusCell="{ data: cellData }">
          <StatusTag :status="cellData.value" />
        </template>
        <template #dateCell="{ data: cellData }">
          <span>{{ formatDate(cellData.value) }}</span>
        </template>
        <template #dateTimeCell="{ data: cellData }">
          <span>{{ formatDateTime(cellData.value) }}</span>
        </template>
        <template #amountCell="{ data: cellData }">
          <span>{{ formatAmount(cellData.value) }}</span>
        </template>
        <template #actionCell="{ data: cellData }">
          <DxButton
            v-if="!['Voided', 'Paid'].includes(cellData.data.invoiceStatus) && perm.has(BILLING_INVOICE_VOID)"
            text="作废"
            styling-mode="text"
            type="danger"
            @click="onVoid(cellData.data.id)"
          />
        </template>
        <DxPaging :page-size="20" />
        <DxPager :show-page-size-selector="true" :allowed-page-sizes="[10, 20, 50]" :show-info="true" />
      </DxDataGrid>
    </div>

    <!-- 新增账单弹窗 -->
    <DxPopup
      :visible="showCreatePopup"
      title="新增账单"
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
        <DxSimpleItem data-field="subscriptionId" editor-type="dxNumberBox">
          <DxLabel text="订阅ID" />
        </DxSimpleItem>
        <DxSimpleItem data-field="billingPeriodStart" editor-type="dxDateBox">
          <DxLabel text="账期开始" />
        </DxSimpleItem>
        <DxSimpleItem data-field="billingPeriodEnd" editor-type="dxDateBox">
          <DxLabel text="账期结束" />
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
      title="账单管理操作指引"
      entry-path="左侧菜单 → 计费管理体系 → 账单管理"
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
import { formatDateTime, formatDate, formatAmount } from '@/utils/format'
import {
  getInvoices,
  createInvoice,
  voidInvoice,
  type BillingInvoiceDto,
  type CreateBillingInvoiceRequest,
} from '@/api/billing'
import {
  BILLING_INVOICE_CREATE,
  BILLING_INVOICE_VOID,
} from '@/constants/permissions'

const perm = usePermission()
const showGuide = ref(false)
const showCreatePopup = ref(false)
const filterKeyword = ref('')
const filterStatus = ref<string | undefined>(undefined)

const statusOptions = [
  { text: '待支付', value: 'Pending' },
  { text: '已支付', value: 'Paid' },
  { text: '已逾期', value: 'Overdue' },
  { text: '已作废', value: 'Voided' },
]

const gridData = ref<BillingInvoiceDto[]>([])

const createForm = reactive<CreateBillingInvoiceRequest>({
  tenantRefId: 0,
  subscriptionId: 0,
  billingPeriodStart: '',
  billingPeriodEnd: '',
  currencyCode: 'CNY',
})

async function loadData() {
  try {
    const res = await getInvoices({
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
    await createInvoice(createForm)
    showCreatePopup.value = false
    Object.assign(createForm, {
      tenantRefId: 0, subscriptionId: 0,
      billingPeriodStart: '', billingPeriodEnd: '',
      currencyCode: 'CNY',
    })
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

async function onVoid(id: number) {
  try {
    await voidInvoice(id)
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

const guideSteps = [
  '点击【新增账单】按钮创建新的计费账单',
  '在列表中搜索或按状态筛选账单',
  '点击【作废】将无效账单标记为作废',
]
const guideFieldNotes = [
  '账单号：系统自动生成的唯一编号',
  '账期：账单对应的计费周期起止日期',
  '小计金额：套餐基础费用',
  '附加金额：超出套餐的额外费用',
  '折扣金额：优惠减免的金额',
  '总金额：小计 + 附加 - 折扣的最终应付金额',
]
const guideErrorNotes = [
  '已支付的账单不可作废',
  '作废账单后不可恢复',
]

onMounted(loadData)
</script>
