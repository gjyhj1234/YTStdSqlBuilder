using Xunit;
using YTStdAdo;

namespace YTStdAdo.Tests;

public class DbOptionsTests
{
    [Fact]
    public void BuildConnectionString_ReturnsCorrectFormat()
    {
        var options = new DbOptions
        {
            Host = "localhost",
            Port = 5432,
            Database = "testdb",
            Username = "admin",
            Password = "secret",
            ConnectionTimeoutSeconds = 30
        };

        var connStr = options.BuildConnectionString();

        Assert.Contains("Host=localhost", connStr);
        Assert.Contains("Port=5432", connStr);
        Assert.Contains("Database=testdb", connStr);
        Assert.Contains("Username=admin", connStr);
        Assert.Contains("Password=secret", connStr);
    }

    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var options = new DbOptions();

        Assert.Equal("localhost", options.Host);
        Assert.Equal(5432, options.Port);
        Assert.Equal(2, options.MinPoolSize);
        Assert.Equal(20, options.MaxPoolSize);
        Assert.Equal(30, options.ConnectionTimeoutSeconds);
        Assert.Equal(3, options.RetryCount);
        Assert.Equal(1, options.MachineCode);
        Assert.Equal(300, options.IdleTimeoutSeconds);
    }
}
