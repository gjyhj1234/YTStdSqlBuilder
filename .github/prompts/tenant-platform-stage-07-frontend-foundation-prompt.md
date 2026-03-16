# 租户平台阶段 07：前端工程骨架、鉴权、布局与帮助系统提示词

## 目标

在后端核心接口基本稳定后，本阶段创建前端工程骨架：

1. `web/tenant-platform-web/` 工程
2. Vite + Vue 3 + TypeScript + devextreme-vue 基础配置
3. 路由、布局、Pinia、HTTP 层、登录鉴权
4. 菜单与权限显隐骨架
5. “功能说明 / 操作指引”统一组件体系
6. 首页 / 仪表盘 / 登录页 / 少量核心模块示例页

---

## 必须阅读的文件

- `.github/prompts/tenant-platform-frontend-prompt.md`
- `.github/prompts/tenant-platform-backend-prompt.md`
- `docs/TenantPlatform/architecture.md`
- 后端已完成的接口与 DTO 注释说明

---

## 本阶段建议目录

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
│   ├── components/
│   ├── composables/
│   ├── constants/
│   ├── utils/
│   └── views/
```

---

## 本阶段必须实现的内容

1. 登录页与登录态管理
2. 路由守卫与权限显隐基础能力
3. 主布局、侧边菜单、顶栏、面包屑
4. API 基础封装与分页模型
5. 通用组件骨架：
   - Grid / Form / Drawer / DetailTabs
   - 状态标签 / 筛选栏 / Empty / Error / Loading
6. 帮助系统组件：
   - 功能说明卡片
   - 操作指引抽屉 / 弹窗
   - 页面级帮助入口
7. 首页 / 仪表盘
8. 1-2 个核心模块示例页（例如平台用户、租户列表）

---

## 特别要求

- 必须使用 `devextreme-vue`，不要换 UI 栈。
- 前端权限码必须和后端保持一致。
- 每个页面模板都要预留“功能说明”和“操作指引”入口。
- 页面结构要为后续模块批量扩展做好统一模板。

---

## 验证要求

至少执行并报告：
- 前端依赖安装 / 构建命令（如工程已创建）
- TypeScript / Vite 基础构建是否通过
- 核心路由是否可访问

---

## 明确禁止

- 不要在本阶段一次性实现全部业务模块页面；
- 不要忽略帮助系统组件；
- 不要把 API URL 直接散落到页面中。
