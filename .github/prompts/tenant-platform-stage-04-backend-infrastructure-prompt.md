# 租户平台阶段 04：后端主程序骨架、中间件与基础设施提示词

## 目标

本阶段只负责后端基础设施，不负责完成全部业务 API。

请实现：
1. 单体 WebAPI 主程序骨架；
2. 服务注册与路由注册骨架；
3. 全局异常、请求日志、TraceId、权限、限流、审计等中间件；
4. Local Cache 与失效/刷新协调组件；
5. 认证授权、后台任务调度、健康检查等基础设施骨架。

---

## 必须阅读的文件

- `.github/prompts/tenant-platform-backend-prompt.md`
- `docs/TenantPlatform/architecture.md`
- `docs/existing-projects-reference.md`

---

## 本阶段建议目录

```text
src/YTStdTenantPlatform/
├── Program.cs
├── Bootstrap/
│   ├── ServiceRegistration.cs
│   ├── RouteRegistration.cs
│   └── StartupInitialization.cs
├── Infrastructure/
│   ├── Auth/
│   ├── Cache/
│   ├── Scheduling/
│   ├── Persistence/
│   └── Initialization/
└── Endpoints/
```

---

## 本阶段必须实现的内容

1. **Program / Bootstrap**
   - 服务注册入口
   - RouteGroup 注册入口
   - 启动初始化挂载点

2. **中间件或等效组件**
   - 全局异常处理中间件
   - 请求日志 / TraceId 中间件
   - 权限中间件
   - 限流中间件
   - 审计记录中间件
   - Local Cache 刷新 / 失效协调组件
   - 启动初始化引导器

3. **缓存基础设施**
   - 进程内快照结构
   - 原子替换 / 显式失效 / 轻量刷新
   - 与权限、配置、功能开关联动

4. **后台任务骨架**
   - 租户初始化任务
   - 到期提醒任务
   - 账单生成任务
   - Webhook 重试任务

---

## 输出要求

请优先保证：
- 结构清晰；
- AOT 友好；
- 全部公开类型 / 方法补齐中文 XML 注释；
- 为后续阶段的 Endpoint 落地提供稳定挂载点。

---

## 验证要求

至少执行并报告：
- `dotnet build YTStd.slnx`
- 如有新增基础设施测试，请执行定向测试

---

## 明确禁止

- 不要在本阶段一次性完成所有业务服务；
- 不要引入 Redis / MQ / 微服务治理组件；
- 不要使用反射式 Controller 方案替代 AOT 友好的 Minimal API / RouteGroup。
