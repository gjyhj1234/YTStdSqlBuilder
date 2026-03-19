using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using YTStdLogger.Core;
using YTStdTenantPlatform.Infrastructure.Serialization;

namespace YTStdTenantPlatform.Infrastructure.Middleware
{
    /// <summary>全局异常处理中间件，统一捕获未处理异常并返回标准错误响应</summary>
    public sealed class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>构造全局异常处理中间件</summary>
        public GlobalExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>处理请求，捕获所有未处理异常</summary>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Warn(0, 0, "[GlobalExceptionMiddleware] 未授权访问: " + ex.Message);
                await WriteErrorResponse(context, HttpStatusCode.Forbidden, "权限不足", ex.Message);
            }
            catch (ArgumentException ex)
            {
                Logger.Warn(0, 0, "[GlobalExceptionMiddleware] 参数错误: " + ex.Message);
                await WriteErrorResponse(context, HttpStatusCode.BadRequest, "参数错误", ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Warn(0, 0, "[GlobalExceptionMiddleware] 操作无效: " + ex.Message);
                await WriteErrorResponse(context, HttpStatusCode.BadRequest, "操作无效", ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Error(0, 0, "[GlobalExceptionMiddleware] 未处理异常: " + ex.ToString());
                await WriteErrorResponse(context, HttpStatusCode.InternalServerError,
                    "服务器内部错误", "请联系管理员或稍后重试");
            }
        }

        /// <summary>写入标准错误响应</summary>
        private static async Task WriteErrorResponse(
            HttpContext context,
            HttpStatusCode statusCode,
            string error,
            string message)
        {
            if (context.Response.HasStarted) return;

            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json; charset=utf-8";

            var traceId = context.TraceIdentifier;
            await Utf8JsonWriterHelper.WriteResponseAsync(
                context.Response,
                (error, message, traceId),
                static (writer, state) =>
                {
                    writer.WriteStartObject();
                    writer.WriteBoolean("success", false);
                    writer.WriteString("message", state.error + ": " + state.message);
                    writer.WriteNull("data");
                    writer.WriteString("traceId", state.traceId);
                    writer.WriteEndObject();
                },
                context.RequestAborted);
        }
    }
}
