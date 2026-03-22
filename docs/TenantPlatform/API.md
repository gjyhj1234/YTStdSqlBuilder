# YTStd 租户平台 API 文档

> **Base URL**: `/api`
>
> **认证方式**: 除"健康检查"和"认证"模块外，所有接口均需在请求头中携带 Bearer Token：
> ```
> Authorization: Bearer <token>
> ```

---

## 统一响应格式

所有接口均采用 `ApiResult` / `ApiResult<T>` 统一包装：

```json
{
  "code": 0,
  "message": "operation.success",
  "data": { ... }
}
```

| 字段 | 类型 | 说明 |
|------|------|------|
| code | int | 响应代码，`0` 表示成功，非 `0` 为错误码（对应 `ErrorCodes` 常量） |
| message | string | 国际化消息键（对应 `Messages` 常量），供前端 i18n 翻译 |
| data | T? | 响应数据，仅成功时携带；无数据的操作不含此字段 |

### 分页请求参数（PagedRequest）

所有分页列表接口通过 Query String 传递以下参数：

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| Page | int | 否 | 页码，从 1 开始，默认 `1` |
| PageSize | int | 否 | 每页条数，默认 `20`，范围 `1~200` |
| Keyword | string | 否 | 关键字搜索 |
| Status | string | 否 | 状态过滤 |

### 分页响应结构（PagedResult\<T\>）

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
| items | T[] | 数据列表 |
| total | long | 总条数 |
| page | int | 当前页码 |
| pageSize | int | 每页条数 |
| totalPages | int | 总页数（计算值） |

---

## 目录

1. [健康检查](#1-健康检查)
2. [认证](#2-认证)
3. [平台用户管理](#3-平台用户管理)
4. [平台角色管理](#4-平台角色管理)
5. [平台权限管理](#5-平台权限管理)
6. [租户生命周期管理](#6-租户生命周期管理)
7. [租户信息管理（分组、域名、标签）](#7-租户信息管理分组域名标签)
8. [租户资源管理](#8-租户资源管理)
9. [租户配置中心](#9-租户配置中心)
10. [SaaS 套餐系统](#10-saas-套餐系统)
11. [订阅系统](#11-订阅系统)
12. [计费与账单系统](#12-计费与账单系统)
13. [API 与集成平台](#13-api-与集成平台)
14. [平台运营体系](#14-平台运营体系)
15. [日志与审计](#15-日志与审计)
16. [通知系统](#16-通知系统)
17. [文件与存储](#17-文件与存储)
18. [错误码总表](#错误码总表)

---

## 1. 健康检查

### 1.1 综合健康检查

- **接口路径**: `GET /api/health/`
- **请求方式**: GET
- **是否需要授权**: 否
- **特别说明**: 返回 HTTP 200 表示健康，503 表示不健康。

**请求参数**: 无

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=健康，非0=不健康 |
| message | string | 消息键 |

**错误码说明**: `OperationFailed`

---

### 1.2 数据库健康检查

- **接口路径**: `GET /api/health/db`
- **请求方式**: GET
- **是否需要授权**: 否
- **特别说明**: 仅检查数据库连通性。返回 HTTP 200 或 503。

**请求参数**: 无

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=健康，非0=不健康 |
| message | string | 消息键 |

**错误码说明**: `OperationFailed`

---

### 1.3 缓存健康检查

- **接口路径**: `GET /api/health/cache`
- **请求方式**: GET
- **是否需要授权**: 否
- **特别说明**: 仅检查缓存服务状态。返回 HTTP 200 或 503。

**请求参数**: 无

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=健康，非0=不健康 |
| message | string | 消息键 |

**错误码说明**: `OperationFailed`

---

## 2. 认证

### 2.1 平台用户登录

- **接口路径**: `POST /api/auth/login`
- **请求方式**: POST
- **是否需要授权**: 否
- **特别说明**: 登录失败次数达到阈值（默认5次）后账户将被锁定（默认30分钟）。密码过期时仍可登录，但 `RequirePasswordReset` 为 `true`，前端需提示修改密码。

**请求参数结构体（LoginReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| Username | string | 是 | 用户名 |
| Password | string | 是 | 密码 |

**返回参数结构体（LoginRepDTO）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Token | string | JWT 访问令牌 |
| ExpiresIn | int | 令牌过期时间（秒） |
| UserId | long | 用户 ID |
| Username | string | 用户名 |
| DisplayName | string | 显示名称 |
| RequirePasswordReset | bool | 是否需要重置密码 |
| Roles | string[] | 角色编码列表 |
| Permissions | string[] | 权限编码列表 |
| IsSuperAdmin | bool | 是否超级管理员 |

**错误码说明**: `AuthCredentialsRequired`, `AuthInvalidCredentials`, `AuthAccountDisabled`, `AuthAccountLocked`, `AuthLoginUpdateFailed`, `SystemBusy`

---

### 2.2 刷新访问令牌

- **接口路径**: `POST /api/auth/refresh`
- **请求方式**: POST
- **是否需要授权**: 否
- **特别说明**: 可通过请求体传入 Token，也可通过 `Authorization: Bearer <token>` 请求头传入。返回一个新的令牌。

**请求参数结构体（RefreshTokenReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| Token | string | 否 | 当前令牌（可选，也可通过请求头传入） |

**返回参数结构体（LoginRepDTO）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Token | string | 新的 JWT 访问令牌 |
| ExpiresIn | int | 令牌过期时间（秒） |
| UserId | long | 用户 ID |
| Username | string | 用户名 |
| DisplayName | string | 显示名称 |
| RequirePasswordReset | bool | 是否需要重置密码（刷新时固定为 false） |
| Roles | string[] | 角色编码列表 |
| Permissions | string[] | 权限编码列表 |
| IsSuperAdmin | bool | 是否超级管理员 |

**错误码说明**: `AuthTokenInvalid`

---

### 2.3 获取当前登录用户信息

- **接口路径**: `GET /api/auth/me`
- **请求方式**: GET
- **是否需要授权**: 否（但需携带有效 Token 以获取真实用户信息，否则返回匿名用户）
- **特别说明**: 未携带有效 Token 时返回匿名用户信息。

**请求参数**: 无

**返回参数结构体（CurrentUserRepDTO）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| UserId | long | 用户 ID（匿名时为 0） |
| Username | string | 用户名 |
| DisplayName | string | 显示名称 |
| IsSuperAdmin | bool | 是否超级管理员 |

**错误码说明**: 无

---

## 3. 平台用户管理

### 3.1 获取平台用户列表

- **接口路径**: `GET /api/platform-users/`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 支持分页、关键字搜索和状态过滤。

**请求参数结构体**: [PagedRequest](#分页请求参数pagedrequest)

**返回参数结构体（PagedResult\<PlatformUserRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 用户 ID |
| Username | string | 用户名 |
| Email | string | 邮箱 |
| Phone | string? | 手机号 |
| DisplayName | string | 显示名称 |
| Status | string | 状态（active/disabled/locked） |
| MfaEnabled | bool | 是否启用 MFA |
| LastLoginAt | DateTime? | 最后登录时间 |
| CreatedAt | DateTime | 创建时间 |

**错误码说明**: `UserQueryFailed`

---

### 3.2 获取平台用户详情

- **接口路径**: `GET /api/platform-users/{id}`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 用户 ID（路径参数） |

**返回参数结构体（PlatformUserRepDTO）**: 同 3.1

**错误码说明**: `UserNotFound`

---

### 3.3 创建平台用户

- **接口路径**: `POST /api/platform-users/`
- **请求方式**: POST
- **是否需要授权**: 是
- **特别说明**: 成功返回 HTTP 201。

**请求参数结构体（CreatePlatformUserReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| Username | string | 是 | 用户名 |
| Email | string | 是 | 邮箱 |
| Phone | string | 否 | 手机号 |
| DisplayName | string | 是 | 显示名称 |
| Password | string | 是 | 初始密码 |
| Remark | string | 否 | 备注 |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `InvalidRequestBody`, `UserUsernameRequired`, `UserEmailRequired`, `UserPasswordRequired`, `UserCreateFailed`

---

### 3.4 更新平台用户

- **接口路径**: `PUT /api/platform-users/{id}`
- **请求方式**: PUT
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体（UpdatePlatformUserReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 用户 ID（路径参数） |
| DisplayName | string | 否 | 显示名称 |
| Phone | string | 否 | 手机号 |
| Email | string | 否 | 邮箱 |
| Remark | string | 否 | 备注 |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `InvalidRequestBody`, `UserNotFound`, `UserUpdateFailed`

---

### 3.5 启用平台用户

- **接口路径**: `PUT /api/platform-users/{id}/enable`
- **请求方式**: PUT
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 用户 ID（路径参数） |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `UserNotFound`, `UserStatusChangeFailed`

---

### 3.6 禁用平台用户

- **接口路径**: `PUT /api/platform-users/{id}/disable`
- **请求方式**: PUT
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 用户 ID（路径参数） |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `UserNotFound`, `UserStatusChangeFailed`

---

## 4. 平台角色管理

### 4.1 获取角色列表

- **接口路径**: `GET /api/platform-roles/`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 支持分页、关键字搜索和状态过滤。

**请求参数结构体**: [PagedRequest](#分页请求参数pagedrequest)

**返回参数结构体（PagedResult\<PlatformRoleRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 角色 ID |
| Code | string | 角色编码 |
| Name | string | 角色名称 |
| Description | string? | 描述 |
| Status | string | 状态 |
| CreatedAt | DateTime | 创建时间 |

**错误码说明**: `RoleQueryFailed`

---

### 4.2 获取角色详情

- **接口路径**: `GET /api/platform-roles/{id}`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 角色 ID（路径参数） |

**返回参数结构体（PlatformRoleRepDTO）**: 同 4.1

**错误码说明**: `RoleNotFound`

---

### 4.3 创建角色

- **接口路径**: `POST /api/platform-roles/`
- **请求方式**: POST
- **是否需要授权**: 是
- **特别说明**: 成功返回 HTTP 201。

**请求参数结构体（CreatePlatformRoleReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| Code | string | 是 | 角色编码 |
| Name | string | 是 | 角色名称 |
| Description | string | 否 | 描述 |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `InvalidRequestBody`, `RoleCodeRequired`, `RoleNameRequired`, `RoleCreateFailed`

---

### 4.4 更新角色

- **接口路径**: `PUT /api/platform-roles/{id}`
- **请求方式**: PUT
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体（UpdatePlatformRoleReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 角色 ID（路径参数） |
| Name | string | 否 | 角色名称 |
| Description | string | 否 | 描述 |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `InvalidRequestBody`, `RoleNotFound`, `RoleUpdateFailed`

---

### 4.5 启用角色

- **接口路径**: `PUT /api/platform-roles/{id}/enable`
- **请求方式**: PUT
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 角色 ID（路径参数） |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `RoleNotFound`, `RoleStatusChangeFailed`

---

### 4.6 禁用角色

- **接口路径**: `PUT /api/platform-roles/{id}/disable`
- **请求方式**: PUT
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 角色 ID（路径参数） |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `RoleNotFound`, `RoleStatusChangeFailed`

---

### 4.7 绑定角色权限

- **接口路径**: `POST /api/platform-roles/{id}/permissions`
- **请求方式**: POST
- **是否需要授权**: 是
- **特别说明**: 批量绑定权限，传入的权限 ID 列表将替换该角色的所有权限。

**请求参数结构体（RolePermissionBindReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 角色 ID（路径参数） |
| PermissionIds | long[] | 是 | 权限 ID 列表 |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `InvalidRequestBody`, `RoleNotFound`, `RolePermissionBindFailed`

---

### 4.8 绑定角色成员

- **接口路径**: `POST /api/platform-roles/{id}/members`
- **请求方式**: POST
- **是否需要授权**: 是
- **特别说明**: 批量绑定用户，传入的用户 ID 列表将替换该角色的所有成员。

**请求参数结构体（RoleMemberBindReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 角色 ID（路径参数） |
| UserIds | long[] | 是 | 用户 ID 列表 |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `InvalidRequestBody`, `RoleNotFound`, `RoleMemberBindFailed`

---

## 5. 平台权限管理

### 5.1 获取权限树

- **接口路径**: `GET /api/platform-permissions/tree`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 返回树形结构的权限列表，子节点嵌套在 `Children` 字段中。

**请求参数**: 无

**返回参数结构体（List\<PlatformPermissionRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 权限 ID |
| Code | string | 权限编码 |
| Name | string | 权限名称 |
| PermissionType | string | 权限类型（menu/api/action） |
| ParentId | long? | 父级权限 ID |
| Path | string? | 路径 |
| Method | string? | HTTP 方法 |
| Children | PlatformPermissionRepDTO[]? | 子节点列表 |

**错误码说明**: 无

---

### 5.2 获取权限平铺列表

- **接口路径**: `GET /api/platform-permissions/`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 返回平铺的权限列表（非树形），不含 `Children` 嵌套。

**请求参数**: 无

**返回参数结构体（List\<PlatformPermissionRepDTO\>）**: 同 5.1

**错误码说明**: 无

---

### 5.3 获取权限详情

- **接口路径**: `GET /api/platform-permissions/{id}`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 权限 ID（路径参数） |

**返回参数结构体（PlatformPermissionRepDTO）**: 同 5.1

**错误码说明**: `ResourceNotFound`

---

### 5.4 根据编码获取权限

- **接口路径**: `GET /api/platform-permissions/code/{code}`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| code | string | 是 | 权限编码（路径参数） |

**返回参数结构体（PlatformPermissionRepDTO）**: 同 5.1

**错误码说明**: `ResourceNotFound`

---

## 6. 租户生命周期管理

### 6.1 获取租户列表

- **接口路径**: `GET /api/tenants/`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 支持分页、关键字搜索和状态过滤。

**请求参数结构体**: [PagedRequest](#分页请求参数pagedrequest)

**返回参数结构体（PagedResult\<TenantRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 租户 ID |
| TenantCode | string | 租户编码 |
| TenantName | string | 租户名称 |
| EnterpriseName | string? | 企业名称 |
| ContactName | string? | 联系人姓名 |
| ContactEmail | string? | 联系人邮箱 |
| LifecycleStatus | string | 生命周期状态 |
| IsolationMode | string | 隔离模式 |
| Enabled | bool | 是否启用 |
| OpenedAt | DateTime? | 开通时间 |
| ExpiresAt | DateTime? | 到期时间 |
| CreatedAt | DateTime | 创建时间 |

**错误码说明**: `TenantQueryFailed`

---

### 6.2 获取租户详情

- **接口路径**: `GET /api/tenants/{id}`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 租户 ID（路径参数） |

**返回参数结构体（TenantRepDTO）**: 同 6.1

**错误码说明**: `TenantNotFound`

---

### 6.3 创建租户

- **接口路径**: `POST /api/tenants/`
- **请求方式**: POST
- **是否需要授权**: 是
- **特别说明**: 成功返回 HTTP 201。

**请求参数结构体（CreateTenantReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| TenantCode | string | 是 | 租户编码 |
| TenantName | string | 是 | 租户名称 |
| EnterpriseName | string | 否 | 企业名称 |
| ContactName | string | 否 | 联系人姓名 |
| ContactPhone | string | 否 | 联系人手机 |
| ContactEmail | string | 否 | 联系人邮箱 |
| SourceType | string | 否 | 来源类型，默认 `"manual"` |
| IsolationMode | string | 否 | 隔离模式（shared/schema/database），默认 `"shared"` |
| DefaultLanguage | string | 否 | 默认语言，默认 `"zh-CN"` |
| DefaultTimezone | string | 否 | 默认时区，默认 `"Asia/Shanghai"` |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `InvalidRequestBody`, `TenantCodeRequired`, `TenantNameRequired`, `TenantCreateFailed`

---

### 6.4 更新租户信息

- **接口路径**: `PUT /api/tenants/{id}`
- **请求方式**: PUT
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体（UpdateTenantReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 租户 ID（路径参数） |
| TenantName | string | 否 | 租户名称 |
| EnterpriseName | string | 否 | 企业名称 |
| ContactName | string | 否 | 联系人姓名 |
| ContactPhone | string | 否 | 联系人手机 |
| ContactEmail | string | 否 | 联系人邮箱 |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `InvalidRequestBody`, `TenantNotFound`, `TenantUpdateFailed`

---

### 6.5 变更租户状态

- **接口路径**: `PUT /api/tenants/{id}/status`
- **请求方式**: PUT
- **是否需要授权**: 是
- **特别说明**: 状态流转有校验规则，不允许的状态转换将返回 `TenantStatusTransitionDenied`。

**请求参数结构体（TenantStatusChangeReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 租户 ID（路径参数） |
| TargetStatus | string | 是 | 目标状态（active/suspended/closed） |
| Reason | string | 否 | 变更原因 |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `InvalidRequestBody`, `TenantNotFound`, `TenantStatusChangeFailed`, `TenantStatusTransitionDenied`

---

### 6.6 获取租户生命周期事件列表

- **接口路径**: `GET /api/tenants/{id}/lifecycle-events`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 返回该租户的所有生命周期变更事件，支持分页。

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 租户 ID（路径参数） |
| 其他 | - | - | [PagedRequest](#分页请求参数pagedrequest) |

**返回参数结构体（PagedResult\<TenantLifecycleEventRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 事件 ID |
| TenantRefId | long | 关联租户 ID |
| EventType | string | 事件类型 |
| FromStatus | string? | 原状态 |
| ToStatus | string? | 目标状态 |
| Reason | string? | 原因 |
| OccurredAt | DateTime | 发生时间 |

**错误码说明**: `TenantNotFound`

---

## 7. 租户信息管理（分组、域名、标签）

### 7.1 获取分组树

- **接口路径**: `GET /api/tenant-groups/tree`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 返回树形结构的分组列表，子节点嵌套在 `Children` 字段中。

**请求参数**: 无

**返回参数结构体（List\<TenantGroupRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 分组 ID |
| GroupCode | string | 分组编码 |
| GroupName | string | 分组名称 |
| Description | string? | 描述 |
| ParentId | long? | 父级分组 ID |
| Children | TenantGroupRepDTO[]? | 子节点列表 |
| CreatedAt | DateTime | 创建时间 |

**错误码说明**: 无

---

### 7.2 获取分组平铺列表

- **接口路径**: `GET /api/tenant-groups/`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 返回平铺的分组列表。

**请求参数**: 无

**返回参数结构体（List\<TenantGroupRepDTO\>）**: 同 7.1

**错误码说明**: 无

---

### 7.3 创建租户分组

- **接口路径**: `POST /api/tenant-groups/`
- **请求方式**: POST
- **是否需要授权**: 是
- **特别说明**: 成功返回 HTTP 201。

**请求参数结构体（CreateTenantGroupReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| GroupCode | string | 是 | 分组编码 |
| GroupName | string | 是 | 分组名称 |
| Description | string | 否 | 描述 |
| ParentId | long? | 否 | 父级分组 ID |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `InvalidRequestBody`, `GroupCodeRequired`, `GroupCreateFailed`

---

### 7.4 获取租户域名列表

- **接口路径**: `GET /api/tenant-domains/`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 返回所有租户域名。

**请求参数**: 无

**返回参数结构体（List\<TenantDomainRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 域名 ID |
| TenantRefId | long | 关联租户 ID |
| Domain | string | 域名 |
| DomainType | string | 域名类型 |
| IsPrimary | bool | 是否为主域名 |
| VerificationStatus | string | 验证状态 |
| CreatedAt | DateTime | 创建时间 |

**错误码说明**: 无

---

### 7.5 创建租户域名

- **接口路径**: `POST /api/tenant-domains/`
- **请求方式**: POST
- **是否需要授权**: 是
- **特别说明**: 成功返回 HTTP 201。

**请求参数结构体（CreateTenantDomainReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| TenantRefId | long | 是 | 关联租户 ID |
| Domain | string | 是 | 域名 |
| DomainType | string | 否 | 域名类型（primary/alias/custom），默认 `"custom"` |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `InvalidRequestBody`, `DomainRequired`, `DomainCreateFailed`

---

### 7.6 获取租户标签列表

- **接口路径**: `GET /api/tenant-tags/`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 支持分页。

**请求参数结构体**: [PagedRequest](#分页请求参数pagedrequest)

**返回参数结构体（PagedResult\<TenantTagRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 标签 ID |
| TagKey | string | 标签键 |
| TagValue | string | 标签值 |
| TagType | string | 标签类型 |
| Description | string? | 描述 |
| CreatedAt | DateTime | 创建时间 |

**错误码说明**: 无

---

### 7.7 创建租户标签

- **接口路径**: `POST /api/tenant-tags/`
- **请求方式**: POST
- **是否需要授权**: 是
- **特别说明**: 成功返回 HTTP 201。

**请求参数结构体（CreateTenantTagReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| TagKey | string | 是 | 标签键 |
| TagValue | string | 是 | 标签值 |
| TagType | string | 否 | 标签类型，默认 `"custom"` |
| Description | string | 否 | 描述 |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `InvalidRequestBody`, `TagKeyRequired`, `TagCreateFailed`

---

### 7.8 批量绑定标签

- **接口路径**: `POST /api/tenant-tags/bind`
- **请求方式**: POST
- **是否需要授权**: 是
- **特别说明**: 将多个标签批量绑定到指定租户。

**请求参数结构体（TagBindReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| TenantRefId | long | 是 | 租户 ID |
| TagIds | long[] | 是 | 标签 ID 列表 |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `InvalidRequestBody`, `TagBindFailed`

---

## 8. 租户资源管理

### 8.1 获取资源配额列表

- **接口路径**: `GET /api/tenant-resource-quotas/`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 支持分页。

**请求参数结构体**: [PagedRequest](#分页请求参数pagedrequest)

**返回参数结构体（PagedResult\<TenantResourceQuotaRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 配额 ID |
| TenantRefId | long | 关联租户 ID |
| QuotaType | string | 配额类型 |
| QuotaLimit | long | 配额上限 |
| WarningThreshold | long? | 告警阈值 |
| ResetCycle | string? | 重置周期 |
| CreatedAt | DateTime | 创建时间 |

**错误码说明**: 无

---

### 8.2 获取资源配额详情

- **接口路径**: `GET /api/tenant-resource-quotas/{id}`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 配额 ID（路径参数） |

**返回参数结构体（TenantResourceQuotaRepDTO）**: 同 8.1

**错误码说明**: `ResourceNotFound`

---

### 8.3 创建/更新资源配额

- **接口路径**: `POST /api/tenant-resource-quotas/`
- **请求方式**: POST
- **是否需要授权**: 是
- **特别说明**: 若已存在相同租户和类型的配额则更新，否则创建。成功返回 HTTP 201。

**请求参数结构体（SaveTenantResourceQuotaReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| TenantRefId | long | 是 | 关联租户 ID |
| QuotaType | string | 是 | 配额类型 |
| QuotaLimit | long | 是 | 配额上限（必须大于 0） |
| WarningThreshold | long? | 否 | 告警阈值 |
| ResetCycle | string | 否 | 重置周期（daily/monthly/yearly/none） |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `InvalidRequestBody`, `QuotaTypeRequired`, `QuotaLimitInvalid`, `QuotaSaveFailed`

---

## 9. 租户配置中心

### 9.1 获取租户系统配置

- **接口路径**: `GET /api/tenant-system-configs/{tenantRefId}`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| tenantRefId | long | 是 | 租户 ID（路径参数） |

**返回参数结构体（TenantSystemConfigRepDTO）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 配置 ID |
| TenantRefId | long | 关联租户 ID |
| SystemName | string? | 系统名称 |
| LogoUrl | string? | Logo 地址 |
| SystemTheme | string? | 系统主题 |
| DefaultLanguage | string? | 默认语言 |
| DefaultTimezone | string? | 默认时区 |
| UpdatedAt | DateTime | 更新时间 |

**错误码说明**: `ConfigQueryFailed`

---

### 9.2 更新租户系统配置

- **接口路径**: `PUT /api/tenant-system-configs/{tenantRefId}`
- **请求方式**: PUT
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体（UpdateTenantSystemConfigReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| tenantRefId | long | 是 | 租户 ID（路径参数） |
| SystemName | string | 否 | 系统名称 |
| LogoUrl | string | 否 | Logo 地址 |
| SystemTheme | string | 否 | 系统主题 |
| DefaultLanguage | string | 否 | 默认语言 |
| DefaultTimezone | string | 否 | 默认时区 |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `InvalidRequestBody`, `ConfigUpdateFailed`

---

### 9.3 获取功能开关列表

- **接口路径**: `GET /api/tenant-feature-flags/`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 支持分页。

**请求参数结构体**: [PagedRequest](#分页请求参数pagedrequest)

**返回参数结构体（PagedResult\<TenantFeatureFlagRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 功能开关 ID |
| TenantRefId | long | 关联租户 ID |
| FeatureKey | string | 功能键 |
| FeatureName | string | 功能名称 |
| Enabled | bool | 是否启用 |
| RolloutType | string | 灰度类型 |
| UpdatedAt | DateTime | 更新时间 |

**错误码说明**: 无

---

### 9.4 创建/更新功能开关

- **接口路径**: `POST /api/tenant-feature-flags/`
- **请求方式**: POST
- **是否需要授权**: 是
- **特别说明**: 若已存在相同租户和功能键的记录则更新，否则创建。成功返回 HTTP 201。

**请求参数结构体（SaveTenantFeatureFlagReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| TenantRefId | long | 是 | 关联租户 ID |
| FeatureKey | string | 是 | 功能键 |
| FeatureName | string | 是 | 功能名称 |
| Enabled | bool | 是 | 是否启用 |
| RolloutType | string | 否 | 灰度类型（all/percentage/whitelist），默认 `"all"` |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `InvalidRequestBody`, `FeatureKeyRequired`, `FeatureFlagSaveFailed`

---

### 9.5 切换功能开关状态

- **接口路径**: `PUT /api/tenant-feature-flags/{id}/toggle`
- **请求方式**: PUT
- **是否需要授权**: 是
- **特别说明**: 通过 Query String 传入 `enabled` 参数。

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 功能开关 ID（路径参数） |
| enabled | bool | 是 | 是否启用（Query 参数） |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `FeatureFlagNotFound`, `FeatureFlagToggleFailed`

---

### 9.6 获取租户参数列表

- **接口路径**: `GET /api/tenant-parameters/`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 支持分页。

**请求参数结构体**: [PagedRequest](#分页请求参数pagedrequest)

**返回参数结构体（PagedResult\<TenantParameterRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 参数 ID |
| TenantRefId | long | 关联租户 ID |
| ParamKey | string | 参数键 |
| ParamName | string | 参数名称 |
| ParamType | string | 参数类型 |
| ParamValue | string | 参数值 |
| UpdatedAt | DateTime | 更新时间 |

**错误码说明**: 无

---

### 9.7 创建/更新租户参数

- **接口路径**: `POST /api/tenant-parameters/`
- **请求方式**: POST
- **是否需要授权**: 是
- **特别说明**: 若已存在相同租户和参数键的记录则更新，否则创建。成功返回 HTTP 201。

**请求参数结构体（SaveTenantParameterReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| TenantRefId | long | 是 | 关联租户 ID |
| ParamKey | string | 是 | 参数键 |
| ParamName | string | 是 | 参数名称 |
| ParamType | string | 否 | 参数类型（string/number/boolean/json），默认 `"string"` |
| ParamValue | string | 是 | 参数值 |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `InvalidRequestBody`, `ParamKeyRequired`, `ParamSaveFailed`

---

### 9.8 删除租户参数

- **接口路径**: `DELETE /api/tenant-parameters/{id}`
- **请求方式**: DELETE
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 参数 ID（路径参数） |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `ParamDeleteFailed`

---

## 10. SaaS 套餐系统

### 10.1 获取套餐列表

- **接口路径**: `GET /api/saas-packages/`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 支持分页、关键字搜索和状态过滤。

**请求参数结构体**: [PagedRequest](#分页请求参数pagedrequest)

**返回参数结构体（PagedResult\<SaasPackageRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 套餐 ID |
| PackageCode | string | 套餐编码 |
| PackageName | string | 套餐名称 |
| Description | string? | 描述 |
| Status | string | 状态 |
| CreatedAt | DateTime | 创建时间 |

**错误码说明**: `PackageQueryFailed`

---

### 10.2 获取套餐详情

- **接口路径**: `GET /api/saas-packages/{id}`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 套餐 ID（路径参数） |

**返回参数结构体（SaasPackageRepDTO）**: 同 10.1

**错误码说明**: `PackageNotFound`

---

### 10.3 创建套餐

- **接口路径**: `POST /api/saas-packages/`
- **请求方式**: POST
- **是否需要授权**: 是
- **特别说明**: 成功返回 HTTP 201。

**请求参数结构体（CreateSaasPackageReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| PackageCode | string | 是 | 套餐编码 |
| PackageName | string | 是 | 套餐名称 |
| Description | string | 否 | 描述 |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `InvalidRequestBody`, `PackageCodeRequired`, `PackageNameRequired`, `PackageCreateFailed`

---

### 10.4 更新套餐

- **接口路径**: `PUT /api/saas-packages/{id}`
- **请求方式**: PUT
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体（UpdateSaasPackageReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 套餐 ID（路径参数） |
| PackageName | string | 否 | 套餐名称 |
| Description | string | 否 | 描述 |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `InvalidRequestBody`, `PackageNotFound`, `PackageUpdateFailed`

---

### 10.5 启用套餐

- **接口路径**: `PUT /api/saas-packages/{id}/enable`
- **请求方式**: PUT
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 套餐 ID（路径参数） |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `PackageNotFound`, `PackageStatusChangeFailed`

---

### 10.6 禁用套餐

- **接口路径**: `PUT /api/saas-packages/{id}/disable`
- **请求方式**: PUT
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 套餐 ID（路径参数） |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `PackageNotFound`, `PackageStatusChangeFailed`

---

### 10.7 获取套餐版本列表

- **接口路径**: `GET /api/saas-package-versions/{packageId}`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 支持分页。

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| packageId | long | 是 | 套餐 ID（路径参数） |
| 其他 | - | - | [PagedRequest](#分页请求参数pagedrequest) |

**返回参数结构体（PagedResult\<SaasPackageVersionRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 版本 ID |
| PackageId | long | 套餐 ID |
| VersionCode | string | 版本编码 |
| VersionName | string | 版本名称 |
| EditionType | string | 版本类型 |
| BillingCycle | string | 计费周期 |
| Price | decimal | 价格 |
| CurrencyCode | string | 货币编码 |
| TrialDays | int | 试用天数 |
| IsDefault | bool | 是否默认 |
| Enabled | bool | 是否启用 |
| EffectiveFrom | DateTime? | 生效开始时间 |
| EffectiveTo | DateTime? | 生效结束时间 |
| CreatedAt | DateTime | 创建时间 |

**错误码说明**: 无

---

### 10.8 创建套餐版本

- **接口路径**: `POST /api/saas-package-versions/{packageId}`
- **请求方式**: POST
- **是否需要授权**: 是
- **特别说明**: 成功返回 HTTP 201。

**请求参数结构体（CreateSaasPackageVersionReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| packageId | long | 是 | 套餐 ID（路径参数） |
| PackageId | long | 是 | 套餐 ID |
| VersionCode | string | 是 | 版本编码 |
| VersionName | string | 是 | 版本名称 |
| EditionType | string | 是 | 版本类型 |
| BillingCycle | string | 否 | 计费周期（monthly/quarterly/yearly），默认 `"monthly"` |
| Price | decimal | 是 | 价格 |
| CurrencyCode | string | 否 | 货币编码，默认 `"CNY"` |
| TrialDays | int | 否 | 试用天数，默认 `0` |
| IsDefault | bool | 否 | 是否默认，默认 `false` |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `InvalidRequestBody`, `PackageVersionCodeRequired`, `PackageVersionNameRequired`, `PackageVersionCreateFailed`

---

### 10.9 获取套餐能力列表

- **接口路径**: `GET /api/saas-package-capabilities/{packageVersionId}`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 支持分页。

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| packageVersionId | long | 是 | 套餐版本 ID（路径参数） |
| 其他 | - | - | [PagedRequest](#分页请求参数pagedrequest) |

**返回参数结构体（PagedResult\<SaasPackageCapabilityRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 能力 ID |
| PackageVersionId | long | 套餐版本 ID |
| CapabilityKey | string | 能力键 |
| CapabilityName | string | 能力名称 |
| CapabilityType | string | 能力类型 |
| CapabilityValue | string | 能力值 |
| CreatedAt | DateTime | 创建时间 |

**错误码说明**: 无

---

### 10.10 创建/更新套餐能力

- **接口路径**: `POST /api/saas-package-capabilities/{packageVersionId}`
- **请求方式**: POST
- **是否需要授权**: 是
- **特别说明**: 若已存在相同版本和能力键的记录则更新，否则创建。成功返回 HTTP 201。

**请求参数结构体（SaveSaasPackageCapabilityReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| packageVersionId | long | 是 | 套餐版本 ID（路径参数） |
| PackageVersionId | long | 是 | 套餐版本 ID |
| CapabilityKey | string | 是 | 能力键 |
| CapabilityName | string | 是 | 能力名称 |
| CapabilityType | string | 是 | 能力类型 |
| CapabilityValue | string | 是 | 能力值 |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `InvalidRequestBody`, `PackageCapabilityKeyRequired`, `PackageCapabilitySaveFailed`

---

## 11. 订阅系统

### 11.1 获取订阅列表

- **接口路径**: `GET /api/tenant-subscriptions/`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 支持分页。

**请求参数结构体**: [PagedRequest](#分页请求参数pagedrequest)

**返回参数结构体（PagedResult\<TenantSubscriptionRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 订阅 ID |
| TenantRefId | long | 关联租户 ID |
| PackageVersionId | long | 套餐版本 ID |
| SubscriptionStatus | string | 订阅状态 |
| SubscriptionType | string | 订阅类型 |
| StartedAt | DateTime | 开始时间 |
| ExpiresAt | DateTime | 到期时间 |
| AutoRenew | bool | 是否自动续费 |
| CancelledAt | DateTime? | 取消时间 |
| CreatedAt | DateTime | 创建时间 |

**错误码说明**: `SubscriptionQueryFailed`

---

### 11.2 获取订阅详情

- **接口路径**: `GET /api/tenant-subscriptions/{id}`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 订阅 ID（路径参数） |

**返回参数结构体（TenantSubscriptionRepDTO）**: 同 11.1

**错误码说明**: `SubscriptionNotFound`

---

### 11.3 创建订阅

- **接口路径**: `POST /api/tenant-subscriptions/`
- **请求方式**: POST
- **是否需要授权**: 是
- **特别说明**: 成功返回 HTTP 201。

**请求参数结构体（CreateSubscriptionReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| TenantRefId | long | 是 | 关联租户 ID |
| PackageVersionId | long | 是 | 套餐版本 ID |
| SubscriptionType | string | 否 | 订阅类型（standard/enterprise/custom），默认 `"standard"` |
| AutoRenew | bool | 否 | 是否自动续费，默认 `false` |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `InvalidRequestBody`, `SubscriptionCreateFailed`

---

### 11.4 取消订阅

- **接口路径**: `PUT /api/tenant-subscriptions/{id}/cancel`
- **请求方式**: PUT
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 订阅 ID（路径参数） |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `SubscriptionNotFound`, `SubscriptionCancelFailed`

---

### 11.5 获取试用列表

- **接口路径**: `GET /api/tenant-trials/`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 支持分页。

**请求参数结构体**: [PagedRequest](#分页请求参数pagedrequest)

**返回参数结构体（PagedResult\<TenantTrialRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 试用 ID |
| TenantRefId | long | 关联租户 ID |
| PackageVersionId | long? | 套餐版本 ID |
| Status | string | 状态 |
| StartedAt | DateTime | 开始时间 |
| ExpiresAt | DateTime | 到期时间 |
| ConvertedSubscriptionId | long? | 转化订阅 ID |
| CreatedAt | DateTime | 创建时间 |

**错误码说明**: 无

---

### 11.6 创建试用

- **接口路径**: `POST /api/tenant-trials/`
- **请求方式**: POST
- **是否需要授权**: 是
- **特别说明**: 成功返回 HTTP 201。

**请求参数结构体（CreateTrialReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| TenantRefId | long | 是 | 关联租户 ID |
| PackageVersionId | long | 是 | 套餐版本 ID |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `InvalidRequestBody`, `TrialCreateFailed`

---

### 11.7 获取订阅变更列表

- **接口路径**: `GET /api/tenant-subscription-changes/`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 支持分页。

**请求参数结构体**: [PagedRequest](#分页请求参数pagedrequest)

**返回参数结构体（PagedResult\<TenantSubscriptionChangeRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 变更 ID |
| TenantRefId | long | 关联租户 ID |
| SubscriptionId | long? | 订阅 ID |
| ChangeType | string | 变更类型 |
| FromPackageVersionId | long? | 原套餐版本 ID |
| ToPackageVersionId | long? | 目标套餐版本 ID |
| EffectiveAt | DateTime | 生效时间 |
| Remark | string? | 备注 |
| CreatedAt | DateTime | 创建时间 |

**错误码说明**: 无

---

## 12. 计费与账单系统

### 12.1 获取发票列表

- **接口路径**: `GET /api/billing-invoices/`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 支持分页。

**请求参数结构体**: [PagedRequest](#分页请求参数pagedrequest)

**返回参数结构体（PagedResult\<BillingInvoiceRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 发票 ID |
| InvoiceNo | string | 发票编号 |
| TenantRefId | long | 关联租户 ID |
| SubscriptionId | long? | 订阅 ID |
| InvoiceStatus | string | 发票状态 |
| BillingPeriodStart | DateTime | 账期开始 |
| BillingPeriodEnd | DateTime | 账期结束 |
| SubtotalAmount | decimal | 小计金额 |
| ExtraAmount | decimal | 附加金额 |
| DiscountAmount | decimal | 折扣金额 |
| TotalAmount | decimal | 总金额 |
| CurrencyCode | string | 货币编码 |
| IssuedAt | DateTime? | 开票时间 |
| DueAt | DateTime? | 到期时间 |
| PaidAt | DateTime? | 支付时间 |
| CreatedAt | DateTime | 创建时间 |

**错误码说明**: `InvoiceQueryFailed`

---

### 12.2 获取发票详情

- **接口路径**: `GET /api/billing-invoices/{id}`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 发票 ID（路径参数） |

**返回参数结构体（BillingInvoiceRepDTO）**: 同 12.1

**错误码说明**: `InvoiceNotFound`

---

### 12.3 创建发票

- **接口路径**: `POST /api/billing-invoices/`
- **请求方式**: POST
- **是否需要授权**: 是
- **特别说明**: 成功返回 HTTP 201。

**请求参数结构体（CreateBillingInvoiceReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| TenantRefId | long | 是 | 关联租户 ID |
| SubscriptionId | long? | 否 | 订阅 ID |
| BillingPeriodStart | DateTime | 是 | 账期开始 |
| BillingPeriodEnd | DateTime | 是 | 账期结束 |
| CurrencyCode | string | 否 | 货币编码，默认 `"CNY"` |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `InvalidRequestBody`, `InvoiceCreateFailed`

---

### 12.4 作废发票

- **接口路径**: `PUT /api/billing-invoices/{id}/void`
- **请求方式**: PUT
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 发票 ID（路径参数） |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `InvoiceNotFound`, `InvoiceVoidFailed`

---

### 12.5 获取发票明细列表

- **接口路径**: `GET /api/billing-invoices/{invoiceId}/items`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 支持分页。

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| invoiceId | long | 是 | 发票 ID（路径参数） |
| 其他 | - | - | [PagedRequest](#分页请求参数pagedrequest) |

**返回参数结构体（PagedResult\<BillingInvoiceItemRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 明细 ID |
| InvoiceId | long | 发票 ID |
| ItemType | string | 项目类型 |
| ItemName | string | 项目名称 |
| Quantity | decimal | 数量 |
| UnitPrice | decimal | 单价 |
| Amount | decimal | 金额 |
| CreatedAt | DateTime | 创建时间 |

**错误码说明**: 无

---

### 12.6 获取支付订单列表

- **接口路径**: `GET /api/payment-orders/`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 支持分页。

**请求参数结构体**: [PagedRequest](#分页请求参数pagedrequest)

**返回参数结构体（PagedResult\<PaymentOrderRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 订单 ID |
| OrderNo | string | 订单号 |
| TenantRefId | long | 关联租户 ID |
| InvoiceId | long? | 发票 ID |
| PaymentChannel | string | 支付渠道 |
| PaymentStatus | string | 支付状态 |
| Amount | decimal | 金额 |
| CurrencyCode | string | 货币编码 |
| ThirdPartyTxnNo | string? | 第三方交易号 |
| PaidAt | DateTime? | 支付时间 |
| CreatedAt | DateTime | 创建时间 |

**错误码说明**: 无

---

### 12.7 创建支付订单

- **接口路径**: `POST /api/payment-orders/`
- **请求方式**: POST
- **是否需要授权**: 是
- **特别说明**: 成功返回 HTTP 201。

**请求参数结构体（CreatePaymentOrderReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| TenantRefId | long | 是 | 关联租户 ID |
| InvoiceId | long? | 否 | 发票 ID |
| PaymentChannel | string | 否 | 支付渠道（manual/alipay/wechat/bank），默认 `"manual"` |
| Amount | decimal | 是 | 金额 |
| CurrencyCode | string | 否 | 货币编码，默认 `"CNY"` |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `InvalidRequestBody`, `PaymentOrderCreateFailed`

---

### 12.8 获取退款列表

- **接口路径**: `GET /api/payment-refunds/`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 支持分页。

**请求参数结构体**: [PagedRequest](#分页请求参数pagedrequest)

**返回参数结构体（PagedResult\<PaymentRefundRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 退款 ID |
| RefundNo | string | 退款编号 |
| PaymentOrderId | long | 支付订单 ID |
| RefundStatus | string | 退款状态 |
| RefundAmount | decimal | 退款金额 |
| RefundReason | string? | 退款原因 |
| RefundedAt | DateTime? | 退款时间 |
| CreatedAt | DateTime | 创建时间 |

**错误码说明**: 无

---

### 12.9 创建退款

- **接口路径**: `POST /api/payment-refunds/`
- **请求方式**: POST
- **是否需要授权**: 是
- **特别说明**: 成功返回 HTTP 201。

**请求参数结构体（CreateRefundReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| PaymentOrderId | long | 是 | 支付订单 ID |
| RefundAmount | decimal | 是 | 退款金额 |
| RefundReason | string | 否 | 退款原因 |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `InvalidRequestBody`, `RefundCreateFailed`

---

## 13. API 与集成平台

### 13.1 获取 API 密钥列表

- **接口路径**: `GET /api/tenant-api-keys/`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 支持分页。列表中不包含 SecretKey。

**请求参数结构体**: [PagedRequest](#分页请求参数pagedrequest)

**返回参数结构体（PagedResult\<TenantApiKeyRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 密钥 ID |
| TenantRefId | long | 关联租户 ID |
| KeyName | string | 密钥名称 |
| AccessKey | string | 访问密钥 |
| Status | string | 状态 |
| QuotaLimit | long? | 配额上限 |
| RateLimit | int? | 速率限制 |
| LastUsedAt | DateTime? | 最后使用时间 |
| ExpiresAt | DateTime? | 过期时间 |
| CreatedAt | DateTime | 创建时间 |

**错误码说明**: `ApiKeyQueryFailed`

---

### 13.2 创建 API 密钥

- **接口路径**: `POST /api/tenant-api-keys/`
- **请求方式**: POST
- **是否需要授权**: 是
- **特别说明**: 成功返回 HTTP 201。响应中包含 SecretKey，**仅在创建时返回一次，后续不可查看**。

**请求参数结构体（CreateApiKeyReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| TenantRefId | long | 是 | 关联租户 ID |
| KeyName | string | 是 | 密钥名称 |
| ExpiresAt | DateTime? | 否 | 过期时间 |

**返回参数结构体（ApiKeyCreatedRepDTO）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 密钥 ID |
| AccessKey | string | Access Key |
| SecretKey | string | Secret Key（仅此次返回） |

**错误码说明**: `InvalidRequestBody`, `ApiKeyCreateFailed`

---

### 13.3 禁用 API 密钥

- **接口路径**: `PUT /api/tenant-api-keys/{id}/disable`
- **请求方式**: PUT
- **是否需要授权**: 是
- **特别说明**: 禁用后该密钥无法再使用。

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 密钥 ID（路径参数） |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `ApiKeyNotFound`, `ApiKeyDisableFailed`

---

### 13.4 获取 API 用量统计列表

- **接口路径**: `GET /api/tenant-api-usage-stats/`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 支持分页。

**请求参数结构体**: [PagedRequest](#分页请求参数pagedrequest)

**返回参数结构体（PagedResult\<TenantApiUsageStatRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 统计 ID |
| TenantRefId | long | 关联租户 ID |
| ApiKeyId | long? | API 密钥 ID |
| StatDate | DateTime | 统计日期 |
| ApiPath | string | API 路径 |
| RequestCount | long | 请求次数 |
| SuccessCount | long | 成功次数 |
| ErrorCount | long | 错误次数 |
| AverageLatencyMs | int | 平均延迟（毫秒） |
| CreatedAt | DateTime | 创建时间 |

**错误码说明**: 无

---

### 13.5 获取 Webhook 事件列表

- **接口路径**: `GET /api/webhook-events/`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 支持分页。

**请求参数结构体**: [PagedRequest](#分页请求参数pagedrequest)

**返回参数结构体（PagedResult\<WebhookEventRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 事件 ID |
| EventCode | string | 事件编码 |
| EventName | string | 事件名称 |
| Description | string? | 描述 |
| CreatedAt | DateTime | 创建时间 |

**错误码说明**: 无

---

### 13.6 获取 Webhook 列表

- **接口路径**: `GET /api/tenant-webhooks/`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 支持分页。

**请求参数结构体**: [PagedRequest](#分页请求参数pagedrequest)

**返回参数结构体（PagedResult\<TenantWebhookRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | Webhook ID |
| TenantRefId | long | 关联租户 ID |
| WebhookName | string | Webhook 名称 |
| TargetUrl | string | 目标地址 |
| Status | string | 状态 |
| CreatedAt | DateTime | 创建时间 |

**错误码说明**: `WebhookQueryFailed`

---

### 13.7 创建 Webhook

- **接口路径**: `POST /api/tenant-webhooks/`
- **请求方式**: POST
- **是否需要授权**: 是
- **特别说明**: 成功返回 HTTP 201。

**请求参数结构体（CreateWebhookReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| TenantRefId | long | 是 | 关联租户 ID |
| WebhookName | string | 是 | Webhook 名称 |
| TargetUrl | string | 是 | 目标地址 |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `InvalidRequestBody`, `WebhookCreateFailed`

---

### 13.8 更新 Webhook

- **接口路径**: `PUT /api/tenant-webhooks/{id}`
- **请求方式**: PUT
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体（UpdateWebhookReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | Webhook ID（路径参数） |
| WebhookName | string | 否 | Webhook 名称 |
| TargetUrl | string | 否 | 目标地址 |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `InvalidRequestBody`, `WebhookNotFound`, `WebhookUpdateFailed`

---

### 13.9 启用 Webhook

- **接口路径**: `PUT /api/tenant-webhooks/{id}/enable`
- **请求方式**: PUT
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | Webhook ID（路径参数） |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `WebhookNotFound`, `WebhookStatusChangeFailed`

---

### 13.10 禁用 Webhook

- **接口路径**: `PUT /api/tenant-webhooks/{id}/disable`
- **请求方式**: PUT
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | Webhook ID（路径参数） |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `WebhookNotFound`, `WebhookStatusChangeFailed`

---

### 13.11 获取 Webhook 投递日志列表

- **接口路径**: `GET /api/webhook-delivery-logs/`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 支持分页。

**请求参数结构体**: [PagedRequest](#分页请求参数pagedrequest)

**返回参数结构体（PagedResult\<WebhookDeliveryLogRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 日志 ID |
| WebhookId | long | Webhook ID |
| EventId | long? | 事件 ID |
| DeliveryStatus | string | 投递状态 |
| ResponseStatusCode | int? | 响应状态码 |
| RetryCount | int | 重试次数 |
| DeliveredAt | DateTime? | 投递时间 |
| CreatedAt | DateTime | 创建时间 |

**错误码说明**: 无

---

## 14. 平台运营体系

### 14.1 获取租户每日统计列表

- **接口路径**: `GET /api/tenant-daily-stats/`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 支持分页。

**请求参数结构体**: [PagedRequest](#分页请求参数pagedrequest)

**返回参数结构体（PagedResult\<TenantDailyStatRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 统计 ID |
| TenantRefId | long | 关联租户 ID |
| StatDate | DateTime | 统计日期 |
| ActiveUserCount | int | 活跃用户数 |
| NewUserCount | int | 新增用户数 |
| ApiCallCount | long | API 调用次数 |
| StorageBytes | long | 存储字节数 |
| ResourceScore | decimal | 资源评分 |
| CreatedAt | DateTime | 创建时间 |

**错误码说明**: 无

---

### 14.2 获取平台监控指标列表

- **接口路径**: `GET /api/platform-monitor-metrics/`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 支持分页。

**请求参数结构体**: [PagedRequest](#分页请求参数pagedrequest)

**返回参数结构体（PagedResult\<PlatformMonitorMetricRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 指标 ID |
| ComponentName | string | 组件名称 |
| MetricType | string | 指标类型 |
| MetricKey | string | 指标键 |
| MetricValue | decimal | 指标值 |
| MetricUnit | string? | 指标单位 |
| CollectedAt | DateTime | 采集时间 |

**错误码说明**: 无

---

## 15. 日志与审计

### 15.1 获取操作日志列表

- **接口路径**: `GET /api/operation-logs/`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 支持分页。

**请求参数结构体**: [PagedRequest](#分页请求参数pagedrequest)

**返回参数结构体（PagedResult\<OperationLogRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 日志 ID |
| TenantRefId | long? | 关联租户 ID |
| OperatorType | string | 操作者类型 |
| OperatorId | long? | 操作者 ID |
| Action | string | 操作动作 |
| ResourceType | string? | 资源类型 |
| ResourceId | string? | 资源 ID |
| IpAddress | string? | IP 地址 |
| OperationResult | string | 操作结果 |
| CreatedAt | DateTime | 创建时间 |

**错误码说明**: 无

---

### 15.2 获取操作日志详情

- **接口路径**: `GET /api/operation-logs/{id}`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 日志 ID（路径参数） |

**返回参数结构体（OperationLogRepDTO）**: 同 15.1

**错误码说明**: `ResourceNotFound`

---

### 15.3 获取审计日志列表

- **接口路径**: `GET /api/audit-logs/`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 支持分页。

**请求参数结构体**: [PagedRequest](#分页请求参数pagedrequest)

**返回参数结构体（PagedResult\<AuditLogRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 日志 ID |
| TenantRefId | long? | 关联租户 ID |
| AuditType | string | 审计类型 |
| Severity | string | 严重级别 |
| SubjectType | string? | 主体类型 |
| SubjectId | string? | 主体 ID |
| ComplianceTag | string? | 合规标签 |
| CreatedAt | DateTime | 创建时间 |

**错误码说明**: 无

---

### 15.4 获取审计日志详情

- **接口路径**: `GET /api/audit-logs/{id}`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 日志 ID（路径参数） |

**返回参数结构体（AuditLogRepDTO）**: 同 15.3

**错误码说明**: `ResourceNotFound`

---

### 15.5 获取系统日志列表

- **接口路径**: `GET /api/system-logs/`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 支持分页。

**请求参数结构体**: [PagedRequest](#分页请求参数pagedrequest)

**返回参数结构体（PagedResult\<SystemLogRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 日志 ID |
| ServiceName | string | 服务名称 |
| LogLevel | string | 日志级别 |
| TraceId | string? | 链路 ID |
| Message | string | 日志消息 |
| CreatedAt | DateTime | 创建时间 |

**错误码说明**: 无

---

### 15.6 获取系统日志详情

- **接口路径**: `GET /api/system-logs/{id}`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 日志 ID（路径参数） |

**返回参数结构体（SystemLogRepDTO）**: 同 15.5

**错误码说明**: `ResourceNotFound`

---

## 16. 通知系统

### 16.1 获取通知模板列表

- **接口路径**: `GET /api/notification-templates/`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 支持分页。

**请求参数结构体**: [PagedRequest](#分页请求参数pagedrequest)

**返回参数结构体（PagedResult\<NotificationTemplateRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 模板 ID |
| TemplateCode | string | 模板编码 |
| TemplateName | string | 模板名称 |
| Channel | string | 通知渠道 |
| SubjectTemplate | string? | 主题模板 |
| BodyTemplate | string | 正文模板 |
| Status | string | 状态 |
| CreatedAt | DateTime | 创建时间 |

**错误码说明**: `NotificationTemplateQueryFailed`

---

### 16.2 获取通知模板详情

- **接口路径**: `GET /api/notification-templates/{id}`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 模板 ID（路径参数） |

**返回参数结构体（NotificationTemplateRepDTO）**: 同 16.1

**错误码说明**: `NotificationTemplateNotFound`

---

### 16.3 创建通知模板

- **接口路径**: `POST /api/notification-templates/`
- **请求方式**: POST
- **是否需要授权**: 是
- **特别说明**: 成功返回 HTTP 201。

**请求参数结构体（CreateNotificationTemplateReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| TemplateCode | string | 是 | 模板编码 |
| TemplateName | string | 是 | 模板名称 |
| Channel | string | 否 | 通知渠道（email/sms/webhook/in_app），默认 `"email"` |
| SubjectTemplate | string | 否 | 主题模板 |
| BodyTemplate | string | 是 | 正文模板 |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `InvalidRequestBody`, `NotificationTemplateNameRequired`, `NotificationTemplateCreateFailed`

---

### 16.4 更新通知模板

- **接口路径**: `PUT /api/notification-templates/{id}`
- **请求方式**: PUT
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体（UpdateNotificationTemplateReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 模板 ID（路径参数） |
| TemplateName | string | 否 | 模板名称 |
| SubjectTemplate | string | 否 | 主题模板 |
| BodyTemplate | string | 否 | 正文模板 |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `InvalidRequestBody`, `NotificationTemplateNotFound`, `NotificationTemplateUpdateFailed`

---

### 16.5 启用通知模板

- **接口路径**: `PUT /api/notification-templates/{id}/enable`
- **请求方式**: PUT
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 模板 ID（路径参数） |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `NotificationTemplateNotFound`, `NotificationTemplateStatusChangeFailed`

---

### 16.6 禁用通知模板

- **接口路径**: `PUT /api/notification-templates/{id}/disable`
- **请求方式**: PUT
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 模板 ID（路径参数） |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `NotificationTemplateNotFound`, `NotificationTemplateStatusChangeFailed`

---

### 16.7 获取通知列表

- **接口路径**: `GET /api/notifications/`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 支持分页。

**请求参数结构体**: [PagedRequest](#分页请求参数pagedrequest)

**返回参数结构体（PagedResult\<NotificationRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 通知 ID |
| TenantRefId | long? | 关联租户 ID |
| TemplateId | long? | 模板 ID |
| Channel | string | 通知渠道 |
| Recipient | string | 接收人 |
| Subject | string? | 主题 |
| Body | string | 正文 |
| SendStatus | string | 发送状态 |
| SentAt | DateTime? | 发送时间 |
| ReadAt | DateTime? | 阅读时间 |
| CreatedAt | DateTime | 创建时间 |

**错误码说明**: `NotificationQueryFailed`

---

### 16.8 获取通知详情

- **接口路径**: `GET /api/notifications/{id}`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 通知 ID（路径参数） |

**返回参数结构体（NotificationRepDTO）**: 同 16.7

**错误码说明**: `NotificationNotFound`

---

### 16.9 创建通知

- **接口路径**: `POST /api/notifications/`
- **请求方式**: POST
- **是否需要授权**: 是
- **特别说明**: 成功返回 HTTP 201。

**请求参数结构体（CreateNotificationReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| TenantRefId | long? | 否 | 关联租户 ID |
| TemplateId | long? | 否 | 模板 ID |
| Channel | string | 否 | 通知渠道（email/sms/webhook/in_app），默认 `"email"` |
| Recipient | string | 是 | 接收人 |
| Subject | string | 否 | 主题 |
| Body | string | 是 | 正文 |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `InvalidRequestBody`, `NotificationCreateFailed`

---

### 16.10 标记通知已读

- **接口路径**: `PUT /api/notifications/{id}/read`
- **请求方式**: PUT
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 通知 ID（路径参数） |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `NotificationNotFound`, `NotificationMarkReadFailed`

---

## 17. 文件与存储

### 17.1 获取存储策略列表

- **接口路径**: `GET /api/storage-strategies/`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 支持分页。

**请求参数结构体**: [PagedRequest](#分页请求参数pagedrequest)

**返回参数结构体（PagedResult\<StorageStrategyRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 策略 ID |
| StrategyCode | string | 策略编码 |
| StrategyName | string | 策略名称 |
| ProviderType | string | 提供商类型 |
| BucketName | string? | 存储桶名称 |
| BasePath | string? | 基础路径 |
| Status | string | 状态 |
| CreatedAt | DateTime | 创建时间 |

**错误码说明**: `StorageStrategyQueryFailed`

---

### 17.2 获取存储策略详情

- **接口路径**: `GET /api/storage-strategies/{id}`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 策略 ID（路径参数） |

**返回参数结构体（StorageStrategyRepDTO）**: 同 17.1

**错误码说明**: `StorageStrategyNotFound`

---

### 17.3 创建存储策略

- **接口路径**: `POST /api/storage-strategies/`
- **请求方式**: POST
- **是否需要授权**: 是
- **特别说明**: 成功返回 HTTP 201。

**请求参数结构体（CreateStorageStrategyReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| StrategyCode | string | 是 | 策略编码 |
| StrategyName | string | 是 | 策略名称 |
| ProviderType | string | 否 | 提供商类型（local/s3/oss/azure），默认 `"local"` |
| BucketName | string | 否 | 存储桶名称 |
| BasePath | string | 否 | 基础路径 |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `InvalidRequestBody`, `StorageStrategyNameRequired`, `StorageStrategyCreateFailed`

---

### 17.4 更新存储策略

- **接口路径**: `PUT /api/storage-strategies/{id}`
- **请求方式**: PUT
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体（UpdateStorageStrategyReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 策略 ID（路径参数） |
| StrategyName | string | 否 | 策略名称 |
| BucketName | string | 否 | 存储桶名称 |
| BasePath | string | 否 | 基础路径 |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `InvalidRequestBody`, `StorageStrategyNotFound`, `StorageStrategyUpdateFailed`

---

### 17.5 启用存储策略

- **接口路径**: `PUT /api/storage-strategies/{id}/enable`
- **请求方式**: PUT
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 策略 ID（路径参数） |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `StorageStrategyNotFound`, `StorageStrategyStatusChangeFailed`

---

### 17.6 禁用存储策略

- **接口路径**: `PUT /api/storage-strategies/{id}/disable`
- **请求方式**: PUT
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 策略 ID（路径参数） |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `StorageStrategyNotFound`, `StorageStrategyStatusChangeFailed`

---

### 17.7 获取租户文件列表

- **接口路径**: `GET /api/tenant-files/`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 支持分页。

**请求参数结构体**: [PagedRequest](#分页请求参数pagedrequest)

**返回参数结构体（PagedResult\<TenantFileRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 文件 ID |
| TenantRefId | long | 关联租户 ID |
| StorageStrategyId | long? | 存储策略 ID |
| FileName | string | 文件名 |
| FilePath | string | 文件路径 |
| FileExt | string? | 文件扩展名 |
| MimeType | string? | MIME 类型 |
| FileSize | long | 文件大小 |
| UploaderType | string | 上传者类型 |
| UploaderId | long? | 上传者 ID |
| Visibility | string | 可见性 |
| DownloadCount | long | 下载次数 |
| CreatedAt | DateTime | 创建时间 |

**错误码说明**: 无

---

### 17.8 获取文件详情

- **接口路径**: `GET /api/tenant-files/{id}`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 文件 ID（路径参数） |

**返回参数结构体（TenantFileRepDTO）**: 同 17.7

**错误码说明**: `ResourceNotFound`

---

### 17.9 删除文件

- **接口路径**: `DELETE /api/tenant-files/{id}`
- **请求方式**: DELETE
- **是否需要授权**: 是
- **特别说明**: 无

**请求参数结构体**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| id | long | 是 | 文件 ID（路径参数） |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `FileDeleteFailed`

---

### 17.10 获取文件访问策略列表

- **接口路径**: `GET /api/file-access-policies/`
- **请求方式**: GET
- **是否需要授权**: 是
- **特别说明**: 支持分页。

**请求参数结构体**: [PagedRequest](#分页请求参数pagedrequest)

**返回参数结构体（PagedResult\<FileAccessPolicyRepDTO\>）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | long | 策略 ID |
| FileId | long | 文件 ID |
| SubjectType | string | 主体类型 |
| SubjectId | string? | 主体 ID |
| PermissionCode | string | 权限编码 |
| CreatedAt | DateTime | 创建时间 |

**错误码说明**: 无

---

### 17.11 创建/更新文件访问策略

- **接口路径**: `POST /api/file-access-policies/`
- **请求方式**: POST
- **是否需要授权**: 是
- **特别说明**: 若已存在相同文件和主体的策略则更新，否则创建。成功返回 HTTP 201。

**请求参数结构体（SaveFileAccessPolicyReqDTO）**:

| 字段名 | 类型 | 是否必填 | 说明 |
|--------|------|----------|------|
| FileId | long | 是 | 文件 ID |
| SubjectType | string | 是 | 主体类型 |
| SubjectId | string | 否 | 主体 ID |
| PermissionCode | string | 否 | 权限编码（read/write/delete/admin），默认 `"read"` |

**返回参数结构体（ApiResult）**:

| 字段名 | 类型 | 说明 |
|--------|------|------|
| code | int | 0=成功 |
| message | string | 消息键 |

**错误码说明**: `InvalidRequestBody`, `FileAccessPolicySaveFailed`

---

## 错误码总表

以下为 `ErrorCodes` 中定义的所有错误码常量：

### 通用错误（1xxx）

| 常量名 | 值 | 说明 |
|--------|-----|------|
| Success | 0 | 操作成功 |
| InvalidRequestBody | 1001 | 请求体无效 |
| ResourceNotFound | 1002 | 资源不存在 |
| OperationFailed | 1003 | 操作失败 |
| InternalServerError | 1004 | 服务器内部错误 |
| InvalidParameter | 1005 | 参数错误 |
| InvalidOperation | 1006 | 操作无效 |
| Forbidden | 1007 | 权限不足 |
| SystemBusy | 1008 | 系统繁忙 |

### 认证错误（2xxx）

| 常量名 | 值 | 说明 |
|--------|-----|------|
| AuthCredentialsRequired | 2001 | 用户名或密码不能为空 |
| AuthInvalidCredentials | 2002 | 用户名或密码错误 |
| AuthAccountDisabled | 2003 | 账户已禁用 |
| AuthAccountLocked | 2004 | 账户已锁定 |
| AuthLoginUpdateFailed | 2005 | 登录状态更新失败 |
| AuthTokenInvalid | 2006 | 令牌无效或已过期 |

### 平台用户错误（3xxx）

| 常量名 | 值 | 说明 |
|--------|-----|------|
| UserUsernameRequired | 3001 | 用户名不能为空 |
| UserEmailRequired | 3002 | 邮箱不能为空 |
| UserPasswordRequired | 3003 | 密码不能为空 |
| UserCreateFailed | 3004 | 创建用户失败 |
| UserQueryFailed | 3005 | 查询用户失败 |
| UserNotFound | 3006 | 用户不存在 |
| UserUpdateFailed | 3007 | 更新用户失败 |
| UserStatusChangeFailed | 3008 | 用户状态变更失败 |

### 平台角色错误（4xxx）

| 常量名 | 值 | 说明 |
|--------|-----|------|
| RoleCodeRequired | 4001 | 角色编码不能为空 |
| RoleNameRequired | 4002 | 角色名称不能为空 |
| RoleCreateFailed | 4003 | 创建角色失败 |
| RoleQueryFailed | 4004 | 查询角色失败 |
| RoleNotFound | 4005 | 角色不存在 |
| RoleUpdateFailed | 4006 | 更新角色失败 |
| RoleStatusChangeFailed | 4007 | 角色状态变更失败 |
| RolePermissionBindFailed | 4008 | 角色权限绑定失败 |
| RoleMemberBindFailed | 4009 | 角色成员绑定失败 |

### 租户生命周期错误（6xxx）

| 常量名 | 值 | 说明 |
|--------|-----|------|
| TenantCodeRequired | 6001 | 租户编码不能为空 |
| TenantNameRequired | 6002 | 租户名称不能为空 |
| TenantCreateFailed | 6003 | 创建租户失败 |
| TenantQueryFailed | 6004 | 查询租户失败 |
| TenantNotFound | 6005 | 租户不存在 |
| TenantUpdateFailed | 6006 | 更新租户失败 |
| TenantStatusChangeFailed | 6007 | 租户状态变更失败 |
| TenantStatusTransitionDenied | 6008 | 租户状态流转不允许 |

### 租户信息错误（7xxx）

| 常量名 | 值 | 说明 |
|--------|-----|------|
| GroupCodeRequired | 7001 | 分组编码不能为空 |
| GroupCreateFailed | 7002 | 创建分组失败 |
| DomainRequired | 7003 | 域名不能为空 |
| DomainCreateFailed | 7004 | 创建域名失败 |
| TagKeyRequired | 7005 | 标签键不能为空 |
| TagCreateFailed | 7006 | 创建标签失败 |
| TagBindFailed | 7007 | 标签绑定失败 |

### 租户资源错误（8xxx）

| 常量名 | 值 | 说明 |
|--------|-----|------|
| QuotaTypeRequired | 8001 | 配额类型不能为空 |
| QuotaLimitInvalid | 8002 | 配额上限必须大于0 |
| QuotaSaveFailed | 8003 | 保存配额失败 |

### 租户配置错误（9xxx）

| 常量名 | 值 | 说明 |
|--------|-----|------|
| ConfigQueryFailed | 9001 | 查询配置失败 |
| ConfigUpdateFailed | 9002 | 更新配置失败 |
| FeatureKeyRequired | 9003 | 功能键不能为空 |
| FeatureFlagSaveFailed | 9004 | 保存功能开关失败 |
| FeatureFlagNotFound | 9005 | 功能开关不存在 |
| FeatureFlagToggleFailed | 9006 | 功能开关状态变更失败 |
| ParamKeyRequired | 9007 | 参数键不能为空 |
| ParamSaveFailed | 9008 | 保存参数失败 |
| ParamDeleteFailed | 9009 | 删除参数失败 |

### 套餐错误（10xxx）

| 常量名 | 值 | 说明 |
|--------|-----|------|
| PackageCodeRequired | 10001 | 套餐编码不能为空 |
| PackageNameRequired | 10002 | 套餐名称不能为空 |
| PackageCreateFailed | 10003 | 创建套餐失败 |
| PackageQueryFailed | 10004 | 查询套餐失败 |
| PackageNotFound | 10005 | 套餐不存在 |
| PackageUpdateFailed | 10006 | 更新套餐失败 |
| PackageStatusChangeFailed | 10007 | 套餐状态变更失败 |
| PackageVersionCodeRequired | 10008 | 版本编码不能为空 |
| PackageVersionNameRequired | 10009 | 版本名称不能为空 |
| PackageVersionCreateFailed | 10010 | 创建版本失败 |
| PackageCapabilityKeyRequired | 10011 | 能力键不能为空 |
| PackageCapabilitySaveFailed | 10012 | 保存能力失败 |

### 订阅错误（11xxx）

| 常量名 | 值 | 说明 |
|--------|-----|------|
| SubscriptionCreateFailed | 11001 | 创建订阅失败 |
| SubscriptionQueryFailed | 11002 | 查询订阅失败 |
| SubscriptionNotFound | 11003 | 订阅不存在 |
| SubscriptionCancelFailed | 11004 | 取消订阅失败 |
| TrialCreateFailed | 11005 | 创建试用失败 |

### 计费错误（12xxx）

| 常量名 | 值 | 说明 |
|--------|-----|------|
| InvoiceCreateFailed | 12001 | 创建发票失败 |
| InvoiceQueryFailed | 12002 | 查询发票失败 |
| InvoiceNotFound | 12003 | 发票不存在 |
| InvoiceVoidFailed | 12004 | 作废发票失败 |
| PaymentOrderCreateFailed | 12005 | 创建支付单失败 |
| RefundCreateFailed | 12006 | 创建退款失败 |

### API 集成错误（13xxx）

| 常量名 | 值 | 说明 |
|--------|-----|------|
| ApiKeyCreateFailed | 13001 | 创建API密钥失败 |
| ApiKeyDisableFailed | 13002 | 禁用API密钥失败 |
| ApiKeyQueryFailed | 13003 | 查询API密钥失败 |
| ApiKeyNotFound | 13004 | API密钥不存在 |
| WebhookCreateFailed | 13005 | 创建Webhook失败 |
| WebhookQueryFailed | 13006 | 查询Webhook失败 |
| WebhookNotFound | 13007 | Webhook不存在 |
| WebhookUpdateFailed | 13008 | 更新Webhook失败 |
| WebhookStatusChangeFailed | 13009 | Webhook状态变更失败 |

### 通知错误（16xxx）

| 常量名 | 值 | 说明 |
|--------|-----|------|
| NotificationTemplateNameRequired | 16001 | 模板名称不能为空 |
| NotificationTemplateCreateFailed | 16002 | 创建通知模板失败 |
| NotificationTemplateQueryFailed | 16003 | 查询通知模板失败 |
| NotificationTemplateNotFound | 16004 | 通知模板不存在 |
| NotificationTemplateUpdateFailed | 16005 | 更新通知模板失败 |
| NotificationTemplateStatusChangeFailed | 16006 | 通知模板状态变更失败 |
| NotificationCreateFailed | 16007 | 创建通知失败 |
| NotificationQueryFailed | 16008 | 查询通知失败 |
| NotificationNotFound | 16009 | 通知不存在 |
| NotificationMarkReadFailed | 16010 | 标记通知已读失败 |

### 存储错误（17xxx）

| 常量名 | 值 | 说明 |
|--------|-----|------|
| StorageStrategyNameRequired | 17001 | 策略名称不能为空 |
| StorageStrategyCreateFailed | 17002 | 创建存储策略失败 |
| StorageStrategyQueryFailed | 17003 | 查询存储策略失败 |
| StorageStrategyNotFound | 17004 | 存储策略不存在 |
| StorageStrategyUpdateFailed | 17005 | 更新存储策略失败 |
| StorageStrategyStatusChangeFailed | 17006 | 存储策略状态变更失败 |
| FileDeleteFailed | 17007 | 删除文件失败 |
| FileAccessPolicySaveFailed | 17008 | 保存文件访问策略失败 |
