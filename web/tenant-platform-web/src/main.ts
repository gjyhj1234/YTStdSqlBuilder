import { createApp } from 'vue'
import { createPinia } from 'pinia'
import 'devextreme/dist/css/dx.light.css'
import './styles/global.css'
import App from './App.vue'
import router from './router'
import { i18n } from './locales'
import config from "devextreme/core/config";
import { licenseKey } from './devextreme-license';
async function bootstrap() {
  if (import.meta.env.DEV && import.meta.env.VITE_ENABLE_MOCK === 'true') {
    const { worker } = await import('./mocks/browser')
    await worker.start({ onUnhandledRequest: 'bypass' })
  }
config({
    forceIsoDateParsing: true,
    licenseKey
})
  const app = createApp(App)
  app.use(createPinia())
  app.use(i18n)
  app.use(router)
  app.mount('#app')
}

bootstrap()
