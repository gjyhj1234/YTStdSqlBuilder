using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace YTStdTenantPlatform.Infrastructure.Serialization;

internal static class TenantPlatformJsonRequestReader
{
    public static async ValueTask<T?> ReadAsync<T>(HttpRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.ContentLength == 0)
        {
            return default;
        }

        var typeInfo = TenantPlatformJsonSerializerContext.Default.GetTypeInfo(typeof(T));
        if (typeInfo == null)
        {
            throw new NotSupportedException("缺少 JSON 源生成元数据: " + typeof(T).FullName);
        }

        try
        {
            var value = await JsonSerializer.DeserializeAsync(request.Body, typeInfo, cancellationToken).ConfigureAwait(false);
            return value is T typedValue ? typedValue : default;
        }
        catch (JsonException)
        {
            return default;
        }
    }
}
