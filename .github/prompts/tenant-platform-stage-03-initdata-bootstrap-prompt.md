# 租户平台阶段 03：数据库引导、初始化数据与缓存预热提示词

## 目标

在实体与生成代码稳定后，本阶段负责：

1. 通过生成后的 DAL / CRUD 能力创建数据库结构；
2. 实现初始化数据基础设施（SeedRunner / Contributor / 强类型 SeedData）；
3. 实现幂等初始化策略；
4. 实现缓存预热与启动引导；
5. 添加初始化相关测试。

---

## 必须阅读的文件

- `.github/prompts/tenant-platform-initdata-prompt.md`
- `.github/prompts/tenant-platform-backend-prompt.md`
- `.github/prompts/entity-prompt.md`
- `docs/TenantPlatform/database_dictionary.md`
- `docs/TenantPlatform/architecture.md`

---

## 本阶段实现范围

### 1. 启动初始化与数据库引导
- `Bootstrap/StartupInitialization.cs` 或等效组件
- 建表 / 索引维护调用链
- 初始化顺序编排

### 2. 初始化数据基础设施
建议目录：

```text
Infrastructure/Initialization/
├── PlatformSeedContext.cs
├── ISeedContributor.cs
├── SeedRunner.cs
├── Contributors/
└── SeedData/
```

### 3. 初始化数据范围
至少覆盖：
- 默认平台管理员
- 默认角色与权限
- 默认角色-权限绑定、角色-成员绑定
- 默认菜单权限 / API 权限 / 操作权限
- 默认安全策略 / 密码策略
- 默认功能开关 / 系统参数 / UI 品牌配置
- 默认套餐 / 套餐版本 / 能力配置
- 默认通知模板
- 可选演示数据（必须与正式种子分层）

### 4. 缓存预热
至少考虑：
- `PermissionSnapshotCache`
- `RolePermissionCache`
- `UserRoleCache`
- `FeatureFlagCache`
- `TenantPlatformConfigCache`

---

## 幂等策略要求

请明确实现并说明：
1. 不存在则新增；
2. 已存在则跳过或补齐缺失；
3. 多对多关系按集合补齐，不重复插入；
4. 不删除用户后续新增的数据；
5. 敏感字段只写摘要 / 占位结构。

---

## 本阶段交付物

- 初始化基础设施代码
- SeedData 强类型数据清单
- StartupInitialization / 数据库引导代码
- 缓存预热与初始化联动代码
- 初始化测试（首次执行、重复执行、缺失关系补齐、缓存预热触发）

---

## 验证要求

至少执行并报告：
- `dotnet build YTStd.slnx`
- 与初始化相关的测试（如新增测试项目，请执行定向测试）

---

## 明确禁止

- 不要退回到手写 SQL 文件；
- 不要把初始化逻辑做成独立微服务；
- 不要在本阶段一次性实现所有业务 API；
- 不要把缓存预热遗漏到后续再说。
