# 租户平台阶段 06：扩展后端 API、接口测试与审计联动提示词

## 目标

在核心 API 完成后，本阶段补齐扩展业务域：

1. SaaS 套餐系统
2. 订阅系统
3. 计费与账单系统
4. API 与集成平台
5. 平台运营体系
6. 日志与审计
7. 通知系统
8. 文件与存储

并补齐接口测试。

---

## 必须阅读的文件

- `.github/prompts/tenant-platform-backend-prompt.md`
- `docs/TenantPlatform/architecture.md`
- `docs/TenantPlatform/database_dictionary.md`
- 前一阶段已完成的基础设施与核心 API

---

## 本阶段范围

### 应用服务 / Endpoint 建议
- `PackageEndpoints`
- `SubscriptionEndpoints`
- `BillingEndpoints`
- `ApiIntegrationEndpoints`
- `PlatformOperationEndpoints`
- `AuditEndpoints`
- `NotificationEndpoints`
- `StorageEndpoints`

### 重点能力
- 套餐管理、版本、能力配置
- 租户订阅、升级、降级、续费、取消
- 账单、支付、退款、导出
- API Key、配额、限流、Webhook、推送日志
- 平台监控、统计
- 操作日志、审计日志、登录日志、系统日志
- 通知模板、站内信 / 邮件 / 短信
- 文件上传、下载、权限、存储策略

---

## 接口测试要求

请至少补齐：
- 核心扩展域的成功路径测试
- 权限拒绝测试
- 限流基础测试
- 审计记录触发测试
- 初始化数据依赖项存在性测试
- 缓存刷新 / 失效联动测试（如适用）

---

## 输出要求

请明确区分：
- 本阶段新增的 API
- 与核心域 API 的依赖关系
- 哪些测试是新增的接口测试
- 哪些审计 / 日志 / 权限处理已接入

---

## 验证要求

至少执行并报告：
- `dotnet build YTStd.slnx`
- 新增后端测试项目或定向测试

---

## 明确禁止

- 不要把前端代码掺入本阶段；
- 不要为了省事跳过接口测试；
- 不要在日志、通知、文件、Webhook 这些模块中省略审计与权限考虑。
