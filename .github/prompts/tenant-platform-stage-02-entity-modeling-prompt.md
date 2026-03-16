# 租户平台阶段 02：实体建模与首次编译提示词

## 目标

本阶段只负责：

1. 创建租户平台实体、枚举、必要值对象、DTO 初稿；
2. 将所有手写实体放入 `src/YTStdTenantPlatform/entity/TenantPlatform/*.cs`；
3. 按 `entity-prompt.md` 的规则完成 `[Entity]` / `[Column]` / `[Index]` / 审计 / 主从关系建模；
4. 完成首次编译，触发 `YTStdEntity.Generator` 生成相关代码；
5. 修正实体层面的编译问题。

**本阶段不要大量实现 WebAPI、初始化数据和前端页面。**

---

## 必须阅读的文件

- `.github/prompts/entity-prompt.md`
- `.github/prompts/tenant-platform-backend-prompt.md`
- `docs/TenantPlatform/database_dictionary.md`
- `docs/TenantPlatform/architecture.md`
- `docs/existing-projects-reference.md`

---

## 建模硬约束

1. 所有实体命名空间统一建议为 `YTStdTenantPlatform.Entity.TenantPlatform`。
2. 实体文件位置固定为：`src/YTStdTenantPlatform/entity/TenantPlatform/*.cs`。
3. **禁止出现裸 `TenantId` / `tenant_id` 字段名**。
4. 所有公开类型与成员必须有**中文 XML 注释**。
5. 需要根据字典中的 CHECK 约束生成对应枚举。
6. 根据唯一键、索引说明、关系说明补齐 `[Index]` 等元数据。
7. 如有适合审计的实体，按现有实体生成器模式启用审计表能力。
8. 先保证实体层语义正确与可编译，再考虑上层服务。

---

## 本阶段建议实施顺序

1. 创建 `src/YTStdTenantPlatform/` 主项目骨架与 `tests/YTStdTenantPlatform.Tests/` 测试项目骨架（如尚不存在）。
2. 按模块优先级落地实体：
   - 平台管理体系
   - 租户生命周期体系
   - 租户信息体系
   - 租户资源 / 配置中心
   - 套餐 / 订阅 / 计费
   - API 与集成 / 运营 / 日志 / 通知 / 文件
3. 补充必要枚举、值对象与基础 DTO。
4. 执行 `dotnet build YTStd.slnx`，触发源生成器。
5. 修正生成器相关报错，直到实体层稳定。

---

## 本阶段交付物

- `src/YTStdTenantPlatform/entity/TenantPlatform/*.cs`
- `src/YTStdTenantPlatform/Domain/Enums/*.cs`
- 必要的值对象与 DTO 初稿
- 编译成功后的生成代码（通过编译产物验证，不手写 `.g.cs`）
- 与实体建模相关的最基础测试（如需要）

---

## 验证要求

至少执行并报告：
- `dotnet build YTStd.slnx`
- 如新增测试项目可编译，也请说明测试项目是否已纳入 solution

并明确说明：
- 哪些实体已完成
- 哪些关系/索引/枚举已覆盖
- 哪些内容留到下一阶段（初始化/WebAPI/前端）

---

## 明确禁止

- 不要在本阶段大规模实现 Endpoint；
- 不要绕开生成器手写 DAL/CRUD；
- 不要使用 SQL 文件替代实体建模；
- 不要把实体散落到非 `entity/TenantPlatform/` 目录。
