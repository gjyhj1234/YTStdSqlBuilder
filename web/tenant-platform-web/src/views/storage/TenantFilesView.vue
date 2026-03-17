<template>
  <div>
    <div class="page-header">
      <h2>租户文件管理</h2>
      <div class="page-header-actions">
        <PageHelpEntry @click="showGuide = true" />
      </div>
    </div>

    <FunctionDescriptionCard
      purpose="租户文件管理用于查看和管理租户上传的文件信息，包括文件元数据、可见性和下载统计。支持删除操作。"
      data-scope="全平台租户文件数据。"
      permission-note="需要 infra:component:view 权限查看列表。"
      risk-note="删除文件操作不可恢复，请谨慎操作。"
      :collapsible="true"
    />

    <div class="card">
      <div class="filter-bar">
        <DxTextBox
          v-model:value="filterKeyword"
          placeholder="搜索文件名 / 扩展名 / MIME 类型"
          :width="280"
          mode="search"
          value-change-event="input"
        />
        <DxNumberBox
          v-model:value="filterTenantId"
          placeholder="按租户ID筛选"
          :width="160"
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
        <DxColumn data-field="tenantRefId" caption="租户ID" :width="80" />
        <DxColumn data-field="storageStrategyId" caption="策略ID" :width="80" />
        <DxColumn data-field="fileName" caption="文件名" />
        <DxColumn data-field="filePath" caption="文件路径" />
        <DxColumn data-field="fileExt" caption="扩展名" :width="80" />
        <DxColumn data-field="mimeType" caption="MIME 类型" :width="120" />
        <DxColumn data-field="fileSize" caption="文件大小" cell-template="sizeCell" :width="100" />
        <DxColumn data-field="uploaderType" caption="上传者类型" :width="100" />
        <DxColumn data-field="uploaderId" caption="上传者ID" :width="90" />
        <DxColumn data-field="visibility" caption="可见性" :width="80" />
        <DxColumn data-field="downloadCount" caption="下载次数" :width="90" />
        <DxColumn data-field="createdAt" caption="创建时间" cell-template="dateCell" />
        <DxColumn caption="操作" cell-template="actionCell" :width="80" />
        <template #sizeCell="{ data: cellData }">
          <span>{{ formatFileSize(cellData.value) }}</span>
        </template>
        <template #dateCell="{ data: cellData }">
          <span>{{ formatDateTime(cellData.value) }}</span>
        </template>
        <template #actionCell="{ data: cellData }">
          <DxButton
            v-if="perm.has(INFRA_COMPONENT_DELETE)"
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

    <OperationGuideDrawer
      v-model:visible="showGuide"
      title="租户文件操作指引"
      entry-path="左侧菜单 → 技术基础设施 → 租户文件"
      :steps="guideSteps"
      :field-notes="guideFieldNotes"
      :error-notes="guideErrorNotes"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { DxDataGrid, DxColumn, DxPaging, DxPager } from 'devextreme-vue/data-grid'
import { DxButton } from 'devextreme-vue/button'
import { DxTextBox } from 'devextreme-vue/text-box'
import DxNumberBox from 'devextreme-vue/number-box'
import FunctionDescriptionCard from '@/components/help/FunctionDescriptionCard.vue'
import OperationGuideDrawer from '@/components/help/OperationGuideDrawer.vue'
import PageHelpEntry from '@/components/help/PageHelpEntry.vue'
import { usePermission } from '@/composables/usePermission'
import { formatDateTime } from '@/utils/format'
import {
  getTenantFiles,
  deleteTenantFile,
  type TenantFileDto,
} from '@/api/storage'
import {
  INFRA_COMPONENT_DELETE,
} from '@/constants/permissions'

const perm = usePermission()
const showGuide = ref(false)
const filterKeyword = ref('')
const filterTenantId = ref<number | undefined>(undefined)

const gridData = ref<TenantFileDto[]>([])

function formatFileSize(bytes: number | null | undefined): string {
  if (bytes == null || bytes === 0) return '0 B'
  const units = ['B', 'KB', 'MB', 'GB', 'TB']
  const i = Math.floor(Math.log(bytes) / Math.log(1024))
  return (bytes / Math.pow(1024, i)).toFixed(i === 0 ? 0 : 1) + ' ' + units[i]
}

async function loadData() {
  try {
    const res = await getTenantFiles({
      page: 1,
      pageSize: 20,
      keyword: filterKeyword.value || undefined,
      tenantRefId: filterTenantId.value || undefined,
    })
    gridData.value = res.data.items
  } catch {
    // 接口未就绪时保持空列表
  }
}

async function onDelete(id: number) {
  try {
    await deleteTenantFile(id)
    await loadData()
  } catch {
    // 错误由 http 层统一处理
  }
}

const guideSteps = [
  '在列表中查看所有租户上传的文件',
  '在租户ID输入框中输入租户编号按租户筛选文件',
  '使用搜索框按文件名、扩展名或 MIME 类型筛选',
  '点击【删除】移除文件记录',
]
const guideFieldNotes = [
  '文件大小：自动转换为合适的单位（B/KB/MB/GB）',
  '可见性：文件的访问权限范围',
  '下载次数：文件被下载的累计次数',
]
const guideErrorNotes = [
  '删除文件操作不可恢复',
  '删除文件不会影响已下载的副本',
]

onMounted(loadData)
</script>
