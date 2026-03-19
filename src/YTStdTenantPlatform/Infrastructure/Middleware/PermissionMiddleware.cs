using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using YTStdLogger.Core;
using YTStdTenantPlatform.Infrastructure.Auth;
using YTStdTenantPlatform.Infrastructure.Cache;
using YTStdTenantPlatform.Infrastructure.Serialization;

namespace YTStdTenantPlatform.Infrastructure.Middleware
{
    /// <summary>平台权限中间件，基于 Local Cache 进行权限码校验</summary>
    public sealed class PermissionMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>不需要认证的路径前缀列表</summary>
        private static readonly string[] PublicPaths = new[]
        {
            "/api/auth/login",
            "/api/auth/refresh",
            "/api/health",
            "/healthz"
        };

        /// <summary>构造权限中间件</summary>
        public PermissionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>处理请求，校验用户认证与权限</summary>
        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? "/";

            // 公开路径跳过认证
            if (IsPublicPath(path))
            {
                await _next(context);
                return;
            }

            // 尝试解析当前用户
            var currentUser = PlatformAuthHandler.TryResolveUser(context);
            if (currentUser == null)
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json; charset=utf-8";
                await Utf8JsonWriterHelper.WriteResponseAsync(
                    context.Response,
                    context.TraceIdentifier,
                    static (writer, traceId) =>
                    {
                        writer.WriteStartObject();
                        writer.WriteBoolean("success", false);
                        writer.WriteString("message", "未认证: 请先登录");
                        writer.WriteNull("data");
                        writer.WriteString("traceId", traceId);
                        writer.WriteEndObject();
                    },
                    context.RequestAborted);
                return;
            }

            // 存储当前用户到 HttpContext
            context.Items[CurrentUser.HttpContextKey] = currentUser;

            // 超级管理员跳过权限检查
            if (currentUser.IsSuperAdmin)
            {
                await _next(context);
                return;
            }

            // 检查 API 权限
            var method = context.Request.Method;
            var requiredPermission = FindRequiredPermission(path, method);
            if (requiredPermission != null && !currentUser.HasPermission(requiredPermission))
            {
                Logger.Warn(0, currentUser.UserId,
                    "[PermissionMiddleware] 权限不足: " + currentUser.Username +
                    " 缺少权限 " + requiredPermission +
                    " 访问 " + method + " " + path);

                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json; charset=utf-8";
                await Utf8JsonWriterHelper.WriteResponseAsync(
                    context.Response,
                    context.TraceIdentifier,
                    static (writer, traceId) =>
                    {
                        writer.WriteStartObject();
                        writer.WriteBoolean("success", false);
                        writer.WriteString("message", "权限不足: 您没有执行此操作的权限");
                        writer.WriteNull("data");
                        writer.WriteString("traceId", traceId);
                        writer.WriteEndObject();
                    },
                    context.RequestAborted);
                return;
            }

            await _next(context);
        }

        /// <summary>判断是否为公开路径</summary>
        private static bool IsPublicPath(string path)
        {
            for (int i = 0; i < PublicPaths.Length; i++)
            {
                if (path.StartsWith(PublicPaths[i], StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 查找请求路径对应的权限编码。
        /// 基于 Local Cache 中的权限快照匹配 Path 和 Method。
        /// </summary>
        private static string? FindRequiredPermission(string path, string method)
        {
            var permCache = PlatformCacheWarmer.PermissionCache;
            foreach (var kvp in permCache)
            {
                var perm = kvp.Value;
                if (!string.IsNullOrEmpty(perm.Path) &&
                    !string.IsNullOrEmpty(perm.Method) &&
                    path.StartsWith(perm.Path, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(perm.Method, method, StringComparison.OrdinalIgnoreCase))
                {
                    return perm.Code;
                }
            }
            return null;
        }
    }
}
