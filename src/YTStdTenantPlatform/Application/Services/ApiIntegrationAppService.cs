using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using YTStdLogger.Core;
using YTStdTenantPlatform.Application.Dtos;
using YTStdTenantPlatform.Entity.TenantPlatform;
using YTStdTenantPlatform.Application.Constants;

namespace YTStdTenantPlatform.Application.Services
{
    /// <summary>API 与集成平台应用服务</summary>
    public static class ApiIntegrationAppService
    {
        // ──────────────────────────────────────────────────────
        // API 密钥
        // ──────────────────────────────────────────────────────

        /// <summary>获取 API 密钥分页列表</summary>
        public static async ValueTask<PagedResult<TenantApiKeyRepDTO>> GetApiKeyListAsync(
            int tenantId, long operatorId, long tenantRefId, PagedRequest request)
        {
            var (result, data) = await TenantApiKeyCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null)
                return new PagedResult<TenantApiKeyRepDTO> { Page = request.NormalizedPage, PageSize = request.NormalizedPageSize };

            var filtered = new List<TenantApiKey>();
            foreach (var k in data)
            {
                if (k.TenantRefId == tenantRefId)
                    filtered.Add(k);
            }

            var items = new List<TenantApiKeyRepDTO>();
            var offset = request.Offset;
            var size = request.NormalizedPageSize;
            for (int i = offset; i < filtered.Count && i < offset + size; i++)
                items.Add(MapApiKeyToDto(filtered[i]));

            return new PagedResult<TenantApiKeyRepDTO>
            {
                Items = items, Total = filtered.Count,
                Page = request.NormalizedPage, PageSize = request.NormalizedPageSize
            };
        }

        /// <summary>创建 API 密钥</summary>
        public static async ValueTask<ApiResult<ApiKeyCreatedRepDTO>> CreateApiKeyAsync(
            int tenantId, long operatorId, CreateApiKeyReqDTO req)
        {
            if (string.IsNullOrWhiteSpace(req.KeyName))
                return ApiResult<ApiKeyCreatedRepDTO>.Fail(ErrorCodes.InvalidParameter, Messages.InvalidParameter);

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
                return ApiResult<ApiKeyCreatedRepDTO>.Fail(ErrorCodes.ApiKeyCreateFailed, Messages.ApiKeyCreateFailed);

            Logger.Info(tenantId, operatorId,
                "[ApiIntegrationAppService] 创建 API 密钥: " + req.KeyName);
            return ApiResult<ApiKeyCreatedRepDTO>.Ok(new ApiKeyCreatedRepDTO
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
            if (!getResult.Success || keys == null) return ApiResult.Fail(ErrorCodes.ApiKeyQueryFailed, Messages.ApiKeyQueryFailed);

            TenantApiKey? target = null;
            foreach (var k in keys) { if (k.Id == id) { target = k; break; } }
            if (target == null) return ApiResult.Fail(ErrorCodes.ApiKeyNotFound, Messages.ApiKeyNotFound);

            target.Status = "disabled";
            target.UpdatedAt = DateTime.UtcNow;

            var updResult = await TenantApiKeyCRUD.UpdateAsync(tenantId, operatorId, target);
            if (!updResult.Success) return ApiResult.Fail(ErrorCodes.ApiKeyDisableFailed, Messages.ApiKeyDisableFailed);

            Logger.Info(tenantId, operatorId,
                "[ApiIntegrationAppService] 禁用 API 密钥: " + target.KeyName);
            return ApiResult.Ok(Messages.OperationSuccess);
        }

        // ──────────────────────────────────────────────────────
        // API 用量统计
        // ──────────────────────────────────────────────────────

        /// <summary>获取 API 用量统计列表</summary>
        public static async ValueTask<PagedResult<TenantApiUsageStatRepDTO>> GetApiUsageStatsAsync(
            int tenantId, long operatorId, long tenantRefId, PagedRequest request)
        {
            var (result, data) = await TenantApiUsageStatCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null)
                return new PagedResult<TenantApiUsageStatRepDTO> { Page = request.NormalizedPage, PageSize = request.NormalizedPageSize };

            var filtered = new List<TenantApiUsageStat>();
            foreach (var s in data)
            {
                if (s.TenantRefId == tenantRefId)
                    filtered.Add(s);
            }

            var items = new List<TenantApiUsageStatRepDTO>();
            var offset = request.Offset;
            var size = request.NormalizedPageSize;
            for (int i = offset; i < filtered.Count && i < offset + size; i++)
                items.Add(MapUsageStatToDto(filtered[i]));

            return new PagedResult<TenantApiUsageStatRepDTO>
            {
                Items = items, Total = filtered.Count,
                Page = request.NormalizedPage, PageSize = request.NormalizedPageSize
            };
        }

        // ──────────────────────────────────────────────────────
        // Webhook 事件定义
        // ──────────────────────────────────────────────────────

        /// <summary>获取 Webhook 事件列表</summary>
        public static async ValueTask<PagedResult<WebhookEventRepDTO>> GetWebhookEventListAsync(
            int tenantId, long operatorId, PagedRequest request)
        {
            var (result, data) = await WebhookEventCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null)
                return new PagedResult<WebhookEventRepDTO> { Page = request.NormalizedPage, PageSize = request.NormalizedPageSize };

            var items = new List<WebhookEventRepDTO>();
            var offset = request.Offset;
            var size = request.NormalizedPageSize;
            for (int i = offset; i < data.Count && i < offset + size; i++)
            {
                var e = data[i];
                items.Add(new WebhookEventRepDTO
                {
                    Id = e.Id, EventCode = e.EventCode, EventName = e.EventName,
                    Description = e.Description, CreatedAt = e.CreatedAt
                });
            }

            return new PagedResult<WebhookEventRepDTO>
            {
                Items = items, Total = data.Count,
                Page = request.NormalizedPage, PageSize = request.NormalizedPageSize
            };
        }

        // ──────────────────────────────────────────────────────
        // 租户 Webhook
        // ──────────────────────────────────────────────────────

        /// <summary>获取租户 Webhook 列表</summary>
        public static async ValueTask<PagedResult<TenantWebhookRepDTO>> GetWebhookListAsync(
            int tenantId, long operatorId, long tenantRefId, PagedRequest request)
        {
            var (result, data) = await TenantWebhookCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null)
                return new PagedResult<TenantWebhookRepDTO> { Page = request.NormalizedPage, PageSize = request.NormalizedPageSize };

            var filtered = new List<TenantWebhook>();
            foreach (var w in data)
            {
                if (w.TenantRefId == tenantRefId)
                    filtered.Add(w);
            }

            var items = new List<TenantWebhookRepDTO>();
            var offset = request.Offset;
            var size = request.NormalizedPageSize;
            for (int i = offset; i < filtered.Count && i < offset + size; i++)
                items.Add(MapWebhookToDto(filtered[i]));

            return new PagedResult<TenantWebhookRepDTO>
            {
                Items = items, Total = filtered.Count,
                Page = request.NormalizedPage, PageSize = request.NormalizedPageSize
            };
        }

        /// <summary>创建租户 Webhook</summary>
        public static async ValueTask<ApiResult<long>> CreateWebhookAsync(
            int tenantId, long operatorId, CreateWebhookReqDTO req)
        {
            if (string.IsNullOrWhiteSpace(req.WebhookName))
                return ApiResult<long>.Fail(ErrorCodes.InvalidParameter, Messages.InvalidParameter);
            if (string.IsNullOrWhiteSpace(req.TargetUrl))
                return ApiResult<long>.Fail(ErrorCodes.InvalidParameter, Messages.InvalidParameter);

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
                return ApiResult<long>.Fail(ErrorCodes.WebhookCreateFailed, Messages.WebhookCreateFailed);

            Logger.Info(tenantId, operatorId,
                "[ApiIntegrationAppService] 创建 Webhook: " + req.WebhookName);
            return ApiResult<long>.Ok(insResult.Id);
        }

        /// <summary>更新租户 Webhook</summary>
        public static async ValueTask<ApiResult> UpdateWebhookAsync(
            int tenantId, long operatorId, long id, UpdateWebhookReqDTO req)
        {
            var (getResult, webhooks) = await TenantWebhookCRUD.GetListAsync(tenantId, operatorId);
            if (!getResult.Success || webhooks == null) return ApiResult.Fail(ErrorCodes.WebhookQueryFailed, Messages.WebhookQueryFailed);

            TenantWebhook? target = null;
            foreach (var w in webhooks) { if (w.Id == id) { target = w; break; } }
            if (target == null) return ApiResult.Fail(ErrorCodes.WebhookNotFound, Messages.WebhookNotFound);

            if (req.WebhookName != null) target.WebhookName = req.WebhookName;
            if (req.TargetUrl != null) target.TargetUrl = req.TargetUrl;
            target.UpdatedAt = DateTime.UtcNow;

            var updResult = await TenantWebhookCRUD.UpdateAsync(tenantId, operatorId, target);
            if (!updResult.Success) return ApiResult.Fail(ErrorCodes.WebhookUpdateFailed, Messages.WebhookUpdateFailed);

            Logger.Info(tenantId, operatorId,
                "[ApiIntegrationAppService] 更新 Webhook: " + target.WebhookName);
            return ApiResult.Ok(Messages.OperationSuccess);
        }

        /// <summary>设置 Webhook 状态（启用/禁用）</summary>
        public static async ValueTask<ApiResult> SetWebhookStatusAsync(
            int tenantId, long operatorId, long id, string status)
        {
            var (getResult, webhooks) = await TenantWebhookCRUD.GetListAsync(tenantId, operatorId);
            if (!getResult.Success || webhooks == null) return ApiResult.Fail(ErrorCodes.WebhookQueryFailed, Messages.WebhookQueryFailed);

            TenantWebhook? target = null;
            foreach (var w in webhooks) { if (w.Id == id) { target = w; break; } }
            if (target == null) return ApiResult.Fail(ErrorCodes.WebhookNotFound, Messages.WebhookNotFound);

            target.Status = status;
            target.UpdatedAt = DateTime.UtcNow;

            var updResult = await TenantWebhookCRUD.UpdateAsync(tenantId, operatorId, target);
            if (!updResult.Success) return ApiResult.Fail(ErrorCodes.WebhookStatusChangeFailed, Messages.WebhookStatusChangeFailed);

            Logger.Info(tenantId, operatorId,
                "[ApiIntegrationAppService] Webhook 状态变更: " + target.WebhookName + " → " + status);
            return ApiResult.Ok(Messages.OperationSuccess);
        }

        // ──────────────────────────────────────────────────────
        // Webhook 投递日志
        // ──────────────────────────────────────────────────────

        /// <summary>获取 Webhook 投递日志列表</summary>
        public static async ValueTask<PagedResult<WebhookDeliveryLogRepDTO>> GetDeliveryLogsAsync(
            int tenantId, long operatorId, long webhookId, PagedRequest request)
        {
            var (result, data) = await WebhookDeliveryLogCRUD.GetListAsync(tenantId, operatorId);
            if (!result.Success || data == null)
                return new PagedResult<WebhookDeliveryLogRepDTO> { Page = request.NormalizedPage, PageSize = request.NormalizedPageSize };

            var filtered = new List<WebhookDeliveryLog>();
            foreach (var log in data)
            {
                if (log.WebhookId == webhookId)
                    filtered.Add(log);
            }

            var items = new List<WebhookDeliveryLogRepDTO>();
            var offset = request.Offset;
            var size = request.NormalizedPageSize;
            for (int i = offset; i < filtered.Count && i < offset + size; i++)
            {
                var l = filtered[i];
                items.Add(new WebhookDeliveryLogRepDTO
                {
                    Id = l.Id, WebhookId = l.WebhookId, EventId = l.EventId,
                    DeliveryStatus = l.DeliveryStatus,
                    ResponseStatusCode = l.ResponseStatusCode,
                    RetryCount = l.RetryCount, DeliveredAt = l.DeliveredAt,
                    CreatedAt = l.CreatedAt
                });
            }

            return new PagedResult<WebhookDeliveryLogRepDTO>
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

        private static TenantApiKeyRepDTO MapApiKeyToDto(TenantApiKey k) => new TenantApiKeyRepDTO
        {
            Id = k.Id, TenantRefId = k.TenantRefId, KeyName = k.KeyName,
            AccessKey = k.AccessKey, Status = k.Status,
            QuotaLimit = k.QuotaLimit, RateLimit = k.RateLimit,
            LastUsedAt = k.LastUsedAt, ExpiresAt = k.ExpiresAt,
            CreatedAt = k.CreatedAt
        };

        private static TenantApiUsageStatRepDTO MapUsageStatToDto(TenantApiUsageStat s) => new TenantApiUsageStatRepDTO
        {
            Id = s.Id, TenantRefId = s.TenantRefId, ApiKeyId = s.ApiKeyId,
            StatDate = s.StatDate, ApiPath = s.ApiPath,
            RequestCount = s.RequestCount, SuccessCount = s.SuccessCount,
            ErrorCount = s.ErrorCount, AverageLatencyMs = s.AverageLatencyMs,
            CreatedAt = s.CreatedAt
        };

        private static TenantWebhookRepDTO MapWebhookToDto(TenantWebhook w) => new TenantWebhookRepDTO
        {
            Id = w.Id, TenantRefId = w.TenantRefId, WebhookName = w.WebhookName,
            TargetUrl = w.TargetUrl, Status = w.Status, CreatedAt = w.CreatedAt
        };
    }
}
