<template>
  <div>
    <div class="page-header">
      <h2>{{ $t('route.saasPackageVersions') }}</h2>
      <div class="page-header-actions">
        <PageHelpEntry @click="showGuide = true" />
        <DxButton
          v-if="perm.has(PACKAGE_VERSION_CREATE)"
          :text="$t('新增版本')"
          icon="add"
          type="default"
          @click="showCreatePopup = true"
        />
      </div>
    </div>

    <FunctionDescriptionCard
      purpose="版本管理用于维护 SaaS 套餐的版本信息，包括定价、计费周期、试用天数等。每个套餐可拥有多个版本。"
      data-scope="全平台所有套餐版本记录。"
      permission-note="需要 package:version:view 权限查看列表，package:version:create 创建版本。"
      risk-note="版本一旦关联订阅后不建议删除，建议通过设置有效期来控制版本生命周期。"
      :collapsible="true"
    />

    <div class="card">
      <div class="filter-bar">
        <DxNumberBox
          v-model:value="filterPackageId"
          :placeholder="$t('套餐ID')"
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
        <DxColumn data-field="packageId" :caption="$t('套餐ID')" :width="80" />
        <DxColumn data-field="versionCode" :caption="$t('版本编码')" :width="120" />
        <DxColumn data-field="versionName" :caption="$t('版本名称')" :width="140" />
        <DxColumn data-field="editionType" :caption="$t('版本类型')" :width="100" />
        <DxColumn data-field="billingCycle" :caption="$t('计费周期')" :width="100" />
        <DxColumn data-field="price" :caption="$t('价格')" :width="100" cell-template="priceCell" />
        <DxColumn data-field="currencyCode" :caption="$t('币种')" :width="60" />
        <DxColumn data-field="trialDays" :caption="$t('试用天数')" :width="80" />
        <DxColumn data-field="isDefault" :caption="$t('默认')" cell-template="booleanCell" :width="60" />
        <DxColumn data-field="enabled" :caption="$t('common.enable')" cell-template="booleanCell" :width="60" />
        <DxColumn data-field="effectiveFrom" :caption="$t('生效时间')" cell-template="dateTimeCell" :width="140" />
        <DxColumn data-field="effectiveTo" :caption="$t('失效时间')" cell-template="dateTimeCell" :width="140" />
        <DxColumn data-field="createdAt" :caption="$t('common.createdAt')" cell-template="dateTimeCell" :width="140" />
        <template #priceCell="{ data: cellData }">
          <span>{{ formatCurrency(Number(cellData.value), cellData.data.currencyCode) }}</span>
        </template>
        <template #dateTimeCell="{ data: cellData }">
          <span>{{ formatDateTime(cellData.value) }}</span>
        </template>
        <template #booleanCell="{ data: cellData }">
          <span>{{ cellData.value ? $t('common.yes') : $t('common.no') }}</span>
        </template>
        <DxPaging :page-size="20" />
        <DxPager :show-page-size-selector="true" :allowed-page-sizes="[10, 20, 50]" :show-info="true" />
      </DxDataGrid>
    </div>

    <!-- 新增版本弹窗 -->
    <DxPopup
      :visible="showCreatePopup"
      :title="$t('新增版本')"
      :width="520"
      :height="'auto'"
      :show-close-button="true"
      @hiding="showCreatePopup = false"
    >
      <DxForm
        :form-data="createForm"
        :col-count="1"
        label-mode="floating"
      >
        <DxSimpleItem data-field="packageId" editor-type="dxNumberBox">
          <DxLabel :text="$t('套餐ID')" />
        </DxSimpleItem>
        <DxSimpleItem data-field="versionCode" editor-type="dxTextBox">
          <DxLabel :text="$t('版本编码')" />
        </DxSimpleItem>
        <DxSimpleItem data-field="versionName" editor-type="dxTextBox">
          <DxLabel :text="$t('版本名称')" />
        </DxSimpleItem>
        <DxSimpleItem data-field="editionType" editor-type="dxSelectBox"
          :editor-options="{ items: editionTypes, displayExpr: 'text', valueExpr: 'value' }">
          <DxLabel :text="$t('版本类型')" />
        </DxSimpleItem>
        <DxSimpleItem data-field="billingCycle" editor-type="dxSelectBox"
          :editor-options="{ items: billingCycles, displayExpr: 'text', valueExpr: 'value' }">
          <DxLabel :text="$t('计费周期')" />
        </DxSimpleItem>
        <DxSimpleItem data-field="price" editor-type="dxNumberBox"
          :editor-options="{ format: '#,##0.00', min: 0 }">
          <DxLabel :text="$t('价格')" />
        </DxSimpleItem>
        <DxSimpleItem data-field="currencyCode" editor-type="dxTextBox">
          <DxLabel :text="$t('币种')" />
        </DxSimpleItem>
        <DxSimpleItem data-field="trialDays" editor-type="dxNumberBox"
          :editor-options="{ min: 0 }">
          <DxLabel :text="$t('试用天数')" />
        </DxSimpleItem>
        <DxSimpleItem data-field="isDefault" editor-type="dxCheckBox">
          <DxLabel :text="$t('默认版本')" />
        </DxSimpleItem>
        <DxButtonItem>
          <DxButtonOptions :text="$t('提交')" type="default" :use-submit-behavior="false" @click="handleCreate" />
        </DxButtonItem>
      </DxForm>
    </DxPopup>

    <OperationGuideDrawer
      v-model:visible="showGuide"
      title="版本管理操作指引"
      entry-path="左侧菜单 → SaaS 套餐系统 → 版本管理"
      :steps="guideSteps"
      :field-notes="guideFieldNotes"
      :error-notes="guideErrorNotes"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { DxDataGrid, DxColumn, DxPaging, DxPager } from 'devextreme-vue/data-grid'
import { DxButton } from 'devextreme-vue/button'
import { DxNumberBox } from 'devextreme-vue/number-box'
import { DxPopup } from 'devextreme-vue/popup'
import { DxForm, DxSimpleItem, DxLabel, DxButtonItem, DxButtonOptions } from 'devextreme-vue/form'
import FunctionDescriptionCard from '@/components/help/FunctionDescriptionCard.vue'
import OperationGuideDrawer from '@/components/help/OperationGuideDrawer.vue'
import PageHelpEntry from '@/components/help/PageHelpEntry.vue'
import { usePermission } from '@/composables/usePermission'
import { useI18n } from 'vue-i18n'
import { formatCurrency, formatDateTime } from '@/utils/format'
import {
  getPackageVersions,
  createPackageVersion,
  type SaasPackageVersionDto,
  type CreateSaasPackageVersionRequest,
} from '@/api/packages'
import {
  PACKAGE_VERSION_CREATE,
} from '@/constants/permissions'

const perm = usePermission()
const { t } = useI18n()
const showGuide = ref(false)
const showCreatePopup = ref(false)
const filterPackageId = ref<number | undefined>(undefined)

const editionTypes = computed(() => [
  { text: t('免费版'), value: 'Free' },
  { text: t('基础版'), value: 'Basic' },
  { text: t('标准版'), value: 'Standard' },
  { text: t('专业版'), value: 'Professional' },
  { text: t('企业版'), value: 'Enterprise' },
])

const billingCycles = computed(() => [
  { text: t('月付'), value: 'Monthly' },
  { text: t('季付'), value: 'Quarterly' },
  { text: t('年付'), value: 'Yearly' },
  { text: t('一次性'), value: 'OneTime' },
])

const gridData = ref<SaasPackageVersionDto[]>([])

const createForm = reactive<CreateSaasPackageVersionRequest>({
  packageId: 0,
  versionCode: '',
  versionName: '',
  editionType: 'Standard',
  billingCycle: 'Monthly',
  price: 0,
  currencyCode: 'CNY',
  trialDays: 0,
  isDefault: false,
})

async function loadData() {
  if (!filterPackageId.value) return
  try {
    const res = await getPackageVersions(filterPackageId.value, {
      page: 1,
      pageSize: 20,
    })
    gridData.value = res.data.items
  } catch {
    // 接口未就绪时保持空列表
  }
}

async function handleCreate() {
  try {
    await createPackageVersion(createForm.packageId, createForm)
    showCreatePopup.value = false
    Object.assign(createForm, {
      packageId: 0,
      versionCode: '',
      versionName: '',
      editionType: 'Standard',
      billingCycle: 'Monthly',
      price: 0,
      currencyCode: 'CNY',
      trialDays: 0,
      isDefault: false,
    })
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

const guideSteps = [
  '先输入套餐ID并查询，查看该套餐下的版本列表',
  '点击【新增版本】按钮创建新的套餐版本',
  '设置版本的定价、计费周期和试用天数',
]
const guideFieldNotes = [
  '版本编码：同一套餐下唯一标识，创建后不可修改',
  '版本类型：Free（免费）、Basic（基础）、Standard（标准）、Professional（专业）、Enterprise（企业）',
  '计费周期：Monthly（月付）、Quarterly（季付）、Yearly（年付）、OneTime（一次性）',
  '试用天数：该版本允许的免费试用天数，0 表示不支持试用',
  '默认版本：标记为默认版本后，新订阅将默认选择该版本',
]
const guideErrorNotes = [
  '同一套餐下版本编码不能重复',
  '版本关联订阅后不建议删除，建议通过有效期控制生命周期',
]

onMounted(() => {
  // 版本列表需要先指定套餐ID
})
</script>
