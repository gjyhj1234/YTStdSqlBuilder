# 面向 GitHub Agents 的租户平台分阶段执行总提示词

## 用途

本提示词不是直接要求 Agent 一次性生成整个租户平台，而是用于**先整体分析、再按阶段执行**。

适用场景：
- 只有 GitHub Copilot / GitHub Agents，没有外部 Claude API；
- 单次 Agent 执行希望控制在 **60 分钟以内**；
- 希望先完成后端实体与生成代码，再做初始化数据、WebAPI、中间件、接口测试、前端工程与前端测试。

---

## 依赖上下文

在开始任何阶段之前，请先阅读以下文件：

- `docs/TenantPlatform/architecture.md`
- `docs/TenantPlatform/database_dictionary.md`
- `.github/prompts/entity-prompt.md`
- `.github/prompts/tenant-platform-backend-prompt.md`
- `.github/prompts/tenant-platform-initdata-prompt.md`
- `.github/prompts/tenant-platform-frontend-prompt.md`
- `docs/existing-projects-reference.md`

---

## 总体原则

1. **先分析，后编码**。
2. **先实体，后编译触发生成器，再初始化数据，再 WebAPI，再前端**。
3. **所有手写实体必须放在 `src/YTStdTenantPlatform/entity/TenantPlatform/*.cs`**。
4. **禁止使用裸 `TenantId` / `tenant_id` 作为租户平台实体字段名**，必须使用业务语义化命名（如 `TenantRefId`、`OwnerTenantId`）。
5. **所有公开类型、公开方法、DTO、Endpoint 注册方法都必须补齐中文 XML 注释**。
6. **后端必须保持单体主程序 + Local Cache + 进程内任务调度**，不得拆微服务。
7. **初始化数据必须基于实体与生成后的 DAL/CRUD，禁止手写一次性 SQL 文件**。
8. **前端每个功能模块必须提供“功能说明”与“操作指引”能力**。
9. **除当前阶段要求外，不要提前实现后续阶段的大量内容**，以避免 Agent 超时和大规模返工。

---

## 推荐执行顺序

| 阶段 | 提示词 | 目标 | 建议单次时长 |
|------|--------|------|-------------|
| 0 | `tenant-platform-stage-01-analysis-prompt.md` | 整体分析、模块拆分、实体/接口/中间件/测试清单 | 15-30 分钟 |
| 1 | `tenant-platform-stage-02-entity-modeling-prompt.md` | 实体、枚举、值对象、DTO 初稿；首次编译触发生成器 | 30-50 分钟 |
| 2 | `tenant-platform-stage-03-initdata-bootstrap-prompt.md` | 建表引导、初始化数据、缓存预热、初始化测试 | 30-50 分钟 |
| 3 | `tenant-platform-stage-04-backend-infrastructure-prompt.md` | 主程序骨架、中间件、缓存、认证/授权、后台任务骨架 | 30-50 分钟 |
| 4 | `tenant-platform-stage-05-backend-api-core-prompt.md` | 平台管理体系 + 租户生命周期/信息核心 API | 30-55 分钟 |
| 5 | `tenant-platform-stage-06-backend-api-extended-prompt.md` | 套餐/订阅/计费/API 集成/运营/日志通知等扩展 API + 接口测试 | 30-55 分钟 |
| 6 | `tenant-platform-stage-07-frontend-foundation-prompt.md` | 前端脚手架、登录鉴权、布局、路由、帮助组件、基础页面 | 30-50 分钟 |
| 7 | `tenant-platform-stage-08-frontend-modules-prompt.md` | 业务模块页面、API 封装、功能说明与操作指引 | 30-55 分钟 |
| 8 | `tenant-platform-stage-09-final-validation-prompt.md` | 全量校验、补漏、统一测试、交付说明 | 20-40 分钟 |

---

## 交付策略

每执行完一个阶段，请要求 Agent：

1. 先说明本阶段完成了哪些文件；
2. 再说明依赖的上阶段成果；
3. 最后给出本阶段的验证结果（如 `dotnet build`、`dotnet test`、前端 `npm run build` / 测试）；
4. 只在确认当前阶段稳定后，再进入下一个阶段。

---

## 最终指令

请不要一次性实现完整租户平台。
请严格按分阶段提示词逐个执行，并在每个阶段结束时输出：

- 已完成内容
- 未完成但留待后续阶段的内容
- 风险点与待确认点
- 本阶段验证结果
