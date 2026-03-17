<template>
  <div>
    <div class="page-header">
      <h2>租户管理</h2>
      <div class="page-header-actions">
        <PageHelpEntry @click="showGuide = true" />
        <DxButton
          v-if="perm.has(TENANT_LIST_CREATE)"
          text="新增租户"
          icon="add"
          type="default"
          @click="showCreatePopup = true"
        />
      </div>
    </div>

    <FunctionDescriptionCard
      purpose="管理所有租户的基本信息和生命周期状态，包括开通、暂停、关闭等操作。"
      data-scope="全平台所有租户数据。"
      permission-note="需要 tenant:list:view 权限查看列表，tenant:list:create 创建租户。"
      risk-note="关闭租户将导致该租户下所有服务不可用，请谨慎操作。"
      :collapsible="true"
    />

    <div class="card">
      <div class="filter-bar">
        <DxTextBox
          v-model:value="filterKeyword"
          placeholder="搜索租户编码 / 名称 / 企业名"
          :width="280"
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
        <DxColumn data-field="tenantCode" caption="租户编码" />
        <DxColumn data-field="tenantName" caption="租户名称" />
        <DxColumn data-field="enterpriseName" caption="企业名称" />
        <DxColumn data-field="contactName" caption="联系人" />
        <DxColumn data-field="contactEmail" caption="联系邮箱" />
        <DxColumn data-field="lifecycleStatus" caption="状态" cell-template="statusCell" :width="100" />
        <DxColumn data-field="isolationMode" caption="隔离模式" :width="120" />
        <DxColumn data-field="openedAt" caption="开通时间" cell-template="dateCell" />
        <DxColumn data-field="expiresAt" caption="到期时间" cell-template="dateCell" />
        <DxColumn caption="操作" cell-template="actionCell" :width="200" />
        <template #statusCell="{ data: cellData }">
          <StatusTag :status="cellData.value" />
        </template>
        <template #dateCell="{ data: cellData }">
          <span>{{ formatDateTime(cellData.value) }}</span>
        </template>
        <template #actionCell="{ data: cellData }">
          <DxButton
            v-if="perm.has(TENANT_LIST_UPDATE)"
            text="编辑"
            styling-mode="text"
            @click="onEdit(cellData.data)"
          />
          <DxButton
            v-if="cellData.data.lifecycleStatus === 'Trial' && perm.has(TENANT_LIST_ACTIVATE)"
            text="激活"
            styling-mode="text"
            type="success"
            @click="onStatusChange(cellData.data.id, 'Active', '激活租户')"
          />
          <DxButton
            v-if="cellData.data.lifecycleStatus === 'Active' && perm.has(TENANT_LIST_SUSPEND)"
            text="暂停"
            styling-mode="text"
            type="danger"
            @click="onStatusChange(cellData.data.id, 'Suspended', '暂停租户服务')"
          />
          <DxButton
            v-if="perm.has(TENANT_LIST_CLOSE) && !['Closed'].includes(cellData.data.lifecycleStatus)"
            text="关闭"
            styling-mode="text"
            type="danger"
            @click="onStatusChange(cellData.data.id, 'Closed', '关闭租户')"
          />
        </template>
        <DxPaging :page-size="20" />
        <DxPager :show-page-size-selector="true" :allowed-page-sizes="[10, 20, 50]" :show-info="true" />
      </DxDataGrid>
    </div>

    <!-- 新增租户弹窗 -->
    <DxPopup
      :visible="showCreatePopup"
      title="新增租户"
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
        <DxSimpleItem data-field="tenantCode">
          <DxLabel text="租户编码" />
        </DxSimpleItem>
        <DxSimpleItem data-field="tenantName">
          <DxLabel text="租户名称" />
        </DxSimpleItem>
        <DxSimpleItem data-field="enterpriseName" :col-span="2">
          <DxLabel text="企业名称" />
        </DxSimpleItem>
        <DxSimpleItem data-field="contactName">
          <DxLabel text="联系人" />
        </DxSimpleItem>
        <DxSimpleItem data-field="contactPhone">
          <DxLabel text="联系电话" />
        </DxSimpleItem>
        <DxSimpleItem data-field="contactEmail" :col-span="2">
          <DxLabel text="联系邮箱" />
        </DxSimpleItem>
        <DxSimpleItem data-field="sourceType" editor-type="dxSelectBox"
          :editor-options="{ items: sourceTypes, displayExpr: 'text', valueExpr: 'value' }">
          <DxLabel text="来源" />
        </DxSimpleItem>
        <DxSimpleItem data-field="isolationMode" editor-type="dxSelectBox"
          :editor-options="{ items: isolationModes, displayExpr: 'text', valueExpr: 'value' }">
          <DxLabel text="隔离模式" />
        </DxSimpleItem>
        <DxButtonItem :col-span="2">
          <DxButtonOptions text="提交" type="default" :use-submit-behavior="false" @click="handleCreate" />
        </DxButtonItem>
      </DxForm>
    </DxPopup>

    <OperationGuideDrawer
      v-model:visible="showGuide"
      title="租户管理操作指引"
      entry-path="左侧菜单 → 租户生命周期体系 → 租户管理"
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
  getTenants,
  createTenant,
  changeTenantStatus,
  type TenantDto,
  type CreateTenantRequest,
} from '@/api/tenants'
import {
  TENANT_LIST_CREATE,
  TENANT_LIST_UPDATE,
  TENANT_LIST_ACTIVATE,
  TENANT_LIST_SUSPEND,
  TENANT_LIST_CLOSE,
} from '@/constants/permissions'

const perm = usePermission()
const showGuide = ref(false)
const showCreatePopup = ref(false)
const filterKeyword = ref('')
const filterStatus = ref<string | undefined>(undefined)

const statusOptions = [
  { text: '试用中', value: 'Trial' },
  { text: '正常', value: 'Active' },
  { text: '即将到期', value: 'Expiring' },
  { text: '已过期', value: 'Expired' },
  { text: '已暂停', value: 'Suspended' },
  { text: '已关闭', value: 'Closed' },
]

const sourceTypes = [
  { text: '自助注册', value: 'SelfService' },
  { text: '管理员创建', value: 'Admin' },
  { text: 'API 创建', value: 'Api' },
]

const isolationModes = [
  { text: '共享数据库', value: 'SharedDatabase' },
  { text: 'Schema 隔离', value: 'SchemaIsolated' },
  { text: '独立数据库', value: 'DatabaseIsolated' },
  { text: '混合模式', value: 'Hybrid' },
]

const gridData = ref<TenantDto[]>([])

const createForm = reactive<CreateTenantRequest>({
  tenantCode: '',
  tenantName: '',
  enterpriseName: '',
  contactName: '',
  contactPhone: '',
  contactEmail: '',
  sourceType: 'Admin',
  isolationMode: 'SharedDatabase',
})

async function loadData() {
  try {
    const res = await getTenants({
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
    await createTenant(createForm)
    showCreatePopup.value = false
    Object.assign(createForm, {
      tenantCode: '', tenantName: '', enterpriseName: '',
      contactName: '', contactPhone: '', contactEmail: '',
      sourceType: 'Admin', isolationMode: 'SharedDatabase',
    })
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

function onEdit(_tenant: TenantDto) {
  // 后续阶段完善编辑功能
}

async function onStatusChange(id: number, targetStatus: string, reason: string) {
  try {
    await changeTenantStatus(id, { targetStatus, reason })
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

const guideSteps = [
  '点击【新增租户】按钮填写租户基本信息',
  '在列表中搜索或按状态筛选租户',
  '点击【编辑】修改租户信息',
  '使用【激活/暂停/关闭】按钮管理租户生命周期',
]
const guideFieldNotes = [
  '租户编码：全局唯一，创建后不可修改',
  '隔离模式：决定数据隔离方式，创建后通常不可变更',
  '联系邮箱：用于接收系统通知和账单',
]
const guideErrorNotes = [
  '租户编码重复时创建将失败',
  '关闭租户后数据将被保留但服务不可用',
]

onMounted(loadData)
</script>
