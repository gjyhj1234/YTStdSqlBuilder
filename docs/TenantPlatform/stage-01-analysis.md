# 租户平台阶段 01 结构化分析清单

> 本文档是根据 `.github/prompts/tenant-platform-stage-01-analysis-prompt.md` 对租户平台进行的阶段 01 落地分析结果。
>
> 分析依据：
> - `docs/TenantPlatform/architecture.md`
> - `docs/TenantPlatform/database_dictionary.md`
> - `.github/prompts/entity-prompt.md`
> - `.github/prompts/tenant-platform-backend-prompt.md`
> - `.github/prompts/tenant-platform-initdata-prompt.md`
> - `.github/prompts/tenant-platform-frontend-prompt.md`

当前数据字典共包含：

- **14 个业务模块**
- **54 个建议实体**
- **61 处应生成枚举的 CHECK/状态字段**

本阶段只给出后续阶段可直接执行的拆分清单，不提前进入大规模实体/API/前端编码。

---

## 1. 整体实施顺序

建议后续严格按下表推进，以降低单次 Agent 超时和大范围返工风险。

| 顺序 | 阶段 | 目标 | 主要交付物 | 明确不做 |
| --- | --- | --- | --- | --- |
| 1 | 阶段 02：实体建模 | 落地 54 个实体、枚举、值对象、描述性类型 | `src/YTStdTenantPlatform/entity/TenantPlatform/*.cs`、`Domain/Enums`、首次编译 | 不实现全部 API 与前端 |
| 2 | 阶段 02：首次编译 | 触发 `YTStdEntity.Generator` 生成 DAL/CRUD/Audit/Desc | 生成代码、编译通过 | 不补充初始化数据 |
| 3 | 阶段 03：初始化与引导 | 基于实体与生成代码完成建表、初始化数据、缓存预热 | `Infrastructure/Initialization/`、初始化测试 | 不做完整业务接口 |
| 4 | 阶段 04：后端基础设施 | 搭建主程序骨架、中间件、认证授权、Local Cache、调度骨架 | `Program.cs`、`Bootstrap/`、`Infrastructure/*` | 不一次性实现所有业务域接口 |
| 5 | 阶段 05：核心 API | 先实现平台管理、租户生命周期、租户信息、套餐/订阅核心域 | `Application/Services`、`Endpoints/`、核心 API 测试 | 不批量铺开低优先级扩展域 |
| 6 | 阶段 06：扩展 API | 完成账单、Webhook、通知、文件、运营、基础设施类接口 | 扩展 Endpoints、接口测试 | 不提前进入大规模前端页面开发 |
| 7 | 阶段 07：前端骨架 | 完成登录、鉴权、布局、菜单、帮助组件、少量示例页 | `web/tenant-platform-web/` 骨架 | 不一次性生成全部业务页 |
| 8 | 阶段 08：前端业务模块 | 按接口稳定度逐模块接入页面、表单、权限显隐、指引 | 各模块页面、API 封装、前端测试 | 不重写后端接口语义 |
| 9 | 阶段 09：最终校验 | 全量构建、测试、补漏、交付说明 | 最终验证说明、风险收口 | 不继续扩张需求边界 |

### 1.1 推荐模块落地优先级

1. **P0 必须优先**：平台管理体系、租户生命周期体系、SaaS 套餐系统、订阅系统
2. **P1 紧随其后**：租户信息体系、租户配置中心、租户资源管理、计费与账单系统、日志与审计、通知系统、技术基础设施
3. **P2 后续扩展**：API 与集成平台、平台运营体系、文件与存储

### 1.2 阶段边界

- 阶段 01 输出的是**执行底稿**，不是完整业务实现。
- 阶段 02 以前不得脱离数据字典自行发明实体。
- 阶段 04 以前不应提前铺开所有 Endpoint。
- 阶段 07 以前不应批量生成页面。
- 任一阶段完成后都应先验证构建/测试，再进入下一阶段。

---

## 2. 实体清单与建模注意事项

### 2.1 建模总约束

- **所有手写实体统一放在** `src/YTStdTenantPlatform/entity/TenantPlatform/*.cs`
- **禁止使用裸 `TenantId` / `tenant_id` 作为平台实体字段名**，租户引用必须使用 `TenantRefId`、`OwnerTenantRefId`、`SourceTenantRefId`、`TargetTenantRefId` 等语义化命名
- 所有公开类型、公开方法、DTO、Endpoint 注册方法都应补齐**中文 XML 注释**
- `JSONB` 字段建议优先映射为 `JsonNode` 或受限值对象，不要对外暴露松散字典
- 追加型/事件型/统计型实体（如 `*_logs`、`*_events`、`*_stats`）通常不再额外生成复杂更新语义
- 需要幂等初始化的数据应优先依赖**业务唯一键**而不是固定 ID

### 2.2 14 个模块拆分清单

| 模块 | 实体数 | 建议优先级 | 前端依赖程度 | 说明 |
| --- | ---: | --- | --- | --- |
| 1. 平台管理体系 | 10 | P0 | 高 | 登录、权限、角色、平台安全与登录日志 |
| 2. 租户生命周期体系 | 4 | P0 | 高 | 租户主档、初始化任务、状态流转、数据任务 |
| 3. 租户信息体系 | 5 | P1 | 高 | 分组、域名、标签、归类关系 |
| 4. 租户资源管理 | 2 | P1 | 中 | 配额、资源使用统计 |
| 5. 租户配置中心 | 4 | P1 | 高 | 系统配置、功能开关、参数、品牌配置 |
| 6. SaaS 套餐系统 | 3 | P0 | 高 | 套餐主档、版本、能力明细 |
| 7. 订阅系统 | 3 | P0 | 高 | 订阅、试用、变更历史 |
| 8. 计费与账单系统 | 4 | P1 | 高 | 发票、行项目、支付单、退款 |
| 9. API 与集成平台 | 6 | P2 | 中 | API Key、用量统计、Webhook 事件与投递 |
| 10. 平台运营体系 | 2 | P2 | 中 | 日统计与平台监控 |
| 11. 日志与审计 | 3 | P1 | 中 | 操作日志、审计日志、系统日志 |
| 12. 通知系统 | 2 | P1 | 中 | 模板与通知记录 |
| 13. 文件与存储 | 3 | P2 | 中 | 存储策略、租户文件、文件访问策略 |
| 14. 技术基础设施 | 3 | P1 | 低 | 限流策略、数据隔离策略、基础设施组件 |

### 2.3 实体清单、枚举字段、审计建议与关系说明

> 说明：
> - “审计表建议”表示更适合开启 `NeedAuditTable` 或等效审计追踪。
> - “多对多/主从/索引说明”用于后续阶段决定 `Index`、轻量关系实体与主从建模方式。
> - “缓存建议”用于后续 `Infrastructure/Cache` 规划。

#### 模块 1：平台管理体系

| 实体 | 枚举字段 | 审计表建议 | 多对多/主从/索引说明 | 缓存建议 |
| --- | --- | --- | --- | --- |
| `PlatformUser` | `Status` | 是 | `Username`、`Email` 唯一；平台核心主档 | 登录用户缓存、用户状态缓存 |
| `PlatformRole` | `Status` | 是 | `Code` 唯一；权限模型主档 | 角色快照缓存 |
| `PlatformPermission` | `PermissionType` | 是 | `Code` 唯一；`ParentId` 自关联树形结构 | 权限码与菜单树缓存 |
| `PlatformRolePermission` | 无 | 否（关系表即可） | **多对多**：角色 ↔ 权限；唯一键 `(RoleId, PermissionId)` | 角色权限缓存 |
| `PlatformRoleMember` | 无 | 否（关系表即可） | **多对多**：用户 ↔ 角色；唯一键 `(RoleId, UserId)` | 用户角色缓存 |
| `PlatformPasswordPolicy` | 无 | 是 | 平台安全配置主档 | 默认策略缓存 |
| `PlatformSecurityPolicy` | 无 | 是 | 关联 `PlatformPasswordPolicy` | 安全策略缓存 |
| `PlatformIpWhitelist` | `SubjectType`、`Status` | 是 | `SubjectType + SubjectId` 形成作用域 | 安全策略缓存 |
| `PlatformMfaSetting` | `ProviderType`、`Status` | 是 | 关联平台用户，按用户聚合 | 登录安全缓存 |
| `PlatformLoginLog` | `LoginType`、`LoginStatus` | 否（本身即日志） | 索引 `idx_platform_login_logs_user_time` | 不做强缓存 |

#### 模块 2：租户生命周期体系

| 实体 | 枚举字段 | 审计表建议 | 多对多/主从/索引说明 | 缓存建议 |
| --- | --- | --- | --- | --- |
| `Tenant` | `SourceType`、`LifecycleStatus`、`IsolationMode` | 是 | `TenantCode` 唯一；关联当前套餐/当前订阅指针；**禁止裸 `TenantId`** | 租户主档缓存 |
| `TenantInitializationTask` | `TaskType`、`TaskStatus` | 否（任务自身保留历史） | 关联租户；适合作为后台任务表 | 初始化任务缓存可选 |
| `TenantLifecycleEvent` | `EventType`、`FromStatus`、`ToStatus` | 否（本身即事件） | 租户状态流转历史；索引 `idx_tenant_lifecycle_events_tenant_time` | 不做强缓存 |
| `TenantDataJob` | `JobType`、`JobStatus` | 否（本身即任务） | 索引 `idx_tenant_data_jobs_tenant_type` | 任务调度读取可选缓存 |

#### 模块 3：租户信息体系

| 实体 | 枚举字段 | 审计表建议 | 多对多/主从/索引说明 | 缓存建议 |
| --- | --- | --- | --- | --- |
| `TenantGroup` | 无 | 是 | `GroupCode` 唯一；`ParentId` 自关联树 | 分组树缓存 |
| `TenantDomain` | `DomainType`、`VerificationStatus` | 是 | `Domain` 唯一、`(TenantRefId, Domain)` 唯一 | 域名解析缓存可选 |
| `TenantTag` | `TagType` | 是 | `(TagKey, TagValue)` 唯一 | 标签字典缓存 |
| `TenantTagBinding` | 无 | 否（关系表即可） | **多对多**：租户 ↔ 标签；唯一键 `(TenantRefId, TagId)` | 标签绑定缓存可选 |
| `TenantGroupMember` | 无 | 否（关系表即可） | **多对多**：分组 ↔ 租户；唯一键 `(GroupId, TenantRefId)` | 分组成员缓存可选 |

#### 模块 4：租户资源管理

| 实体 | 枚举字段 | 审计表建议 | 多对多/主从/索引说明 | 缓存建议 |
| --- | --- | --- | --- | --- |
| `TenantResourceQuota` | `QuotaType`、`ResetCycle` | 是 | `(TenantRefId, QuotaType)` 唯一 | 租户资源配额缓存 |
| `TenantResourceUsageStat` | 无 | 否（统计表） | `(TenantRefId, MetricDate)` 唯一；索引 `idx_usage_stats_tenant_date` | 不建议长期缓存 |

#### 模块 5：租户配置中心

| 实体 | 枚举字段 | 审计表建议 | 多对多/主从/索引说明 | 缓存建议 |
| --- | --- | --- | --- | --- |
| `TenantSystemConfig` | 无 | 是 | `(TenantRefId)` 唯一，单租户单配置聚合 | 系统配置缓存 |
| `TenantFeatureFlag` | `RolloutType` | 是 | `(TenantRefId, FeatureKey)` 唯一；索引 `idx_feature_flags_tenant_enabled` | **功能开关缓存** |
| `TenantParameter` | `ParamType` | 是 | `(TenantRefId, ParamKey)` 唯一 | 参数缓存 |
| `TenantUiBranding` | 无 | 是 | `(TenantRefId)` 唯一 | UI 品牌配置缓存 |

#### 模块 6：SaaS 套餐系统

| 实体 | 枚举字段 | 审计表建议 | 多对多/主从/索引说明 | 缓存建议 |
| --- | --- | --- | --- | --- |
| `SaasPackage` | `Status` | 是 | `PackageCode` 应作为业务唯一键 | 套餐主档缓存 |
| `SaasPackageVersion` | `EditionType`、`BillingCycle` | 是 | `(PackageId, VersionCode)` 唯一；**主从：Package → PackageVersion** | 当前版本缓存 |
| `SaasPackageCapability` | `CapabilityType` | 是 | `(PackageVersionId, CapabilityKey)` 唯一；**主从：PackageVersion → Capability** | 套餐能力缓存 |

#### 模块 7：订阅系统

| 实体 | 枚举字段 | 审计表建议 | 多对多/主从/索引说明 | 缓存建议 |
| --- | --- | --- | --- | --- |
| `TenantSubscription` | `SubscriptionStatus`、`SubscriptionType` | 是 | 索引 `idx_subscriptions_tenant_status`；租户当前订阅主档 | 当前订阅缓存 |
| `TenantTrial` | `Status` | 是 | `(TenantRefId, StartedAt)` 唯一；试用记录历史 | 试用状态缓存可选 |
| `TenantSubscriptionChange` | `ChangeType` | 否（变更历史表） | 订阅变更流水；适合按租户时间查询 | 不做强缓存 |

#### 模块 8：计费与账单系统

| 实体 | 枚举字段 | 审计表建议 | 多对多/主从/索引说明 | 缓存建议 |
| --- | --- | --- | --- | --- |
| `BillingInvoice` | `InvoiceStatus` | 是 | 索引 `idx_invoices_tenant_status`；账单头 | 只缓存待处理账单摘要 |
| `BillingInvoiceItem` | `ItemType` | 否（明细表） | **主从：Invoice → InvoiceItem** | 不做强缓存 |
| `PaymentOrder` | `PaymentChannel`、`PaymentStatus` | 是 | 索引 `idx_payment_orders_tenant_status` | 支付状态短期缓存可选 |
| `PaymentRefund` | `RefundStatus` | 是 | 与 `PaymentOrder` 形成退款主从关系 | 不做强缓存 |

#### 模块 9：API 与集成平台

| 实体 | 枚举字段 | 审计表建议 | 多对多/主从/索引说明 | 缓存建议 |
| --- | --- | --- | --- | --- |
| `TenantApiKey` | `Status` | 是 | 应按 `api_key_hash` 或业务 Key 唯一处理；敏感字段只存摘要/密文 | API Key 校验缓存 |
| `TenantApiUsageStat` | 无 | 否（统计表） | 多个唯一索引：带/不带 Key 的用量去重；索引 `idx_api_usage_tenant_date` | 不做强缓存 |
| `WebhookEvent` | 无 | 是 | 事件定义主档 | Webhook 事件字典缓存 |
| `TenantWebhook` | `Status` | 是 | 单租户 Webhook 配置主档 | Webhook 配置缓存 |
| `TenantWebhookEvent` | 无 | 否（关系表即可） | **多对多**：Webhook ↔ Event；唯一键 `(WebhookId, EventId)` | Webhook 订阅缓存 |
| `WebhookDeliveryLog` | `DeliveryStatus` | 否（本身即投递日志） | 索引 `idx_webhook_delivery_status_time` | 不做强缓存 |

#### 模块 10：平台运营体系

| 实体 | 枚举字段 | 审计表建议 | 多对多/主从/索引说明 | 缓存建议 |
| --- | --- | --- | --- | --- |
| `TenantDailyStat` | 无 | 否（统计表） | `(TenantRefId, StatDate)` 唯一 | 不做强缓存 |
| `PlatformMonitorMetric` | `MetricType` | 否（监控指标表） | 索引 `idx_platform_monitor_metrics_type_time` | 可做短周期监控快照缓存 |

#### 模块 11：日志与审计

| 实体 | 枚举字段 | 审计表建议 | 多对多/主从/索引说明 | 缓存建议 |
| --- | --- | --- | --- | --- |
| `OperationLog` | `OperatorType`、`OperationResult` | 否（本身即操作日志） | 索引 `idx_operation_logs_tenant_time` | 不做强缓存 |
| `AuditLog` | `Severity` | 否（本身即审计日志） | 索引 `idx_audit_logs_tenant_time` | 不做强缓存 |
| `SystemLog` | `LogLevel` | 否（本身即系统日志） | 按时间追加写入 | 不做强缓存 |

#### 模块 12：通知系统

| 实体 | 枚举字段 | 审计表建议 | 多对多/主从/索引说明 | 缓存建议 |
| --- | --- | --- | --- | --- |
| `NotificationTemplate` | `Channel`、`Status` | 是 | `TemplateCode` 应作为业务唯一键 | 模板缓存 |
| `Notification` | `Channel`、`SendStatus` | 否（消息记录表） | 索引 `idx_notifications_tenant_status` | 仅待发送队列可短期缓存 |

#### 模块 13：文件与存储

| 实体 | 枚举字段 | 审计表建议 | 多对多/主从/索引说明 | 缓存建议 |
| --- | --- | --- | --- | --- |
| `StorageStrategy` | `ProviderType`、`Status` | 是 | 存储策略主档 | 存储策略缓存 |
| `TenantFile` | `UploaderType`、`Visibility` | 是 | 索引 `idx_tenant_files_tenant_visibility` | 文件元数据热点缓存可选 |
| `FileAccessPolicy` | `SubjectType`、`PermissionCode` | 是 | 唯一索引 `uq_file_access_policy_scoped_subject` / `uq_file_access_policy_public`；作用域授权关系 | 文件访问策略缓存 |

#### 模块 14：技术基础设施

| 实体 | 枚举字段 | 审计表建议 | 多对多/主从/索引说明 | 缓存建议 |
| --- | --- | --- | --- | --- |
| `RateLimitPolicy` | `SubjectType`、`Status` | 是 | `(SubjectType, SubjectKey)` 唯一 | **限流策略缓存** |
| `DataIsolationPolicy` | `IsolationType`、`Status` | 是 | 平台隔离策略主档 | **隔离策略缓存** |
| `InfrastructureComponent` | `ComponentType`、`Status` | 是 | `(ComponentType, ComponentName)` 唯一 | 基础设施组件字典缓存 |

### 2.4 需要重点识别的关系类型

#### 2.4.1 多对多关系表

- `PlatformRolePermission`
- `PlatformRoleMember`
- `TenantTagBinding`
- `TenantGroupMember`
- `TenantWebhookEvent`

#### 2.4.2 主从关系

- `SaasPackage` → `SaasPackageVersion` → `SaasPackageCapability`
- `BillingInvoice` → `BillingInvoiceItem`
- `PaymentOrder` → `PaymentRefund`
- `TenantWebhook` → `WebhookDeliveryLog`（业务上属于一对多历史）

#### 2.4.3 树形/层级结构

- `PlatformPermission`（权限/菜单树）
- `TenantGroup`（租户分组树）

### 2.5 适合优先启用审计的实体

后续阶段优先考虑开启审计表或等效审计追踪的实体：

- `PlatformUser`
- `PlatformRole`
- `PlatformPermission`
- `PlatformPasswordPolicy`
- `PlatformSecurityPolicy`
- `PlatformIpWhitelist`
- `PlatformMfaSetting`
- `Tenant`
- `TenantGroup`
- `TenantDomain`
- `TenantTag`
- `TenantResourceQuota`
- `TenantSystemConfig`
- `TenantFeatureFlag`
- `TenantParameter`
- `TenantUiBranding`
- `SaasPackage`
- `SaasPackageVersion`
- `SaasPackageCapability`
- `TenantSubscription`
- `TenantTrial`
- `BillingInvoice`
- `PaymentOrder`
- `PaymentRefund`
- `TenantApiKey`
- `WebhookEvent`
- `TenantWebhook`
- `NotificationTemplate`
- `StorageStrategy`
- `TenantFile`
- `FileAccessPolicy`
- `RateLimitPolicy`
- `DataIsolationPolicy`
- `InfrastructureComponent`

### 2.6 适合优先做 Local Cache 的数据

- 权限树与权限码：`PlatformPermission`
- 用户-角色：`PlatformRoleMember`
- 角色-权限：`PlatformRolePermission`
- 默认安全/密码策略：`PlatformPasswordPolicy`、`PlatformSecurityPolicy`
- 租户功能开关：`TenantFeatureFlag`
- 租户系统配置 / 参数 / 品牌：`TenantSystemConfig`、`TenantParameter`、`TenantUiBranding`
- 套餐/版本/能力：`SaasPackage`、`SaasPackageVersion`、`SaasPackageCapability`
- 当前订阅与租户主档摘要：`Tenant`、`TenantSubscription`
- 限流与隔离：`RateLimitPolicy`、`DataIsolationPolicy`
- Webhook 事件订阅：`WebhookEvent`、`TenantWebhook`、`TenantWebhookEvent`

---

## 3. 后端工程结构建议

### 3.1 项目结构

```text
src/YTStdTenantPlatform/
├── Program.cs
├── Bootstrap/
│   ├── ServiceRegistration.cs
│   ├── RouteRegistration.cs
│   └── StartupInitialization.cs
├── entity/
│   └── TenantPlatform/
│       ├── PlatformUser.cs
│       ├── PlatformRole.cs
│       ├── Tenant.cs
│       ├── SaasPackage.cs
│       └── ...
├── Domain/
│   ├── Enums/
│   ├── ValueObjects/
│   └── Descriptors/
├── Application/
│   ├── Services/
│   ├── Commands/
│   ├── Queries/
│   └── Dtos/
├── Infrastructure/
│   ├── Auth/
│   ├── Cache/
│   ├── Initialization/
│   ├── Scheduling/
│   ├── Persistence/
│   └── Middleware/
└── Endpoints/
    ├── PlatformManagementEndpoints.cs
    ├── TenantLifecycleEndpoints.cs
    ├── TenantInfoEndpoints.cs
    ├── TenantResourceEndpoints.cs
    ├── TenantConfigEndpoints.cs
    ├── PackageEndpoints.cs
    ├── SubscriptionEndpoints.cs
    ├── BillingEndpoints.cs
    ├── IntegrationEndpoints.cs
    ├── OperationsEndpoints.cs
    ├── AuditEndpoints.cs
    ├── NotificationEndpoints.cs
    ├── FileStorageEndpoints.cs
    └── InfrastructureEndpoints.cs
```

### 3.2 分层职责

| 层 | 主要职责 | 说明 |
| --- | --- | --- |
| `Program.cs` | 主程序入口 | 仅做宿主组装、日志、配置、路由挂接 |
| `Bootstrap/` | 启动装配 | 服务注册、路由注册、启动初始化编排 |
| `Domain/Enums` | 枚举类型 | 承接 61 处 CHECK/状态字段 |
| `Domain/ValueObjects` | 值对象 | 邮箱、域名、金额、配额策略、灰度配置等 |
| `Application/Services` | 业务服务 | 承担编排、权限判断、事务边界、缓存刷新 |
| `Application/Dtos` | 输入输出模型 | 供 API 与前端对接，需补中文 XML 注释 |
| `Infrastructure/Auth` | 认证鉴权 | Token、当前用户解析、权限码校验 |
| `Infrastructure/Cache` | Local Cache | 启动预热、原子替换、定时轻刷新、显式失效 |
| `Infrastructure/Initialization` | 建表与初始化 | DAL 建表、基础数据填充、缓存预热 |
| `Infrastructure/Scheduling` | 进程内任务调度 | 初始化任务、到期提醒、账单任务、Webhook 重试 |
| `Endpoints/` | Minimal API 路由分组 | 只负责参数绑定、结果转换、调用应用服务 |

### 3.3 阶段 02 后需要率先补齐的后端类型

1. `Domain/Enums/*`
2. 平台用户/角色/权限相关 DTO
3. 租户主档、套餐、订阅相关 DTO
4. 分页请求/响应模型
5. 通用结果模型、错误响应模型
6. 审计查询过滤 DTO

---

## 4. 初始化数据结构建议

### 4.1 初始化范围拆分

#### 必须先做的正式初始化数据

- 默认超级管理员
- 默认管理员角色
- 默认平台权限树（菜单/API/操作）
- 默认角色-权限绑定
- 默认角色-成员绑定
- 默认密码策略
- 默认安全策略
- 默认套餐、套餐版本、套餐能力
- 默认通知模板
- 默认限流策略 / 数据隔离策略 / 基础设施组件字典

#### 可以延后做的演示数据

- 演示租户
- 演示试用订阅
- 演示资源配额
- 演示通知记录

### 4.2 初始化目录结构建议

```text
Infrastructure/Initialization/
├── PlatformSeedContext.cs
├── ISeedContributor.cs
├── SeedRunner.cs
├── Contributors/
│   ├── PlatformUserSeedContributor.cs
│   ├── PermissionSeedContributor.cs
│   ├── RoleSeedContributor.cs
│   ├── SecurityPolicySeedContributor.cs
│   ├── PackageSeedContributor.cs
│   ├── NotificationTemplateSeedContributor.cs
│   ├── RateLimitSeedContributor.cs
│   └── DataIsolationSeedContributor.cs
└── SeedData/
    ├── DefaultPlatformUsers.cs
    ├── DefaultPermissions.cs
    ├── DefaultRoles.cs
    ├── DefaultPackages.cs
    ├── DefaultNotificationTemplates.cs
    └── DemoTenantData.cs
```

### 4.3 初始化顺序建议

1. 建表 / 建索引
2. 平台用户与安全策略
3. 权限树
4. 角色与角色成员
5. 套餐、版本、能力
6. 通知模板
7. 限流/隔离/基础设施组件
8. 演示数据（可选）
9. 缓存预热

### 4.4 幂等策略

- 平台用户：按 `Username`
- 角色：按 `Code`
- 权限：按 `Code`
- 套餐：按 `PackageCode`
- 套餐版本：按 `(PackageId, VersionCode)`
- 套餐能力：按 `(PackageVersionId, CapabilityKey)`
- 通知模板：按 `TemplateCode`
- 限流策略：按 `(SubjectType, SubjectKey)`
- 基础设施组件：按 `(ComponentType, ComponentName)`

---

## 5. WebAPI 分组建议

> 标记规则：
> - **前端依赖：高** = 前端页面生成前需要优先稳定的接口
> - 每组都至少考虑：列表、详情、新增、编辑、启停/状态流转、审计查询、批量操作、分页筛选

| 模块 | 推荐路由组 | 核心操作 | 前端依赖 |
| --- | --- | --- | --- |
| 平台管理体系 | `/api/platform-users` | 列表、详情、新增、编辑、启停、重置密码、审计查询 | 高 |
| 平台管理体系 | `/api/platform-roles` | 列表、详情、新增、编辑、授权、成员管理、启停 | 高 |
| 平台管理体系 | `/api/platform-permissions` | 权限树、详情、权限码查询 | 高 |
| 平台管理体系 | `/api/password-policies` | 列表、详情、新增、编辑、设默认 | 中 |
| 平台管理体系 | `/api/security-policies` | 列表、详情、新增、编辑、设默认 | 中 |
| 平台管理体系 | `/api/ip-whitelists` | 列表、详情、新增、编辑、启停 | 中 |
| 平台管理体系 | `/api/mfa-settings` | 用户 MFA 状态查询、启用、禁用、重置 | 中 |
| 平台管理体系 | `/api/login-logs` | 登录日志列表、详情、导出 | 中 |
| 租户生命周期体系 | `/api/tenants` | 列表、详情、新增、编辑、启用、暂停、恢复、关闭、删除 | 高 |
| 租户生命周期体系 | `/api/tenant-initialization-tasks` | 列表、详情、触发初始化、重试、状态查询 | 高 |
| 租户生命周期体系 | `/api/tenant-lifecycle-events` | 生命周期事件列表、详情 | 中 |
| 租户生命周期体系 | `/api/tenant-data-jobs` | 数据归档/备份/迁移/清理任务创建、查询、重试 | 中 |
| 租户信息体系 | `/api/tenant-groups` | 列表、树、详情、新增、编辑、归类 | 高 |
| 租户信息体系 | `/api/tenant-domains` | 列表、详情、新增、验证、设主域名 | 高 |
| 租户信息体系 | `/api/tenant-tags` | 标签字典列表、详情、新增、编辑 | 高 |
| 租户信息体系 | `/api/tenant-tag-bindings` | 批量绑定、批量解绑、查询 | 中 |
| 租户信息体系 | `/api/tenant-group-members` | 批量归组、取消归组、查询 | 中 |
| 租户资源管理 | `/api/tenant-resource-quotas` | 列表、详情、新增、编辑、重置周期设置 | 高 |
| 租户资源管理 | `/api/tenant-resource-usage-stats` | 列表、趋势图、导出 | 中 |
| 租户配置中心 | `/api/tenant-system-configs` | 获取、更新、回滚、审计查询 | 高 |
| 租户配置中心 | `/api/tenant-feature-flags` | 列表、启停、灰度发布、批量更新 | 高 |
| 租户配置中心 | `/api/tenant-parameters` | 列表、详情、新增、编辑、删除 | 高 |
| 租户配置中心 | `/api/tenant-ui-brandings` | 获取、更新、预览 | 高 |
| SaaS 套餐系统 | `/api/saas-packages` | 列表、详情、新增、编辑、启停 | 高 |
| SaaS 套餐系统 | `/api/saas-package-versions` | 版本列表、详情、新增、编辑、发布 | 高 |
| SaaS 套餐系统 | `/api/saas-package-capabilities` | 能力明细列表、批量更新 | 高 |
| 订阅系统 | `/api/tenant-subscriptions` | 列表、详情、开通、升级、降级、续费、取消 | 高 |
| 订阅系统 | `/api/tenant-trials` | 试用开通、转正式、终止、查询 | 高 |
| 订阅系统 | `/api/tenant-subscription-changes` | 变更历史查询 | 中 |
| 计费与账单系统 | `/api/billing-invoices` | 列表、详情、生成、作废、导出 | 高 |
| 计费与账单系统 | `/api/billing-invoice-items` | 明细查询 | 中 |
| 计费与账单系统 | `/api/payment-orders` | 列表、详情、状态更新、对账 | 高 |
| 计费与账单系统 | `/api/payment-refunds` | 发起退款、详情、状态查询 | 中 |
| API 与集成平台 | `/api/tenant-api-keys` | 列表、创建、禁用、删除、重置密钥 | 中 |
| API 与集成平台 | `/api/tenant-api-usage-stats` | 用量统计查询、趋势 | 中 |
| API 与集成平台 | `/api/webhook-events` | 事件字典查询 | 中 |
| API 与集成平台 | `/api/tenant-webhooks` | 列表、详情、新增、编辑、启停、测试推送 | 中 |
| API 与集成平台 | `/api/tenant-webhook-events` | 事件订阅绑定查询/更新 | 中 |
| API 与集成平台 | `/api/webhook-delivery-logs` | 投递日志查询、重试 | 中 |
| 平台运营体系 | `/api/tenant-daily-stats` | 列表、趋势、导出 | 中 |
| 平台运营体系 | `/api/platform-monitor-metrics` | 监控指标查询、聚合视图 | 中 |
| 日志与审计 | `/api/operation-logs` | 列表、详情、按租户过滤 | 中 |
| 日志与审计 | `/api/audit-logs` | 列表、详情、按实体/时间/风险等级过滤 | 高 |
| 日志与审计 | `/api/system-logs` | 列表、详情、按等级过滤 | 中 |
| 通知系统 | `/api/notification-templates` | 列表、详情、新增、编辑、启停 | 中 |
| 通知系统 | `/api/notifications` | 列表、详情、重发、标记已读 | 中 |
| 文件与存储 | `/api/storage-strategies` | 列表、详情、新增、编辑、启停 | 中 |
| 文件与存储 | `/api/tenant-files` | 上传、下载、列表、详情、删除、权限查询 | 中 |
| 文件与存储 | `/api/file-access-policies` | 批量授权、取消授权、详情查询 | 中 |
| 技术基础设施 | `/api/rate-limit-policies` | 列表、详情、新增、编辑、启停 | 中 |
| 技术基础设施 | `/api/data-isolation-policies` | 列表、详情、新增、编辑、启停 | 低 |
| 技术基础设施 | `/api/infrastructure-components` | 列表、详情、新增、编辑、降级/恢复状态 | 低 |

---

## 6. 中间件与缓存设计建议

### 6.1 中间件 / 基础设施清单

| 组件 | 作用 | 必须输出的能力 |
| --- | --- | --- |
| 全局异常处理中间件 | 统一错误返回 | 标准错误响应、异常日志、I18n 友好消息 |
| 请求日志 / TraceId 中间件 | 链路追踪 | TraceId、耗时、路由、租户/用户上下文 |
| 平台权限中间件 | 权限判定 | 基于权限码、角色快照、菜单/API/操作权限 |
| 限流中间件 | 防滥用 | 按租户、IP、用户、API Key 做本地限流 |
| 审计记录中间件 | 合规追踪 | 记录写操作的前后状态、操作人、实体标识 |
| Local Cache 刷新 / 失效协调组件 | 缓存一致性 | 显式失效、局部刷新、定时轻量刷新 |
| 启动初始化引导器 | 启动编排 | 建表、初始化数据、缓存预热、健康检查 |

### 6.2 建议优先落地的缓存

1. `PermissionSnapshotCache`
2. `RolePermissionCache`
3. `UserRoleCache`
4. `FeatureFlagCache`
5. `TenantPlatformConfigCache`
6. `PackageCapabilityCache`
7. `RateLimitPolicyCache`

### 6.3 缓存刷新策略

- **启动预热**：主程序启动后一次性构建只读快照
- **显式失效**：写操作成功后刷新相关缓存
- **定时轻刷新**：适合策略/模板/组件字典等低频变更数据
- **原子替换**：先构建新快照，再替换旧引用，避免读路径加锁

### 6.4 后台任务建议

- 租户初始化任务执行器
- 订阅到期提醒任务
- 账单生成任务
- Webhook 投递重试任务
- 通知发送补偿任务

---

## 7. 前端工程结构建议

### 7.1 工程骨架

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
│   │   ├── logs/
│   │   ├── files/
│   │   └── infrastructure/
│   ├── components/
│   ├── composables/
│   ├── constants/
│   ├── utils/
│   └── styles/
└── package.json
```

### 7.2 路由与菜单建议

一级菜单必须直接映射 `architecture.md` 中的 14 个模块：

1. 平台管理体系
2. 租户生命周期体系
3. 租户信息体系
4. 租户资源管理
5. 租户配置中心
6. SaaS 套餐系统
7. 订阅系统
8. 计费与账单系统
9. API 与集成平台
10. 平台运营体系
11. 日志与审计
12. 通知系统
13. 文件与存储
14. 技术基础设施

### 7.3 页面模式建议

- 列表页：`DxDataGrid`
- 新增/编辑：`DxForm`
- 详情：`DxDrawer` / `DxPopup`
- 树结构：`DxTreeList`
- 多选：`DxTagBox` / `DxSelectBox`
- 详情分栏：`DxTabs`

---

## 8. 前端“功能说明 / 操作指引”统一设计建议

### 8.1 统一组件

建议所有页面共用以下帮助组件：

- `FunctionDescriptionCard`
- `OperationGuideDrawer`
- `FieldHelpTooltip`
- `FaqPanel`
- `QuickEntryLinks`

### 8.2 每个页面至少要展示的说明内容

#### 功能说明

- 模块用途
- 关键数据范围
- 权限要求
- 典型风险提示

#### 操作指引

- 进入路径
- 常见操作步骤
- 关键字段填写说明
- 常见错误处理

### 8.3 适合优先接入帮助组件的页面

- 平台用户管理
- 角色授权
- 租户开通 / 状态流转
- 功能开关灰度配置
- 套餐能力配置
- 订阅升级 / 续费
- 账单导出 / 退款
- API Key 创建
- Webhook 测试推送
- 文件授权策略

---

## 9. 测试阶段建议

### 9.1 后续阶段必须覆盖的测试类型

| 测试类型 | 目标 | 建议所在阶段 |
| --- | --- | --- |
| 实体编译与生成器协同测试 | 验证实体、索引、审计、枚举生成是否正确 | 阶段 02 |
| 初始化数据幂等测试 | 验证重复执行不产生脏数据 | 阶段 03 |
| 中间件行为测试 | 验证异常、TraceId、审计、权限与限流 | 阶段 04 |
| 权限与限流测试 | 验证缓存命中、权限码判断、限流命中 | 阶段 04-05 |
| 核心接口测试 | 平台用户、角色、租户、套餐、订阅主流程 | 阶段 05 |
| 扩展接口测试 | 账单、Webhook、通知、文件、策略管理 | 阶段 06 |
| 前端路由与权限显隐测试 | 菜单可见性、按钮权限、未授权路由拦截 | 阶段 07-08 |
| 帮助组件测试 | “功能说明 / 操作指引”渲染与入口可见性 | 阶段 07-08 |
| 最终全链路测试 | 构建、后端接口、前端构建与回归 | 阶段 09 |

### 9.2 推荐重点测试场景

#### 实体 / 生成器

- `Tenant`、`PlatformPermission`、`SaasPackageVersion` 等关键实体首次编译触发生成
- 多对多关系实体索引/唯一键生成
- 审计表开启实体的生成代码验证

#### 初始化

- 首次初始化成功
- 重复执行幂等
- 缺失部分关系数据时自动补齐
- 初始化后权限缓存、配置缓存成功预热

#### API

- 角色授权后缓存刷新
- 租户状态流转时事件表写入
- 功能开关修改后缓存立即生效
- 订阅升级后 `Tenant.CurrentPlanId` / `CurrentSubscriptionId` 一致性

#### 前端

- 菜单根据权限树显隐
- 列表页筛选/分页/状态标签
- 帮助抽屉、说明卡片与字段帮助提示正常展示

---

## 10. 风险点与容易超时的点

### 10.1 主要风险

| 风险点 | 说明 | 建议应对 |
| --- | --- | --- |
| 裸 `TenantId` 误用 | 会触发现有框架租户分区语义，导致平台表建模偏离 | 阶段 02 统一用 `TenantRefId` 语义化命名 |
| 一次性生成全部实体/API/前端 | 极易超时，也不利于回滚 | 严格按 stage-02 ~ stage-09 分阶段推进 |
| 权限缓存与数据库不一致 | 授权、菜单、功能开关更新后易出现缓存过期问题 | 写操作成功后显式刷新相关缓存 |
| 初始化数据依赖固定 ID | 容易破坏幂等和环境兼容性 | 全部改用业务唯一键查找与补齐 |
| 日志/统计表误开审计 | 事件型表本身就是历史记录，重复审计会造成噪音 | 对 `*_logs` / `*_events` / `*_stats` 以追加型实体处理 |
| 前后端接口语义漂移 | 前端过早开始实现，后端 DTO/路由仍在变化 | 阶段 07 仅做骨架，阶段 08 再批量接入 |

### 10.2 容易超时的环节

| 环节 | 超时原因 | 建议拆分 |
| --- | --- | --- |
| 实体阶段 | 54 个实体 + 61 处枚举字段一次性落地量大 | 先做 P0/P1 模块，再补 P2 |
| 初始化阶段 | 权限树、角色、套餐、模板等种子数据较多 | 先正式数据，演示数据后置 |
| API 阶段 | 14 个模块接口面太广 | 阶段 05 做核心域，阶段 06 做扩展域 |
| 前端阶段 | 页面量和交互点多 | 阶段 07 做骨架与帮助组件，阶段 08 分模块接入 |
| 最终校验 | 后端/前端/测试全量跑耗时 | 先做分阶段针对性验证，再做最终全量验证 |

### 10.3 进入阶段 02 前的完成定义

- 已明确 14 个模块与优先级
- 已整理 54 个实体的落地清单
- 已标记主要枚举字段、审计候选、多对多、主从关系和 Local Cache 范围
- 已明确后端结构、初始化结构、API 分组和前端工程骨架
- 已给出测试与风险清单，后续阶段可以直接按本清单实施

