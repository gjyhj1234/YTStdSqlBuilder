import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
//import { mockDevServerPlugin } from 'vite-plugin-mock-dev-server'
import { resolve } from 'path'

export default defineConfig({
  plugins: [
    vue(),
  //  mockDevServerPlugin(),
  ],
  resolve: {
    alias: {
      '@': resolve(__dirname, 'src'),
    },
  },
  server: {
    proxy: {
      '/api': {
        target: 'http://127.0.0.1:5000',
        changeOrigin: true,
      },
    },
  },
})
