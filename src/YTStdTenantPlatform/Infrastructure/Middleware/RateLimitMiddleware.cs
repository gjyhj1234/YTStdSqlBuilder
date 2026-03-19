using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using YTStdLogger.Core;
using YTStdTenantPlatform.Infrastructure.Serialization;

namespace YTStdTenantPlatform.Infrastructure.Middleware
{
    /// <summary>本地限流中间件，基于滑动窗口计数器按 IP 进行请求限流</summary>
    public sealed class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>每个窗口周期内的最大请求数</summary>
        private static volatile int _maxRequestsPerWindow = 100;

        /// <summary>滑动窗口大小（秒）</summary>
        private static volatile int _windowSeconds = 60;

        /// <summary>限流计数器（Key = 客户端标识）</summary>
        private static readonly ConcurrentDictionary<string, RateLimitEntry> _counters = new();

        /// <summary>构造限流中间件</summary>
        public RateLimitMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>设置限流参数</summary>
        public static void Configure(int maxRequestsPerWindow, int windowSeconds)
        {
            _maxRequestsPerWindow = maxRequestsPerWindow;
            _windowSeconds = windowSeconds;
        }

        /// <summary>处理请求，检查并执行限流</summary>
        public async Task InvokeAsync(HttpContext context)
        {
            var clientKey = GetClientKey(context);
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var entry = _counters.GetOrAdd(clientKey, _ => new RateLimitEntry(now));

            if (entry.TryIncrement(now, _windowSeconds, _maxRequestsPerWindow))
            {
                await _next(context);
            }
            else
            {
                Logger.Warn(0, 0, "[RateLimitMiddleware] 请求被限流: " + clientKey);

                context.Response.StatusCode = 429;
                context.Response.ContentType = "application/json; charset=utf-8";
                context.Response.Headers["Retry-After"] = _windowSeconds.ToString();
                await Utf8JsonWriterHelper.WriteResponseAsync(
                    context.Response,
                    context.TraceIdentifier,
                    static (writer, traceId) =>
                    {
                        writer.WriteStartObject();
                        writer.WriteBoolean("success", false);
                        writer.WriteString("message", "请求过于频繁: 请稍后再试");
                        writer.WriteNull("data");
                        writer.WriteString("traceId", traceId);
                        writer.WriteEndObject();
                    },
                    context.RequestAborted);
            }
        }

        /// <summary>获取客户端限流标识（优先使用 IP 地址）</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetClientKey(HttpContext context)
        {
            var forwarded = context.Request.Headers["X-Forwarded-For"].ToString();
            if (!string.IsNullOrEmpty(forwarded))
            {
                var commaIndex = forwarded.IndexOf(',');
                return commaIndex > 0 ? forwarded.Substring(0, commaIndex).Trim() : forwarded.Trim();
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        /// <summary>清理过期的限流计数器</summary>
        public static void CleanupExpired()
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var windowSize = _windowSeconds;

            foreach (var kvp in _counters)
            {
                if (now - kvp.Value.WindowStart > windowSize * 2)
                {
                    _counters.TryRemove(kvp.Key, out _);
                }
            }
        }
    }

    /// <summary>限流计数条目，使用滑动窗口算法</summary>
    public sealed class RateLimitEntry
    {
        private long _windowStart;
        private int _count;

        /// <summary>当前窗口起始时间</summary>
        public long WindowStart => Volatile.Read(ref _windowStart);

        /// <summary>构造限流条目</summary>
        public RateLimitEntry(long windowStart)
        {
            _windowStart = windowStart;
            _count = 0;
        }

        /// <summary>尝试递增计数，若未超限返回 true</summary>
        public bool TryIncrement(long now, int windowSeconds, int maxRequests)
        {
            var start = Volatile.Read(ref _windowStart);

            // 窗口过期，重置
            if (now - start >= windowSeconds)
            {
                Interlocked.Exchange(ref _windowStart, now);
                Interlocked.Exchange(ref _count, 1);
                return true;
            }

            var current = Interlocked.Increment(ref _count);
            return current <= maxRequests;
        }
    }
}
