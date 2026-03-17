<template>
  <div>
    <div class="page-header">
      <h2>平台安全中心</h2>
      <div class="page-header-actions">
        <PageHelpEntry @click="showGuide = true" />
      </div>
    </div>

    <FunctionDescriptionCard
      purpose="平台安全中心集中展示密码策略、IP白名单和多因素认证配置，帮助管理员了解安全状态。"
      data-scope="平台全局安全策略配置。"
      permission-note="需要 platform:security:view 权限查看安全配置。"
      risk-note="安全策略变更将影响所有平台管理员的登录和操作行为，请谨慎调整。"
      :collapsible="true"
    />

    <div class="security-dashboard">
      <!-- 密码策略 -->
      <div class="card security-card">
        <div class="security-card-header">
          <i class="dx-icon dx-icon-key" />
          <h3>密码策略</h3>
        </div>
        <div class="security-card-body">
          <div class="security-item">
            <span class="security-label">最小长度</span>
            <span class="security-value">{{ securityInfo.passwordPolicy.minLength }} 位</span>
          </div>
          <div class="security-item">
            <span class="security-label">要求大写字母</span>
            <span class="security-value">{{ securityInfo.passwordPolicy.requireUppercase ? '是' : '否' }}</span>
          </div>
          <div class="security-item">
            <span class="security-label">要求小写字母</span>
            <span class="security-value">{{ securityInfo.passwordPolicy.requireLowercase ? '是' : '否' }}</span>
          </div>
          <div class="security-item">
            <span class="security-label">要求数字</span>
            <span class="security-value">{{ securityInfo.passwordPolicy.requireDigit ? '是' : '否' }}</span>
          </div>
          <div class="security-item">
            <span class="security-label">要求特殊字符</span>
            <span class="security-value">{{ securityInfo.passwordPolicy.requireSpecialChar ? '是' : '否' }}</span>
          </div>
          <div class="security-item">
            <span class="security-label">密码过期天数</span>
            <span class="security-value">{{ securityInfo.passwordPolicy.expirationDays }} 天</span>
          </div>
        </div>
      </div>

      <!-- IP 白名单 -->
      <div class="card security-card">
        <div class="security-card-header">
          <i class="dx-icon dx-icon-globe" />
          <h3>IP 白名单</h3>
        </div>
        <div class="security-card-body">
          <div class="security-item">
            <span class="security-label">启用状态</span>
            <StatusTag :status="securityInfo.ipWhitelist.enabled ? 'Active' : 'Disabled'" />
          </div>
          <div class="security-item">
            <span class="security-label">白名单条目数</span>
            <span class="security-value">{{ securityInfo.ipWhitelist.entryCount }} 条</span>
          </div>
          <div class="security-item">
            <span class="security-label">最近更新</span>
            <span class="security-value">{{ formatDateTime(securityInfo.ipWhitelist.lastUpdatedAt) }}</span>
          </div>
        </div>
      </div>

      <!-- 多因素认证 -->
      <div class="card security-card">
        <div class="security-card-header">
          <i class="dx-icon dx-icon-lock" />
          <h3>多因素认证 (MFA)</h3>
        </div>
        <div class="security-card-body">
          <div class="security-item">
            <span class="security-label">启用状态</span>
            <StatusTag :status="securityInfo.mfa.enabled ? 'Active' : 'Disabled'" />
          </div>
          <div class="security-item">
            <span class="security-label">强制要求</span>
            <span class="security-value">{{ securityInfo.mfa.enforced ? '是' : '否' }}</span>
          </div>
          <div class="security-item">
            <span class="security-label">支持方式</span>
            <span class="security-value">{{ securityInfo.mfa.supportedMethods.join('、') || '未配置' }}</span>
          </div>
        </div>
      </div>
    </div>

    <OperationGuideDrawer
      v-model:visible="showGuide"
      title="安全中心操作指引"
      entry-path="左侧菜单 → 平台管理体系 → 安全中心"
      :steps="guideSteps"
      :field-notes="guideFieldNotes"
      :error-notes="guideErrorNotes"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import StatusTag from '@/components/StatusTag.vue'
import FunctionDescriptionCard from '@/components/help/FunctionDescriptionCard.vue'
import OperationGuideDrawer from '@/components/help/OperationGuideDrawer.vue'
import PageHelpEntry from '@/components/help/PageHelpEntry.vue'
import { formatDateTime } from '@/utils/format'

interface SecurityInfo {
  passwordPolicy: {
    minLength: number
    requireUppercase: boolean
    requireLowercase: boolean
    requireDigit: boolean
    requireSpecialChar: boolean
    expirationDays: number
  }
  ipWhitelist: {
    enabled: boolean
    entryCount: number
    lastUpdatedAt: string
  }
  mfa: {
    enabled: boolean
    enforced: boolean
    supportedMethods: string[]
  }
}

const showGuide = ref(false)

const securityInfo = reactive<SecurityInfo>({
  passwordPolicy: {
    minLength: 8,
    requireUppercase: true,
    requireLowercase: true,
    requireDigit: true,
    requireSpecialChar: false,
    expirationDays: 90,
  },
  ipWhitelist: {
    enabled: false,
    entryCount: 0,
    lastUpdatedAt: '',
  },
  mfa: {
    enabled: false,
    enforced: false,
    supportedMethods: [],
  },
})

async function loadData() {
  // 后续阶段对接安全策略 API，当前使用默认值展示
}

const guideSteps = [
  '进入安全中心页面查看当前平台安全策略概览',
  '查看密码策略了解密码复杂度和过期要求',
  '查看IP白名单状态了解访问控制配置',
  '查看MFA配置了解多因素认证要求',
]
const guideFieldNotes = [
  '密码策略：控制用户密码的复杂度要求和过期时间',
  'IP白名单：启用后仅允许白名单内的IP地址访问平台',
  'MFA：多因素认证提供额外的登录安全保护',
]
const guideErrorNotes = [
  '安全策略变更后需要所有在线用户重新登录',
  '启用IP白名单前请确保已添加管理员的IP地址',
]

onMounted(loadData)
</script>

<style scoped>
.security-dashboard {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(360px, 1fr));
  gap: 16px;
}
.security-card {
  padding: 20px;
}
.security-card-header {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 16px;
}
.security-card-header h3 {
  font-size: 16px;
  margin: 0;
  color: #333;
}
.security-card-header i {
  font-size: 20px;
  color: #1976d2;
}
.security-card-body {
  display: flex;
  flex-direction: column;
  gap: 12px;
}
.security-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 13px;
  line-height: 1.6;
}
.security-label {
  color: #666;
}
.security-value {
  color: #333;
  font-weight: 500;
}
</style>
