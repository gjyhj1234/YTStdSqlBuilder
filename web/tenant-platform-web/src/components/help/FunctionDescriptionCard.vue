<template>
  <div class="card function-description-card">
    <div class="func-card-header">
      <h4>
        <i class="dx-icon dx-icon-info" />
        {{ t('components.functionDescription.title') }}
      </h4>
      <button v-if="collapsible" class="func-card-toggle" @click="collapsed = !collapsed">
        {{ collapsed ? t('components.functionDescription.expand') : t('components.functionDescription.collapse') }}
      </button>
    </div>
    <div v-show="!collapsed" class="func-card-body">
      <div v-if="purpose" class="func-section">
        <strong>{{ t('components.functionDescription.purpose') }}</strong>
        <span>{{ resolvedPurpose }}</span>
      </div>
      <div v-if="dataScope" class="func-section">
        <strong>{{ t('components.functionDescription.dataScope') }}</strong>
        <span>{{ resolvedDataScope }}</span>
      </div>
      <div v-if="permissionNote" class="func-section">
        <strong>{{ t('components.functionDescription.permissionNote') }}</strong>
        <span>{{ resolvedPermissionNote }}</span>
      </div>
      <div v-if="riskNote" class="func-section func-risk">
        <strong>{{ t('components.functionDescription.riskNote') }}</strong>
        <span>{{ resolvedRiskNote }}</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { translateText } from '@/locales'

const { t } = useI18n()

const props = defineProps<{
  purpose?: string
  dataScope?: string
  permissionNote?: string
  riskNote?: string
  collapsible?: boolean
}>()

const collapsed = ref(false)
const resolvedPurpose = computed(() => translateText(props.purpose))
const resolvedDataScope = computed(() => translateText(props.dataScope))
const resolvedPermissionNote = computed(() => translateText(props.permissionNote))
const resolvedRiskNote = computed(() => translateText(props.riskNote))
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
