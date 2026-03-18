import { createApp } from 'vue'
import { createPinia } from 'pinia'
import 'devextreme/dist/css/dx.light.css'
import './styles/global.css'
import App from './App.vue'
import router from './router'
import { i18n } from './locales'

const app = createApp(App)
app.use(createPinia())
app.use(i18n)
app.use(router)
app.mount('#app')
