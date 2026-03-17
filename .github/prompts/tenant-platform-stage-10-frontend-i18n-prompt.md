# 租户平台阶段 10：前端国际化（i18n）提示词

## 目标

为 `web/tenant-platform-web/` 前端项目接入完整的国际化支持，覆盖：

1. **语言切换**：支持简体中文 (zh-CN)、英语 (en-US)、日语 (ja-JP)、繁体中文 (zh-TW)
2. **时间/日期格式化**：根据 locale 自动调整日期时间显示格式
3. **数字格式化**：根据 locale 调整千位分隔、小数点符号
4. **货币格式化**：根据 locale 调整货币符号与位置
5. **排序规则**：字符排序遵循当前 locale 的 collation 规则
6. **DevExtreme 组件本地化**：DataGrid、Form、Pager 等组件的内置文本

---

## 前提依赖

- 后端已有 `YTStdI18n` + `YTStdI18n.Generator` 提供后端多语言支持
- 后端 `Lang` 枚举支持 `ZhCn=0, En=1, Ja=2, ZhTw=3`
- 前端使用 Vue 3 + TypeScript + Vite + DevExtreme Vue

---

## 实施方案

### 1. 安装 vue-i18n

```bash
cd web/tenant-platform-web
npm install vue-i18n@next
```

### 2. 创建语言资源文件

在 `src/locales/` 下按语言创建 JSON 资源文件：

```
src/locales/
├── zh-CN.json    # 简体中文（基准语言）
├── en-US.json    # 英语
├── ja-JP.json    # 日语
├── zh-TW.json    # 繁体中文
└── index.ts      # i18n 实例创建与导出
```

资源 key 建议按模块分组：

```json
{
  "common": {
    "search": "查询",
    "create": "新增",
    "edit": "编辑",
    "delete": "删除",
    "save": "保存",
    "cancel": "取消",
    "enable": "启用",
    "disable": "禁用",
    "yes": "是",
    "no": "否",
    "status": "状态",
    "actions": "操作",
    "createdAt": "创建时间",
    "updatedAt": "更新时间"
  },
  "menu": {
    "dashboard": "仪表盘",
    "platformManagement": "平台管理体系",
    "platformUsers": "用户管理",
    ...
  },
  "views": {
    "platformUsers": {
      "title": "用户管理",
      "columns": { ... },
      "form": { ... },
      "guide": { ... },
      "description": { ... }
    },
    ...
  }
}
```

### 3. 修改 main.ts

```typescript
import { createApp } from 'vue'
import { createI18n } from 'vue-i18n'
import zhCN from './locales/zh-CN.json'
import enUS from './locales/en-US.json'
// ...
```

### 4. 逐模块替换硬编码文本

需要替换的文本类别：

| 类别 | 当前数量（估计） | 示例 |
|------|:---:|------|
| 菜单 label | 40+ | `'用户管理'` → `t('menu.platformUsers')` |
| 路由 title | 33 | `'仪表盘'` → `t('menu.dashboard')` |
| DataGrid caption | 200+ | `'状态'` → `t('common.status')` |
| Form label | 100+ | `'套餐编码'` → `t('views.packages.form.packageCode')` |
| FunctionDescriptionCard | 35×4 props | purpose/data-scope/permission-note/risk-note |
| OperationGuideDrawer | 35 组 steps/notes | 操作指引步骤与说明 |
| 按钮文本 | 60+ | `'新增'` → `t('common.create')` |
| 状态映射 | 30+ | `{ Active: '正常' }` → locale map |
| Placeholder | 30+ | `'搜索...'` → `t('common.searchPlaceholder')` |

### 5. 时间/数字/货币格式化

在 `src/utils/format.ts` 中封装 locale-aware 格式化函数：

```typescript
export function formatDateTime(value: string | null, locale?: string): string {
  if (!value) return ''
  return new Intl.DateTimeFormat(locale || currentLocale.value, {
    year: 'numeric', month: '2-digit', day: '2-digit',
    hour: '2-digit', minute: '2-digit', second: '2-digit'
  }).format(new Date(value))
}

export function formatNumber(value: number, locale?: string): string {
  return new Intl.NumberFormat(locale || currentLocale.value).format(value)
}

export function formatCurrency(value: number, currency: string = 'CNY', locale?: string): string {
  return new Intl.NumberFormat(locale || currentLocale.value, {
    style: 'currency', currency
  }).format(value)
}
```

### 6. 字符排序

使用 `Intl.Collator` 提供 locale-aware 排序：

```typescript
export function localeCompare(a: string, b: string, locale?: string): number {
  return new Intl.Collator(locale || currentLocale.value).compare(a, b)
}
```

### 7. DevExtreme 本地化

DevExtreme 内置多语言支持：

```typescript
import { locale, loadMessages } from 'devextreme/localization'
import zhMessages from 'devextreme/localization/messages/zh'
import enMessages from 'devextreme/localization/messages/en'
import jaMessages from 'devextreme/localization/messages/ja'
```

### 8. 语言切换 UI

在 MainLayout 顶部工具栏添加语言切换下拉框：

```vue
<DxSelectBox
  :items="languageOptions"
  display-expr="label"
  value-expr="value"
  :value="currentLocale"
  @value-changed="onLocaleChange"
  :width="120"
/>
```

### 9. 持久化用户语言偏好

- 存储到 `localStorage`：`platform_locale`
- 请求 API 时通过 `Accept-Language` header 传递

---

## 常量/菜单文件需同步更新

- `src/constants/menus.ts`：所有 `label` 改为 i18n key
- `src/router/index.ts`：所有 `meta.title` 改为 i18n key
- `src/components/StatusTag.vue`：默认状态映射使用 i18n
- `src/components/help/FunctionDescriptionCard.vue`：支持 i18n props
- `src/components/help/OperationGuideDrawer.vue`：支持 i18n props

---

## 验证要求

1. `npm run build` 无错误
2. 切换语言后所有页面文本正确显示
3. 日期/数字格式化跟随 locale 变化
4. DevExtreme 组件内置文本跟随 locale 变化
5. 不影响现有功能和权限控制

---

## 明确禁止

- 不要修改后端代码
- 不要改变路由结构或权限体系
- 不要引入不必要的第三方库
- 不要在本阶段进行大规模重构
