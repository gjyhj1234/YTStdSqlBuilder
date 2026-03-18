<template>
  <div>
    <div class="page-header">
      <h2>{{ $t('平台用户管理') }}</h2>
      <div class="page-header-actions">
        <PageHelpEntry @click="showGuide = true" />
        <DxButton
          v-if="perm.has(PLATFORM_USER_CREATE)"
          :text="$t('新增用户')"
          icon="add"
          type="default"
          @click="showCreatePopup = true"
        />
      </div>
    </div>

    <FunctionDescriptionCard
      purpose="管理平台管理员账号，包括创建、编辑、启用/禁用、重置密码等操作。"
      data-scope="仅限平台管理员，非租户用户。"
      permission-note="需要 platform:user:view 权限查看列表，platform:user:create 创建用户。"
      risk-note="禁用用户将导致该用户无法登录平台。"
      :collapsible="true"
    />

    <div class="card">
      <div class="filter-bar">
        <DxTextBox
          v-model:value="filterKeyword"
          :placeholder="$t('搜索用户名 / 邮箱 / 姓名')"
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
        <DxColumn data-field="username" :caption="$t('common.username')" />
        <DxColumn data-field="displayName" :caption="$t('common.displayName')" />
        <DxColumn data-field="email" :caption="$t('common.email')" />
        <DxColumn data-field="phone" :caption="$t('common.phone')" />
        <DxColumn data-field="status" :caption="$t('common.status')" cell-template="statusCell" :width="100" />
        <DxColumn data-field="lastLoginAt" :caption="$t('common.lastLoginAt')" cell-template="dateCell" />
        <DxColumn data-field="createdAt" :caption="$t('common.createdAt')" cell-template="dateCell" />
        <DxColumn :caption="$t('common.actions')" cell-template="actionCell" :width="160" />
        <template #statusCell="{ data: cellData }">
          <StatusTag :status="cellData.value" />
        </template>
        <template #dateCell="{ data: cellData }">
          <span>{{ formatDateTime(cellData.value) }}</span>
        </template>
        <template #actionCell="{ data: cellData }">
          <DxButton
            v-if="perm.has(PLATFORM_USER_UPDATE)"
            :text="$t('编辑')"
            styling-mode="text"
            @click="onEdit(cellData.data)"
          />
          <DxButton
            v-if="cellData.data.status === 'Active' && perm.has(PLATFORM_USER_LOCK)"
            :text="$t('common.disable')"
            styling-mode="text"
            type="danger"
            @click="onDisable(cellData.data.id)"
          />
          <DxButton
            v-if="cellData.data.status !== 'Active' && perm.has(PLATFORM_USER_UNLOCK)"
            :text="$t('common.enable')"
            styling-mode="text"
            type="success"
            @click="onEnable(cellData.data.id)"
          />
        </template>
        <DxPaging :page-size="20" />
        <DxPager :show-page-size-selector="true" :allowed-page-sizes="[10, 20, 50]" :show-info="true" />
      </DxDataGrid>
    </div>

    <!-- 新增用户弹窗 -->
    <DxPopup
      :visible="showCreatePopup"
      :title="$t('新增平台用户')"
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
        <DxSimpleItem data-field="username">
          <DxLabel :text="$t('用户名')" />
        </DxSimpleItem>
        <DxSimpleItem data-field="displayName">
          <DxLabel :text="$t('显示名称')" />
        </DxSimpleItem>
        <DxSimpleItem data-field="email">
          <DxLabel :text="$t('邮箱')" />
        </DxSimpleItem>
        <DxSimpleItem data-field="phone">
          <DxLabel :text="$t('手机号')" />
        </DxSimpleItem>
        <DxSimpleItem data-field="password" :editor-options="{ mode: 'password' }">
          <DxLabel :text="$t('密码')" />
        </DxSimpleItem>
        <DxSimpleItem data-field="remark" editor-type="dxTextArea">
          <DxLabel :text="$t('备注')" />
        </DxSimpleItem>
        <DxButtonItem>
          <DxButtonOptions :text="$t('提交')" type="default" :use-submit-behavior="false" @click="handleCreate" />
        </DxButtonItem>
      </DxForm>
    </DxPopup>

    <OperationGuideDrawer
      v-model:visible="showGuide"
      title="用户管理操作指引"
      entry-path="左侧菜单 → 平台管理体系 → 用户管理"
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
  getPlatformUsers,
  createPlatformUser,
  enablePlatformUser,
  disablePlatformUser,
  type PlatformUserDto,
  type CreatePlatformUserRequest,
} from '@/api/platformUsers'
import {
  PLATFORM_USER_CREATE,
  PLATFORM_USER_UPDATE,
  PLATFORM_USER_LOCK,
  PLATFORM_USER_UNLOCK,
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
  { text: t('status.Locked'), value: 'Locked' },
])

const gridData = ref<PlatformUserDto[]>([])

const createForm = reactive<CreatePlatformUserRequest>({
  username: '',
  displayName: '',
  email: '',
  phone: '',
  password: '',
  remark: '',
})

async function loadData() {
  try {
    const res = await getPlatformUsers({
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
    await createPlatformUser(createForm)
    showCreatePopup.value = false
    Object.assign(createForm, { username: '', displayName: '', email: '', phone: '', password: '', remark: '' })
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

function onEdit(_user: PlatformUserDto) {
  // 后续阶段完善编辑功能
}

async function onEnable(id: number) {
  try {
    await enablePlatformUser(id)
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

async function onDisable(id: number) {
  try {
    await disablePlatformUser(id)
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

const guideSteps = [
  '点击【新增用户】按钮创建管理员账号',
  '在列表中使用搜索框按用户名/邮箱筛选',
  '点击【编辑】修改用户信息',
  '点击【禁用/启用】切换用户状态',
]
const guideFieldNotes = [
  '用户名：全局唯一，创建后不可修改',
  '邮箱：用于接收系统通知',
  '密码：初始密码需告知用户，建议登录后修改',
]
const guideErrorNotes = [
  '用户名已存在时创建将失败',
  '禁用超级管理员需谨慎操作',
]

onMounted(loadData)
</script>
