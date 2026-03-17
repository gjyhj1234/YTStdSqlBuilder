<template>
  <span class="status-tag" :class="statusClass">{{ displayText }}</span>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  status: string
  labelMap?: Record<string, string>
}>()

const defaultLabels: Record<string, string> = {
  Active: '正常',
  Disabled: '已禁用',
  Locked: '已锁定',
  Trial: '试用中',
  Expiring: '即将到期',
  Expired: '已过期',
  Suspended: '已暂停',
  Closed: '已关闭',
  Pending: '待处理',
}

const displayText = computed(() => {
  const map = { ...defaultLabels, ...props.labelMap }
  return map[props.status] || props.status
})

const statusClass = computed(() => {
  const s = props.status.toLowerCase()
  return s
})
</script>
