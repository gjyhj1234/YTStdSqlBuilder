using System;

namespace YTStdAdo;

/// <summary>
/// 数据库连接池配置选项。
/// 对象化设置避免拼接字符串导致错误。
/// </summary>
public sealed class DbOptions
{
    /// <summary>PostgreSQL 主机地址</summary>
    public string Host { get; init; } = "localhost";

    /// <summary>PostgreSQL 端口</summary>
    public int Port { get; init; } = 5432;

    /// <summary>数据库名称</summary>
    public string Database { get; init; } = "";

    /// <summary>数据库用户名</summary>
    public string Username { get; init; } = "";

    /// <summary>数据库密码</summary>
    public string Password { get; init; } = "";

    /// <summary>最小连接数（初始化时自动创建）</summary>
    public int MinPoolSize { get; init; } = 2;

    /// <summary>最大连接数</summary>
    public int MaxPoolSize { get; init; } = 20;

    /// <summary>连接超时时间（秒）</summary>
    public int ConnectionTimeoutSeconds { get; init; } = 30;

    /// <summary>连接重试次数</summary>
    public int RetryCount { get; init; } = 3;

    /// <summary>机器码，范围 0-99，用于 long 主键前缀</summary>
    public int MachineCode { get; init; } = 1;

    /// <summary>连接池中空闲连接的注销时间（秒）</summary>
    public int IdleTimeoutSeconds { get; init; } = 300;

    /// <summary>
    /// 构建 Npgsql 连接字符串。对象化设置避免拼接字符串导致错误。
    /// </summary>
    public string BuildConnectionString()
    {
        var vsb = new ValueStringBuilder(stackalloc char[256]);
        vsb.Append("Host=");
        vsb.Append(Host);
        vsb.Append(";Port=");
        vsb.Append(Port);
        vsb.Append(";Database=");
        vsb.Append(Database);
        vsb.Append(";Username=");
        vsb.Append(Username);
        vsb.Append(";Password=");
        vsb.Append(Password);
        vsb.Append(";Timeout=");
        vsb.Append(ConnectionTimeoutSeconds);
        vsb.Append(";Command Timeout=");
        vsb.Append(ConnectionTimeoutSeconds);
        return vsb.ToString();
    }
}
