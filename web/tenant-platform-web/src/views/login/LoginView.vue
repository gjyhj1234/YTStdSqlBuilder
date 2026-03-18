<template>
  <div class="login-page">
    <div class="login-card">
      <h1>{{ $t('app.title') }}</h1>
      <p v-if="errorMsg" class="login-error">{{ errorMsg }}</p>
      <DxForm
        :form-data="formData"
        :col-count="1"
        label-mode="floating"
      >
        <DxSimpleItem data-field="username" :editor-options="{ placeholder: $t('请输入用户名') }">
          <DxLabel :text="$t('用户名')" />
        </DxSimpleItem>
        <DxSimpleItem data-field="password" :editor-options="{ mode: 'password', placeholder: $t('请输入密码') }">
          <DxLabel :text="$t('密码')" />
        </DxSimpleItem>
        <DxButtonItem>
          <DxButtonOptions
            :text="$t('登 录')"
            type="default"
            :use-submit-behavior="false"
            width="100%"
            @click="handleLogin"
          />
        </DxButtonItem>
      </DxForm>
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '@/store/auth'
import { DxForm, DxSimpleItem, DxLabel, DxButtonItem, DxButtonOptions } from 'devextreme-vue/form'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const { t } = useI18n()

const formData = reactive({ username: '', password: '' })
const errorMsg = ref('')

async function handleLogin() {
  errorMsg.value = ''
  if (!formData.username || !formData.password) {
    errorMsg.value = t('请输入用户名和密码')
    return
  }
  try {
    await authStore.login(formData.username, formData.password)
    const redirect = (route.query.redirect as string) || '/dashboard'
    router.push(redirect)
  } catch (e: unknown) {
    errorMsg.value = e instanceof Error ? e.message : t('登录失败')
  }
}
</script>
