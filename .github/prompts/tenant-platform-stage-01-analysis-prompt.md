# 租户平台阶段 01：整体分析与任务拆分提示词

## 目标

本阶段**只做整体分析与拆分，不进行大规模编码**。

请基于以下文件：
- `docs/TenantPlatform/architecture.md`
- `docs/TenantPlatform/database_dictionary.md`
- `.github/prompts/entity-prompt.md`
- `.github/prompts/tenant-platform-backend-prompt.md`
- `.github/prompts/tenant-platform-initdata-prompt.md`
- `.github/prompts/tenant-platform-frontend-prompt.md`

输出一份适合后续 GitHub Agents 分阶段执行的落地清单。

---

## 本阶段必须完成的内容

1. 输出**模块拆分清单**：
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

2. 输出**实体清单**：
   - 从 `database_dictionary.md` 中梳理所有建议实体名；
   - 标记哪些字段应生成枚举；
   - 标记哪些表适合审计表；
   - 标记哪些表需要索引、唯一键、多对多关系、主从关系。

3. 输出**后端分层清单**：
   - `Program.cs`
   - `Bootstrap/`
   - `Application/Services`
   - `Infrastructure/Auth`
   - `Infrastructure/Cache`
   - `Infrastructure/Initialization`
   - `Infrastructure/Scheduling`
   - `Endpoints/`
   - `Domain/Enums`、`ValueObjects`、`Descriptors`

4. 输出**中间件 / 基础设施清单**：
   - 全局异常处理中间件
   - 请求日志 / TraceId 中间件
   - 平台权限中间件
   - 限流中间件
   - 审计记录中间件
   - Local Cache 刷新 / 失效协调组件
   - 启动初始化引导器

5. 输出**接口清单**：
   - 按模块列出 API 路由分组；
   - 区分列表、详情、新增、编辑、启停、授权、状态流转、审计查询；
   - 标记前端依赖程度高的接口。

6. 输出**测试清单**：
   - 实体编译与生成器协同测试
   - 初始化数据幂等测试
   - 中间件行为测试
   - 权限与限流测试
   - 核心接口测试
   - 前端路由、权限显隐、帮助组件测试

---

## 输出要求

请输出一份结构化分析结果，至少包含以下章节：

1. 整体实施顺序
2. 实体清单与建模注意事项
3. 后端工程结构建议
4. 初始化数据结构建议
5. WebAPI 分组建议
6. 中间件与缓存设计建议
7. 前端工程结构建议
8. 前端“功能说明 / 操作指引”统一设计建议
9. 测试阶段建议
10. 风险点与容易超时的点

---

## 明确禁止

- 不要直接开始大规模创建全部实体与 API；
- 不要跳过分析直接生成前端页面；
- 不要脱离 `docs/TenantPlatform/*.md` 自行发明业务模块。

---

## 完成定义

本阶段完成的标志是：
- 后续每个阶段都能直接依据你的清单执行；
- 能明确区分“实体阶段 / 初始化阶段 / 基础设施阶段 / API 阶段 / 前端阶段 / 验证阶段”；
- 每个后续阶段的范围边界清晰，不会一次性过大。
