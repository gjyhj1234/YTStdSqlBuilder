<template>
  <div class="card function-description-card">
    <div class="func-card-header">
      <h4>
        <i class="dx-icon dx-icon-info" />
        功能说明
      </h4>
      <button v-if="collapsible" class="func-card-toggle" @click="collapsed = !collapsed">
        {{ collapsed ? '展开' : '收起' }}
      </button>
    </div>
    <div v-show="!collapsed" class="func-card-body">
      <div v-if="purpose" class="func-section">
        <strong>模块用途：</strong>
        <span>{{ purpose }}</span>
      </div>
      <div v-if="dataScope" class="func-section">
        <strong>关键数据范围：</strong>
        <span>{{ dataScope }}</span>
      </div>
      <div v-if="permissionNote" class="func-section">
        <strong>权限要求：</strong>
        <span>{{ permissionNote }}</span>
      </div>
      <div v-if="riskNote" class="func-section func-risk">
        <strong>⚠ 风险提示：</strong>
        <span>{{ riskNote }}</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'

defineProps<{
  purpose?: string
  dataScope?: string
  permissionNote?: string
  riskNote?: string
  collapsible?: boolean
}>()

const collapsed = ref(false)
</script>

<style scoped>
.function-description-card {
  border-left: 4px solid #1976d2;
  margin-bottom: 16px;
}
.func-card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}
.func-card-header h4 {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 15px;
  color: #1976d2;
}
.func-card-toggle {
  background: none;
  border: none;
  color: #1976d2;
  cursor: pointer;
  font-size: 13px;
}
.func-card-body {
  margin-top: 12px;
}
.func-section {
  margin-bottom: 8px;
  font-size: 13px;
  line-height: 1.6;
}
.func-section strong {
  color: #333;
}
.func-risk {
  color: #e65100;
}
</style>
