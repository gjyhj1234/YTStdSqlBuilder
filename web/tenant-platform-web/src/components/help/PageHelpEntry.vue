<template>
  <span class="help-entry" @click="$emit('click')">
    <i class="dx-icon dx-icon-help" />
    <span>{{ label }}</span>
  </span>
</template>

<script setup lang="ts">
withDefaults(defineProps<{
  label?: string
}>(), {
  label: '操作指引',
})

defineEmits<{
  click: []
}>()
</script>
