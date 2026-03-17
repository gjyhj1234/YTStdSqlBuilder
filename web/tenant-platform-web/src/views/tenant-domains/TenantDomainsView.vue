<template>
  <div>
    <div class="page-header">
      <h2>租户域名管理</h2>
      <div class="page-header-actions">
        <PageHelpEntry @click="showGuide = true" />
        <DxButton
          v-if="perm.has(TENANT_DOMAIN_CREATE)"
          text="新增域名"
          icon="add"
          type="default"
          @click="showCreatePopup = true"
        />
      </div>
    </div>

    <FunctionDescriptionCard
      purpose="域名管理用于为租户配置自定义域名或别名域名，支持域名验证状态跟踪。每个租户可拥有多个域名。"
      data-scope="全平台所有租户域名数据。"
      permission-note="需要 tenant:domain:view 权限查看列表，tenant:domain:create 创建域名。"
      risk-note="删除已验证的主域名将导致租户无法通过该域名访问，请谨慎操作。"
      :collapsible="true"
    />

    <div class="card">
      <div class="filter-bar">
        <DxTextBox
          v-model:value="filterKeyword"
          placeholder="搜索域名"
          :width="260"
          mode="search"
          value-change-event="input"
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
        <DxColumn data-field="domain" caption="域名" />
        <DxColumn data-field="domainType" caption="域名类型" :width="100" />
        <DxColumn data-field="isPrimary" caption="主域名" cell-template="booleanCell" :width="80" />
        <DxColumn data-field="verificationStatus" caption="验证状态" cell-template="statusCell" :width="100" />
        <DxColumn data-field="createdAt" caption="创建时间" cell-template="dateCell" />
        <DxColumn caption="操作" cell-template="actionCell" :width="160" />
        <template #booleanCell="{ data: cellData }">
          <span>{{ cellData.value ? '是' : '否' }}</span>
        </template>
        <template #statusCell="{ data: cellData }">
          <StatusTag :status="cellData.value" />
        </template>
        <template #dateCell="{ data: cellData }">
          <span>{{ formatDateTime(cellData.value) }}</span>
        </template>
        <template #actionCell="{ data: cellData }">
          <DxButton
            v-if="perm.has(TENANT_DOMAIN_UPDATE)"
            text="编辑"
            styling-mode="text"
            @click="onEdit(cellData.data)"
          />
          <DxButton
            v-if="perm.has(TENANT_DOMAIN_VERIFY) && cellData.data.verificationStatus !== 'Verified'"
            text="验证"
            styling-mode="text"
            type="success"
            @click="onVerify(cellData.data.id)"
          />
          <DxButton
            v-if="perm.has(TENANT_DOMAIN_DELETE)"
            text="删除"
            styling-mode="text"
            type="danger"
            @click="onDelete(cellData.data.id)"
          />
        </template>
        <DxPaging :page-size="20" />
        <DxPager :show-page-size-selector="true" :allowed-page-sizes="[10, 20, 50]" :show-info="true" />
      </DxDataGrid>
    </div>

    <!-- 新增域名弹窗 -->
    <DxPopup
      :visible="showCreatePopup"
      title="新增租户域名"
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
        <DxSimpleItem data-field="domain">
          <DxLabel text="域名" />
        </DxSimpleItem>
        <DxSimpleItem data-field="domainType" editor-type="dxSelectBox"
          :editor-options="{ items: domainTypes, displayExpr: 'text', valueExpr: 'value' }">
          <DxLabel text="域名类型" />
        </DxSimpleItem>
        <DxButtonItem>
          <DxButtonOptions text="提交" type="default" :use-submit-behavior="false" @click="handleCreate" />
        </DxButtonItem>
      </DxForm>
    </DxPopup>

    <OperationGuideDrawer
      v-model:visible="showGuide"
      title="域名管理操作指引"
      entry-path="左侧菜单 → 租户信息体系 → 域名管理"
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
import { DxPopup } from 'devextreme-vue/popup'
import { DxForm, DxSimpleItem, DxLabel, DxButtonItem, DxButtonOptions } from 'devextreme-vue/form'
import StatusTag from '@/components/StatusTag.vue'
import FunctionDescriptionCard from '@/components/help/FunctionDescriptionCard.vue'
import OperationGuideDrawer from '@/components/help/OperationGuideDrawer.vue'
import PageHelpEntry from '@/components/help/PageHelpEntry.vue'
import { usePermission } from '@/composables/usePermission'
import { formatDateTime } from '@/utils/format'
import {
  getTenantDomains,
  createTenantDomain,
  type TenantDomainDto,
  type CreateTenantDomainRequest,
} from '@/api/tenantDomains'
import {
  TENANT_DOMAIN_CREATE,
  TENANT_DOMAIN_UPDATE,
  TENANT_DOMAIN_DELETE,
  TENANT_DOMAIN_VERIFY,
} from '@/constants/permissions'

const perm = usePermission()
const showGuide = ref(false)
const showCreatePopup = ref(false)
const filterKeyword = ref('')

const domainTypes = [
  { text: '主域名', value: 'Primary' },
  { text: '别名', value: 'Alias' },
]

const gridData = ref<TenantDomainDto[]>([])

const createForm = reactive<CreateTenantDomainRequest>({
  tenantRefId: 0,
  domain: '',
  domainType: 'Primary',
})

async function loadData() {
  try {
    const res = await getTenantDomains({
      page: 1,
      pageSize: 20,
      keyword: filterKeyword.value || undefined,
    })
    gridData.value = res.data.items
  } catch {
    // 接口未就绪时保持空列表
  }
}

async function handleCreate() {
  try {
    await createTenantDomain(createForm)
    showCreatePopup.value = false
    Object.assign(createForm, { tenantRefId: 0, domain: '', domainType: 'Primary' })
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

function onEdit(_domain: TenantDomainDto) {
  // 后续阶段完善编辑功能
}

function onVerify(_id: number) {
  // 后续阶段完善验证功能
}

function onDelete(_id: number) {
  // 后续阶段完善删除功能
}

const guideSteps = [
  '点击【新增域名】按钮为租户添加域名',
  '填写租户ID、域名地址和域名类型',
  '创建后域名状态为"待验证"',
  '点击【验证】按钮完成域名所有权验证',
]
const guideFieldNotes = [
  '域名类型：Primary 为主域名，Alias 为别名域名',
  '每个租户只能有一个主域名，但可以有多个别名域名',
  '域名验证需要在 DNS 中添加指定的 TXT 记录',
]
const guideErrorNotes = [
  '域名已被其他租户使用时创建将失败',
  '未通过验证的域名无法作为主域名使用',
]

onMounted(loadData)
</script>
