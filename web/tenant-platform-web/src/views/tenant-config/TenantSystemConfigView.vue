<template>
  <div>
    <div class="page-header">
      <h2>租户系统配置</h2>
      <div class="page-header-actions">
        <PageHelpEntry @click="showGuide = true" />
      </div>
    </div>

    <FunctionDescriptionCard
      purpose="系统配置用于管理每个租户的基础系统设置，包括系统名称、Logo、主题色、默认语言和时区。每个租户只有一份系统配置。"
      data-scope="按租户隔离，每次仅查看和编辑单个租户的系统配置。"
      permission-note="需要 tenant:config:view 权限加载配置，tenant:config:update 权限保存修改。"
      risk-note="修改系统名称或主题色将立即影响对应租户的前端展示。"
      :collapsible="true"
    />

    <div class="card">
      <div class="filter-bar">
        <DxNumberBox
          v-model:value="tenantRefId"
          placeholder="请输入租户ID"
          :width="200"
          :show-spin-buttons="true"
          :min="1"
        />
        <DxButton text="加载配置" icon="download" @click="loadConfig" />
      </div>

      <DxForm
        v-if="configLoaded"
        :form-data="configForm"
        :col-count="2"
        label-mode="floating"
        :read-only="!perm.has(TENANT_CONFIG_UPDATE)"
      >
        <DxSimpleItem data-field="systemName">
          <DxLabel text="系统名称" />
        </DxSimpleItem>
        <DxSimpleItem data-field="logoUrl">
          <DxLabel text="Logo 地址" />
        </DxSimpleItem>
        <DxSimpleItem data-field="systemTheme">
          <DxLabel text="系统主题" />
        </DxSimpleItem>
        <DxSimpleItem data-field="defaultLanguage">
          <DxLabel text="默认语言" />
        </DxSimpleItem>
        <DxSimpleItem data-field="defaultTimezone">
          <DxLabel text="默认时区" />
        </DxSimpleItem>
        <DxButtonItem v-if="perm.has(TENANT_CONFIG_UPDATE)" :col-span="2">
          <DxButtonOptions text="保存配置" type="default" :use-submit-behavior="false" @click="handleSave" />
        </DxButtonItem>
      </DxForm>

      <div v-if="!configLoaded" class="empty-hint">请输入租户ID并点击【加载配置】</div>
    </div>

    <OperationGuideDrawer
      v-model:visible="showGuide"
      title="系统配置操作指引"
      entry-path="左侧菜单 → 租户配置中心 → 系统配置"
      :steps="guideSteps"
      :field-notes="guideFieldNotes"
      :error-notes="guideErrorNotes"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { DxButton } from 'devextreme-vue/button'
import { DxNumberBox } from 'devextreme-vue/number-box'
import { DxForm, DxSimpleItem, DxLabel, DxButtonItem, DxButtonOptions } from 'devextreme-vue/form'
import FunctionDescriptionCard from '@/components/help/FunctionDescriptionCard.vue'
import OperationGuideDrawer from '@/components/help/OperationGuideDrawer.vue'
import PageHelpEntry from '@/components/help/PageHelpEntry.vue'
import { usePermission } from '@/composables/usePermission'
import {
  getTenantSystemConfig,
  updateTenantSystemConfig,
  type UpdateTenantSystemConfigRequest,
} from '@/api/tenantConfig'
import {
  TENANT_CONFIG_UPDATE,
} from '@/constants/permissions'

const perm = usePermission()
const showGuide = ref(false)
const tenantRefId = ref<number | undefined>(undefined)
const configLoaded = ref(false)

const configForm = reactive<UpdateTenantSystemConfigRequest>({
  systemName: '',
  logoUrl: '',
  systemTheme: '',
  defaultLanguage: '',
  defaultTimezone: '',
})

async function loadConfig() {
  if (!tenantRefId.value) return
  try {
    const res = await getTenantSystemConfig(tenantRefId.value)
    Object.assign(configForm, {
      systemName: res.data.systemName,
      logoUrl: res.data.logoUrl,
      systemTheme: res.data.systemTheme,
      defaultLanguage: res.data.defaultLanguage,
      defaultTimezone: res.data.defaultTimezone,
    })
    configLoaded.value = true
  } catch {
    // 接口未就绪时保持空表单
  }
}

async function handleSave() {
  if (!tenantRefId.value) return
  try {
    await updateTenantSystemConfig(tenantRefId.value, configForm)
    await loadConfig()
  } catch {
    // 错误由 http 层统一处理
  }
}

const guideSteps = [
  '在顶部输入框中输入目标租户的ID',
  '点击【加载配置】按钮获取该租户的当前系统配置',
  '修改需要调整的配置项（系统名称、Logo、主题等）',
  '点击【保存配置】按钮提交修改',
]
const guideFieldNotes = [
  '系统名称：显示在租户前端页面的标题栏',
  'Logo 地址：支持 URL 格式，建议使用 CDN 地址',
  '系统主题：如 light、dark 或自定义主题标识',
  '默认语言：如 zh-CN、en-US',
  '默认时区：如 Asia/Shanghai、UTC',
]
const guideErrorNotes = [
  '租户ID不存在时加载将返回空结果',
  '未获得 tenant:config:update 权限时表单为只读',
]
</script>
