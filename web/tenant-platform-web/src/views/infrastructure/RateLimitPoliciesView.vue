<template>
  <div>
    <div class="page-header">
      <h2>限流策略</h2>
      <div class="page-header-actions">
        <PageHelpEntry @click="showGuide = true" />
        <DxButton
          v-if="perm.has(INFRA_RATELIMIT_CREATE)"
          text="新增策略"
          icon="add"
          type="default"
          @click="showCreatePopup = true"
        />
      </div>
    </div>

    <FunctionDescriptionCard
      purpose="限流策略用于管理 API 请求的限流规则，保护系统免受过量请求的影响。可按主体类型（租户、用户、API Key 等）配置独立的限流窗口和上限。"
      data-scope="全平台限流策略配置数据。"
      permission-note="需要 infra:ratelimit:view 权限查看列表，infra:ratelimit:create 创建策略，infra:ratelimit:update 编辑策略。"
      risk-note="修改限流策略后立即生效，可能影响在线租户的 API 调用。"
      :collapsible="true"
    />

    <div class="card">
      <div class="filter-bar">
        <DxTextBox
          v-model:value="filterKeyword"
          placeholder="搜索主体类型 / 主体标识"
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
        <DxColumn data-field="subjectType" caption="主体类型" :width="120" />
        <DxColumn data-field="subjectKey" caption="主体标识" :width="180" />
        <DxColumn data-field="windowSeconds" caption="窗口秒数" :width="100" />
        <DxColumn data-field="limitCount" caption="限制次数" :width="100" />
        <DxColumn data-field="burstLimit" caption="突发限制" :width="100" />
        <DxColumn data-field="status" caption="状态" cell-template="statusCell" :width="100" />
        <DxColumn data-field="createdAt" caption="创建时间" cell-template="dateTimeCell" :width="160" />
        <DxColumn data-field="updatedAt" caption="更新时间" cell-template="dateTimeCell" :width="160" />
        <DxColumn caption="操作" cell-template="actionCell" :width="120" />
        <template #statusCell="{ data: cellData }">
          <StatusTag :status="cellData.value" />
        </template>
        <template #dateTimeCell="{ data: cellData }">
          <span>{{ formatDateTime(cellData.value) }}</span>
        </template>
        <template #actionCell="{ data: cellData }">
          <DxButton
            v-if="perm.has(INFRA_RATELIMIT_UPDATE)"
            text="编辑"
            styling-mode="text"
            type="default"
            @click="openEdit(cellData.data)"
          />
          <DxButton
            v-if="perm.has(INFRA_RATELIMIT_DELETE)"
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

    <!-- 新增策略弹窗 -->
    <DxPopup
      :visible="showCreatePopup"
      title="新增限流策略"
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
        <DxSimpleItem data-field="subjectType" editor-type="dxSelectBox"
          :editor-options="{ items: subjectTypes, displayExpr: 'text', valueExpr: 'value' }">
          <DxLabel text="主体类型" />
        </DxSimpleItem>
        <DxSimpleItem data-field="subjectKey" editor-type="dxTextBox">
          <DxLabel text="主体标识" />
        </DxSimpleItem>
        <DxSimpleItem data-field="windowSeconds" editor-type="dxNumberBox" :editor-options="{ min: 1 }">
          <DxLabel text="窗口秒数" />
        </DxSimpleItem>
        <DxSimpleItem data-field="limitCount" editor-type="dxNumberBox" :editor-options="{ min: 1 }">
          <DxLabel text="限制次数" />
        </DxSimpleItem>
        <DxSimpleItem data-field="burstLimit" editor-type="dxNumberBox" :editor-options="{ min: 0 }">
          <DxLabel text="突发限制" />
        </DxSimpleItem>
        <DxButtonItem>
          <DxButtonOptions text="提交" type="default" :use-submit-behavior="false" @click="handleCreate" />
        </DxButtonItem>
      </DxForm>
    </DxPopup>

    <!-- 编辑策略弹窗 -->
    <DxPopup
      :visible="showEditPopup"
      title="编辑限流策略"
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
        <DxSimpleItem data-field="windowSeconds" editor-type="dxNumberBox" :editor-options="{ min: 1 }">
          <DxLabel text="窗口秒数" />
        </DxSimpleItem>
        <DxSimpleItem data-field="limitCount" editor-type="dxNumberBox" :editor-options="{ min: 1 }">
          <DxLabel text="限制次数" />
        </DxSimpleItem>
        <DxSimpleItem data-field="burstLimit" editor-type="dxNumberBox" :editor-options="{ min: 0 }">
          <DxLabel text="突发限制" />
        </DxSimpleItem>
        <DxButtonItem>
          <DxButtonOptions text="保存" type="default" :use-submit-behavior="false" @click="handleEdit" />
        </DxButtonItem>
      </DxForm>
    </DxPopup>

    <OperationGuideDrawer
      v-model:visible="showGuide"
      title="限流策略操作指引"
      entry-path="左侧菜单 → 技术基础设施 → 限流策略"
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
import { get, post, put, del, type PagedResult } from '@/utils/http'
import {
  INFRA_RATELIMIT_CREATE,
  INFRA_RATELIMIT_UPDATE,
  INFRA_RATELIMIT_DELETE,
} from '@/constants/permissions'

/* ---------- 类型 ---------- */
interface RateLimitPolicyDto {
  id: number
  subjectType: string
  subjectKey: string
  windowSeconds: number
  limitCount: number
  burstLimit: number | null
  status: string
  createdAt: string
  updatedAt: string
}

interface CreateRateLimitPolicyRequest {
  subjectType: string
  subjectKey: string
  windowSeconds: number
  limitCount: number
  burstLimit: number | null
}

interface UpdateRateLimitPolicyRequest {
  windowSeconds: number
  limitCount: number
  burstLimit: number | null
}

const perm = usePermission()
const showGuide = ref(false)
const showCreatePopup = ref(false)
const showEditPopup = ref(false)
const filterKeyword = ref('')
const filterStatus = ref<string | undefined>(undefined)
const editingId = ref(0)

const statusOptions = [
  { text: '正常', value: 'Active' },
  { text: '已禁用', value: 'Disabled' },
]

const subjectTypes = [
  { text: '租户', value: 'Tenant' },
  { text: '用户', value: 'User' },
  { text: 'API Key', value: 'ApiKey' },
  { text: 'IP', value: 'Ip' },
  { text: '全局', value: 'Global' },
]

const gridData = ref<RateLimitPolicyDto[]>([])

const createForm = reactive<CreateRateLimitPolicyRequest>({
  subjectType: 'Tenant',
  subjectKey: '',
  windowSeconds: 60,
  limitCount: 100,
  burstLimit: null,
})

const editForm = reactive<UpdateRateLimitPolicyRequest>({
  windowSeconds: 60,
  limitCount: 100,
  burstLimit: null,
})

async function loadData() {
  try {
    const res = await get<PagedResult<RateLimitPolicyDto>>('/api/rate-limit-policies', {
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
    await post('/api/rate-limit-policies', createForm)
    showCreatePopup.value = false
    Object.assign(createForm, { subjectType: 'Tenant', subjectKey: '', windowSeconds: 60, limitCount: 100, burstLimit: null })
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

function openEdit(row: RateLimitPolicyDto) {
  editingId.value = row.id
  editForm.windowSeconds = row.windowSeconds
  editForm.limitCount = row.limitCount
  editForm.burstLimit = row.burstLimit
  showEditPopup.value = true
}

async function handleEdit() {
  try {
    await put(`/api/rate-limit-policies/${editingId.value}`, editForm)
    showEditPopup.value = false
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

async function onDelete(id: number) {
  try {
    await del(`/api/rate-limit-policies/${id}`)
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

const guideSteps = [
  '点击【新增策略】按钮创建新的限流规则',
  '在列表中搜索或按状态筛选限流策略',
  '点击【编辑】修改限流参数',
  '点击【删除】移除不再使用的策略',
]
const guideFieldNotes = [
  '主体类型：Tenant（按租户限流）、User（按用户）、ApiKey（按 API Key）、Ip（按 IP）、Global（全局）',
  '主体标识：具体的主体 ID 或通配符 *（代表全部）',
  '窗口秒数：限流统计的时间窗口大小（秒）',
  '限制次数：时间窗口内允许的最大请求次数',
  '突发限制：允许的突发请求上限，为空则不限制突发',
]
const guideErrorNotes = [
  '同一主体类型和主体标识的组合不能重复',
  '修改限流策略后立即生效，请注意对在线租户的影响',
]

onMounted(loadData)
</script>
