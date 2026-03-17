using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using YTStdLogger.Core;
using YTStdTenantPlatform.Application.Dtos;
using YTStdTenantPlatform.Entity.TenantPlatform;

namespace YTStdTenantPlatform.Application.Services
{
    /// <summary>API 与集成平台应用服务</summary>
    public static class ApiIntegrationAppService
    {
        // ──────────────────────────────────────────────────────
        // API 密钥
        // ──────────────────────────────────────────────────────

        /// <summary>获取 API 密钥分页列表</summary>
        public static async ValueTask<PagedResult<TenantApiKeyDto>> GetApiKeyListAsync(
            int tenantId, long operatorId, long tenantRefId, PagedRequest request)
        {
            var (result, data) = await TenantApiKeyCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null)
                return new PagedResult<TenantApiKeyDto> { Page = request.NormalizedPage, PageSize = request.NormalizedPageSize };

            var filtered = new List<TenantApiKey>();
            foreach (var k in data)
            {
                if (k.TenantRefId == tenantRefId)
                    filtered.Add(k);
            }

            var items = new List<TenantApiKeyDto>();
            var offset = request.Offset;
            var size = request.NormalizedPageSize;
            for (int i = offset; i < filtered.Count && i < offset + size; i++)
                items.Add(MapApiKeyToDto(filtered[i]));

            return new PagedResult<TenantApiKeyDto>
            {
                Items = items, Total = filtered.Count,
                Page = request.NormalizedPage, PageSize = request.NormalizedPageSize
            };
        }

        /// <summary>创建 API 密钥</summary>
        public static async ValueTask<ApiResult<ApiKeyCreatedResult>> CreateApiKeyAsync(
            int tenantId, long operatorId, CreateApiKeyRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.KeyName))
                return ApiResult<ApiKeyCreatedResult>.Fail("密钥名称不能为空");

            var accessKey = "ak_" + Guid.NewGuid().ToString("N");
            var rawSecret = Guid.NewGuid().ToString("N");
            var secretHash = HashSecret(rawSecret);
            var now = DateTime.UtcNow;

            var entity = new TenantApiKey
            {
                TenantRefId = req.TenantRefId,
                KeyName = req.KeyName.Trim(),
                AccessKey = accessKey,
                SecretHash = secretHash,
                Status = "active",
                ExpiresAt = req.ExpiresAt,
                CreatedBy = operatorId,
                CreatedAt = now,
                UpdatedAt = now
            };

            var insResult = await TenantApiKeyCRUD.InsertAsync(tenantId, operatorId, entity);
            if (!insResult.Success)
                return ApiResult<ApiKeyCreatedResult>.Fail("创建 API 密钥失败: " + insResult.ErrorMessage);

            Logger.Info(tenantId, operatorId,
                "[ApiIntegrationAppService] 创建 API 密钥: " + req.KeyName);
            return ApiResult<ApiKeyCreatedResult>.Ok(new ApiKeyCreatedResult
            {
                Id = insResult.Id,
                AccessKey = accessKey,
                SecretKey = rawSecret
            });
        }

        /// <summary>禁用 API 密钥</summary>
        public static async ValueTask<ApiResult> DisableApiKeyAsync(
            int tenantId, long operatorId, long id)
        {
            var (getResult, keys) = await TenantApiKeyCRUD.GetListAsync(tenantId, operatorId);
            if (!getResult.Success || keys == null) return ApiResult.Fail("查询 API 密钥失败");

            TenantApiKey? target = null;
            foreach (var k in keys) { if (k.Id == id) { target = k; break; } }
            if (target == null) return ApiResult.Fail("API 密钥不存在");

            target.Status = "disabled";
            target.UpdatedAt = DateTime.UtcNow;

            var updResult = await TenantApiKeyCRUD.UpdateAsync(tenantId, operatorId, target);
            if (!updResult.Success) return ApiResult.Fail("禁用 API 密钥失败");

            Logger.Info(tenantId, operatorId,
                "[ApiIntegrationAppService] 禁用 API 密钥: " + target.KeyName);
            return ApiResult.Ok();
        }

        // ──────────────────────────────────────────────────────
        // API 用量统计
        // ──────────────────────────────────────────────────────

        /// <summary>获取 API 用量统计列表</summary>
        public static async ValueTask<PagedResult<TenantApiUsageStatDto>> GetApiUsageStatsAsync(
            int tenantId, long operatorId, long tenantRefId, PagedRequest request)
        {
            var (result, data) = await TenantApiUsageStatCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null)
                return new PagedResult<TenantApiUsageStatDto> { Page = request.NormalizedPage, PageSize = request.NormalizedPageSize };

            var filtered = new List<TenantApiUsageStat>();
            foreach (var s in data)
            {
                if (s.TenantRefId == tenantRefId)
                    filtered.Add(s);
            }

            var items = new List<TenantApiUsageStatDto>();
            var offset = request.Offset;
            var size = request.NormalizedPageSize;
            for (int i = offset; i < filtered.Count && i < offset + size; i++)
                items.Add(MapUsageStatToDto(filtered[i]));

            return new PagedResult<TenantApiUsageStatDto>
            {
                Items = items, Total = filtered.Count,
                Page = request.NormalizedPage, PageSize = request.NormalizedPageSize
            };
        }

        // ──────────────────────────────────────────────────────
        // Webhook 事件定义
        // ──────────────────────────────────────────────────────

        /// <summary>获取 Webhook 事件列表</summary>
        public static async ValueTask<PagedResult<WebhookEventDto>> GetWebhookEventListAsync(
            int tenantId, long operatorId, PagedRequest request)
        {
            var (result, data) = await WebhookEventCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null)
                return new PagedResult<WebhookEventDto> { Page = request.NormalizedPage, PageSize = request.NormalizedPageSize };

            var items = new List<WebhookEventDto>();
            var offset = request.Offset;
            var size = request.NormalizedPageSize;
            for (int i = offset; i < data.Count && i < offset + size; i++)
            {
                var e = data[i];
                items.Add(new WebhookEventDto
                {
                    Id = e.Id, EventCode = e.EventCode, EventName = e.EventName,
                    Description = e.Description, CreatedAt = e.CreatedAt
                });
            }

            return new PagedResult<WebhookEventDto>
            {
                Items = items, Total = data.Count,
                Page = request.NormalizedPage, PageSize = request.NormalizedPageSize
            };
        }

        // ──────────────────────────────────────────────────────
        // 租户 Webhook
        // ──────────────────────────────────────────────────────

        /// <summary>获取租户 Webhook 列表</summary>
        public static async ValueTask<PagedResult<TenantWebhookDto>> GetWebhookListAsync(
            int tenantId, long operatorId, long tenantRefId, PagedRequest request)
        {
            var (result, data) = await TenantWebhookCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null)
                return new PagedResult<TenantWebhookDto> { Page = request.NormalizedPage, PageSize = request.NormalizedPageSize };

            var filtered = new List<TenantWebhook>();
            foreach (var w in data)
            {
                if (w.TenantRefId == tenantRefId)
                    filtered.Add(w);
            }

            var items = new List<TenantWebhookDto>();
            var offset = request.Offset;
            var size = request.NormalizedPageSize;
            for (int i = offset; i < filtered.Count && i < offset + size; i++)
                items.Add(MapWebhookToDto(filtered[i]));

            return new PagedResult<TenantWebhookDto>
            {
                Items = items, Total = filtered.Count,
                Page = request.NormalizedPage, PageSize = request.NormalizedPageSize
            };
        }

        /// <summary>创建租户 Webhook</summary>
        public static async ValueTask<ApiResult<long>> CreateWebhookAsync(
            int tenantId, long operatorId, CreateWebhookRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.WebhookName))
                return ApiResult<long>.Fail("Webhook 名称不能为空");
            if (string.IsNullOrWhiteSpace(req.TargetUrl))
                return ApiResult<long>.Fail("目标地址不能为空");

            var now = DateTime.UtcNow;
            var entity = new TenantWebhook
            {
                TenantRefId = req.TenantRefId,
                WebhookName = req.WebhookName.Trim(),
                TargetUrl = req.TargetUrl.Trim(),
                Status = "active",
                CreatedAt = now,
                UpdatedAt = now
            };

            var insResult = await TenantWebhookCRUD.InsertAsync(tenantId, operatorId, entity);
            if (!insResult.Success)
                return ApiResult<long>.Fail("创建 Webhook 失败: " + insResult.ErrorMessage);

            Logger.Info(tenantId, operatorId,
                "[ApiIntegrationAppService] 创建 Webhook: " + req.WebhookName);
            return ApiResult<long>.Ok(insResult.Id);
        }

        /// <summary>更新租户 Webhook</summary>
        public static async ValueTask<ApiResult> UpdateWebhookAsync(
            int tenantId, long operatorId, long id, UpdateWebhookRequest req)
        {
            var (getResult, webhooks) = await TenantWebhookCRUD.GetListAsync(tenantId, operatorId);
            if (!getResult.Success || webhooks == null) return ApiResult.Fail("查询 Webhook 失败");

            TenantWebhook? target = null;
            foreach (var w in webhooks) { if (w.Id == id) { target = w; break; } }
            if (target == null) return ApiResult.Fail("Webhook 不存在");

            if (req.WebhookName != null) target.WebhookName = req.WebhookName;
            if (req.TargetUrl != null) target.TargetUrl = req.TargetUrl;
            target.UpdatedAt = DateTime.UtcNow;

            var updResult = await TenantWebhookCRUD.UpdateAsync(tenantId, operatorId, target);
            if (!updResult.Success) return ApiResult.Fail("更新 Webhook 失败");

            Logger.Info(tenantId, operatorId,
                "[ApiIntegrationAppService] 更新 Webhook: " + target.WebhookName);
            return ApiResult.Ok();
        }

        /// <summary>设置 Webhook 状态（启用/禁用）</summary>
        public static async ValueTask<ApiResult> SetWebhookStatusAsync(
            int tenantId, long operatorId, long id, string status)
        {
            var (getResult, webhooks) = await TenantWebhookCRUD.GetListAsync(tenantId, operatorId);
            if (!getResult.Success || webhooks == null) return ApiResult.Fail("查询 Webhook 失败");

            TenantWebhook? target = null;
            foreach (var w in webhooks) { if (w.Id == id) { target = w; break; } }
            if (target == null) return ApiResult.Fail("Webhook 不存在");

            target.Status = status;
            target.UpdatedAt = DateTime.UtcNow;

            var updResult = await TenantWebhookCRUD.UpdateAsync(tenantId, operatorId, target);
            if (!updResult.Success) return ApiResult.Fail("状态变更失败");

            Logger.Info(tenantId, operatorId,
                "[ApiIntegrationAppService] Webhook 状态变更: " + target.WebhookName + " → " + status);
            return ApiResult.Ok();
        }

        // ──────────────────────────────────────────────────────
        // Webhook 投递日志
        // ──────────────────────────────────────────────────────

        /// <summary>获取 Webhook 投递日志列表</summary>
        public static async ValueTask<PagedResult<WebhookDeliveryLogDto>> GetDeliveryLogsAsync(
            int tenantId, long operatorId, long webhookId, PagedRequest request)
        {
            var (result, data) = await WebhookDeliveryLogCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null)
                return new PagedResult<WebhookDeliveryLogDto> { Page = request.NormalizedPage, PageSize = request.NormalizedPageSize };

            var filtered = new List<WebhookDeliveryLog>();
            foreach (var log in data)
            {
                if (log.WebhookId == webhookId)
                    filtered.Add(log);
            }

            var items = new List<WebhookDeliveryLogDto>();
            var offset = request.Offset;
            var size = request.NormalizedPageSize;
            for (int i = offset; i < filtered.Count && i < offset + size; i++)
            {
                var l = filtered[i];
                items.Add(new WebhookDeliveryLogDto
                {
                    Id = l.Id, WebhookId = l.WebhookId, EventId = l.EventId,
                    DeliveryStatus = l.DeliveryStatus,
                    ResponseStatusCode = l.ResponseStatusCode,
                    RetryCount = l.RetryCount, DeliveredAt = l.DeliveredAt,
                    CreatedAt = l.CreatedAt
                });
            }

            return new PagedResult<WebhookDeliveryLogDto>
            {
                Items = items, Total = filtered.Count,
                Page = request.NormalizedPage, PageSize = request.NormalizedPageSize
            };
        }

        // ──────────────────────────────────────────────────────
        // Helpers
        // ──────────────────────────────────────────────────────

        /// <summary>哈希密钥</summary>
        private static string HashSecret(string secret)
        {
            var data = Encoding.UTF8.GetBytes(secret);
            var hash = SHA256.HashData(data);
            return Convert.ToHexString(hash);
        }

        private static TenantApiKeyDto MapApiKeyToDto(TenantApiKey k) => new TenantApiKeyDto
        {
            Id = k.Id, TenantRefId = k.TenantRefId, KeyName = k.KeyName,
            AccessKey = k.AccessKey, Status = k.Status,
            QuotaLimit = k.QuotaLimit, RateLimit = k.RateLimit,
            LastUsedAt = k.LastUsedAt, ExpiresAt = k.ExpiresAt,
            CreatedAt = k.CreatedAt
        };

        private static TenantApiUsageStatDto MapUsageStatToDto(TenantApiUsageStat s) => new TenantApiUsageStatDto
        {
            Id = s.Id, TenantRefId = s.TenantRefId, ApiKeyId = s.ApiKeyId,
            StatDate = s.StatDate, ApiPath = s.ApiPath,
            RequestCount = s.RequestCount, SuccessCount = s.SuccessCount,
            ErrorCount = s.ErrorCount, AverageLatencyMs = s.AverageLatencyMs,
            CreatedAt = s.CreatedAt
        };

        private static TenantWebhookDto MapWebhookToDto(TenantWebhook w) => new TenantWebhookDto
        {
            Id = w.Id, TenantRefId = w.TenantRefId, WebhookName = w.WebhookName,
            TargetUrl = w.TargetUrl, Status = w.Status, CreatedAt = w.CreatedAt
        };
    }
}
