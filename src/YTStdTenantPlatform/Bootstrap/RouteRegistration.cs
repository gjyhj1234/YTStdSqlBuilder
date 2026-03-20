using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using YTStdAdo;
using YTStdLogger.Core;
using YTStdTenantPlatform.Application.Constants;
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
                    ? ApiResult.Ok(Messages.OperationSuccess)
                    : ApiResult.Fail(ErrorCodes.OperationFailed, result.Message);
                await TenantPlatformJsonResponseWriter.WriteAsync(ctx, apiResult, statusCode);
            }).WithSummary("综合健康检查");

            group.MapGet("/db", async (HttpContext ctx) =>
            {
                var result = await HealthCheck.CheckDatabaseAsync();
                var statusCode = result.IsHealthy ? 200 : 503;
                var apiResult = result.IsHealthy
                    ? ApiResult.Ok(Messages.OperationSuccess)
                    : ApiResult.Fail(ErrorCodes.OperationFailed, result.Message);
                await TenantPlatformJsonResponseWriter.WriteAsync(ctx, apiResult, statusCode);
            }).WithSummary("数据库健康检查");

            group.MapGet("/cache", async (HttpContext ctx) =>
            {
                var result = HealthCheck.CheckCache();
                var statusCode = result.IsHealthy ? 200 : 503;
                var apiResult = result.IsHealthy
                    ? ApiResult.Ok(Messages.OperationSuccess)
                    : ApiResult.Fail(ErrorCodes.OperationFailed, result.Message);
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
                LoginReqDTO? request;
                try
                {
                    request = await context.Request.ReadFromJsonAsync<LoginReqDTO>(cancellationToken: cancellationToken);
                }
                catch
                {
                    request = null;
                }
                if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                {
                    await TenantPlatformJsonResponseWriter.WriteAsync(context,
                        ApiResult<LoginRepDTO>.Fail(ErrorCodes.AuthCredentialsRequired, Messages.AuthCredentialsRequired), 400);
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
                    await TenantPlatformJsonResponseWriter.WriteAsync(context,
                        ApiResult<LoginRepDTO>.Fail(ErrorCodes.SystemBusy, Messages.SystemBusy), 500);
                    return;
                }

                PlatformUser? matchedUser = null;
                for (int i = 0; i < users.Count; i++)
                {
                    var candidate = users[i];
                    if (candidate.DeletedAt != null) continue;
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
                    await TenantPlatformJsonResponseWriter.WriteAsync(context,
                        ApiResult<LoginRepDTO>.Fail(ErrorCodes.AuthInvalidCredentials, Messages.AuthInvalidCredentials), 401);
                    return;
                }

                if (string.Equals(matchedUser.Status, "disabled", StringComparison.OrdinalIgnoreCase))
                {
                    await RecordLoginAsync(matchedUser, username, remoteIp, userAgent, "password", "disabled", "账户已禁用");
                    await TenantPlatformJsonResponseWriter.WriteAsync(context,
                        ApiResult<LoginRepDTO>.Fail(ErrorCodes.AuthAccountDisabled, Messages.AuthAccountDisabled), 403);
                    return;
                }

                if (matchedUser.LockedUntil.HasValue && matchedUser.LockedUntil.Value > now)
                {
                    matchedUser.Status = "locked";
                    await PlatformUserCRUD.UpdateAsync(0, 0, matchedUser);
                    await RecordLoginAsync(matchedUser, username, remoteIp, userAgent, "password", "locked", "账户已锁定");
                    await TenantPlatformJsonResponseWriter.WriteAsync(context,
                        ApiResult<LoginRepDTO>.Fail(ErrorCodes.AuthAccountLocked, Messages.AuthAccountLocked), 423);
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
                    var isLocked = string.Equals(matchedUser.Status, "locked", StringComparison.Ordinal);
                    await RecordLoginAsync(matchedUser, username, remoteIp, userAgent, "password", isLocked ? "locked" : "failed", "用户名或密码错误");
                    var errCode = isLocked ? ErrorCodes.AuthAccountLocked : ErrorCodes.AuthInvalidCredentials;
                    var errMsg = isLocked ? Messages.AuthAccountLocked : Messages.AuthInvalidCredentials;
                    await TenantPlatformJsonResponseWriter.WriteAsync(context,
                        ApiResult<LoginRepDTO>.Fail(errCode, errMsg), isLocked ? 423 : 401);
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
                    await TenantPlatformJsonResponseWriter.WriteAsync(context,
                        ApiResult<LoginRepDTO>.Fail(ErrorCodes.AuthLoginUpdateFailed, Messages.AuthLoginUpdateFailed), 500);
                    return;
                }

                var token = PlatformAuthHandler.GenerateToken(matchedUser.Id, matchedUser.Username);
                var requirePasswordReset = matchedUser.PasswordExpiresAt.HasValue && matchedUser.PasswordExpiresAt.Value <= now;

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
                        loginIsSuperAdmin = true;
                    if (rolePermCache.TryGetValue(loginRoles[ri], out var rp))
                        for (int pi = 0; pi < rp.Count; pi++) permSet.Add(rp[pi]);
                }
                var loginPermissions = new List<string>(permSet.Count);
                foreach (var p in permSet) loginPermissions.Add(p);

                await RecordLoginAsync(matchedUser, username, remoteIp, userAgent, "password", "success", null);

                var loginData = new LoginRepDTO
                {
                    Token = token,
                    ExpiresIn = PlatformAuthHandler.GetTokenExpirySeconds(),
                    UserId = matchedUser.Id,
                    Username = matchedUser.Username,
                    DisplayName = matchedUser.DisplayName,
                    RequirePasswordReset = requirePasswordReset,
                    Roles = loginRoles,
                    Permissions = loginPermissions,
                    IsSuperAdmin = loginIsSuperAdmin
                };
                var loginMsg = requirePasswordReset ? Messages.AuthLoginSuccessPasswordExpired : Messages.AuthLoginSuccess;
                await TenantPlatformJsonResponseWriter.WriteAsync(context,
                    ApiResult<LoginRepDTO>.Ok(loginData, loginMsg));
            }).WithSummary("平台用户登录");

            group.MapPost("/refresh", async (HttpContext context, CancellationToken cancellationToken) =>
            {
                RefreshTokenReqDTO? request;
                try
                {
                    request = await context.Request.ReadFromJsonAsync<RefreshTokenReqDTO>(cancellationToken: cancellationToken);
                }
                catch
                {
                    request = null;
                }
                var token = request?.Token;
                if (string.IsNullOrWhiteSpace(token))
                {
                    var authHeader = context.Request.Headers.Authorization.ToString();
                    const string bearerPrefix = "Bearer ";
                    if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
                        token = authHeader.Substring(bearerPrefix.Length).Trim();
                }

                var currentUser = PlatformAuthHandler.TryResolveToken(token ?? string.Empty, context.TraceIdentifier);
                if (currentUser == null)
                {
                    await TenantPlatformJsonResponseWriter.WriteAsync(context,
                        ApiResult<LoginRepDTO>.Fail(ErrorCodes.AuthTokenInvalid, Messages.AuthTokenInvalid), 401);
                    return;
                }

                var refreshedToken = PlatformAuthHandler.GenerateToken(currentUser.UserId, currentUser.Username);
                var refreshData = new LoginRepDTO
                {
                    Token = refreshedToken,
                    ExpiresIn = PlatformAuthHandler.GetTokenExpirySeconds(),
                    UserId = currentUser.UserId,
                    Username = currentUser.Username,
                    DisplayName = currentUser.DisplayName,
                    RequirePasswordReset = false,
                    Roles = currentUser.Roles,
                    Permissions = currentUser.Permissions,
                    IsSuperAdmin = currentUser.IsSuperAdmin
                };
                await TenantPlatformJsonResponseWriter.WriteAsync(context,
                    ApiResult<LoginRepDTO>.Ok(refreshData, Messages.AuthRefreshSuccess));
            }).WithSummary("刷新访问令牌");

            group.MapGet("/me", async (HttpContext context) =>
            {
                var currentUser = context.Items.TryGetValue(CurrentUser.HttpContextKey, out var userObj) && userObj is CurrentUser cu
                    ? cu
                    : CurrentUser.Anonymous;

                var meData = new CurrentUserRepDTO
                {
                    UserId = currentUser.UserId,
                    Username = currentUser.Username,
                    DisplayName = currentUser.DisplayName,
                    IsSuperAdmin = currentUser.IsSuperAdmin
                };
                await TenantPlatformJsonResponseWriter.WriteAsync(context,
                    ApiResult<CurrentUserRepDTO>.Ok(meData, Messages.OperationSuccess));
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

    }
}
