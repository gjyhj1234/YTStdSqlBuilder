<template>
  <div>
    <div class="page-header">
      <h2>权限管理</h2>
      <div class="page-header-actions">
        <PageHelpEntry @click="showGuide = true" />
      </div>
    </div>

    <FunctionDescriptionCard
      purpose="权限管理展示平台所有权限码的层级结构，包含菜单权限、API权限和操作权限。"
      data-scope="平台全部权限数据（由系统种子数据维护）。"
      permission-note="需要 platform:permission:view 权限查看权限树。"
      risk-note="权限数据由系统种子数据管理，此页面仅供查看，不支持直接编辑。"
      :collapsible="true"
    />

    <div class="card">
      <div class="filter-bar">
        <DxTextBox
          v-model:value="filterKeyword"
          placeholder="搜索权限编码 / 名称"
          :width="260"
          mode="search"
          value-change-event="input"
          @value-changed="onFilterChanged"
        />
      </div>

      <DxTreeList
        :data-source="treeData"
        :show-borders="true"
        :column-auto-width="true"
        :hover-state-enabled="true"
        key-expr="id"
        parent-id-expr="parentId"
        :auto-expand-all="true"
        :filter-mode="'fullBranch'"
        :search-panel="{ visible: false }"
      >
        <DxColumn data-field="id" caption="ID" :width="60" />
        <DxColumn data-field="code" caption="权限编码" />
        <DxColumn data-field="name" caption="权限名称" />
        <DxColumn data-field="permissionType" caption="权限类型" cell-template="typeCell" :width="120" />
        <DxColumn data-field="path" caption="路径" />
        <DxColumn data-field="method" caption="HTTP方法" :width="100" />
        <template #typeCell="{ data: cellData }">
          <span class="permission-type-tag" :class="permissionTypeClass(cellData.value)">
            {{ permissionTypeLabel(cellData.value) }}
          </span>
        </template>
      </DxTreeList>
    </div>

    <OperationGuideDrawer
      v-model:visible="showGuide"
      title="权限管理操作指引"
      entry-path="左侧菜单 → 平台管理体系 → 权限管理"
      :steps="guideSteps"
      :field-notes="guideFieldNotes"
      :error-notes="guideErrorNotes"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { DxTreeList, DxColumn } from 'devextreme-vue/tree-list'
import { DxTextBox } from 'devextreme-vue/text-box'
import FunctionDescriptionCard from '@/components/help/FunctionDescriptionCard.vue'
import OperationGuideDrawer from '@/components/help/OperationGuideDrawer.vue'
import PageHelpEntry from '@/components/help/PageHelpEntry.vue'
import {
  getPermissions,
  type PlatformPermissionDto,
} from '@/api/platformPermissions'

interface FlatPermission {
  id: number
  code: string
  name: string
  permissionType: string
  parentId: number | null
  path: string
  method: string
}

const filterKeyword = ref('')
const showGuide = ref(false)

const allData = ref<FlatPermission[]>([])
const treeData = ref<FlatPermission[]>([])

function flattenTree(nodes: PlatformPermissionDto[]): FlatPermission[] {
  const result: FlatPermission[] = []
  for (const node of nodes) {
    result.push({
      id: node.id,
      code: node.code,
      name: node.name,
      permissionType: node.permissionType,
      parentId: node.parentId,
      path: node.path,
      method: node.method,
    })
    if (node.children && node.children.length > 0) {
      result.push(...flattenTree(node.children))
    }
  }
  return result
}

const permissionTypeMap: Record<string, { label: string; cssClass: string }> = {
  Menu: { label: '菜单权限', cssClass: 'menu' },
  Api: { label: 'API权限', cssClass: 'api' },
  Operation: { label: '操作权限', cssClass: 'operation' },
  Data: { label: '数据权限', cssClass: 'data' },
}

function permissionTypeLabel(type: string): string {
  return permissionTypeMap[type]?.label || type
}

function permissionTypeClass(type: string): string {
  return permissionTypeMap[type]?.cssClass || ''
}

function onFilterChanged() {
  const keyword = filterKeyword.value?.toLowerCase()
  if (!keyword) {
    treeData.value = allData.value
    return
  }
  // Include matching nodes and their ancestors
  const matchedIds = new Set<number>()
  for (const item of allData.value) {
    if (item.code.toLowerCase().includes(keyword) || item.name.toLowerCase().includes(keyword)) {
      matchedIds.add(item.id)
      // Walk up to include ancestors
      let current = item
      while (current.parentId !== null) {
        matchedIds.add(current.parentId)
        const parent = allData.value.find(p => p.id === current.parentId)
        if (!parent) break
        current = parent
      }
    }
  }
  treeData.value = allData.value.filter(item => matchedIds.has(item.id))
}

async function loadData() {
  try {
    const res = await getPermissions()
    allData.value = flattenTree(res.data)
    treeData.value = allData.value
  } catch {
    // 接口未就绪时保持空列表
  }
}

const guideSteps = [
  '进入权限管理页面查看平台所有权限的层级结构',
  '使用搜索框按权限编码或名称筛选',
  '展开/折叠树节点查看子权限',
]
const guideFieldNotes = [
  '权限编码：唯一标识，格式为 模块:资源:操作',
  '权限类型：菜单权限控制页面可见性，API权限控制接口访问，操作权限控制按钮可用性',
  '路径和方法：仅API权限类型会显示对应的HTTP路径和方法',
]
const guideErrorNotes = [
  '权限数据由系统种子数据管理，无法在此页面直接修改',
]

onMounted(loadData)
</script>

<style scoped>
.permission-type-tag {
  display: inline-block;
  padding: 2px 8px;
  border-radius: 4px;
  font-size: 12px;
}
.permission-type-tag.menu {
  background-color: #e3f2fd;
  color: #1565c0;
}
.permission-type-tag.api {
  background-color: #e8f5e9;
  color: #2e7d32;
}
.permission-type-tag.operation {
  background-color: #fff3e0;
  color: #e65100;
}
.permission-type-tag.data {
  background-color: #f3e5f5;
  color: #7b1fa2;
}
</style>
