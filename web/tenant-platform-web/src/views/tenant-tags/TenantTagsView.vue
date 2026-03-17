<template>
  <div>
    <div class="page-header">
      <h2>租户标签管理</h2>
      <div class="page-header-actions">
        <PageHelpEntry @click="showGuide = true" />
        <DxButton
          v-if="perm.has(TENANT_TAG_BIND)"
          text="绑定标签"
          icon="link"
          type="normal"
          @click="showBindPopup = true"
        />
        <DxButton
          v-if="perm.has(TENANT_TAG_CREATE)"
          text="新增标签"
          icon="add"
          type="default"
          @click="showCreatePopup = true"
        />
      </div>
    </div>

    <FunctionDescriptionCard
      purpose="标签管理用于为租户添加灵活的分类标记，支持多维度标签体系。标签可用于筛选、分析和自动化策略。"
      data-scope="全平台所有租户标签数据。"
      permission-note="需要 tenant:tag:view 权限查看列表，tenant:tag:create 创建标签，tenant:tag:bind 绑定标签到租户。"
      risk-note="删除标签将同时解除所有租户的该标签绑定关系。"
      :collapsible="true"
    />

    <div class="card">
      <div class="filter-bar">
        <DxTextBox
          v-model:value="filterKeyword"
          placeholder="搜索标签键 / 值"
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
        <DxColumn data-field="tagKey" caption="标签键" />
        <DxColumn data-field="tagValue" caption="标签值" />
        <DxColumn data-field="tagType" caption="标签类型" :width="100" />
        <DxColumn data-field="description" caption="描述" />
        <DxColumn data-field="createdAt" caption="创建时间" cell-template="dateCell" />
        <DxColumn caption="操作" cell-template="actionCell" :width="120" />
        <template #dateCell="{ data: cellData }">
          <span>{{ formatDateTime(cellData.value) }}</span>
        </template>
        <template #actionCell="{ data: cellData }">
          <DxButton
            v-if="perm.has(TENANT_TAG_UPDATE)"
            text="编辑"
            styling-mode="text"
            @click="onEdit(cellData.data)"
          />
          <DxButton
            v-if="perm.has(TENANT_TAG_DELETE)"
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

    <!-- 新增标签弹窗 -->
    <DxPopup
      :visible="showCreatePopup"
      title="新增租户标签"
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
        <DxSimpleItem data-field="tagKey">
          <DxLabel text="标签键" />
        </DxSimpleItem>
        <DxSimpleItem data-field="tagValue">
          <DxLabel text="标签值" />
        </DxSimpleItem>
        <DxSimpleItem data-field="tagType" editor-type="dxSelectBox"
          :editor-options="{ items: tagTypes, displayExpr: 'text', valueExpr: 'value' }">
          <DxLabel text="标签类型" />
        </DxSimpleItem>
        <DxSimpleItem data-field="description" editor-type="dxTextArea">
          <DxLabel text="描述" />
        </DxSimpleItem>
        <DxButtonItem>
          <DxButtonOptions text="提交" type="default" :use-submit-behavior="false" @click="handleCreate" />
        </DxButtonItem>
      </DxForm>
    </DxPopup>

    <!-- 绑定标签弹窗 -->
    <DxPopup
      :visible="showBindPopup"
      title="绑定标签到租户"
      :width="480"
      :height="'auto'"
      :show-close-button="true"
      @hiding="showBindPopup = false"
    >
      <DxForm
        :form-data="bindForm"
        :col-count="1"
        label-mode="floating"
      >
        <DxSimpleItem data-field="tenantRefId" editor-type="dxNumberBox">
          <DxLabel text="租户ID" />
        </DxSimpleItem>
        <DxSimpleItem data-field="tagIds" editor-type="dxTagBox"
          :editor-options="{ items: tagOptions, displayExpr: 'text', valueExpr: 'value', placeholder: '选择标签' }">
          <DxLabel text="标签" />
        </DxSimpleItem>
        <DxButtonItem>
          <DxButtonOptions text="绑定" type="default" :use-submit-behavior="false" @click="handleBind" />
        </DxButtonItem>
      </DxForm>
    </DxPopup>

    <OperationGuideDrawer
      v-model:visible="showGuide"
      title="标签管理操作指引"
      entry-path="左侧菜单 → 租户信息体系 → 标签管理"
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
import { DxTextBox } from 'devextreme-vue/text-box'
import { DxPopup } from 'devextreme-vue/popup'
import { DxForm, DxSimpleItem, DxLabel, DxButtonItem, DxButtonOptions } from 'devextreme-vue/form'
import FunctionDescriptionCard from '@/components/help/FunctionDescriptionCard.vue'
import OperationGuideDrawer from '@/components/help/OperationGuideDrawer.vue'
import PageHelpEntry from '@/components/help/PageHelpEntry.vue'
import { usePermission } from '@/composables/usePermission'
import { formatDateTime } from '@/utils/format'
import {
  getTenantTags,
  createTenantTag,
  bindTags,
  type TenantTagDto,
  type CreateTenantTagRequest,
  type TagBindRequest,
} from '@/api/tenantTags'
import {
  TENANT_TAG_CREATE,
  TENANT_TAG_UPDATE,
  TENANT_TAG_DELETE,
  TENANT_TAG_BIND,
} from '@/constants/permissions'

const perm = usePermission()
const showGuide = ref(false)
const showCreatePopup = ref(false)
const showBindPopup = ref(false)
const filterKeyword = ref('')

const tagTypes = [
  { text: '业务标签', value: 'Business' },
  { text: '技术标签', value: 'Technical' },
  { text: '运营标签', value: 'Operational' },
  { text: '自定义标签', value: 'Custom' },
]

const gridData = ref<TenantTagDto[]>([])

const tagOptions = computed(() =>
  gridData.value.map((t) => ({ text: `${t.tagKey}: ${t.tagValue}`, value: t.id })),
)

const createForm = reactive<CreateTenantTagRequest>({
  tagKey: '',
  tagValue: '',
  tagType: 'Business',
  description: '',
})

const bindForm = reactive<TagBindRequest>({
  tenantRefId: 0,
  tagIds: [],
})

async function loadData() {
  try {
    const res = await getTenantTags({
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
    await createTenantTag(createForm)
    showCreatePopup.value = false
    Object.assign(createForm, { tagKey: '', tagValue: '', tagType: 'Business', description: '' })
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

async function handleBind() {
  try {
    await bindTags(bindForm)
    showBindPopup.value = false
    Object.assign(bindForm, { tenantRefId: 0, tagIds: [] })
  } catch {
    // 错误由 http 层统一处理
  }
}

function onEdit(_tag: TenantTagDto) {
  // 后续阶段完善编辑功能
}

function onDelete(_id: number) {
  // 后续阶段完善删除功能
}

const guideSteps = [
  '点击【新增标签】按钮创建标签定义',
  '填写标签键、标签值和标签类型',
  '点击【绑定标签】将标签关联到指定租户',
  '在绑定弹窗中选择租户ID和要绑定的标签',
]
const guideFieldNotes = [
  '标签键：标签分类名，如 region、tier、industry',
  '标签值：标签具体取值，如 cn-east、premium、finance',
  '标签类型：Business（业务）、Technical（技术）、Operational（运营）、Custom（自定义）',
]
const guideErrorNotes = [
  '同一租户不可绑定相同的标签',
  '标签键+标签值组合应保持唯一',
]

onMounted(loadData)
</script>
