<template>
  <div>
    <div class="page-header">
      <h2>租户分组管理</h2>
      <div class="page-header-actions">
        <PageHelpEntry @click="showGuide = true" />
        <DxButton
          v-if="perm.has(TENANT_GROUP_CREATE)"
          text="新增分组"
          icon="add"
          type="default"
          @click="showCreatePopup = true"
        />
      </div>
    </div>

    <FunctionDescriptionCard
      purpose="租户分组管理用于按业务维度对租户进行分组归类，支持树形层级结构。可用于批量管理、数据隔离或权限边界划分。"
      data-scope="全平台所有租户分组数据。"
      permission-note="需要 tenant:group:view 权限查看列表，tenant:group:create 创建分组。"
      risk-note="删除分组前请确认没有关联租户，否则可能影响租户归属关系。"
      :collapsible="true"
    />

    <div class="card">
      <div class="filter-bar">
        <DxTextBox
          v-model:value="filterKeyword"
          placeholder="搜索分组编码 / 名称"
          :width="260"
          mode="search"
          value-change-event="input"
        />
        <DxButton text="查询" icon="search" @click="loadData" />
      </div>

      <DxTreeList
        :data-source="treeData"
        :show-borders="true"
        :column-auto-width="true"
        :hover-state-enabled="true"
        key-expr="id"
        parent-id-expr="parentId"
        has-items-expr="hasChildren"
      >
        <DxColumn data-field="id" caption="ID" :width="60" />
        <DxColumn data-field="groupCode" caption="分组编码" />
        <DxColumn data-field="groupName" caption="分组名称" />
        <DxColumn data-field="description" caption="描述" />
        <DxColumn data-field="createdAt" caption="创建时间" cell-template="dateCell" />
        <DxColumn caption="操作" cell-template="actionCell" :width="120" />
        <template #dateCell="{ data: cellData }">
          <span>{{ formatDateTime(cellData.value) }}</span>
        </template>
        <template #actionCell="{ data: cellData }">
          <DxButton
            v-if="perm.has(TENANT_GROUP_UPDATE)"
            text="编辑"
            styling-mode="text"
            @click="onEdit(cellData.data)"
          />
          <DxButton
            v-if="perm.has(TENANT_GROUP_DELETE)"
            text="删除"
            styling-mode="text"
            type="danger"
            @click="onDelete(cellData.data.id)"
          />
        </template>
      </DxTreeList>
    </div>

    <!-- 新增分组弹窗 -->
    <DxPopup
      :visible="showCreatePopup"
      title="新增租户分组"
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
        <DxSimpleItem data-field="groupCode">
          <DxLabel text="分组编码" />
        </DxSimpleItem>
        <DxSimpleItem data-field="groupName">
          <DxLabel text="分组名称" />
        </DxSimpleItem>
        <DxSimpleItem data-field="description" editor-type="dxTextArea">
          <DxLabel text="描述" />
        </DxSimpleItem>
        <DxSimpleItem data-field="parentId" editor-type="dxSelectBox"
          :editor-options="{ items: parentOptions, displayExpr: 'text', valueExpr: 'value', showClearButton: true, placeholder: '无（顶级分组）' }">
          <DxLabel text="上级分组" />
        </DxSimpleItem>
        <DxButtonItem>
          <DxButtonOptions text="提交" type="default" :use-submit-behavior="false" @click="handleCreate" />
        </DxButtonItem>
      </DxForm>
    </DxPopup>

    <OperationGuideDrawer
      v-model:visible="showGuide"
      title="租户分组操作指引"
      entry-path="左侧菜单 → 平台管理 → 租户分组"
      :steps="guideSteps"
      :field-notes="guideFieldNotes"
      :error-notes="guideErrorNotes"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { DxTreeList, DxColumn } from 'devextreme-vue/tree-list'
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
  getTenantGroups,
  createTenantGroup,
  type TenantGroupDto,
  type CreateTenantGroupRequest,
} from '@/api/tenantGroups'
import {
  TENANT_GROUP_CREATE,
  TENANT_GROUP_UPDATE,
  TENANT_GROUP_DELETE,
} from '@/constants/permissions'

const perm = usePermission()
const showGuide = ref(false)
const showCreatePopup = ref(false)
const filterKeyword = ref('')

const allGroups = ref<TenantGroupDto[]>([])

const treeData = computed(() => {
  const kw = filterKeyword.value.toLowerCase()
  if (!kw) return allGroups.value
  return allGroups.value.filter(
    (g) => g.groupCode.toLowerCase().includes(kw) || g.groupName.toLowerCase().includes(kw),
  )
})

const parentOptions = computed(() =>
  allGroups.value.map((g) => ({ text: `${g.groupCode} - ${g.groupName}`, value: g.id })),
)

const createForm = reactive<CreateTenantGroupRequest>({
  groupCode: '',
  groupName: '',
  description: '',
  parentId: undefined,
})

async function loadData() {
  try {
    const res = await getTenantGroups()
    allGroups.value = res.data
  } catch {
    // 接口未就绪时保持空列表
  }
}

async function handleCreate() {
  try {
    await createTenantGroup(createForm)
    showCreatePopup.value = false
    Object.assign(createForm, { groupCode: '', groupName: '', description: '', parentId: undefined })
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

function onEdit(_group: TenantGroupDto) {
  // 后续阶段完善编辑功能
}

function onDelete(_id: number) {
  // 后续阶段完善删除功能
}

const guideSteps = [
  '进入路径：平台管理 > 租户分组',
  '点击【新增分组】按钮填写分组信息',
  '填写分组编码（全局唯一）、分组名称、描述',
  '选择上级分组以构建树形层级结构',
]
const guideFieldNotes = [
  '分组编码：全局唯一标识，创建后不可修改',
  '上级分组：留空则创建为顶级分组',
  '描述：可填写分组的业务用途说明',
]
const guideErrorNotes = [
  '分组编码重复时创建将失败',
  '删除含子分组的分组前需先移除子分组',
]

onMounted(loadData)
</script>
