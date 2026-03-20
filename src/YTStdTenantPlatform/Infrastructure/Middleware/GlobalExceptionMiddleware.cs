using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using YTStdLogger.Core;
using YTStdTenantPlatform.Application.Constants;
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
                await WriteErrorResponse(context, HttpStatusCode.Forbidden, ErrorCodes.Forbidden, Messages.Forbidden);
            }
            catch (ArgumentException ex)
            {
                Logger.Warn(0, 0, "[GlobalExceptionMiddleware] 参数错误: " + ex.Message);
                await WriteErrorResponse(context, HttpStatusCode.BadRequest, ErrorCodes.InvalidParameter, Messages.InvalidParameter);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Warn(0, 0, "[GlobalExceptionMiddleware] 操作无效: " + ex.Message);
                await WriteErrorResponse(context, HttpStatusCode.BadRequest, ErrorCodes.InvalidOperation, Messages.InvalidOperation);
            }
            catch (Exception ex)
            {
                Logger.Error(0, 0, "[GlobalExceptionMiddleware] 未处理异常: " + ex.ToString());
                await WriteErrorResponse(context, HttpStatusCode.InternalServerError, ErrorCodes.InternalServerError, Messages.InternalServerError);
            }
        }

        /// <summary>写入标准错误响应</summary>
        private static async Task WriteErrorResponse(
            HttpContext context,
            HttpStatusCode statusCode,
            int errorCode,
            string message)
        {
            if (context.Response.HasStarted) return;

            var result = Application.Dtos.ApiResult.Fail(errorCode, message);
            await TenantPlatformJsonResponseWriter.WriteAsync(context, result, (int)statusCode);
        }
    }
}
