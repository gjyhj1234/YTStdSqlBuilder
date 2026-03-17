<template>
  <div class="placeholder-page">
    <i class="dx-icon dx-icon-toolbox" />
    <h3>{{ route.meta.title || '页面' }}</h3>
    <p>该模块将在后续阶段实现</p>
  </div>
</template>

<script setup lang="ts">
import { useRoute } from 'vue-router'

const route = useRoute()
</script>
