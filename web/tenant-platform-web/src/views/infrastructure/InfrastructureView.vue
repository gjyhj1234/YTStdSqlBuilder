<template>
  <div>
    <div class="page-header">
      <h2>技术基础设施</h2>
      <div class="page-header-actions">
        <PageHelpEntry @click="showGuide = true" />
      </div>
    </div>

    <FunctionDescriptionCard
      purpose="技术基础设施管理用于查看和管理平台的限流策略、数据隔离策略和基础设施组件。这些配置影响全平台的技术治理。"
      data-scope="全平台基础设施配置数据。"
      permission-note="需要 infra:component:view 权限查看。"
      risk-note="修改基础设施配置可能影响全平台服务，请谨慎操作。"
      :collapsible="true"
    />

    <div class="infra-cards">
      <!-- 限流策略 -->
      <div class="card infra-card">
        <h3>🚦 限流策略</h3>
        <p class="infra-desc">管理 API 请求的限流规则，保护系统免受过量请求的影响。</p>
        <ul class="infra-list">
          <li>按租户维度配置请求频率上限</li>
          <li>支持按 API 路径设置独立限流规则</li>
          <li>超限请求返回 429 状态码</li>
        </ul>
        <div class="infra-footer">
          <StatusTag status="Active" />
          <span class="infra-note">配置变更实时生效</span>
        </div>
      </div>

      <!-- 数据隔离策略 -->
      <div class="card infra-card">
        <h3>🔒 数据隔离策略</h3>
        <p class="infra-desc">定义租户之间的数据隔离方式，确保多租户环境下的数据安全。</p>
        <ul class="infra-list">
          <li>共享数据库（行级隔离）</li>
          <li>Schema 隔离（独立表空间）</li>
          <li>独立数据库（完全隔离）</li>
          <li>混合模式（按需组合）</li>
        </ul>
        <div class="infra-footer">
          <StatusTag status="Active" />
          <span class="infra-note">租户创建后隔离模式不可变更</span>
        </div>
      </div>

      <!-- 基础设施组件 -->
      <div class="card infra-card">
        <h3>⚙️ 基础设施组件</h3>
        <p class="infra-desc">管理平台核心基础设施组件的运行状态和配置信息。</p>
        <ul class="infra-list">
          <li>数据库服务：连接池、读写分离配置</li>
          <li>缓存服务：Redis 集群状态</li>
          <li>消息队列：RabbitMQ / Kafka 状态</li>
          <li>文件存储：存储策略与容量监控</li>
        </ul>
        <div class="infra-footer">
          <StatusTag status="Active" />
          <span class="infra-note">组件支持健康检查与重启</span>
        </div>
      </div>
    </div>

    <OperationGuideDrawer
      v-model:visible="showGuide"
      title="基础设施操作指引"
      entry-path="左侧菜单 → 技术基础设施 → 概览"
      :steps="guideSteps"
      :field-notes="guideFieldNotes"
      :error-notes="guideErrorNotes"
    />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import StatusTag from '@/components/StatusTag.vue'
import FunctionDescriptionCard from '@/components/help/FunctionDescriptionCard.vue'
import OperationGuideDrawer from '@/components/help/OperationGuideDrawer.vue'
import PageHelpEntry from '@/components/help/PageHelpEntry.vue'

const showGuide = ref(false)

const guideSteps = [
  '查看限流策略卡片了解当前限流配置',
  '查看数据隔离策略卡片了解租户隔离方式',
  '查看基础设施组件卡片了解各组件运行状态',
  '在对应子模块中进行详细的配置管理',
]
const guideFieldNotes = [
  '限流策略：控制每个租户的 API 请求频率',
  '数据隔离：决定租户数据的物理存储方式',
  '基础设施组件：平台依赖的核心服务组件',
]
const guideErrorNotes = [
  '修改限流策略后立即生效，可能影响在线租户',
  '数据隔离模式创建后不可变更',
]
</script>

<style scoped>
.infra-cards {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(340px, 1fr));
  gap: 20px;
  margin-top: 16px;
}

.infra-card {
  padding: 20px;
}

.infra-card h3 {
  margin: 0 0 8px;
  font-size: 16px;
}

.infra-desc {
  color: #666;
  margin: 0 0 12px;
  font-size: 14px;
}

.infra-list {
  margin: 0 0 16px;
  padding-left: 20px;
  font-size: 13px;
  line-height: 1.8;
  color: #555;
}

.infra-footer {
  display: flex;
  align-items: center;
  gap: 12px;
  padding-top: 12px;
  border-top: 1px solid #eee;
}

.infra-note {
  font-size: 12px;
  color: #999;
}
</style>
