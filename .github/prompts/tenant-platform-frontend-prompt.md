# 面向 Claude Opus 4.6 的租户平台前端工程实现提示词

## Vue 3 + TypeScript + DevExtreme Vue 的平台管理前端

你是一名**顶级前端架构师 / Vue 3 专家 / DevExpress DevExtreme 专家 / 企业级中后台产品工程师**。
请为租户平台生成一个生产级前端工程，技术栈固定为：

- `Vue 3`
- `TypeScript`
- `Vite`
- `devextreme-vue`

目标是交付一个适合租户平台后台管理的**高可维护、高性能、模块化**前端，而不是简单页面拼接 demo。

---

## 依赖业务文档

> 请严格依据以下文档生成前端工程：
> - [租户平台架构说明](../../docs/TenantPlatform/architecture.md)
> - [租户平台实体数据字典](../../docs/TenantPlatform/database_dictionary.md)
> - [租户平台后端服务工程提示词](./tenant-platform-backend-prompt.md)
> - [租户平台初始化数据实现提示词](./tenant-platform-initdata-prompt.md)

前端的信息架构、菜单结构、模块划分、字段命名、权限码，必须与上述文档保持一致。

---

## 1. 最终目标

生成一个完整的租户平台前端工程，至少包括：

1. 登录与鉴权界面
2. 平台首页 / 仪表盘
3. 平台用户、角色、权限管理页面
4. 租户生命周期、租户信息、资源、配置管理页面
5. 套餐、订阅、计费、账单页面
6. API Key、Webhook、通知、日志、审计页面
7. 通用表格、表单、详情抽屉、筛选器、状态标签、权限指令体系

---

## 2. 技术约束

- 框架：`Vue 3` + `TypeScript`
- 构建：`Vite`
- UI：`devextreme-vue`
- 路由：`vue-router`
- 状态管理：推荐 `pinia`
- HTTP：推荐 `fetch` 封装或轻量请求层
- **禁止**引入与 `devextreme-vue` 重复定位的大型 UI 组件库
- **禁止**为了少量页面引入重量级低收益依赖

---

## 3. 工程结构

建议生成在以下路径：

| 项目 | 路径 | 说明 |
|------|------|------|
| 前端主项目 | `web/tenant-platform-web/` | 平台管理前端 |

建议目录结构：

```text
web/tenant-platform-web/
├── src/
│   ├── main.ts
│   ├── App.vue
│   ├── router/
│   ├── store/
│   ├── api/
│   ├── auth/
│   ├── layouts/
│   ├── views/
│   │   ├── dashboard/
│   │   ├── platform-users/
│   │   ├── platform-roles/
│   │   ├── platform-permissions/
│   │   ├── tenants/
│   │   ├── packages/
│   │   ├── subscriptions/
│   │   ├── billing/
│   │   ├── api-integration/
│   │   ├── notifications/
│   │   └── logs/
│   ├── components/
│   ├── composables/
│   ├── utils/
│   ├── constants/
│   └── styles/
└── package.json
```

---

## 4. 页面与交互要求

### 4.1 菜单结构

菜单必须直接映射 `docs/TenantPlatform/architecture.md` 中的功能树。

至少要有以下一级菜单：
- 平台管理体系
- 租户生命周期体系
- 租户信息体系
- 租户资源管理
- 租户配置中心
- SaaS 套餐系统
- 订阅系统
- 计费与账单系统
- API 与集成平台
- 平台运营体系
- 日志与审计
- 通知系统
- 文件与存储
- 技术基础设施

### 4.2 通用页面模式

平台类页面优先采用以下组合：
- `DxDataGrid`：列表页、分页、排序、筛选、导出
- `DxForm`：新建/编辑表单
- `DxPopup` / `DxDrawer`：详情、编辑、授权
- `DxTreeList`：权限树、菜单树、组织树
- `DxTagBox` / `DxSelectBox`：多选与字典选择
- `DxTabs`：详情页分组信息

### 4.2.1 功能说明与操作指引

- **每个功能模块必须同时提供“功能说明”与“操作指引”能力**。
- 建议在每个一级/二级页面提供统一的说明区域，可采用以下任一模式：
  - 页面顶部说明卡片
  - “操作指引”侧边抽屉
  - “帮助 / 使用说明”弹窗
- 功能说明至少包含：模块用途、关键数据范围、权限要求、常见风险提示。
- 操作指引至少包含：进入路径、典型操作步骤、字段填写说明、常见错误处理。
- 对于新增/编辑/授权/状态流转等高频操作，需提供内嵌式步骤提示，便于非开发用户理解。

### 4.3 页面体验要求

- 列表页统一支持关键词搜索、状态筛选、时间范围筛选
- 表单页统一支持字段校验、枚举下拉、布尔开关、只读态展示
- 详情页统一支持基础信息、关联信息、日志/审计标签页
- 状态字段统一展示为颜色标签
- 金额、配额、时间、状态等字段统一格式化

---

## 5. 权限与前端缓存要求

- 前端权限码必须与后端平台权限码一致。
- 路由守卫、按钮显隐、菜单可见性都要基于权限码。
- 当前登录用户的菜单树、按钮权限、基础字典可在前端做**轻量内存缓存**，但不能替代后端权限校验。
- 后端权限判断主方案仍然是**Local Cache + 服务端校验**；前端只负责体验优化。

---

## 6. API 对接要求

- 前端 API 模块按业务域拆分，例如：
  - `api/platformUsers.ts`
  - `api/platformRoles.ts`
  - `api/tenants.ts`
  - `api/packages.ts`
  - `api/billing.ts`
- 所有 API 类型定义要与后端 DTO 保持一致。
- 列表接口统一使用分页模型。
- 新增/编辑/状态流转/授权操作统一封装为明确方法，不要在页面中散落 URL 字符串。
- 只有在后端接口注释、DTO 注释和路由清单稳定后，前端阶段才开始批量生成页面，以减少接口语义漂移。

### 6.1 GitHub Agents 分阶段执行建议

如果当前使用 GitHub Agents / Copilot 分阶段推进，而不是一次性生成整个前端工程，
建议按以下顺序配合执行：

1. `tenant-platform-stage-07-frontend-foundation-prompt.md`
2. `tenant-platform-stage-08-frontend-modules-prompt.md`
3. `tenant-platform-stage-09-final-validation-prompt.md`

其中：
- 阶段 07 负责前端工程骨架、登录鉴权、布局、帮助组件和少量示例页；
- 阶段 08 负责业务模块页面、API 对接、“功能说明 / 操作指引”能力；
- 阶段 09 负责统一构建、测试、补漏与交付说明。

---

## 7. 设计风格要求

- 风格定位：企业级中后台、信息密度高、交互稳定、操作路径清晰。
- 避免炫技型动效。
- 页面布局优先保证可读性、表单效率、表格操作效率。
- 组件封装要服务于“平台表格 + 详情抽屉 + 表单编辑 + 权限显隐”这四类高频场景。

---

## 8. 测试与质量要求

至少考虑：
- 路由访问控制
- 权限显隐
- 列表页核心交互
- 表单校验
- API 错误处理
- 登录态失效处理
- 功能说明 / 操作指引组件渲染与入口可见性

---

## 9. 最终质量标准

**正确性 > 与业务文档一致 > 可维护性 > 可用性 > 性能 > 组件复用性 > 页面美观度**

---

## 10. 最终指令

请现在开始实现，直接交付：

- 完整前端源码
- 路由与菜单结构
- API 封装
- 通用组件
- 关键页面实现
- 权限控制设计说明

不要只给页面原型。
不要输出与 `devextreme-vue` 无关的 UI 技术栈。
必须按**Vue 3 + TypeScript + Vite + devextreme-vue**完成。
