using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using YTStdAdo;
using YTStdLogger.Core;
using YTStdTenantPlatform.Application.Dtos;
using YTStdTenantPlatform.Endpoints;
using YTStdTenantPlatform.Entity.TenantPlatform;
using YTStdTenantPlatform.Infrastructure.Auth;
using YTStdTenantPlatform.Infrastructure.Cache;
using YTStdTenantPlatform.Infrastructure.Persistence;
using YTStdTenantPlatform.Infrastructure.Serialization;

namespace YTStdTenantPlatform.Bootstrap
{
    /// <summary>路由注册入口，集中管理所有 API 路由组的注册</summary>
    public static class RouteRegistration
    {
        /// <summary>注册所有路由</summary>
        public static void MapRoutes(WebApplication app)
        {
            // 健康检查端点
            MapHealthEndpoints(app);

            // 认证端点
            MapAuthEndpoints(app);

            // ──────────────────────────────────────────────
            // 模块 1：平台管理体系
            // ──────────────────────────────────────────────
            PlatformUserEndpoints.Map(app);
            PlatformRoleEndpoints.Map(app);
            PlatformPermissionEndpoints.Map(app);

            // ──────────────────────────────────────────────
            // 模块 2：租户生命周期体系
            // ──────────────────────────────────────────────
            TenantEndpoints.Map(app);

            // ──────────────────────────────────────────────
            // 模块 3：租户信息体系（分组、域名、标签）
            // ──────────────────────────────────────────────
            TenantInfoEndpoints.Map(app);

            // ──────────────────────────────────────────────
            // 模块 4：租户资源管理
            // ──────────────────────────────────────────────
            TenantResourceEndpoints.Map(app);

            // ──────────────────────────────────────────────
            // 模块 5：租户配置中心（系统配置、功能开关、参数）
            // ──────────────────────────────────────────────
            TenantConfigEndpoints.Map(app);

            // ──────────────────────────────────────────────
            // 模块 6：SaaS 套餐系统
            // ──────────────────────────────────────────────
            PackageEndpoints.Map(app);

            // ──────────────────────────────────────────────
            // 模块 7：订阅系统
            // ──────────────────────────────────────────────
            SubscriptionEndpoints.Map(app);

            // ──────────────────────────────────────────────
            // 模块 8：计费与账单系统
            // ──────────────────────────────────────────────
            BillingEndpoints.Map(app);

            // ──────────────────────────────────────────────
            // 模块 9：API 与集成平台
            // ──────────────────────────────────────────────
            ApiIntegrationEndpoints.Map(app);

            // ──────────────────────────────────────────────
            // 模块 10：平台运营体系
            // ──────────────────────────────────────────────
            PlatformOperationEndpoints.Map(app);

            // ──────────────────────────────────────────────
            // 模块 11：日志与审计
            // ──────────────────────────────────────────────
            AuditEndpoints.Map(app);

            // ──────────────────────────────────────────────
            // 模块 12：通知系统
            // ──────────────────────────────────────────────
            NotificationEndpoints.Map(app);

            // ──────────────────────────────────────────────
            // 模块 13：文件与存储
            // ──────────────────────────────────────────────
            StorageEndpoints.Map(app);
        }

        /// <summary>注册健康检查端点</summary>
        private static void MapHealthEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/health")
                .WithTags("健康检查");

            group.MapGet("/", async (HttpContext ctx) =>
            {
                var result = await HealthCheck.CheckAllAsync();
                var statusCode = result.IsHealthy ? 200 : 503;
                var apiResult = result.IsHealthy
                    ? ApiResult.Ok(result.Message)
                    : ApiResult.Fail(result.Message);
                await TenantPlatformJsonResponseWriter.WriteAsync(ctx, apiResult, statusCode);
            }).WithSummary("综合健康检查");

            group.MapGet("/db", async (HttpContext ctx) =>
            {
                var result = await HealthCheck.CheckDatabaseAsync();
                var statusCode = result.IsHealthy ? 200 : 503;
                var apiResult = result.IsHealthy
                    ? ApiResult.Ok(result.Message)
                    : ApiResult.Fail(result.Message);
                await TenantPlatformJsonResponseWriter.WriteAsync(ctx, apiResult, statusCode);
            }).WithSummary("数据库健康检查");

            group.MapGet("/cache", async (HttpContext ctx) =>
            {
                var result = HealthCheck.CheckCache();
                var statusCode = result.IsHealthy ? 200 : 503;
                var apiResult = result.IsHealthy
                    ? ApiResult.Ok(result.Message)
                    : ApiResult.Fail(result.Message);
                await TenantPlatformJsonResponseWriter.WriteAsync(ctx, apiResult, statusCode);
            }).WithSummary("缓存健康检查");
        }

        /// <summary>注册认证端点（登录/刷新 Token）</summary>
        private static void MapAuthEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/auth")
                .WithTags("认证");

            group.MapPost("/login", async (HttpContext context, CancellationToken cancellationToken) =>
            {
                var request = await ReadLoginRequestAsync(context.Request, cancellationToken);
                if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                {
                    await WriteAuthJsonAsync(context, new AuthResponse(false, "用户名或密码不能为空", null, 0, 0, string.Empty, string.Empty, false), 400);
                    return;
                }

                var username = request.Username.Trim();
                var password = request.Password;
                var now = DateTime.UtcNow;
                var remoteIp = context.Connection.RemoteIpAddress?.ToString();
                var userAgent = context.Request.Headers.UserAgent.ToString();
                var (queryResult, users) = await PlatformUserCRUD.GetListAsync(0, 0);
                if (!queryResult.Success || users == null)
                {
                    Logger.Error(0, 0, "[RouteRegistration] 查询平台用户失败: " + queryResult.ErrorMessage);
                    await WriteAuthJsonAsync(context, new AuthResponse(false, "系统繁忙，请稍后再试", null, 0, 0, string.Empty, string.Empty, false), 500);
                    return;
                }

                PlatformUser? matchedUser = null;
                for (int i = 0; i < users.Count; i++)
                {
                    var candidate = users[i];
                    if (candidate.DeletedAt != null)
                    {
                        continue;
                    }

                    if (string.Equals(candidate.Username, username, StringComparison.OrdinalIgnoreCase))
                    {
                        matchedUser = candidate;
                        break;
                    }
                }

                var policy = PlatformCacheWarmer.ConfigSnapshot?.DefaultPasswordPolicy;
                var lockThreshold = policy?.LoginFailLockThreshold ?? 5;
                var lockDurationMinutes = policy?.LockDurationMinutes ?? 30;

                if (matchedUser == null)
                {
                    await RecordLoginAsync(null, username, remoteIp, userAgent, "password", "failed", "用户名或密码错误");
                    await WriteAuthJsonAsync(context, new AuthResponse(false, "用户名或密码错误", null, 0, 0, string.Empty, string.Empty, false), 401);
                    return;
                }

                if (string.Equals(matchedUser.Status, "disabled", StringComparison.OrdinalIgnoreCase))
                {
                    await RecordLoginAsync(matchedUser, username, remoteIp, userAgent, "password", "disabled", "账户已禁用");
                    await WriteAuthJsonAsync(context, new AuthResponse(false, "账户已禁用", null, 0, matchedUser.Id, matchedUser.Username, matchedUser.DisplayName, false), 403);
                    return;
                }

                if (matchedUser.LockedUntil.HasValue && matchedUser.LockedUntil.Value > now)
                {
                    matchedUser.Status = "locked";
                    await PlatformUserCRUD.UpdateAsync(0, 0, matchedUser);
                    await RecordLoginAsync(matchedUser, username, remoteIp, userAgent, "password", "locked", "账户已锁定");
                    await WriteAuthJsonAsync(context, new AuthResponse(false, "账户已锁定", null, 0, matchedUser.Id, matchedUser.Username, matchedUser.DisplayName, false), 423);
                    return;
                }

                if (string.Equals(matchedUser.Status, "locked", StringComparison.OrdinalIgnoreCase) && (!matchedUser.LockedUntil.HasValue || matchedUser.LockedUntil.Value <= now))
                {
                    matchedUser.Status = "active";
                    matchedUser.LockedUntil = null;
                }

                if (!VerifyPassword(password, matchedUser.PasswordHash, matchedUser.PasswordSalt))
                {
                    matchedUser.FailedLoginCount = matchedUser.FailedLoginCount + 1;
                    matchedUser.UpdatedAt = now;
                    if (matchedUser.FailedLoginCount >= lockThreshold)
                    {
                        matchedUser.Status = "locked";
                        matchedUser.LockedUntil = now.AddMinutes(lockDurationMinutes);
                    }

                    await PlatformUserCRUD.UpdateAsync(0, 0, matchedUser);
                    await RecordLoginAsync(matchedUser, username, remoteIp, userAgent, "password", matchedUser.Status == "locked" ? "locked" : "failed", "用户名或密码错误");
                    await WriteAuthJsonAsync(context, new AuthResponse(false, matchedUser.Status == "locked" ? "账户已锁定" : "用户名或密码错误", null, 0, matchedUser.Id, matchedUser.Username, matchedUser.DisplayName, false), matchedUser.Status == "locked" ? 423 : 401);
                    return;
                }

                matchedUser.Status = "active";
                matchedUser.FailedLoginCount = 0;
                matchedUser.LockedUntil = null;
                matchedUser.LastLoginAt = now;
                matchedUser.LastLoginIp = remoteIp;
                matchedUser.UpdatedAt = now;

                var updateResult = await PlatformUserCRUD.UpdateAsync(0, 0, matchedUser);
                if (!updateResult.Success)
                {
                    Logger.Error(0, 0, "[RouteRegistration] 更新登录状态失败: " + updateResult.ErrorMessage);
                    await WriteAuthJsonAsync(context, new AuthResponse(false, "登录状态更新失败", null, 0, matchedUser.Id, matchedUser.Username, matchedUser.DisplayName, false), 500);
                    return;
                }

                var token = PlatformAuthHandler.GenerateToken(matchedUser.Id, matchedUser.Username);
                var requirePasswordReset = matchedUser.PasswordExpiresAt.HasValue && matchedUser.PasswordExpiresAt.Value <= now;

                // 从缓存获取用户角色、权限、超管标识
                var userRoleCache = PlatformCacheWarmer.UserRoleCache;
                IReadOnlyList<string> loginRoles = userRoleCache.TryGetValue(matchedUser.Id, out var cachedRoles)
                    ? cachedRoles
                    : Array.Empty<string>();
                var rolePermCache = PlatformCacheWarmer.RoleCodePermissionCache;
                var permSet = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var loginIsSuperAdmin = false;
                for (int ri = 0; ri < loginRoles.Count; ri++)
                {
                    if (string.Equals(loginRoles[ri], "super_admin", StringComparison.OrdinalIgnoreCase))
                    {
                        loginIsSuperAdmin = true;
                    }
                    if (rolePermCache.TryGetValue(loginRoles[ri], out var rp))
                    {
                        for (int pi = 0; pi < rp.Count; pi++) permSet.Add(rp[pi]);
                    }
                }
                var loginPermissions = new List<string>(permSet.Count);
                foreach (var p in permSet) loginPermissions.Add(p);

                await RecordLoginAsync(matchedUser, username, remoteIp, userAgent, "password", "success", null);
                await WriteAuthJsonAsync(context, new AuthResponse(true, requirePasswordReset ? "登录成功，需尽快修改密码" : "登录成功", token, PlatformAuthHandler.GetTokenExpirySeconds(), matchedUser.Id, matchedUser.Username, matchedUser.DisplayName, requirePasswordReset, loginRoles, loginPermissions, loginIsSuperAdmin));
            }).WithSummary("平台用户登录");

            group.MapPost("/refresh", async (HttpContext context, CancellationToken cancellationToken) =>
            {
                var request = await ReadRefreshTokenRequestAsync(context.Request, cancellationToken);
                var token = request?.Token;
                if (string.IsNullOrWhiteSpace(token))
                {
                    var authHeader = context.Request.Headers.Authorization.ToString();
                    const string bearerPrefix = "Bearer ";
                    if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        token = authHeader.Substring(bearerPrefix.Length).Trim();
                    }
                }

                var currentUser = PlatformAuthHandler.TryResolveToken(token ?? string.Empty, context.TraceIdentifier);
                if (currentUser == null)
                {
                    await WriteAuthJsonAsync(context, new AuthResponse(false, "令牌无效或已过期", null, 0, 0, string.Empty, string.Empty, false), 401);
                    return;
                }

                var refreshedToken = PlatformAuthHandler.GenerateToken(currentUser.UserId, currentUser.Username);
                await WriteAuthJsonAsync(context, new AuthResponse(true, "刷新成功", refreshedToken, PlatformAuthHandler.GetTokenExpirySeconds(), currentUser.UserId, currentUser.Username, currentUser.DisplayName, false, currentUser.Roles, currentUser.Permissions, currentUser.IsSuperAdmin));
            }).WithSummary("刷新访问令牌");

            group.MapGet("/me", async (HttpContext context) =>
            {
                var currentUser = context.Items.TryGetValue(CurrentUser.HttpContextKey, out var userObj) && userObj is CurrentUser cu
                    ? cu
                    : CurrentUser.Anonymous;

                context.Response.ContentType = "application/json; charset=utf-8";
                await Utf8JsonWriterHelper.WriteResponseAsync(
                    context.Response,
                    currentUser,
                    static (writer, state) =>
                    {
                        writer.WriteStartObject();
                        writer.WriteBoolean("success", true);
                        writer.WriteString("message", "操作成功");
                        writer.WritePropertyName("data");
                        writer.WriteStartObject();
                        writer.WriteNumber("userId", state.UserId);
                        writer.WriteString("username", state.Username);
                        writer.WriteString("displayName", state.DisplayName);
                        writer.WriteBoolean("isSuperAdmin", state.IsSuperAdmin);
                        writer.WriteEndObject();
                        writer.WriteString("traceId", "");
                        writer.WriteEndObject();
                    },
                    context.RequestAborted);
            }).WithSummary("获取当前登录用户信息");
        }

        private static bool VerifyPassword(string password, string storedHash, string? storedSalt)
        {
            if (string.IsNullOrEmpty(storedHash))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(storedSalt))
            {
                var computedHash = HashPassword(password, storedSalt);
                if (string.Equals(storedHash, computedHash, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return string.Equals(storedHash, password, StringComparison.Ordinal) &&
                   string.Equals(storedSalt, password, StringComparison.Ordinal);
        }

        private static string HashPassword(string password, string salt)
        {
            var data = Encoding.UTF8.GetBytes(password + salt);
            var hash = SHA256.HashData(data);
            return Convert.ToHexString(hash);
        }

        private static async ValueTask RecordLoginAsync(
            PlatformUser? user,
            string username,
            string? remoteIp,
            string? userAgent,
            string loginType,
            string loginStatus,
            string? failureReason)
        {
            try
            {
                var log = new PlatformLoginLog
                {
                    Id = await DB.GetNextLongIdAsync(),
                    UserId = user?.Id,
                    Username = username,
                    LoginType = loginType,
                    LoginStatus = loginStatus,
                    IpAddress = remoteIp,
                    UserAgent = userAgent,
                    FailureReason = failureReason,
                    OccurredAt = DateTime.UtcNow
                };

                await PlatformLoginLogCRUD.InsertAsync(0, 0, log);
            }
            catch (Exception ex)
            {
                Logger.Error(0, 0, "[RouteRegistration] 写入登录日志失败: " + ex.Message);
            }
        }

        private static async Task<LoginRequest?> ReadLoginRequestAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            try
            {
                using var document = await JsonDocument.ParseAsync(request.Body, cancellationToken: cancellationToken);
                if (document.RootElement.ValueKind != JsonValueKind.Object)
                {
                    return null;
                }

                var root = document.RootElement;
                return new LoginRequest
                {
                    Username = TryGetString(root, "username"),
                    Password = TryGetString(root, "password")
                };
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static async Task<RefreshTokenRequest?> ReadRefreshTokenRequestAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            try
            {
                using var document = await JsonDocument.ParseAsync(request.Body, cancellationToken: cancellationToken);
                if (document.RootElement.ValueKind != JsonValueKind.Object)
                {
                    return null;
                }

                var root = document.RootElement;
                return new RefreshTokenRequest
                {
                    Token = TryGetString(root, "token")
                };
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static string TryGetString(JsonElement root, string propertyName)
        {
            if (!root.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.String)
            {
                return string.Empty;
            }

            return property.GetString() ?? string.Empty;
        }

        private static async Task WriteAuthJsonAsync(HttpContext context, AuthResponse response, int statusCode = 200)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json; charset=utf-8";
            await Utf8JsonWriterHelper.WriteResponseAsync(
                context.Response,
                response,
                static (writer, state) =>
                {
                    writer.WriteStartObject();
                    writer.WriteBoolean("success", state.Success);
                    writer.WriteString("message", state.Message);
                    writer.WritePropertyName("data");
                    writer.WriteStartObject();
                    if (string.IsNullOrEmpty(state.Token))
                    {
                        writer.WriteNull("token");
                    }
                    else
                    {
                        writer.WriteString("token", state.Token);
                    }

                    writer.WriteNumber("expiresIn", state.ExpiresIn);
                    writer.WriteNumber("userId", state.UserId);
                    writer.WriteString("username", state.Username);
                    writer.WriteString("displayName", state.DisplayName);
                    writer.WriteBoolean("requirePasswordReset", state.RequirePasswordReset);
                    writer.WritePropertyName("roles");
                    writer.WriteStartArray();
                    for (int i = 0; i < state.Roles.Count; i++)
                    {
                        writer.WriteStringValue(state.Roles[i]);
                    }
                    writer.WriteEndArray();
                    writer.WritePropertyName("permissions");
                    writer.WriteStartArray();
                    for (int i = 0; i < state.Permissions.Count; i++)
                    {
                        writer.WriteStringValue(state.Permissions[i]);
                    }
                    writer.WriteEndArray();
                    writer.WriteBoolean("isSuperAdmin", state.IsSuperAdmin);
                    writer.WriteEndObject();
                    writer.WriteString("traceId", "");
                    writer.WriteEndObject();
                },
                context.RequestAborted);
        }

        private sealed class LoginRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        private sealed class RefreshTokenRequest
        {
            public string Token { get; set; } = string.Empty;
        }

        private sealed class AuthResponse
        {
            public bool Success { get; }
            public string Message { get; }
            public string? Token { get; }
            public int ExpiresIn { get; }
            public long UserId { get; }
            public string Username { get; }
            public string DisplayName { get; }
            public bool RequirePasswordReset { get; }
            public IReadOnlyList<string> Roles { get; }
            public IReadOnlyList<string> Permissions { get; }
            public bool IsSuperAdmin { get; }

            public AuthResponse(bool success, string message, string? token, int expiresIn, long userId, string username, string displayName, bool requirePasswordReset,
                IReadOnlyList<string>? roles = null, IReadOnlyList<string>? permissions = null, bool isSuperAdmin = false)
            {
                Success = success;
                Message = message;
                Token = token;
                ExpiresIn = expiresIn;
                UserId = userId;
                Username = username;
                DisplayName = displayName;
                RequirePasswordReset = requirePasswordReset;
                Roles = roles ?? Array.Empty<string>();
                Permissions = permissions ?? Array.Empty<string>();
                IsSuperAdmin = isSuperAdmin;
            }
        }
    }
}
