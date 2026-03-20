# 前端 API 集成开发提示词

> **用途**：本文档为前端开发者（或 AI Agent）提供完整的 API 集成指南，可直接用于在 Vue 3 + TypeScript + DevExtreme 项目中实现租户管理平台的所有后端接口对接。

---

## 1. 项目背景

本项目是一个 **多租户 SaaS 管理平台（租户管理平台）**，用于管理租户生命周期、订阅计费、权限控制、通知系统、文件存储等核心业务。

- **后端技术栈**：.NET 10 Minimal API + NativeAOT，数据库为 PostgreSQL
- **前端技术栈**：Vue 3 + TypeScript + DevExtreme（DevExpress UI 组件库）
- **通信协议**：RESTful HTTP API，JSON 格式
- **国际化**：后端返回 i18n 消息键（非硬编码文本），前端负责根据消息键展示本地化文字

---

## 2. API 规范说明

### 2.1 Base URL 配置

```typescript
// .env.development
VITE_API_BASE_URL=http://localhost:5000/api

// .env.production
VITE_API_BASE_URL=/api
```

所有接口路径均以 `/api` 为前缀，例如 `/api/auth/login`。

### 2.2 统一响应格式 `ApiResult<T>`

所有接口返回统一的 JSON 包装结构：

```json
{
  "code": 0,
  "message": "operation.success",
  "data": { ... }
}
```

| 字段 | 类型 | 说明 |
|------|------|------|
| `code` | `number` | `0` 表示成功，非 `0` 为错误码（对应后端 `ErrorCodes` 常量） |
| `message` | `string` | i18n 消息键（对应后端 `Messages` 常量），前端用 `vue-i18n` 的 `t(message)` 翻译后展示 |
| `data` | `T \| undefined` | 响应数据，仅成功时携带；无数据的写操作（创建/更新/删除）不含此字段 |

### 2.3 认证机制

- **登录**：`POST /api/auth/login` 返回 `LoginRepDTO`，其中包含 `token` 字段
- **后续请求**：在 HTTP 请求头中携带 `Authorization: Bearer <token>`
- **刷新令牌**：`POST /api/auth/refresh`，可通过请求体或 `Authorization` 头传入当前 token
- **当前用户**：`GET /api/auth/me`，未携带有效 token 返回匿名用户
- **无需认证的接口**：健康检查 (`/api/health/*`) 和认证模块 (`/api/auth/*`)

### 2.4 分页请求参数

所有分页列表接口通过 Query String 传递以下参数：

| 参数名 | 类型 | 是否必填 | 默认值 | 说明 |
|--------|------|----------|--------|------|
| `Page` | `number` | 否 | `1` | 页码，从 1 开始 |
| `PageSize` | `number` | 否 | `20` | 每页条数，范围 `1~200` |
| `Keyword` | `string` | 否 | - | 关键字搜索 |
| `Status` | `string` | 否 | - | 状态过滤 |

### 2.5 分页响应结构

```json
{
  "items": [ ... ],
  "total": 100,
  "page": 1,
  "pageSize": 20,
  "totalPages": 5
}
```

| 字段名 | 类型 | 说明 |
|--------|------|------|
| `items` | `T[]` | 数据列表 |
| `total` | `number` | 总条数 |
| `page` | `number` | 当前页码 |
| `pageSize` | `number` | 每页条数 |
| `totalPages` | `number` | 总页数 |

---

## 3. 错误码常量表

前端需创建 `src/constants/errorCodes.ts`，完整映射如下：

### 3.1 通用错误（1xxx）

| 常量名 | 值 | 中文消息 | English Message |
|--------|----|----------|-----------------|
| `SUCCESS` | `0` | 操作成功 | Operation succeeded |
| `INVALID_REQUEST_BODY` | `1001` | 请求体无效 | Invalid request body |
| `RESOURCE_NOT_FOUND` | `1002` | 资源不存在 | Resource not found |
| `OPERATION_FAILED` | `1003` | 操作失败 | Operation failed |
| `INTERNAL_SERVER_ERROR` | `1004` | 服务器内部错误 | Internal server error |
| `INVALID_PARAMETER` | `1005` | 参数错误 | Invalid parameter |
| `INVALID_OPERATION` | `1006` | 操作无效 | Invalid operation |
| `FORBIDDEN` | `1007` | 权限不足 | Forbidden |
| `SYSTEM_BUSY` | `1008` | 系统繁忙 | System busy |

### 3.2 认证错误（2xxx）

| 常量名 | 值 | 中文消息 | English Message |
|--------|----|----------|-----------------|
| `AUTH_CREDENTIALS_REQUIRED` | `2001` | 用户名或密码不能为空 | Username and password are required |
| `AUTH_INVALID_CREDENTIALS` | `2002` | 用户名或密码错误 | Invalid username or password |
| `AUTH_ACCOUNT_DISABLED` | `2003` | 账户已禁用 | Account is disabled |
| `AUTH_ACCOUNT_LOCKED` | `2004` | 账户已锁定 | Account is locked |
| `AUTH_LOGIN_UPDATE_FAILED` | `2005` | 登录状态更新失败 | Login status update failed |
| `AUTH_TOKEN_INVALID` | `2006` | 令牌无效或已过期 | Token is invalid or expired |

### 3.3 平台用户错误（3xxx）

| 常量名 | 值 | 中文消息 | English Message |
|--------|----|----------|-----------------|
| `USER_USERNAME_REQUIRED` | `3001` | 用户名不能为空 | Username is required |
| `USER_EMAIL_REQUIRED` | `3002` | 邮箱不能为空 | Email is required |
| `USER_PASSWORD_REQUIRED` | `3003` | 密码不能为空 | Password is required |
| `USER_CREATE_FAILED` | `3004` | 创建用户失败 | Failed to create user |
| `USER_QUERY_FAILED` | `3005` | 查询用户失败 | Failed to query users |
| `USER_NOT_FOUND` | `3006` | 用户不存在 | User not found |
| `USER_UPDATE_FAILED` | `3007` | 更新用户失败 | Failed to update user |
| `USER_STATUS_CHANGE_FAILED` | `3008` | 用户状态变更失败 | Failed to change user status |

### 3.4 平台角色错误（4xxx）

| 常量名 | 值 | 中文消息 | English Message |
|--------|----|----------|-----------------|
| `ROLE_CODE_REQUIRED` | `4001` | 角色编码不能为空 | Role code is required |
| `ROLE_NAME_REQUIRED` | `4002` | 角色名称不能为空 | Role name is required |
| `ROLE_CREATE_FAILED` | `4003` | 创建角色失败 | Failed to create role |
| `ROLE_QUERY_FAILED` | `4004` | 查询角色失败 | Failed to query roles |
| `ROLE_NOT_FOUND` | `4005` | 角色不存在 | Role not found |
| `ROLE_UPDATE_FAILED` | `4006` | 更新角色失败 | Failed to update role |
| `ROLE_STATUS_CHANGE_FAILED` | `4007` | 角色状态变更失败 | Failed to change role status |
| `ROLE_PERMISSION_BIND_FAILED` | `4008` | 角色权限绑定失败 | Failed to bind role permissions |
| `ROLE_MEMBER_BIND_FAILED` | `4009` | 角色成员绑定失败 | Failed to bind role members |

### 3.5 租户生命周期错误（6xxx）

| 常量名 | 值 | 中文消息 | English Message |
|--------|----|----------|-----------------|
| `TENANT_CODE_REQUIRED` | `6001` | 租户编码不能为空 | Tenant code is required |
| `TENANT_NAME_REQUIRED` | `6002` | 租户名称不能为空 | Tenant name is required |
| `TENANT_CREATE_FAILED` | `6003` | 创建租户失败 | Failed to create tenant |
| `TENANT_QUERY_FAILED` | `6004` | 查询租户失败 | Failed to query tenants |
| `TENANT_NOT_FOUND` | `6005` | 租户不存在 | Tenant not found |
| `TENANT_UPDATE_FAILED` | `6006` | 更新租户失败 | Failed to update tenant |
| `TENANT_STATUS_CHANGE_FAILED` | `6007` | 租户状态变更失败 | Failed to change tenant status |
| `TENANT_STATUS_TRANSITION_DENIED` | `6008` | 租户状态流转不允许 | Tenant status transition not allowed |

### 3.6 租户信息错误（7xxx）

| 常量名 | 值 | 中文消息 | English Message |
|--------|----|----------|-----------------|
| `GROUP_CODE_REQUIRED` | `7001` | 分组编码不能为空 | Group code is required |
| `GROUP_CREATE_FAILED` | `7002` | 创建分组失败 | Failed to create group |
| `DOMAIN_REQUIRED` | `7003` | 域名不能为空 | Domain is required |
| `DOMAIN_CREATE_FAILED` | `7004` | 创建域名失败 | Failed to create domain |
| `TAG_KEY_REQUIRED` | `7005` | 标签键不能为空 | Tag key is required |
| `TAG_CREATE_FAILED` | `7006` | 创建标签失败 | Failed to create tag |
| `TAG_BIND_FAILED` | `7007` | 标签绑定失败 | Failed to bind tags |

### 3.7 租户资源错误（8xxx）

| 常量名 | 值 | 中文消息 | English Message |
|--------|----|----------|-----------------|
| `QUOTA_TYPE_REQUIRED` | `8001` | 配额类型不能为空 | Quota type is required |
| `QUOTA_LIMIT_INVALID` | `8002` | 配额上限必须大于0 | Quota limit must be greater than 0 |
| `QUOTA_SAVE_FAILED` | `8003` | 保存配额失败 | Failed to save quota |

### 3.8 租户配置错误（9xxx）

| 常量名 | 值 | 中文消息 | English Message |
|--------|----|----------|-----------------|
| `CONFIG_QUERY_FAILED` | `9001` | 查询配置失败 | Failed to query config |
| `CONFIG_UPDATE_FAILED` | `9002` | 更新配置失败 | Failed to update config |
| `FEATURE_KEY_REQUIRED` | `9003` | 功能键不能为空 | Feature key is required |
| `FEATURE_FLAG_SAVE_FAILED` | `9004` | 保存功能开关失败 | Failed to save feature flag |
| `FEATURE_FLAG_NOT_FOUND` | `9005` | 功能开关不存在 | Feature flag not found |
| `FEATURE_FLAG_TOGGLE_FAILED` | `9006` | 功能开关状态变更失败 | Failed to toggle feature flag |
| `PARAM_KEY_REQUIRED` | `9007` | 参数键不能为空 | Parameter key is required |
| `PARAM_SAVE_FAILED` | `9008` | 保存参数失败 | Failed to save parameter |
| `PARAM_DELETE_FAILED` | `9009` | 删除参数失败 | Failed to delete parameter |

### 3.9 套餐错误（10xxx）

| 常量名 | 值 | 中文消息 | English Message |
|--------|----|----------|-----------------|
| `PACKAGE_CODE_REQUIRED` | `10001` | 套餐编码不能为空 | Package code is required |
| `PACKAGE_NAME_REQUIRED` | `10002` | 套餐名称不能为空 | Package name is required |
| `PACKAGE_CREATE_FAILED` | `10003` | 创建套餐失败 | Failed to create package |
| `PACKAGE_QUERY_FAILED` | `10004` | 查询套餐失败 | Failed to query packages |
| `PACKAGE_NOT_FOUND` | `10005` | 套餐不存在 | Package not found |
| `PACKAGE_UPDATE_FAILED` | `10006` | 更新套餐失败 | Failed to update package |
| `PACKAGE_STATUS_CHANGE_FAILED` | `10007` | 套餐状态变更失败 | Failed to change package status |
| `PACKAGE_VERSION_CODE_REQUIRED` | `10008` | 版本编码不能为空 | Package version code is required |
| `PACKAGE_VERSION_NAME_REQUIRED` | `10009` | 版本名称不能为空 | Package version name is required |
| `PACKAGE_VERSION_CREATE_FAILED` | `10010` | 创建版本失败 | Failed to create package version |
| `PACKAGE_CAPABILITY_KEY_REQUIRED` | `10011` | 能力键不能为空 | Package capability key is required |
| `PACKAGE_CAPABILITY_SAVE_FAILED` | `10012` | 保存能力失败 | Failed to save package capability |

### 3.10 订阅错误（11xxx）

| 常量名 | 值 | 中文消息 | English Message |
|--------|----|----------|-----------------|
| `SUBSCRIPTION_CREATE_FAILED` | `11001` | 创建订阅失败 | Failed to create subscription |
| `SUBSCRIPTION_QUERY_FAILED` | `11002` | 查询订阅失败 | Failed to query subscriptions |
| `SUBSCRIPTION_NOT_FOUND` | `11003` | 订阅不存在 | Subscription not found |
| `SUBSCRIPTION_CANCEL_FAILED` | `11004` | 取消订阅失败 | Failed to cancel subscription |
| `TRIAL_CREATE_FAILED` | `11005` | 创建试用失败 | Failed to create trial |

### 3.11 计费错误（12xxx）

| 常量名 | 值 | 中文消息 | English Message |
|--------|----|----------|-----------------|
| `INVOICE_CREATE_FAILED` | `12001` | 创建发票失败 | Failed to create invoice |
| `INVOICE_QUERY_FAILED` | `12002` | 查询发票失败 | Failed to query invoices |
| `INVOICE_NOT_FOUND` | `12003` | 发票不存在 | Invoice not found |
| `INVOICE_VOID_FAILED` | `12004` | 作废发票失败 | Failed to void invoice |
| `PAYMENT_ORDER_CREATE_FAILED` | `12005` | 创建支付单失败 | Failed to create payment order |
| `REFUND_CREATE_FAILED` | `12006` | 创建退款失败 | Failed to create refund |

### 3.12 API 集成错误（13xxx）

| 常量名 | 值 | 中文消息 | English Message |
|--------|----|----------|-----------------|
| `API_KEY_CREATE_FAILED` | `13001` | 创建API密钥失败 | Failed to create API key |
| `API_KEY_DISABLE_FAILED` | `13002` | 禁用API密钥失败 | Failed to disable API key |
| `API_KEY_QUERY_FAILED` | `13003` | 查询API密钥失败 | Failed to query API keys |
| `API_KEY_NOT_FOUND` | `13004` | API密钥不存在 | API key not found |
| `WEBHOOK_CREATE_FAILED` | `13005` | 创建Webhook失败 | Failed to create webhook |
| `WEBHOOK_QUERY_FAILED` | `13006` | 查询Webhook失败 | Failed to query webhooks |
| `WEBHOOK_NOT_FOUND` | `13007` | Webhook不存在 | Webhook not found |
| `WEBHOOK_UPDATE_FAILED` | `13008` | 更新Webhook失败 | Failed to update webhook |
| `WEBHOOK_STATUS_CHANGE_FAILED` | `13009` | Webhook状态变更失败 | Failed to change webhook status |

### 3.13 通知错误（16xxx）

| 常量名 | 值 | 中文消息 | English Message |
|--------|----|----------|-----------------|
| `NOTIFICATION_TEMPLATE_NAME_REQUIRED` | `16001` | 模板名称不能为空 | Notification template name is required |
| `NOTIFICATION_TEMPLATE_CREATE_FAILED` | `16002` | 创建通知模板失败 | Failed to create notification template |
| `NOTIFICATION_TEMPLATE_QUERY_FAILED` | `16003` | 查询通知模板失败 | Failed to query notification templates |
| `NOTIFICATION_TEMPLATE_NOT_FOUND` | `16004` | 通知模板不存在 | Notification template not found |
| `NOTIFICATION_TEMPLATE_UPDATE_FAILED` | `16005` | 更新通知模板失败 | Failed to update notification template |
| `NOTIFICATION_TEMPLATE_STATUS_CHANGE_FAILED` | `16006` | 通知模板状态变更失败 | Failed to change notification template status |
| `NOTIFICATION_CREATE_FAILED` | `16007` | 创建通知失败 | Failed to create notification |
| `NOTIFICATION_QUERY_FAILED` | `16008` | 查询通知失败 | Failed to query notifications |
| `NOTIFICATION_NOT_FOUND` | `16009` | 通知不存在 | Notification not found |
| `NOTIFICATION_MARK_READ_FAILED` | `16010` | 标记通知已读失败 | Failed to mark notification as read |

### 3.14 存储错误（17xxx）

| 常量名 | 值 | 中文消息 | English Message |
|--------|----|----------|-----------------|
| `STORAGE_STRATEGY_NAME_REQUIRED` | `17001` | 策略名称不能为空 | Storage strategy name is required |
| `STORAGE_STRATEGY_CREATE_FAILED` | `17002` | 创建存储策略失败 | Failed to create storage strategy |
| `STORAGE_STRATEGY_QUERY_FAILED` | `17003` | 查询存储策略失败 | Failed to query storage strategies |
| `STORAGE_STRATEGY_NOT_FOUND` | `17004` | 存储策略不存在 | Storage strategy not found |
| `STORAGE_STRATEGY_UPDATE_FAILED` | `17005` | 更新存储策略失败 | Failed to update storage strategy |
| `STORAGE_STRATEGY_STATUS_CHANGE_FAILED` | `17006` | 存储策略状态变更失败 | Failed to change storage strategy status |
| `FILE_DELETE_FAILED` | `17007` | 删除文件失败 | Failed to delete file |
| `FILE_ACCESS_POLICY_SAVE_FAILED` | `17008` | 保存文件访问策略失败 | Failed to save file access policy |

---

## 4. 国际化消息键

前端需创建 i18n locale 文件，以下为所有后端返回的消息键。将它们写入 `src/locales/zh-CN.json` 和 `src/locales/en-US.json`。

### 4.1 zh-CN.json

```json
{
  "operation.success": "操作成功",
  "operation.failed": "操作失败",

  "common.invalid_request_body": "请求体无效",
  "common.resource_not_found": "资源不存在",
  "common.internal_server_error": "服务器内部错误",
  "common.contact_admin_or_retry": "请联系管理员或稍后重试",
  "common.invalid_parameter": "参数错误",
  "common.invalid_operation": "操作无效",
  "common.forbidden": "权限不足",
  "common.system_busy": "系统繁忙，请稍后再试",

  "auth.credentials_required": "用户名或密码不能为空",
  "auth.invalid_credentials": "用户名或密码错误",
  "auth.account_disabled": "账户已禁用",
  "auth.account_locked": "账户已锁定",
  "auth.login_update_failed": "登录状态更新失败",
  "auth.token_invalid": "令牌无效或已过期",
  "auth.login_success": "登录成功",
  "auth.login_success_password_expired": "登录成功，需尽快修改密码",
  "auth.refresh_success": "刷新成功",

  "user.username_required": "用户名不能为空",
  "user.email_required": "邮箱不能为空",
  "user.password_required": "密码不能为空",
  "user.create_failed": "创建用户失败",
  "user.query_failed": "查询用户失败",
  "user.not_found": "用户不存在",
  "user.update_failed": "更新用户失败",
  "user.status_change_failed": "用户状态变更失败",

  "role.code_required": "角色编码不能为空",
  "role.name_required": "角色名称不能为空",
  "role.create_failed": "创建角色失败",
  "role.query_failed": "查询角色失败",
  "role.not_found": "角色不存在",
  "role.update_failed": "更新角色失败",
  "role.status_change_failed": "角色状态变更失败",

  "tenant.code_required": "租户编码不能为空",
  "tenant.name_required": "租户名称不能为空",
  "tenant.create_failed": "创建租户失败",
  "tenant.query_failed": "查询租户失败",
  "tenant.not_found": "租户不存在",
  "tenant.update_failed": "更新租户失败",
  "tenant.status_change_failed": "租户状态变更失败",
  "tenant.status_transition_denied": "租户状态流转不允许",

  "group.code_required": "分组编码不能为空",
  "group.create_failed": "创建分组失败",
  "domain.required": "域名不能为空",
  "domain.create_failed": "创建域名失败",
  "tag.key_required": "标签键不能为空",
  "tag.create_failed": "创建标签失败",
  "tag.bind_failed": "标签绑定失败",

  "quota.type_required": "配额类型不能为空",
  "quota.limit_invalid": "配额上限必须大于0",
  "quota.save_failed": "保存配额失败",

  "config.query_failed": "查询配置失败",
  "config.update_failed": "更新配置失败",
  "feature.key_required": "功能键不能为空",
  "feature.save_failed": "保存功能开关失败",
  "feature.not_found": "功能开关不存在",
  "feature.toggle_failed": "功能开关状态变更失败",
  "param.key_required": "参数键不能为空",
  "param.save_failed": "保存参数失败",
  "param.delete_failed": "删除参数失败",

  "package.code_required": "套餐编码不能为空",
  "package.name_required": "套餐名称不能为空",
  "package.create_failed": "创建套餐失败",
  "package.query_failed": "查询套餐失败",
  "package.not_found": "套餐不存在",
  "package.update_failed": "更新套餐失败",
  "package.status_change_failed": "套餐状态变更失败",
  "package.version_code_required": "版本编码不能为空",
  "package.version_name_required": "版本名称不能为空",
  "package.version_create_failed": "创建版本失败",
  "package.capability_key_required": "能力键不能为空",
  "package.capability_save_failed": "保存能力失败",

  "subscription.create_failed": "创建订阅失败",
  "subscription.query_failed": "查询订阅失败",
  "subscription.not_found": "订阅不存在",
  "subscription.cancel_failed": "取消订阅失败",
  "subscription.trial_create_failed": "创建试用失败",

  "billing.invoice_create_failed": "创建发票失败",
  "billing.invoice_query_failed": "查询发票失败",
  "billing.invoice_not_found": "发票不存在",
  "billing.invoice_void_failed": "作废发票失败",
  "billing.payment_create_failed": "创建支付单失败",
  "billing.refund_create_failed": "创建退款失败",

  "api.key_create_failed": "创建API密钥失败",
  "api.key_disable_failed": "禁用API密钥失败",
  "api.key_query_failed": "查询API密钥失败",
  "api.key_not_found": "API密钥不存在",
  "api.webhook_create_failed": "创建Webhook失败",
  "api.webhook_query_failed": "查询Webhook失败",
  "api.webhook_not_found": "Webhook不存在",
  "api.webhook_update_failed": "更新Webhook失败",
  "api.webhook_status_change_failed": "Webhook状态变更失败",

  "notification.template_name_required": "模板名称不能为空",
  "notification.template_create_failed": "创建通知模板失败",
  "notification.template_query_failed": "查询通知模板失败",
  "notification.template_not_found": "通知模板不存在",
  "notification.template_update_failed": "更新通知模板失败",
  "notification.template_status_change_failed": "通知模板状态变更失败",
  "notification.create_failed": "创建通知失败",
  "notification.query_failed": "查询通知失败",
  "notification.not_found": "通知不存在",
  "notification.mark_read_failed": "标记通知已读失败",

  "storage.strategy_name_required": "策略名称不能为空",
  "storage.strategy_create_failed": "创建存储策略失败",
  "storage.strategy_query_failed": "查询存储策略失败",
  "storage.strategy_not_found": "存储策略不存在",
  "storage.strategy_update_failed": "更新存储策略失败",
  "storage.strategy_status_change_failed": "存储策略状态变更失败",
  "storage.file_delete_failed": "删除文件失败",
  "storage.file_access_policy_save_failed": "保存文件访问策略失败"
}
```

### 4.2 en-US.json

```json
{
  "operation.success": "Operation succeeded",
  "operation.failed": "Operation failed",

  "common.invalid_request_body": "Invalid request body",
  "common.resource_not_found": "Resource not found",
  "common.internal_server_error": "Internal server error",
  "common.contact_admin_or_retry": "Please contact admin or try again later",
  "common.invalid_parameter": "Invalid parameter",
  "common.invalid_operation": "Invalid operation",
  "common.forbidden": "Forbidden",
  "common.system_busy": "System busy, please try again later",

  "auth.credentials_required": "Username and password are required",
  "auth.invalid_credentials": "Invalid username or password",
  "auth.account_disabled": "Account is disabled",
  "auth.account_locked": "Account is locked",
  "auth.login_update_failed": "Login status update failed",
  "auth.token_invalid": "Token is invalid or expired",
  "auth.login_success": "Login succeeded",
  "auth.login_success_password_expired": "Login succeeded, please change your password soon",
  "auth.refresh_success": "Token refreshed",

  "user.username_required": "Username is required",
  "user.email_required": "Email is required",
  "user.password_required": "Password is required",
  "user.create_failed": "Failed to create user",
  "user.query_failed": "Failed to query users",
  "user.not_found": "User not found",
  "user.update_failed": "Failed to update user",
  "user.status_change_failed": "Failed to change user status",

  "role.code_required": "Role code is required",
  "role.name_required": "Role name is required",
  "role.create_failed": "Failed to create role",
  "role.query_failed": "Failed to query roles",
  "role.not_found": "Role not found",
  "role.update_failed": "Failed to update role",
  "role.status_change_failed": "Failed to change role status",

  "tenant.code_required": "Tenant code is required",
  "tenant.name_required": "Tenant name is required",
  "tenant.create_failed": "Failed to create tenant",
  "tenant.query_failed": "Failed to query tenants",
  "tenant.not_found": "Tenant not found",
  "tenant.update_failed": "Failed to update tenant",
  "tenant.status_change_failed": "Failed to change tenant status",
  "tenant.status_transition_denied": "Tenant status transition not allowed",

  "group.code_required": "Group code is required",
  "group.create_failed": "Failed to create group",
  "domain.required": "Domain is required",
  "domain.create_failed": "Failed to create domain",
  "tag.key_required": "Tag key is required",
  "tag.create_failed": "Failed to create tag",
  "tag.bind_failed": "Failed to bind tags",

  "quota.type_required": "Quota type is required",
  "quota.limit_invalid": "Quota limit must be greater than 0",
  "quota.save_failed": "Failed to save quota",

  "config.query_failed": "Failed to query config",
  "config.update_failed": "Failed to update config",
  "feature.key_required": "Feature key is required",
  "feature.save_failed": "Failed to save feature flag",
  "feature.not_found": "Feature flag not found",
  "feature.toggle_failed": "Failed to toggle feature flag",
  "param.key_required": "Parameter key is required",
  "param.save_failed": "Failed to save parameter",
  "param.delete_failed": "Failed to delete parameter",

  "package.code_required": "Package code is required",
  "package.name_required": "Package name is required",
  "package.create_failed": "Failed to create package",
  "package.query_failed": "Failed to query packages",
  "package.not_found": "Package not found",
  "package.update_failed": "Failed to update package",
  "package.status_change_failed": "Failed to change package status",
  "package.version_code_required": "Package version code is required",
  "package.version_name_required": "Package version name is required",
  "package.version_create_failed": "Failed to create package version",
  "package.capability_key_required": "Package capability key is required",
  "package.capability_save_failed": "Failed to save package capability",

  "subscription.create_failed": "Failed to create subscription",
  "subscription.query_failed": "Failed to query subscriptions",
  "subscription.not_found": "Subscription not found",
  "subscription.cancel_failed": "Failed to cancel subscription",
  "subscription.trial_create_failed": "Failed to create trial",

  "billing.invoice_create_failed": "Failed to create invoice",
  "billing.invoice_query_failed": "Failed to query invoices",
  "billing.invoice_not_found": "Invoice not found",
  "billing.invoice_void_failed": "Failed to void invoice",
  "billing.payment_create_failed": "Failed to create payment order",
  "billing.refund_create_failed": "Failed to create refund",

  "api.key_create_failed": "Failed to create API key",
  "api.key_disable_failed": "Failed to disable API key",
  "api.key_query_failed": "Failed to query API keys",
  "api.key_not_found": "API key not found",
  "api.webhook_create_failed": "Failed to create webhook",
  "api.webhook_query_failed": "Failed to query webhooks",
  "api.webhook_not_found": "Webhook not found",
  "api.webhook_update_failed": "Failed to update webhook",
  "api.webhook_status_change_failed": "Failed to change webhook status",

  "notification.template_name_required": "Notification template name is required",
  "notification.template_create_failed": "Failed to create notification template",
  "notification.template_query_failed": "Failed to query notification templates",
  "notification.template_not_found": "Notification template not found",
  "notification.template_update_failed": "Failed to update notification template",
  "notification.template_status_change_failed": "Failed to change notification template status",
  "notification.create_failed": "Failed to create notification",
  "notification.query_failed": "Failed to query notifications",
  "notification.not_found": "Notification not found",
  "notification.mark_read_failed": "Failed to mark notification as read",

  "storage.strategy_name_required": "Storage strategy name is required",
  "storage.strategy_create_failed": "Failed to create storage strategy",
  "storage.strategy_query_failed": "Failed to query storage strategies",
  "storage.strategy_not_found": "Storage strategy not found",
  "storage.strategy_update_failed": "Failed to update storage strategy",
  "storage.strategy_status_change_failed": "Failed to change storage strategy status",
  "storage.file_delete_failed": "Failed to delete file",
  "storage.file_access_policy_save_failed": "Failed to save file access policy"
}
```

---

## 5. TypeScript 接口定义

以下为前端需要创建的完整 TypeScript 类型定义，建议放在 `src/types/` 目录下。

### 5.1 基础类型 (`src/types/base.ts`)

```typescript
/** 统一响应结构 */
export interface ApiResult<T = void> {
  code: number
  message: string
  data?: T
}

/** 分页请求参数 */
export interface PagedRequest {
  Page?: number
  PageSize?: number
  Keyword?: string
  Status?: string
}

/** 分页响应结构 */
export interface PagedResult<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
  totalPages: number
}
```

### 5.2 认证模块 (`src/types/auth.ts`)

```typescript
/** 登录请求 */
export interface LoginReqDTO {
  Username: string
  Password: string
}

/** 登录/刷新 响应 */
export interface LoginRepDTO {
  Token: string
  ExpiresIn: number
  UserId: number
  Username: string
  DisplayName: string
  RequirePasswordReset: boolean
  Roles: string[]
  Permissions: string[]
  IsSuperAdmin: boolean
}

/** 刷新令牌请求 */
export interface RefreshTokenReqDTO {
  Token?: string
}

/** 当前用户响应 */
export interface CurrentUserRepDTO {
  UserId: number
  Username: string
  DisplayName: string
  IsSuperAdmin: boolean
}
```

### 5.3 平台用户模块 (`src/types/platformUser.ts`)

```typescript
/** 平台用户响应 */
export interface PlatformUserRepDTO {
  Id: number
  Username: string
  Email: string
  Phone: string | null
  DisplayName: string
  Status: string
  MfaEnabled: boolean
  LastLoginAt: string | null
  CreatedAt: string
}

/** 创建平台用户请求 */
export interface CreatePlatformUserReqDTO {
  Username: string
  Email: string
  Phone?: string
  DisplayName: string
  Password: string
  Remark?: string
}

/** 更新平台用户请求 */
export interface UpdatePlatformUserReqDTO {
  DisplayName?: string
  Phone?: string
  Email?: string
  Remark?: string
}
```

### 5.4 平台角色模块 (`src/types/platformRole.ts`)

```typescript
/** 平台角色响应 */
export interface PlatformRoleRepDTO {
  Id: number
  Code: string
  Name: string
  Description: string | null
  Status: string
  CreatedAt: string
}

/** 创建角色请求 */
export interface CreatePlatformRoleReqDTO {
  Code: string
  Name: string
  Description?: string
}

/** 更新角色请求 */
export interface UpdatePlatformRoleReqDTO {
  Name?: string
  Description?: string
}

/** 角色权限绑定请求 */
export interface RolePermissionBindReqDTO {
  PermissionIds: number[]
}

/** 角色成员绑定请求 */
export interface RoleMemberBindReqDTO {
  UserIds: number[]
}
```

### 5.5 平台权限模块 (`src/types/platformPermission.ts`)

```typescript
/** 平台权限响应（支持树形） */
export interface PlatformPermissionRepDTO {
  Id: number
  Code: string
  Name: string
  PermissionType: string
  ParentId: number | null
  Path: string | null
  Method: string | null
  Children: PlatformPermissionRepDTO[] | null
}
```

### 5.6 租户生命周期模块 (`src/types/tenant.ts`)

```typescript
/** 租户响应 */
export interface TenantRepDTO {
  Id: number
  TenantCode: string
  TenantName: string
  EnterpriseName: string | null
  ContactName: string | null
  ContactEmail: string | null
  LifecycleStatus: string
  IsolationMode: string
  Enabled: boolean
  OpenedAt: string | null
  ExpiresAt: string | null
  CreatedAt: string
}

/** 创建租户请求 */
export interface CreateTenantReqDTO {
  TenantCode: string
  TenantName: string
  EnterpriseName?: string
  ContactName?: string
  ContactPhone?: string
  ContactEmail?: string
  SourceType?: string
  IsolationMode?: string
  DefaultLanguage?: string
  DefaultTimezone?: string
}

/** 更新租户请求 */
export interface UpdateTenantReqDTO {
  TenantName?: string
  EnterpriseName?: string
  ContactName?: string
  ContactPhone?: string
  ContactEmail?: string
}

/** 变更租户状态请求 */
export interface TenantStatusChangeReqDTO {
  TargetStatus: string
  Reason?: string
}

/** 租户生命周期事件响应 */
export interface TenantLifecycleEventRepDTO {
  Id: number
  TenantRefId: number
  EventType: string
  FromStatus: string | null
  ToStatus: string | null
  Reason: string | null
  OccurredAt: string
}
```

### 5.7 租户信息模块（分组、域名、标签）(`src/types/tenantInfo.ts`)

```typescript
/** 租户分组响应（支持树形） */
export interface TenantGroupRepDTO {
  Id: number
  GroupCode: string
  GroupName: string
  Description: string | null
  ParentId: number | null
  Children: TenantGroupRepDTO[] | null
  CreatedAt: string
}

/** 创建租户分组请求 */
export interface CreateTenantGroupReqDTO {
  GroupCode: string
  GroupName: string
  Description?: string
  ParentId?: number | null
}

/** 租户域名响应 */
export interface TenantDomainRepDTO {
  Id: number
  TenantRefId: number
  Domain: string
  DomainType: string
  IsPrimary: boolean
  VerificationStatus: string
  CreatedAt: string
}

/** 创建租户域名请求 */
export interface CreateTenantDomainReqDTO {
  TenantRefId: number
  Domain: string
  DomainType?: string
}

/** 租户标签响应 */
export interface TenantTagRepDTO {
  Id: number
  TagKey: string
  TagValue: string
  TagType: string
  Description: string | null
  CreatedAt: string
}

/** 创建租户标签请求 */
export interface CreateTenantTagReqDTO {
  TagKey: string
  TagValue: string
  TagType?: string
  Description?: string
}

/** 批量绑定标签请求 */
export interface TagBindReqDTO {
  TenantRefId: number
  TagIds: number[]
}
```

### 5.8 租户资源模块 (`src/types/tenantResource.ts`)

```typescript
/** 资源配额响应 */
export interface TenantResourceQuotaRepDTO {
  Id: number
  TenantRefId: number
  QuotaType: string
  QuotaLimit: number
  WarningThreshold: number | null
  ResetCycle: string | null
  CreatedAt: string
}

/** 创建/更新资源配额请求 */
export interface SaveTenantResourceQuotaReqDTO {
  TenantRefId: number
  QuotaType: string
  QuotaLimit: number
  WarningThreshold?: number | null
  ResetCycle?: string
}
```

### 5.9 租户配置模块 (`src/types/tenantConfig.ts`)

```typescript
/** 租户系统配置响应 */
export interface TenantSystemConfigRepDTO {
  Id: number
  TenantRefId: number
  SystemName: string | null
  LogoUrl: string | null
  SystemTheme: string | null
  DefaultLanguage: string | null
  DefaultTimezone: string | null
  UpdatedAt: string
}

/** 更新租户系统配置请求 */
export interface UpdateTenantSystemConfigReqDTO {
  SystemName?: string
  LogoUrl?: string
  SystemTheme?: string
  DefaultLanguage?: string
  DefaultTimezone?: string
}

/** 功能开关响应 */
export interface TenantFeatureFlagRepDTO {
  Id: number
  TenantRefId: number
  FeatureKey: string
  FeatureName: string
  Enabled: boolean
  RolloutType: string
  UpdatedAt: string
}

/** 创建/更新功能开关请求 */
export interface SaveTenantFeatureFlagReqDTO {
  TenantRefId: number
  FeatureKey: string
  FeatureName: string
  Enabled: boolean
  RolloutType?: string
}

/** 租户参数响应 */
export interface TenantParameterRepDTO {
  Id: number
  TenantRefId: number
  ParamKey: string
  ParamName: string
  ParamType: string
  ParamValue: string
  UpdatedAt: string
}

/** 创建/更新租户参数请求 */
export interface SaveTenantParameterReqDTO {
  TenantRefId: number
  ParamKey: string
  ParamName: string
  ParamType?: string
  ParamValue: string
}
```

### 5.10 SaaS 套餐模块 (`src/types/saasPackage.ts`)

```typescript
/** 套餐响应 */
export interface SaasPackageRepDTO {
  Id: number
  PackageCode: string
  PackageName: string
  Description: string | null
  Status: string
  CreatedAt: string
}

/** 创建套餐请求 */
export interface CreateSaasPackageReqDTO {
  PackageCode: string
  PackageName: string
  Description?: string
}

/** 更新套餐请求 */
export interface UpdateSaasPackageReqDTO {
  PackageName?: string
  Description?: string
}

/** 套餐版本响应 */
export interface SaasPackageVersionRepDTO {
  Id: number
  PackageId: number
  VersionCode: string
  VersionName: string
  EditionType: string
  BillingCycle: string
  Price: number
  CurrencyCode: string
  TrialDays: number
  IsDefault: boolean
  Enabled: boolean
  EffectiveFrom: string | null
  EffectiveTo: string | null
  CreatedAt: string
}

/** 创建套餐版本请求 */
export interface CreateSaasPackageVersionReqDTO {
  PackageId: number
  VersionCode: string
  VersionName: string
  EditionType: string
  BillingCycle?: string
  Price: number
  CurrencyCode?: string
  TrialDays?: number
  IsDefault?: boolean
}

/** 套餐能力响应 */
export interface SaasPackageCapabilityRepDTO {
  Id: number
  PackageVersionId: number
  CapabilityKey: string
  CapabilityName: string
  CapabilityType: string
  CapabilityValue: string
  CreatedAt: string
}

/** 创建/更新套餐能力请求 */
export interface SaveSaasPackageCapabilityReqDTO {
  PackageVersionId: number
  CapabilityKey: string
  CapabilityName: string
  CapabilityType: string
  CapabilityValue: string
}
```

### 5.11 订阅模块 (`src/types/subscription.ts`)

```typescript
/** 订阅响应 */
export interface TenantSubscriptionRepDTO {
  Id: number
  TenantRefId: number
  PackageVersionId: number
  SubscriptionStatus: string
  SubscriptionType: string
  StartedAt: string
  ExpiresAt: string
  AutoRenew: boolean
  CancelledAt: string | null
  CreatedAt: string
}

/** 创建订阅请求 */
export interface CreateSubscriptionReqDTO {
  TenantRefId: number
  PackageVersionId: number
  SubscriptionType?: string
  AutoRenew?: boolean
}

/** 试用响应 */
export interface TenantTrialRepDTO {
  Id: number
  TenantRefId: number
  PackageVersionId: number | null
  Status: string
  StartedAt: string
  ExpiresAt: string
  ConvertedSubscriptionId: number | null
  CreatedAt: string
}

/** 创建试用请求 */
export interface CreateTrialReqDTO {
  TenantRefId: number
  PackageVersionId: number
}

/** 订阅变更响应 */
export interface TenantSubscriptionChangeRepDTO {
  Id: number
  TenantRefId: number
  SubscriptionId: number | null
  ChangeType: string
  FromPackageVersionId: number | null
  ToPackageVersionId: number | null
  EffectiveAt: string
  Remark: string | null
  CreatedAt: string
}
```

### 5.12 计费与账单模块 (`src/types/billing.ts`)

```typescript
/** 发票响应 */
export interface BillingInvoiceRepDTO {
  Id: number
  InvoiceNo: string
  TenantRefId: number
  SubscriptionId: number | null
  InvoiceStatus: string
  BillingPeriodStart: string
  BillingPeriodEnd: string
  SubtotalAmount: number
  ExtraAmount: number
  DiscountAmount: number
  TotalAmount: number
  CurrencyCode: string
  IssuedAt: string | null
  DueAt: string | null
  PaidAt: string | null
  CreatedAt: string
}

/** 创建发票请求 */
export interface CreateBillingInvoiceReqDTO {
  TenantRefId: number
  SubscriptionId?: number | null
  BillingPeriodStart: string
  BillingPeriodEnd: string
  CurrencyCode?: string
}

/** 发票明细响应 */
export interface BillingInvoiceItemRepDTO {
  Id: number
  InvoiceId: number
  ItemType: string
  ItemName: string
  Quantity: number
  UnitPrice: number
  Amount: number
  CreatedAt: string
}

/** 支付订单响应 */
export interface PaymentOrderRepDTO {
  Id: number
  OrderNo: string
  TenantRefId: number
  InvoiceId: number | null
  PaymentChannel: string
  PaymentStatus: string
  Amount: number
  CurrencyCode: string
  ThirdPartyTxnNo: string | null
  PaidAt: string | null
  CreatedAt: string
}

/** 创建支付订单请求 */
export interface CreatePaymentOrderReqDTO {
  TenantRefId: number
  InvoiceId?: number | null
  PaymentChannel?: string
  Amount: number
  CurrencyCode?: string
}

/** 退款响应 */
export interface PaymentRefundRepDTO {
  Id: number
  RefundNo: string
  PaymentOrderId: number
  RefundStatus: string
  RefundAmount: number
  RefundReason: string | null
  RefundedAt: string | null
  CreatedAt: string
}

/** 创建退款请求 */
export interface CreateRefundReqDTO {
  PaymentOrderId: number
  RefundAmount: number
  RefundReason?: string
}
```

### 5.13 API 与集成平台模块 (`src/types/apiIntegration.ts`)

```typescript
/** API 密钥响应 */
export interface TenantApiKeyRepDTO {
  Id: number
  TenantRefId: number
  KeyName: string
  AccessKey: string
  Status: string
  QuotaLimit: number | null
  RateLimit: number | null
  LastUsedAt: string | null
  ExpiresAt: string | null
  CreatedAt: string
}

/** 创建 API 密钥请求 */
export interface CreateApiKeyReqDTO {
  TenantRefId: number
  KeyName: string
  ExpiresAt?: string | null
}

/** 创建 API 密钥响应（仅创建时返回 SecretKey） */
export interface ApiKeyCreatedRepDTO {
  Id: number
  AccessKey: string
  SecretKey: string
}

/** API 用量统计响应 */
export interface TenantApiUsageStatRepDTO {
  Id: number
  TenantRefId: number
  ApiKeyId: number | null
  StatDate: string
  ApiPath: string
  RequestCount: number
  SuccessCount: number
  ErrorCount: number
  AverageLatencyMs: number
  CreatedAt: string
}

/** Webhook 事件响应 */
export interface WebhookEventRepDTO {
  Id: number
  EventCode: string
  EventName: string
  Description: string | null
  CreatedAt: string
}

/** Webhook 响应 */
export interface TenantWebhookRepDTO {
  Id: number
  TenantRefId: number
  WebhookName: string
  TargetUrl: string
  Status: string
  CreatedAt: string
}

/** 创建 Webhook 请求 */
export interface CreateWebhookReqDTO {
  TenantRefId: number
  WebhookName: string
  TargetUrl: string
}

/** 更新 Webhook 请求 */
export interface UpdateWebhookReqDTO {
  WebhookName?: string
  TargetUrl?: string
}

/** Webhook 投递日志响应 */
export interface WebhookDeliveryLogRepDTO {
  Id: number
  WebhookId: number
  EventId: number | null
  DeliveryStatus: string
  ResponseStatusCode: number | null
  RetryCount: number
  DeliveredAt: string | null
  CreatedAt: string
}
```

### 5.14 平台运营模块 (`src/types/operations.ts`)

```typescript
/** 租户每日统计响应 */
export interface TenantDailyStatRepDTO {
  Id: number
  TenantRefId: number
  StatDate: string
  ActiveUserCount: number
  NewUserCount: number
  ApiCallCount: number
  StorageBytes: number
  ResourceScore: number
  CreatedAt: string
}

/** 平台监控指标响应 */
export interface PlatformMonitorMetricRepDTO {
  Id: number
  ComponentName: string
  MetricType: string
  MetricKey: string
  MetricValue: number
  MetricUnit: string | null
  CollectedAt: string
}
```

### 5.15 日志与审计模块 (`src/types/logs.ts`)

```typescript
/** 操作日志响应 */
export interface OperationLogRepDTO {
  Id: number
  TenantRefId: number | null
  OperatorType: string
  OperatorId: number | null
  Action: string
  ResourceType: string | null
  ResourceId: string | null
  IpAddress: string | null
  OperationResult: string
  CreatedAt: string
}

/** 审计日志响应 */
export interface AuditLogRepDTO {
  Id: number
  TenantRefId: number | null
  AuditType: string
  Severity: string
  SubjectType: string | null
  SubjectId: string | null
  ComplianceTag: string | null
  CreatedAt: string
}

/** 系统日志响应 */
export interface SystemLogRepDTO {
  Id: number
  ServiceName: string
  LogLevel: string
  TraceId: string | null
  Message: string
  CreatedAt: string
}
```

### 5.16 通知模块 (`src/types/notification.ts`)

```typescript
/** 通知模板响应 */
export interface NotificationTemplateRepDTO {
  Id: number
  TemplateCode: string
  TemplateName: string
  Channel: string
  SubjectTemplate: string | null
  BodyTemplate: string
  Status: string
  CreatedAt: string
}

/** 创建通知模板请求 */
export interface CreateNotificationTemplateReqDTO {
  TemplateCode: string
  TemplateName: string
  Channel?: string
  SubjectTemplate?: string
  BodyTemplate: string
}

/** 更新通知模板请求 */
export interface UpdateNotificationTemplateReqDTO {
  TemplateName?: string
  SubjectTemplate?: string
  BodyTemplate?: string
}

/** 通知响应 */
export interface NotificationRepDTO {
  Id: number
  TenantRefId: number | null
  TemplateId: number | null
  Channel: string
  Recipient: string
  Subject: string | null
  Body: string
  SendStatus: string
  SentAt: string | null
  ReadAt: string | null
  CreatedAt: string
}

/** 创建通知请求 */
export interface CreateNotificationReqDTO {
  TenantRefId?: number | null
  TemplateId?: number | null
  Channel?: string
  Recipient: string
  Subject?: string
  Body: string
}
```

### 5.17 文件与存储模块 (`src/types/storage.ts`)

```typescript
/** 存储策略响应 */
export interface StorageStrategyRepDTO {
  Id: number
  StrategyCode: string
  StrategyName: string
  ProviderType: string
  BucketName: string | null
  BasePath: string | null
  Status: string
  CreatedAt: string
}

/** 创建存储策略请求 */
export interface CreateStorageStrategyReqDTO {
  StrategyCode: string
  StrategyName: string
  ProviderType?: string
  BucketName?: string
  BasePath?: string
}

/** 更新存储策略请求 */
export interface UpdateStorageStrategyReqDTO {
  StrategyName?: string
  BucketName?: string
  BasePath?: string
}

/** 租户文件响应 */
export interface TenantFileRepDTO {
  Id: number
  TenantRefId: number
  StorageStrategyId: number | null
  FileName: string
  FilePath: string
  FileExt: string | null
  MimeType: string | null
  FileSize: number
  UploaderType: string
  UploaderId: number | null
  Visibility: string
  DownloadCount: number
  CreatedAt: string
}

/** 文件访问策略响应 */
export interface FileAccessPolicyRepDTO {
  Id: number
  FileId: number
  SubjectType: string
  SubjectId: string | null
  PermissionCode: string
  CreatedAt: string
}

/** 创建/更新文件访问策略请求 */
export interface SaveFileAccessPolicyReqDTO {
  FileId: number
  SubjectType: string
  SubjectId?: string
  PermissionCode?: string
}
```

---

## 6. API 模块列表

以下为所有 API 端点的完整列表，按模块分组。

### 6.1 健康检查

| 方法 | 路径 | 认证 | 请求类型 | 响应类型 |
|------|------|------|----------|----------|
| GET | `/api/health/` | 否 | - | `ApiResult` |
| GET | `/api/health/db` | 否 | - | `ApiResult` |
| GET | `/api/health/cache` | 否 | - | `ApiResult` |

### 6.2 认证

| 方法 | 路径 | 认证 | 请求类型 | 响应类型 |
|------|------|------|----------|----------|
| POST | `/api/auth/login` | 否 | `LoginReqDTO` | `ApiResult<LoginRepDTO>` |
| POST | `/api/auth/refresh` | 否 | `RefreshTokenReqDTO` | `ApiResult<LoginRepDTO>` |
| GET | `/api/auth/me` | 否* | - | `ApiResult<CurrentUserRepDTO>` |

> *`/api/auth/me` 不强制认证，但需携带有效 Token 才能获取真实用户信息。

### 6.3 平台用户管理

| 方法 | 路径 | 认证 | 请求类型 | 响应类型 |
|------|------|------|----------|----------|
| GET | `/api/platform-users/` | 是 | `PagedRequest` (Query) | `ApiResult<PagedResult<PlatformUserRepDTO>>` |
| GET | `/api/platform-users/{id}` | 是 | - | `ApiResult<PlatformUserRepDTO>` |
| POST | `/api/platform-users/` | 是 | `CreatePlatformUserReqDTO` | `ApiResult` |
| PUT | `/api/platform-users/{id}` | 是 | `UpdatePlatformUserReqDTO` | `ApiResult` |
| PUT | `/api/platform-users/{id}/enable` | 是 | - | `ApiResult` |
| PUT | `/api/platform-users/{id}/disable` | 是 | - | `ApiResult` |

### 6.4 平台角色管理

| 方法 | 路径 | 认证 | 请求类型 | 响应类型 |
|------|------|------|----------|----------|
| GET | `/api/platform-roles/` | 是 | `PagedRequest` (Query) | `ApiResult<PagedResult<PlatformRoleRepDTO>>` |
| GET | `/api/platform-roles/{id}` | 是 | - | `ApiResult<PlatformRoleRepDTO>` |
| POST | `/api/platform-roles/` | 是 | `CreatePlatformRoleReqDTO` | `ApiResult` |
| PUT | `/api/platform-roles/{id}` | 是 | `UpdatePlatformRoleReqDTO` | `ApiResult` |
| PUT | `/api/platform-roles/{id}/enable` | 是 | - | `ApiResult` |
| PUT | `/api/platform-roles/{id}/disable` | 是 | - | `ApiResult` |
| POST | `/api/platform-roles/{id}/permissions` | 是 | `RolePermissionBindReqDTO` | `ApiResult` |
| POST | `/api/platform-roles/{id}/members` | 是 | `RoleMemberBindReqDTO` | `ApiResult` |

### 6.5 平台权限管理

| 方法 | 路径 | 认证 | 请求类型 | 响应类型 |
|------|------|------|----------|----------|
| GET | `/api/platform-permissions/tree` | 是 | - | `ApiResult<PlatformPermissionRepDTO[]>` |
| GET | `/api/platform-permissions/` | 是 | - | `ApiResult<PlatformPermissionRepDTO[]>` |
| GET | `/api/platform-permissions/{id}` | 是 | - | `ApiResult<PlatformPermissionRepDTO>` |
| GET | `/api/platform-permissions/code/{code}` | 是 | - | `ApiResult<PlatformPermissionRepDTO>` |

### 6.6 租户生命周期管理

| 方法 | 路径 | 认证 | 请求类型 | 响应类型 |
|------|------|------|----------|----------|
| GET | `/api/tenants/` | 是 | `PagedRequest` (Query) | `ApiResult<PagedResult<TenantRepDTO>>` |
| GET | `/api/tenants/{id}` | 是 | - | `ApiResult<TenantRepDTO>` |
| POST | `/api/tenants/` | 是 | `CreateTenantReqDTO` | `ApiResult` |
| PUT | `/api/tenants/{id}` | 是 | `UpdateTenantReqDTO` | `ApiResult` |
| PUT | `/api/tenants/{id}/status` | 是 | `TenantStatusChangeReqDTO` | `ApiResult` |
| GET | `/api/tenants/{id}/lifecycle-events` | 是 | `PagedRequest` (Query) | `ApiResult<PagedResult<TenantLifecycleEventRepDTO>>` |

### 6.7 租户信息管理（分组、域名、标签）

| 方法 | 路径 | 认证 | 请求类型 | 响应类型 |
|------|------|------|----------|----------|
| GET | `/api/tenant-groups/tree` | 是 | - | `ApiResult<TenantGroupRepDTO[]>` |
| GET | `/api/tenant-groups/` | 是 | - | `ApiResult<TenantGroupRepDTO[]>` |
| POST | `/api/tenant-groups/` | 是 | `CreateTenantGroupReqDTO` | `ApiResult` |
| GET | `/api/tenant-domains/` | 是 | - | `ApiResult<TenantDomainRepDTO[]>` |
| POST | `/api/tenant-domains/` | 是 | `CreateTenantDomainReqDTO` | `ApiResult` |
| GET | `/api/tenant-tags/` | 是 | `PagedRequest` (Query) | `ApiResult<PagedResult<TenantTagRepDTO>>` |
| POST | `/api/tenant-tags/` | 是 | `CreateTenantTagReqDTO` | `ApiResult` |
| POST | `/api/tenant-tags/bind` | 是 | `TagBindReqDTO` | `ApiResult` |

### 6.8 租户资源管理

| 方法 | 路径 | 认证 | 请求类型 | 响应类型 |
|------|------|------|----------|----------|
| GET | `/api/tenant-resource-quotas/` | 是 | `PagedRequest` (Query) | `ApiResult<PagedResult<TenantResourceQuotaRepDTO>>` |
| GET | `/api/tenant-resource-quotas/{id}` | 是 | - | `ApiResult<TenantResourceQuotaRepDTO>` |
| POST | `/api/tenant-resource-quotas/` | 是 | `SaveTenantResourceQuotaReqDTO` | `ApiResult` |

### 6.9 租户配置中心

| 方法 | 路径 | 认证 | 请求类型 | 响应类型 |
|------|------|------|----------|----------|
| GET | `/api/tenant-system-configs/{tenantRefId}` | 是 | - | `ApiResult<TenantSystemConfigRepDTO>` |
| PUT | `/api/tenant-system-configs/{tenantRefId}` | 是 | `UpdateTenantSystemConfigReqDTO` | `ApiResult` |
| GET | `/api/tenant-feature-flags/` | 是 | `PagedRequest` (Query) | `ApiResult<PagedResult<TenantFeatureFlagRepDTO>>` |
| POST | `/api/tenant-feature-flags/` | 是 | `SaveTenantFeatureFlagReqDTO` | `ApiResult` |
| PUT | `/api/tenant-feature-flags/{id}/toggle` | 是 | `?enabled=bool` (Query) | `ApiResult` |
| GET | `/api/tenant-parameters/` | 是 | `PagedRequest` (Query) | `ApiResult<PagedResult<TenantParameterRepDTO>>` |
| POST | `/api/tenant-parameters/` | 是 | `SaveTenantParameterReqDTO` | `ApiResult` |
| DELETE | `/api/tenant-parameters/{id}` | 是 | - | `ApiResult` |

### 6.10 SaaS 套餐系统

| 方法 | 路径 | 认证 | 请求类型 | 响应类型 |
|------|------|------|----------|----------|
| GET | `/api/saas-packages/` | 是 | `PagedRequest` (Query) | `ApiResult<PagedResult<SaasPackageRepDTO>>` |
| GET | `/api/saas-packages/{id}` | 是 | - | `ApiResult<SaasPackageRepDTO>` |
| POST | `/api/saas-packages/` | 是 | `CreateSaasPackageReqDTO` | `ApiResult` |
| PUT | `/api/saas-packages/{id}` | 是 | `UpdateSaasPackageReqDTO` | `ApiResult` |
| PUT | `/api/saas-packages/{id}/enable` | 是 | - | `ApiResult` |
| PUT | `/api/saas-packages/{id}/disable` | 是 | - | `ApiResult` |
| GET | `/api/saas-package-versions/{packageId}` | 是 | `PagedRequest` (Query) | `ApiResult<PagedResult<SaasPackageVersionRepDTO>>` |
| POST | `/api/saas-package-versions/{packageId}` | 是 | `CreateSaasPackageVersionReqDTO` | `ApiResult` |
| GET | `/api/saas-package-capabilities/{packageVersionId}` | 是 | `PagedRequest` (Query) | `ApiResult<PagedResult<SaasPackageCapabilityRepDTO>>` |
| POST | `/api/saas-package-capabilities/{packageVersionId}` | 是 | `SaveSaasPackageCapabilityReqDTO` | `ApiResult` |

### 6.11 订阅系统

| 方法 | 路径 | 认证 | 请求类型 | 响应类型 |
|------|------|------|----------|----------|
| GET | `/api/tenant-subscriptions/` | 是 | `PagedRequest` (Query) | `ApiResult<PagedResult<TenantSubscriptionRepDTO>>` |
| GET | `/api/tenant-subscriptions/{id}` | 是 | - | `ApiResult<TenantSubscriptionRepDTO>` |
| POST | `/api/tenant-subscriptions/` | 是 | `CreateSubscriptionReqDTO` | `ApiResult` |
| PUT | `/api/tenant-subscriptions/{id}/cancel` | 是 | - | `ApiResult` |
| GET | `/api/tenant-trials/` | 是 | `PagedRequest` (Query) | `ApiResult<PagedResult<TenantTrialRepDTO>>` |
| POST | `/api/tenant-trials/` | 是 | `CreateTrialReqDTO` | `ApiResult` |
| GET | `/api/tenant-subscription-changes/` | 是 | `PagedRequest` (Query) | `ApiResult<PagedResult<TenantSubscriptionChangeRepDTO>>` |

### 6.12 计费与账单系统

| 方法 | 路径 | 认证 | 请求类型 | 响应类型 |
|------|------|------|----------|----------|
| GET | `/api/billing-invoices/` | 是 | `PagedRequest` (Query) | `ApiResult<PagedResult<BillingInvoiceRepDTO>>` |
| GET | `/api/billing-invoices/{id}` | 是 | - | `ApiResult<BillingInvoiceRepDTO>` |
| POST | `/api/billing-invoices/` | 是 | `CreateBillingInvoiceReqDTO` | `ApiResult` |
| PUT | `/api/billing-invoices/{id}/void` | 是 | - | `ApiResult` |
| GET | `/api/billing-invoices/{invoiceId}/items` | 是 | `PagedRequest` (Query) | `ApiResult<PagedResult<BillingInvoiceItemRepDTO>>` |
| GET | `/api/payment-orders/` | 是 | `PagedRequest` (Query) | `ApiResult<PagedResult<PaymentOrderRepDTO>>` |
| POST | `/api/payment-orders/` | 是 | `CreatePaymentOrderReqDTO` | `ApiResult` |
| GET | `/api/payment-refunds/` | 是 | `PagedRequest` (Query) | `ApiResult<PagedResult<PaymentRefundRepDTO>>` |
| POST | `/api/payment-refunds/` | 是 | `CreateRefundReqDTO` | `ApiResult` |

### 6.13 API 与集成平台

| 方法 | 路径 | 认证 | 请求类型 | 响应类型 |
|------|------|------|----------|----------|
| GET | `/api/tenant-api-keys/` | 是 | `PagedRequest` (Query) | `ApiResult<PagedResult<TenantApiKeyRepDTO>>` |
| POST | `/api/tenant-api-keys/` | 是 | `CreateApiKeyReqDTO` | `ApiResult<ApiKeyCreatedRepDTO>` |
| PUT | `/api/tenant-api-keys/{id}` | 是 | - | `ApiResult` |
| GET | `/api/tenant-api-usage-stats/` | 是 | `PagedRequest` (Query) | `ApiResult<PagedResult<TenantApiUsageStatRepDTO>>` |
| GET | `/api/webhook-events/` | 是 | `PagedRequest` (Query) | `ApiResult<PagedResult<WebhookEventRepDTO>>` |
| GET | `/api/tenant-webhooks/` | 是 | `PagedRequest` (Query) | `ApiResult<PagedResult<TenantWebhookRepDTO>>` |
| POST | `/api/tenant-webhooks/` | 是 | `CreateWebhookReqDTO` | `ApiResult` |
| PUT | `/api/tenant-webhooks/{id}` | 是 | `UpdateWebhookReqDTO` | `ApiResult` |
| PUT | `/api/tenant-webhooks/{id}/enable` | 是 | - | `ApiResult` |
| PUT | `/api/tenant-webhooks/{id}/disable` | 是 | - | `ApiResult` |
| GET | `/api/webhook-delivery-logs/` | 是 | `PagedRequest` (Query) | `ApiResult<PagedResult<WebhookDeliveryLogRepDTO>>` |

### 6.14 平台运营体系

| 方法 | 路径 | 认证 | 请求类型 | 响应类型 |
|------|------|------|----------|----------|
| GET | `/api/tenant-daily-stats/` | 是 | `PagedRequest` (Query) | `ApiResult<PagedResult<TenantDailyStatRepDTO>>` |
| GET | `/api/platform-monitor-metrics/` | 是 | `PagedRequest` (Query) | `ApiResult<PagedResult<PlatformMonitorMetricRepDTO>>` |

### 6.15 日志与审计

| 方法 | 路径 | 认证 | 请求类型 | 响应类型 |
|------|------|------|----------|----------|
| GET | `/api/operation-logs/` | 是 | `PagedRequest` (Query) | `ApiResult<PagedResult<OperationLogRepDTO>>` |
| GET | `/api/operation-logs/{id}` | 是 | - | `ApiResult<OperationLogRepDTO>` |
| GET | `/api/audit-logs/` | 是 | `PagedRequest` (Query) | `ApiResult<PagedResult<AuditLogRepDTO>>` |
| GET | `/api/audit-logs/{id}` | 是 | - | `ApiResult<AuditLogRepDTO>` |
| GET | `/api/system-logs/` | 是 | `PagedRequest` (Query) | `ApiResult<PagedResult<SystemLogRepDTO>>` |
| GET | `/api/system-logs/{id}` | 是 | - | `ApiResult<SystemLogRepDTO>` |

### 6.16 通知系统

| 方法 | 路径 | 认证 | 请求类型 | 响应类型 |
|------|------|------|----------|----------|
| GET | `/api/notification-templates/` | 是 | `PagedRequest` (Query) | `ApiResult<PagedResult<NotificationTemplateRepDTO>>` |
| GET | `/api/notification-templates/{id}` | 是 | - | `ApiResult<NotificationTemplateRepDTO>` |
| POST | `/api/notification-templates/` | 是 | `CreateNotificationTemplateReqDTO` | `ApiResult` |
| PUT | `/api/notification-templates/{id}` | 是 | `UpdateNotificationTemplateReqDTO` | `ApiResult` |
| PUT | `/api/notification-templates/{id}/enable` | 是 | - | `ApiResult` |
| PUT | `/api/notification-templates/{id}/disable` | 是 | - | `ApiResult` |
| GET | `/api/notifications/` | 是 | `PagedRequest` (Query) | `ApiResult<PagedResult<NotificationRepDTO>>` |
| GET | `/api/notifications/{id}` | 是 | - | `ApiResult<NotificationRepDTO>` |
| POST | `/api/notifications/` | 是 | `CreateNotificationReqDTO` | `ApiResult` |
| PUT | `/api/notifications/{id}/read` | 是 | - | `ApiResult` |

### 6.17 文件与存储

| 方法 | 路径 | 认证 | 请求类型 | 响应类型 |
|------|------|------|----------|----------|
| GET | `/api/storage-strategies/` | 是 | `PagedRequest` (Query) | `ApiResult<PagedResult<StorageStrategyRepDTO>>` |
| GET | `/api/storage-strategies/{id}` | 是 | - | `ApiResult<StorageStrategyRepDTO>` |
| POST | `/api/storage-strategies/` | 是 | `CreateStorageStrategyReqDTO` | `ApiResult` |
| PUT | `/api/storage-strategies/{id}` | 是 | `UpdateStorageStrategyReqDTO` | `ApiResult` |
| PUT | `/api/storage-strategies/{id}/enable` | 是 | - | `ApiResult` |
| PUT | `/api/storage-strategies/{id}/disable` | 是 | - | `ApiResult` |
| GET | `/api/tenant-files/` | 是 | `PagedRequest` (Query) | `ApiResult<PagedResult<TenantFileRepDTO>>` |
| GET | `/api/tenant-files/{id}` | 是 | - | `ApiResult<TenantFileRepDTO>` |
| DELETE | `/api/tenant-files/{id}` | 是 | - | `ApiResult` |
| GET | `/api/file-access-policies/` | 是 | `PagedRequest` (Query) | `ApiResult<PagedResult<FileAccessPolicyRepDTO>>` |
| POST | `/api/file-access-policies/` | 是 | `SaveFileAccessPolicyReqDTO` | `ApiResult` |

---

## 7. 实现建议

### 7.1 HTTP 客户端封装 (`src/utils/request.ts`)

使用 axios 创建全局实例，统一处理认证和错误：

```typescript
import axios from 'axios'
import type { ApiResult } from '@/types/base'
import { useI18n } from 'vue-i18n'
import { ElMessage } from 'element-plus' // 或 DevExtreme notify

const request = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  timeout: 15000,
  headers: { 'Content-Type': 'application/json' },
})

// 请求拦截器：自动注入 Token
request.interceptors.request.use((config) => {
  const token = localStorage.getItem('token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// 响应拦截器：统一处理 ApiResult
request.interceptors.response.use(
  (response) => {
    const result = response.data as ApiResult<unknown>
    if (result.code !== 0) {
      // 使用 i18n 翻译后端返回的消息键
      const message = i18n.global.t(result.message) || result.message
      showError(message)
      return Promise.reject(new ApiError(result.code, result.message))
    }
    return response
  },
  (error) => {
    if (error.response?.status === 401) {
      // Token 过期，跳转登录页
      localStorage.removeItem('token')
      router.push('/login')
    }
    return Promise.reject(error)
  },
)

export default request
```

### 7.2 API 模块组织结构

建议按业务模块拆分，与后端结构保持一致：

```
src/
├── api/
│   ├── auth.ts              # 认证模块
│   ├── platformUser.ts      # 平台用户
│   ├── platformRole.ts      # 平台角色
│   ├── platformPermission.ts # 平台权限
│   ├── tenant.ts            # 租户生命周期
│   ├── tenantGroup.ts       # 租户分组
│   ├── tenantDomain.ts      # 租户域名
│   ├── tenantTag.ts         # 租户标签
│   ├── tenantResource.ts    # 租户资源配额
│   ├── tenantConfig.ts      # 租户配置
│   ├── tenantFeatureFlag.ts # 功能开关
│   ├── tenantParameter.ts   # 租户参数
│   ├── saasPackage.ts       # SaaS 套餐
│   ├── subscription.ts      # 订阅
│   ├── billing.ts           # 计费账单
│   ├── apiKey.ts            # API 密钥
│   ├── webhook.ts           # Webhook
│   ├── operations.ts        # 平台运营统计
│   ├── logs.ts              # 日志审计
│   ├── notification.ts      # 通知系统
│   └── storage.ts           # 文件存储
├── types/                   # 类型定义（见第 5 节）
├── constants/
│   └── errorCodes.ts        # 错误码常量
├── locales/
│   ├── zh-CN.json           # 中文消息
│   └── en-US.json           # 英文消息
└── composables/             # 业务 composable
    ├── useAuth.ts
    ├── usePlatformUser.ts
    ├── useTenant.ts
    └── ...
```

### 7.3 API 模块示例

```typescript
// src/api/tenant.ts
import request from '@/utils/request'
import type { ApiResult, PagedRequest, PagedResult } from '@/types/base'
import type {
  TenantRepDTO,
  CreateTenantReqDTO,
  UpdateTenantReqDTO,
  TenantStatusChangeReqDTO,
  TenantLifecycleEventRepDTO,
} from '@/types/tenant'

export function getTenantList(params: PagedRequest) {
  return request.get<ApiResult<PagedResult<TenantRepDTO>>>('/tenants/', { params })
}

export function getTenantDetail(id: number) {
  return request.get<ApiResult<TenantRepDTO>>(`/tenants/${id}`)
}

export function createTenant(data: CreateTenantReqDTO) {
  return request.post<ApiResult>('/tenants/', data)
}

export function updateTenant(id: number, data: UpdateTenantReqDTO) {
  return request.put<ApiResult>(`/tenants/${id}`, data)
}

export function changeTenantStatus(id: number, data: TenantStatusChangeReqDTO) {
  return request.put<ApiResult>(`/tenants/${id}/status`, data)
}

export function getTenantLifecycleEvents(id: number, params: PagedRequest) {
  return request.get<ApiResult<PagedResult<TenantLifecycleEventRepDTO>>>(
    `/tenants/${id}/lifecycle-events`,
    { params },
  )
}
```

### 7.4 Composable 示例

```typescript
// src/composables/useTenant.ts
import { ref } from 'vue'
import type { PagedRequest, PagedResult } from '@/types/base'
import type { TenantRepDTO, CreateTenantReqDTO } from '@/types/tenant'
import * as tenantApi from '@/api/tenant'

export function useTenant() {
  const loading = ref(false)
  const list = ref<TenantRepDTO[]>([])
  const total = ref(0)

  async function fetchList(params: PagedRequest) {
    loading.value = true
    try {
      const { data } = await tenantApi.getTenantList(params)
      list.value = data.data!.items
      total.value = data.data!.total
    } finally {
      loading.value = false
    }
  }

  async function create(dto: CreateTenantReqDTO) {
    await tenantApi.createTenant(dto)
  }

  return { loading, list, total, fetchList, create }
}
```

### 7.5 Token 管理

- 登录成功后，将 `token` 存储到 `localStorage`（或 `cookies`）
- 同时缓存 `ExpiresIn`，可配合定时器自动刷新（调用 `/api/auth/refresh`）
- `RequirePasswordReset` 为 `true` 时，跳转到强制修改密码页面
- Token 过期（HTTP 401 或错误码 `2006`）时，清除本地 token 并跳转登录页

### 7.6 集中式错误处理

```typescript
// src/utils/errorHandler.ts
import { useI18n } from 'vue-i18n'

export class ApiError extends Error {
  code: number
  messageKey: string

  constructor(code: number, messageKey: string) {
    super(messageKey)
    this.code = code
    this.messageKey = messageKey
  }
}

/** 根据错误码做特殊处理 */
export function handleApiError(error: ApiError) {
  switch (error.code) {
    case 2003: // AUTH_ACCOUNT_DISABLED
    case 2004: // AUTH_ACCOUNT_LOCKED
      // 跳转登录页并显示特定提示
      break
    case 2006: // AUTH_TOKEN_INVALID
      // 尝试刷新 token 或跳转登录页
      break
    case 1007: // FORBIDDEN
      // 显示无权限页面
      break
    default:
      // 通用错误提示（使用 i18n 翻译 messageKey）
      break
  }
}
```

### 7.7 DevExtreme DataGrid 集成

DevExtreme DataGrid 可通过 `CustomStore` 与分页 API 对接：

```typescript
import CustomStore from 'devextreme/data/custom_store'
import { getTenantList } from '@/api/tenant'

const tenantStore = new CustomStore({
  key: 'Id',
  load: async (loadOptions) => {
    const params = {
      Page: (loadOptions.skip ?? 0) / (loadOptions.take ?? 20) + 1,
      PageSize: loadOptions.take ?? 20,
      Keyword: loadOptions.searchValue ?? undefined,
    }
    const { data } = await getTenantList(params)
    return {
      data: data.data!.items,
      totalCount: data.data!.total,
    }
  },
})
```

### 7.8 vue-i18n 配置

```typescript
import { createI18n } from 'vue-i18n'
import zhCN from '@/locales/zh-CN.json'
import enUS from '@/locales/en-US.json'

const i18n = createI18n({
  legacy: false,
  locale: 'zh-CN',
  fallbackLocale: 'en-US',
  messages: {
    'zh-CN': zhCN,
    'en-US': enUS,
  },
})

export default i18n
```

在响应拦截器或错误处理中使用 `i18n.global.t(messageKey)` 将后端返回的消息键翻译为用户可读文本。

---

## 8. 注意事项

1. **后端字段命名**：后端使用 PascalCase（如 `TenantCode`），前端接收时保持原样，不做自动转换。在 TypeScript 类型中保持 PascalCase 以与 API 保持一致。
2. **日期格式**：后端返回 ISO 8601 格式字符串（如 `2025-01-01T00:00:00Z`），前端使用 `string` 类型接收，展示时使用 dayjs 或 date-fns 格式化。
3. **数值精度**：`decimal` 类型（如价格、金额）在 JSON 中序列化为 `number`，前端需注意浮点精度问题，展示时用 `toFixed()` 或专用货币格式化函数。
4. **ID 类型**：后端 `long` 类型在 JSON 中序列化为 `number`。JavaScript `Number` 安全整数范围为 `±2^53`，后端 `long` 最大值为 `2^63-1`，若 ID 超过 `Number.MAX_SAFE_INTEGER` 需特别处理（但通常不会）。
5. **空值处理**：可空字段（如 `string?`, `long?`）在 JSON 中可能为 `null` 或不存在，TypeScript 类型中使用 `| null` 表示。
6. **HTTP 状态码**：创建类接口成功返回 HTTP 201（Created），但响应体仍然是 `ApiResult` 结构（`code === 0`）。
