<template>
  <div>
    <div class="page-header">
      <h2>{{ $t('租户管理') }}</h2>
      <div class="page-header-actions">
        <PageHelpEntry @click="showGuide = true" />
        <DxButton
          v-if="perm.has(TENANT_LIST_CREATE)"
          :text="$t('新增租户')"
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
          :placeholder="$t('搜索租户编码 / 名称 / 企业名')"
          :width="280"
          mode="search"
          value-change-event="input"
        />
        <DxSelectBox
          v-model:value="filterStatus"
          :items="statusOptions"
          display-expr="text"
          value-expr="value"
          :placeholder="$t('状态筛选')"
          :width="140"
          :show-clear-button="true"
        />
        <DxButton :text="$t('查询')" icon="search" @click="loadData" />
      </div>

      <DxDataGrid
        :data-source="gridData"
        :show-borders="true"
        :column-auto-width="true"
        :hover-state-enabled="true"
        key-expr="Id"
      >
        <DxColumn data-field="Id" :caption="$t('common.id')" :width="60" />
        <DxColumn data-field="TenantCode" :caption="$t('common.tenantCode')" />
        <DxColumn data-field="TenantName" :caption="$t('common.tenantName')" />
        <DxColumn data-field="EnterpriseName" :caption="$t('common.enterpriseName')" />
        <DxColumn data-field="ContactName" :caption="$t('common.contactName')" />
        <DxColumn data-field="ContactEmail" :caption="$t('common.contactEmail')" />
        <DxColumn data-field="LifecycleStatus" :caption="$t('common.status')" cell-template="statusCell" :width="100" />
        <DxColumn data-field="IsolationMode" :caption="$t('common.isolationMode')" :width="120" />
        <DxColumn data-field="OpenedAt" :caption="$t('common.openedAt')" cell-template="dateCell" />
        <DxColumn data-field="ExpiresAt" :caption="$t('common.expiresAt')" cell-template="dateCell" />
        <DxColumn :caption="$t('common.actions')" cell-template="actionCell" :width="200" />
        <template #statusCell="{ data: cellData }">
          <StatusTag :status="cellData.value" />
        </template>
        <template #dateCell="{ data: cellData }">
          <span>{{ formatDateTime(cellData.value) }}</span>
        </template>
        <template #actionCell="{ data: cellData }">
          <DxButton
            v-if="perm.has(TENANT_LIST_UPDATE)"
            :text="$t('编辑')"
            styling-mode="text"
            @click="onEdit(cellData.data)"
          />
          <DxButton
            v-if="cellData.data.LifecycleStatus === 'Trial' && perm.has(TENANT_LIST_ACTIVATE)"
            :text="$t('激活')"
            styling-mode="text"
            type="success"
            @click="onStatusChange(cellData.data.Id, 'Active', '激活租户')"
          />
          <DxButton
            v-if="cellData.data.LifecycleStatus === 'Active' && perm.has(TENANT_LIST_SUSPEND)"
            :text="$t('暂停')"
            styling-mode="text"
            type="danger"
            @click="onStatusChange(cellData.data.Id, 'Suspended', '暂停租户服务')"
          />
          <DxButton
            v-if="perm.has(TENANT_LIST_CLOSE) && !['Closed'].includes(cellData.data.LifecycleStatus)"
            :text="$t('关闭')"
            styling-mode="text"
            type="danger"
            @click="onStatusChange(cellData.data.Id, 'Closed', '关闭租户')"
          />
        </template>
        <DxPaging :page-size="20" />
        <DxPager :show-page-size-selector="true" :allowed-page-sizes="[10, 20, 50]" :show-info="true" />
      </DxDataGrid>
    </div>

    <!-- 新增租户弹窗 -->
    <DxPopup
      :visible="showCreatePopup"
      :title="$t('新增租户')"
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
        <DxSimpleItem data-field="TenantCode">
          <DxLabel :text="$t('租户编码')" />
        </DxSimpleItem>
        <DxSimpleItem data-field="TenantName">
          <DxLabel :text="$t('租户名称')" />
        </DxSimpleItem>
        <DxSimpleItem data-field="EnterpriseName" :col-span="2">
          <DxLabel :text="$t('企业名称')" />
        </DxSimpleItem>
        <DxSimpleItem data-field="ContactName">
          <DxLabel :text="$t('联系人')" />
        </DxSimpleItem>
        <DxSimpleItem data-field="ContactPhone">
          <DxLabel :text="$t('联系电话')" />
        </DxSimpleItem>
        <DxSimpleItem data-field="ContactEmail" :col-span="2">
          <DxLabel :text="$t('联系邮箱')" />
        </DxSimpleItem>
        <DxSimpleItem data-field="SourceType" editor-type="dxSelectBox"
          :editor-options="{ items: sourceTypes, displayExpr: 'text', valueExpr: 'value' }">
          <DxLabel :text="$t('来源')" />
        </DxSimpleItem>
        <DxSimpleItem data-field="IsolationMode" editor-type="dxSelectBox"
          :editor-options="{ items: isolationModes, displayExpr: 'text', valueExpr: 'value' }">
          <DxLabel :text="$t('隔离模式')" />
        </DxSimpleItem>
        <DxButtonItem :col-span="2">
          <DxButtonOptions :text="$t('提交')" type="default" :use-submit-behavior="false" @click="handleCreate" />
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
import { ref, reactive, onMounted, computed } from 'vue'
import { DxDataGrid, DxColumn, DxPaging, DxPager } from 'devextreme-vue/data-grid'
import { DxButton } from 'devextreme-vue/button'
import { DxTextBox } from 'devextreme-vue/text-box'
import { DxSelectBox } from 'devextreme-vue/select-box'
import { DxPopup } from 'devextreme-vue/popup'
import { DxForm, DxSimpleItem, DxLabel, DxButtonItem, DxButtonOptions } from 'devextreme-vue/form'
import { useI18n } from 'vue-i18n'
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
  type TenantRepDTO,
  type CreateTenantReqDTO,
} from '@/api/tenants'
import {
  TENANT_LIST_CREATE,
  TENANT_LIST_UPDATE,
  TENANT_LIST_ACTIVATE,
  TENANT_LIST_SUSPEND,
  TENANT_LIST_CLOSE,
} from '@/constants/permissions'

const perm = usePermission()
const { t } = useI18n()
const showGuide = ref(false)
const showCreatePopup = ref(false)
const filterKeyword = ref('')
const filterStatus = ref<string | undefined>(undefined)

const statusOptions = computed(() => [
  { text: t('status.Trial'), value: 'Trial' },
  { text: t('status.Active'), value: 'Active' },
  { text: t('status.Expiring'), value: 'Expiring' },
  { text: t('status.Expired'), value: 'Expired' },
  { text: t('status.Suspended'), value: 'Suspended' },
  { text: t('status.Closed'), value: 'Closed' },
])

const sourceTypes = computed(() => [
  { text: t('enum.sourceType.SelfService'), value: 'SelfService' },
  { text: t('enum.sourceType.Admin'), value: 'Admin' },
  { text: t('enum.sourceType.Api'), value: 'Api' },
])

const isolationModes = computed(() => [
  { text: t('enum.isolationMode.SharedDatabase'), value: 'SharedDatabase' },
  { text: t('enum.isolationMode.SchemaIsolated'), value: 'SchemaIsolated' },
  { text: t('enum.isolationMode.DatabaseIsolated'), value: 'DatabaseIsolated' },
  { text: t('enum.isolationMode.Hybrid'), value: 'Hybrid' },
])

const gridData = ref<TenantRepDTO[]>([])

const createForm = reactive<CreateTenantReqDTO>({
  TenantCode: '',
  TenantName: '',
  EnterpriseName: '',
  ContactName: '',
  ContactPhone: '',
  ContactEmail: '',
  SourceType: 'Admin',
  IsolationMode: 'SharedDatabase',
})

async function loadData() {
  try {
    const res = await getTenants({
      Page: 1,
      PageSize: 20,
      Keyword: filterKeyword.value || undefined,
      Status: filterStatus.value || undefined,
    })
    gridData.value = res.data!.items
  } catch {
    // 接口未就绪时保持空列表
  }
}

async function handleCreate() {
  try {
    await createTenant(createForm)
    showCreatePopup.value = false
    Object.assign(createForm, {
      TenantCode: '', TenantName: '', EnterpriseName: '',
      ContactName: '', ContactPhone: '', ContactEmail: '',
      SourceType: 'Admin', IsolationMode: 'SharedDatabase',
    })
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

function onEdit(_tenant: TenantRepDTO) {
  // 后续阶段完善编辑功能
}

async function onStatusChange(id: number, targetStatus: string, reason: string) {
  try {
    await changeTenantStatus(id, { TargetStatus: targetStatus, Reason: reason })
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
