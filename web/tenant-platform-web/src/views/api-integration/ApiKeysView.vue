<template>
  <div>
    <div class="page-header">
      <h2>API 密钥管理</h2>
      <div class="page-header-actions">
        <PageHelpEntry @click="showGuide = true" />
        <DxButton
          v-if="perm.has(INFRA_APIKEY_CREATE)"
          text="创建密钥"
          icon="add"
          type="default"
          @click="showCreatePopup = true"
        />
      </div>
    </div>

    <FunctionDescriptionCard
      purpose="API密钥管理用于为租户创建和管理API访问凭证。密钥创建后将显示Secret Key，请立即保存，之后不再展示。"
      data-scope="全平台所有租户的 API 密钥数据。"
      permission-note="需要 infra:apikey:view 权限查看列表，infra:apikey:create 创建密钥，infra:apikey:disable 禁用密钥。"
      risk-note="禁用密钥后，使用该密钥的所有 API 调用将立即失效。"
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
        <DxColumn data-field="keyName" caption="密钥名称" />
        <DxColumn data-field="accessKey" caption="Access Key" />
        <DxColumn data-field="status" caption="状态" cell-template="statusCell" :width="100" />
        <DxColumn data-field="quotaLimit" caption="配额上限" :width="100" />
        <DxColumn data-field="rateLimit" caption="速率限制" :width="100" />
        <DxColumn data-field="lastUsedAt" caption="最后使用" cell-template="dateCell" />
        <DxColumn data-field="expiresAt" caption="过期时间" cell-template="dateCell" />
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
            v-if="cellData.data.status === 'Active' && perm.has(INFRA_APIKEY_DISABLE)"
            text="禁用"
            styling-mode="text"
            type="danger"
            @click="onDisable(cellData.data.id)"
          />
        </template>
        <DxPaging :page-size="20" />
        <DxPager :show-page-size-selector="true" :allowed-page-sizes="[10, 20, 50]" :show-info="true" />
      </DxDataGrid>
    </div>

    <!-- 创建密钥弹窗 -->
    <DxPopup
      :visible="showCreatePopup"
      title="创建 API 密钥"
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
        <DxSimpleItem data-field="keyName">
          <DxLabel text="密钥名称" />
        </DxSimpleItem>
        <DxSimpleItem data-field="expiresAt" editor-type="dxDateBox"
          :editor-options="{ type: 'datetime', displayFormat: 'yyyy-MM-dd HH:mm' }">
          <DxLabel text="过期时间" />
        </DxSimpleItem>
        <DxButtonItem>
          <DxButtonOptions text="提交" type="default" :use-submit-behavior="false" @click="handleCreate" />
        </DxButtonItem>
      </DxForm>
    </DxPopup>

    <!-- 密钥创建成功弹窗 -->
    <DxPopup
      :visible="showSecretPopup"
      title="密钥创建成功"
      :width="520"
      :height="'auto'"
      :show-close-button="true"
      @hiding="showSecretPopup = false"
    >
      <div style="padding: 16px;">
        <div style="background: #fff7e6; border: 1px solid #ffd591; border-radius: 4px; padding: 12px; margin-bottom: 16px; color: #d46b08;">
          ⚠️ <strong>请立即复制并妥善保存 Secret Key！</strong>此密钥仅在创建时显示一次，关闭后将无法再次查看。
        </div>
        <p><strong>Access Key：</strong>{{ createdResult.accessKey }}</p>
        <p><strong>Secret Key：</strong>{{ createdResult.secretKey }}</p>
      </div>
    </DxPopup>

    <OperationGuideDrawer
      v-model:visible="showGuide"
      title="API 密钥管理操作指引"
      entry-path="左侧菜单 → API 集成管理 → API 密钥"
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
  getApiKeys,
  createApiKey,
  disableApiKey,
  type TenantApiKeyDto,
  type CreateApiKeyRequest,
  type ApiKeyCreatedResult,
} from '@/api/apiIntegration'
import {
  INFRA_APIKEY_CREATE,
  INFRA_APIKEY_DISABLE,
} from '@/constants/permissions'

const perm = usePermission()
const showGuide = ref(false)
const showCreatePopup = ref(false)
const showSecretPopup = ref(false)

const gridData = ref<TenantApiKeyDto[]>([])

const createForm = reactive<CreateApiKeyRequest>({
  tenantRefId: 0,
  keyName: '',
  expiresAt: '',
})

const createdResult = reactive<ApiKeyCreatedResult>({
  id: 0,
  accessKey: '',
  secretKey: '',
})

async function loadData() {
  try {
    const res = await getApiKeys({ page: 1, pageSize: 20 })
    gridData.value = res.data.items
  } catch {
    // 接口未就绪时保持空列表
  }
}

async function handleCreate() {
  try {
    const res = await createApiKey(createForm)
    showCreatePopup.value = false
    Object.assign(createForm, { tenantRefId: 0, keyName: '', expiresAt: '' })
    Object.assign(createdResult, res.data)
    showSecretPopup.value = true
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

async function onDisable(id: number) {
  try {
    await disableApiKey(id)
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

const guideSteps = [
  '点击【创建密钥】按钮，填写租户ID、密钥名称和过期时间',
  '创建成功后，立即复制并保存弹窗中显示的 Secret Key',
  '在列表中查看密钥状态和使用情况',
  '点击【禁用】停止密钥的 API 访问权限',
]
const guideFieldNotes = [
  '租户ID：目标租户的系统ID',
  '密钥名称：用于标识密钥用途，建议使用有意义的名称',
  '过期时间：密钥到期后将自动失效',
  'Secret Key：仅在创建时显示一次，请务必立即保存',
]
const guideErrorNotes = [
  '创建后未保存 Secret Key 将无法找回，需重新创建',
  '禁用密钥后使用该密钥的服务将立即不可用',
]

onMounted(loadData)
</script>
