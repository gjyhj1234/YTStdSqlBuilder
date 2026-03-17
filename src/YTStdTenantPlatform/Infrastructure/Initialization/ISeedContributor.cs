using System.Threading.Tasks;

namespace YTStdTenantPlatform.Infrastructure.Initialization;

/// <summary>种子数据贡献者接口</summary>
public interface ISeedContributor
{
    /// <summary>贡献者名称，用于日志输出</summary>
    string Name { get; }

    /// <summary>执行顺序，数值越小越先执行</summary>
    int Order { get; }

    /// <summary>执行幂等初始化</summary>
    ValueTask SeedAsync(PlatformSeedContext context);
}
