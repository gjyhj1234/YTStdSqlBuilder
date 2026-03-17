<template>
  <div>
    <DxPopup
      :visible="visible"
      :title="title || '操作指引'"
      :width="520"
      :height="'auto'"
      :show-close-button="true"
      :drag-enabled="true"
      @hiding="$emit('update:visible', false)"
    >
      <div class="guide-content">
        <div v-if="entryPath" class="guide-section">
          <h5>📍 进入路径</h5>
          <p>{{ entryPath }}</p>
        </div>
        <div v-if="steps.length > 0" class="guide-section">
          <h5>📋 操作步骤</h5>
          <ol class="guide-steps">
            <li v-for="(step, idx) in steps" :key="idx">{{ step }}</li>
          </ol>
        </div>
        <div v-if="fieldNotes.length > 0" class="guide-section">
          <h5>📝 字段说明</h5>
          <ul class="guide-fields">
            <li v-for="(note, idx) in fieldNotes" :key="idx">{{ note }}</li>
          </ul>
        </div>
        <div v-if="errorNotes.length > 0" class="guide-section">
          <h5>⚠ 常见错误</h5>
          <ul class="guide-errors">
            <li v-for="(note, idx) in errorNotes" :key="idx">{{ note }}</li>
          </ul>
        </div>
      </div>
    </DxPopup>
  </div>
</template>

<script setup lang="ts">
import { DxPopup } from 'devextreme-vue/popup'

withDefaults(defineProps<{
  visible: boolean
  title?: string
  entryPath?: string
  steps?: string[]
  fieldNotes?: string[]
  errorNotes?: string[]
}>(), {
  steps: () => [],
  fieldNotes: () => [],
  errorNotes: () => [],
})

defineEmits<{
  'update:visible': [value: boolean]
}>()
</script>

<style scoped>
.guide-content {
  padding: 8px;
}
.guide-section {
  margin-bottom: 16px;
}
.guide-section h5 {
  font-size: 14px;
  margin-bottom: 8px;
  color: #333;
}
.guide-steps,
.guide-fields,
.guide-errors {
  padding-left: 20px;
  font-size: 13px;
  line-height: 1.8;
}
.guide-errors li {
  color: #e65100;
}
</style>
