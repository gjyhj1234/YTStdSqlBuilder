<template>
  <span class="status-tag" :class="statusClass">{{ displayText }}</span>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { translateText } from '@/locales'

const props = defineProps<{
  status: string
  labelMap?: Record<string, string>
}>()

const defaultLabels: Record<string, string> = {
  Active: 'i18n:status.Active',
  Disabled: 'i18n:status.Disabled',
  Locked: 'i18n:status.Locked',
  Trial: 'i18n:status.Trial',
  Expiring: 'i18n:status.Expiring',
  Expired: 'i18n:status.Expired',
  Suspended: 'i18n:status.Suspended',
  Closed: 'i18n:status.Closed',
  Pending: 'i18n:status.Pending',
  Cancelled: 'i18n:status.Cancelled',
}

const displayText = computed(() => {
  const map = { ...defaultLabels, ...props.labelMap }
  return translateText(map[props.status] || props.status)
})

const statusClass = computed(() => {
  const s = props.status.toLowerCase()
  return s
})
</script>
