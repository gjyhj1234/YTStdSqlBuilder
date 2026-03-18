<template>
  <div class="placeholder-page">
    <i class="dx-icon dx-icon-toolbox" />
    <h3>{{ t((route.meta.title as string) || 'app.placeholderPage') }}</h3>
    <p>{{ t('app.notImplemented') }}</p>
  </div>
</template>

<script setup lang="ts">
import { useRoute } from 'vue-router'
import { useI18n } from 'vue-i18n'

const route = useRoute()
const { t } = useI18n()
</script>
