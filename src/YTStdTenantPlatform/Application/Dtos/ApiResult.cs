using System;
using System.Collections.Generic;

namespace YTStdTenantPlatform.Application.Dtos
{
    /// <summary>统一 API 响应包装（无数据）</summary>
    public sealed class ApiResult
    {
        /// <summary>响应代码：0=成功，非0=错误码（对应 ErrorCodes 常量）</summary>
        public int Code { get; set; }

        /// <summary>消息键，用于前端国际化展示（对应 Messages 常量）</summary>
        public string Message { get; set; } = "";

        /// <summary>成功结果</summary>
        public static ApiResult Ok(string message = "operation.success")
            => new ApiResult { Code = 0, Message = message };

        /// <summary>失败结果</summary>
        public static ApiResult Fail(int code, string message)
            => new ApiResult { Code = code, Message = message };
    }

    /// <summary>统一 API 响应包装（带数据）</summary>
    public sealed class ApiResult<T>
    {
        /// <summary>响应代码：0=成功，非0=错误码（对应 ErrorCodes 常量）</summary>
        public int Code { get; set; }

        /// <summary>消息键，用于前端国际化展示（对应 Messages 常量）</summary>
        public string Message { get; set; } = "";

        /// <summary>响应数据</summary>
        public T? Data { get; set; }

        /// <summary>成功结果</summary>
        public static ApiResult<T> Ok(T data, string message = "operation.success")
            => new ApiResult<T> { Code = 0, Message = message, Data = data };

        /// <summary>失败结果</summary>
        public static ApiResult<T> Fail(int code, string message)
            => new ApiResult<T> { Code = code, Message = message };
    }

    /// <summary>分页请求</summary>
    public sealed class PagedRequest
    {
        /// <summary>页码（从 1 开始）</summary>
        public int Page { get; set; } = 1;

        /// <summary>每页条数</summary>
        public int PageSize { get; set; } = 20;

        /// <summary>关键字搜索</summary>
        public string? Keyword { get; set; }

        /// <summary>状态过滤</summary>
        public string? Status { get; set; }

        /// <summary>规范化页码（最小为 1）</summary>
        public int NormalizedPage => Page < 1 ? 1 : Page;

        /// <summary>规范化每页条数（1~200）</summary>
        public int NormalizedPageSize => PageSize < 1 ? 20 : (PageSize > 200 ? 200 : PageSize);

        /// <summary>偏移量</summary>
        public int Offset => (NormalizedPage - 1) * NormalizedPageSize;
    }

    /// <summary>分页响应</summary>
    public sealed class PagedResult<T>
    {
        /// <summary>数据列表</summary>
        public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();

        /// <summary>总条数</summary>
        public long Total { get; set; }

        /// <summary>当前页码</summary>
        public int Page { get; set; }

        /// <summary>每页条数</summary>
        public int PageSize { get; set; }

        /// <summary>总页数</summary>
        public int TotalPages => PageSize > 0 ? (int)((Total + PageSize - 1) / PageSize) : 0;
    }
}
