<template>
  <span class="help-entry" @click="$emit('click')">
    <i class="dx-icon dx-icon-help" />
    <span>{{ resolvedLabel }}</span>
  </span>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { translateText } from '@/locales'

const props = withDefaults(defineProps<{
  label?: string
}>(), {
  label: 'i18n:components.helpEntry',
})

const resolvedLabel = computed(() => translateText(props.label))

defineEmits<{
  click: []
}>()
</script>
