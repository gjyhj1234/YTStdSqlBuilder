<template>
  <div>
    <div class="page-header">
      <h2>{{ $t('平台角色管理') }}</h2>
      <div class="page-header-actions">
        <PageHelpEntry @click="showGuide = true" />
        <DxButton
          v-if="perm.has(PLATFORM_ROLE_CREATE)"
          :text="$t('新增角色')"
          icon="add"
          type="default"
          @click="showCreatePopup = true"
        />
      </div>
    </div>

    <FunctionDescriptionCard
      purpose="平台角色管理用于定义不同管理角色及其权限范围，支持角色的创建、编辑、启禁用以及权限和成员绑定。"
      data-scope="仅限平台级别角色，非租户角色。"
      permission-note="需要 platform:role:view 权限查看列表，platform:role:create 创建角色。"
      risk-note="禁用角色将导致绑定该角色的用户失去对应权限。"
      :collapsible="true"
    />

    <div class="card">
      <div class="filter-bar">
        <DxTextBox
          v-model:value="filterKeyword"
          :placeholder="$t('搜索角色编码 / 名称')"
          :width="260"
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
        key-expr="id"
      >
        <DxColumn data-field="id" :caption="$t('common.id')" :width="60" />
        <DxColumn data-field="code" :caption="$t('角色编码')" />
        <DxColumn data-field="name" :caption="$t('角色名称')" />
        <DxColumn data-field="description" :caption="$t('common.description')" />
        <DxColumn data-field="status" :caption="$t('common.status')" cell-template="statusCell" :width="100" />
        <DxColumn data-field="createdAt" :caption="$t('common.createdAt')" cell-template="dateCell" />
        <DxColumn :caption="$t('common.actions')" cell-template="actionCell" :width="280" />
        <template #statusCell="{ data: cellData }">
          <StatusTag :status="cellData.value" />
        </template>
        <template #dateCell="{ data: cellData }">
          <span>{{ formatDateTime(cellData.value) }}</span>
        </template>
        <template #actionCell="{ data: cellData }">
          <DxButton
            v-if="perm.has(PLATFORM_ROLE_UPDATE)"
            :text="$t('编辑')"
            styling-mode="text"
            @click="onEdit(cellData.data)"
          />
          <DxButton
            v-if="cellData.data.status === 'Active' && perm.has(PLATFORM_ROLE_DISABLE)"
            :text="$t('common.disable')"
            styling-mode="text"
            type="danger"
            @click="onDisable(cellData.data.id)"
          />
          <DxButton
            v-if="cellData.data.status !== 'Active' && perm.has(PLATFORM_ROLE_ENABLE)"
            :text="$t('common.enable')"
            styling-mode="text"
            type="success"
            @click="onEnable(cellData.data.id)"
          />
          <DxButton
            v-if="perm.has(PLATFORM_ROLE_ASSIGN_PERMISSION)"
            :text="$t('绑定权限')"
            styling-mode="text"
            @click="onBindPermissions(cellData.data)"
          />
          <DxButton
            v-if="perm.has(PLATFORM_ROLE_ASSIGN_MEMBER)"
            :text="$t('绑定成员')"
            styling-mode="text"
            @click="onBindMembers(cellData.data)"
          />
        </template>
        <DxPaging :page-size="20" />
        <DxPager :show-page-size-selector="true" :allowed-page-sizes="[10, 20, 50]" :show-info="true" />
      </DxDataGrid>
    </div>

    <!-- 新增角色弹窗 -->
    <DxPopup
      :visible="showCreatePopup"
      :title="$t('新增平台角色')"
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
        <DxSimpleItem data-field="code">
          <DxLabel :text="$t('角色编码')" />
        </DxSimpleItem>
        <DxSimpleItem data-field="name">
          <DxLabel :text="$t('角色名称')" />
        </DxSimpleItem>
        <DxSimpleItem data-field="description" editor-type="dxTextArea">
          <DxLabel :text="$t('common.description')" />
        </DxSimpleItem>
        <DxButtonItem>
          <DxButtonOptions :text="$t('提交')" type="default" :use-submit-behavior="false" @click="handleCreate" />
        </DxButtonItem>
      </DxForm>
    </DxPopup>

    <OperationGuideDrawer
      v-model:visible="showGuide"
      title="角色管理操作指引"
      entry-path="左侧菜单 → 平台管理体系 → 角色管理"
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
  getPlatformRoles,
  createPlatformRole,
  enablePlatformRole,
  disablePlatformRole,
  type PlatformRoleDto,
  type CreatePlatformRoleRequest,
} from '@/api/platformRoles'
import {
  PLATFORM_ROLE_CREATE,
  PLATFORM_ROLE_UPDATE,
  PLATFORM_ROLE_ENABLE,
  PLATFORM_ROLE_DISABLE,
  PLATFORM_ROLE_ASSIGN_PERMISSION,
  PLATFORM_ROLE_ASSIGN_MEMBER,
} from '@/constants/permissions'

const perm = usePermission()
const { t } = useI18n()
const showGuide = ref(false)
const showCreatePopup = ref(false)
const filterKeyword = ref('')
const filterStatus = ref<string | undefined>(undefined)

const statusOptions = computed(() => [
  { text: t('status.Active'), value: 'Active' },
  { text: t('status.Disabled'), value: 'Disabled' },
])

const gridData = ref<PlatformRoleDto[]>([])

const createForm = reactive<CreatePlatformRoleRequest>({
  code: '',
  name: '',
  description: '',
})

async function loadData() {
  try {
    const res = await getPlatformRoles({
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
    await createPlatformRole(createForm)
    showCreatePopup.value = false
    Object.assign(createForm, { code: '', name: '', description: '' })
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

function onEdit(_role: PlatformRoleDto) {
  // 后续阶段完善编辑功能
}

async function onEnable(id: number) {
  try {
    await enablePlatformRole(id)
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

async function onDisable(id: number) {
  try {
    await disablePlatformRole(id)
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

function onBindPermissions(_role: PlatformRoleDto) {
  // 后续阶段完善权限绑定功能
}

function onBindMembers(_role: PlatformRoleDto) {
  // 后续阶段完善成员绑定功能
}

const guideSteps = [
  '点击【新增角色】按钮填写角色编码、名称和描述',
  '在列表中搜索或按状态筛选角色',
  '点击【编辑】修改角色信息',
  '点击【禁用/启用】切换角色状态',
  '点击【绑定权限】为角色分配权限码',
  '点击【绑定成员】将用户添加到角色',
]
const guideFieldNotes = [
  '角色编码：全局唯一，创建后不可修改',
  '角色名称：用于界面显示的友好名称',
  '描述：角色的职责和权限范围说明',
]
const guideErrorNotes = [
  '角色编码已存在时创建将失败',
  '禁用角色后绑定该角色的用户将失去对应权限',
  '删除角色前需先解除所有权限和成员绑定',
]

onMounted(loadData)
</script>
