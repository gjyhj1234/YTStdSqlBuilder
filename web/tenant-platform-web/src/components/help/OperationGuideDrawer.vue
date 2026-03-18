<template>
  <div>
    <DxPopup
      :visible="visible"
      :title="resolvedTitle"
      :width="520"
      :height="'auto'"
      :show-close-button="true"
      :drag-enabled="true"
      @hiding="$emit('update:visible', false)"
    >
      <div class="guide-content">
        <div v-if="entryPath" class="guide-section">
          <h5>{{ t('components.operationGuide.entryPath') }}</h5>
          <p>{{ resolvedEntryPath }}</p>
        </div>
        <div v-if="steps.length > 0" class="guide-section">
          <h5>{{ t('components.operationGuide.steps') }}</h5>
          <ol class="guide-steps">
            <li v-for="(step, idx) in resolvedSteps" :key="idx">{{ step }}</li>
          </ol>
        </div>
        <div v-if="fieldNotes.length > 0" class="guide-section">
          <h5>{{ t('components.operationGuide.fieldNotes') }}</h5>
          <ul class="guide-fields">
            <li v-for="(note, idx) in resolvedFieldNotes" :key="idx">{{ note }}</li>
          </ul>
        </div>
        <div v-if="errorNotes.length > 0" class="guide-section">
          <h5>{{ t('components.operationGuide.errorNotes') }}</h5>
          <ul class="guide-errors">
            <li v-for="(note, idx) in resolvedErrorNotes" :key="idx">{{ note }}</li>
          </ul>
        </div>
      </div>
    </DxPopup>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { DxPopup } from 'devextreme-vue/popup'
import { useI18n } from 'vue-i18n'
import { translateList, translateText } from '@/locales'

const { t } = useI18n()

const props = withDefaults(defineProps<{
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

const resolvedTitle = computed(() => translateText(props.title) || t('components.operationGuide.title'))
const resolvedEntryPath = computed(() => translateText(props.entryPath))
const resolvedSteps = computed(() => translateList(props.steps))
const resolvedFieldNotes = computed(() => translateList(props.fieldNotes))
const resolvedErrorNotes = computed(() => translateList(props.errorNotes))

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
