using System;
using Xunit;
using YTStdAdo;
using YTStdSqlBuilder;

namespace YTStdAdo.Sample;

/// <summary>
/// ADO 数据库访问层示例测试。
/// 演示 DbOptions、DbInsResult、DbUdqResult、DbScalarResult、DbField、DbIndex、DDLStatus 等类型的用法。
/// 注意：这些测试不需要实际数据库连接，仅验证类型和 API 的正确性。
/// </summary>
public class AdoSampleTests
{
    /// <summary>
    /// 演示 DbOptions 配置与连接字符串构建。
    /// </summary>
    [Fact]
    public void Sample_DbOptions_BuildConnectionString()
    {
        var options = new DbOptions
        {
            Host = "localhost",
            Port = 5432,
            Database = "testdb",
            Username = "admin",
            Password = "secret",
            MinPoolSize = 5,
            MaxPoolSize = 20,
            RetryCount = 3
        };

        string connStr = options.BuildConnectionString();

        Assert.Contains("localhost", connStr);
        Assert.Contains("5432", connStr);
        Assert.Contains("testdb", connStr);
        Assert.Contains("admin", connStr);
    }

    /// <summary>
    /// 演示 DbOptions 默认值。
    /// </summary>
    [Fact]
    public void Sample_DbOptions_Defaults()
    {
        var options = new DbOptions();

        Assert.Equal("localhost", options.Host);
        Assert.Equal(5432, options.Port);
        Assert.Equal("", options.Database);
        Assert.Equal("", options.Username);
        Assert.Equal("", options.Password);
        Assert.True(options.MinPoolSize > 0);
        Assert.True(options.MaxPoolSize >= options.MinPoolSize);
    }

    /// <summary>
    /// 演示 DbInsResult 插入结果。
    /// </summary>
    [Fact]
    public void Sample_DbInsResult()
    {
        var success = new DbInsResult { Success = true, Id = 42 };
        Assert.True(success.Success);
        Assert.Equal(42, success.Id);
        Assert.Null(success.ErrorMessage);

        var failure = new DbInsResult { Success = false, ErrorMessage = "违反唯一约束" };
        Assert.False(failure.Success);
        Assert.Equal(0, failure.Id);
        Assert.Equal("违反唯一约束", failure.ErrorMessage);
    }

    /// <summary>
    /// 演示 DbUdqResult 更新/删除/查询结果。
    /// </summary>
    [Fact]
    public void Sample_DbUdqResult()
    {
        var result = new DbUdqResult
        {
            Success = true,
            RowsAffected = 5,
            DebugMessage = "UPDATE users SET name='test' -- tenantId=1, userId=1, elapsed=3ms"
        };

        Assert.True(result.Success);
        Assert.Equal(5, result.RowsAffected);
        Assert.NotNull(result.DebugMessage);
    }

    /// <summary>
    /// 演示 DbScalarResult 标量查询结果。
    /// </summary>
    [Fact]
    public void Sample_DbScalarResult()
    {
        var intResult = new DbScalarResult<int> { Success = true, Value = 100 };
        Assert.True(intResult.Success);
        Assert.Equal(100, intResult.Value);

        var strResult = new DbScalarResult<string> { Success = true, Value = "hello" };
        Assert.Equal("hello", strResult.Value);

        var nullResult = new DbScalarResult<long> { Success = true, Value = default };
        Assert.True(nullResult.Success);
    }

    /// <summary>
    /// 演示 DDLStatus 枚举。
    /// </summary>
    [Fact]
    public void Sample_DDLStatus()
    {
        Assert.Equal(DDLStatus.Success, DDLStatus.Success);
        Assert.Equal(DDLStatus.Failed, DDLStatus.Failed);
        Assert.Equal(DDLStatus.Existed, DDLStatus.Existed);

        Assert.NotEqual(DDLStatus.Success, DDLStatus.Failed);
    }

    /// <summary>
    /// 演示 DbField 字段元数据。
    /// </summary>
    [Fact]
    public void Sample_DbField()
    {
        var field = new DbField
        {
            FieldName = "username",
            DataType = "varchar",
            MaxLength = 128,
            IsNullable = false,
            IsPrimaryKey = false
        };

        Assert.Equal("username", field.FieldName);
        Assert.Equal("varchar", field.DataType);
        Assert.Equal(128, field.MaxLength);
        Assert.False(field.IsNullable);
        Assert.False(field.IsPrimaryKey);
    }

    /// <summary>
    /// 演示 DbIndex 索引元数据。
    /// </summary>
    [Fact]
    public void Sample_DbIndex()
    {
        var index = new DbIndex
        {
            IndexName = "idx_users_email",
            TableName = "users",
            IsUnique = true,
            Columns = new[] { "email" }
        };

        Assert.Equal("idx_users_email", index.IndexName);
        Assert.Equal("users", index.TableName);
        Assert.True(index.IsUnique);
    }

    /// <summary>
    /// 演示 PgSqlParam 参数构建。
    /// </summary>
    [Fact]
    public void Sample_PgSqlParam()
    {
        // 不指定 DbType
        var p1 = new PgSqlParam("name", "张三");
        Assert.Equal("name", p1.Name);
        Assert.Equal("张三", p1.Value);
        Assert.Null(p1.DbType);

        // 指定 DbType
        var p2 = new PgSqlParam("id", 42, NpgsqlTypes.NpgsqlDbType.Integer);
        Assert.Equal("id", p2.Name);
        Assert.Equal(42, p2.Value);
        Assert.Equal(NpgsqlTypes.NpgsqlDbType.Integer, p2.DbType);
    }

    /// <summary>
    /// 演示 DbInsResult 调试消息。
    /// </summary>
    [Fact]
    public void Sample_DbInsResult_DebugMessage()
    {
        var result = new DbInsResult
        {
            Success = true,
            Id = 1,
            DebugMessage = "INSERT INTO users (name) VALUES ('test') RETURNING id -- tenantId=1, userId=1, elapsed=2ms"
        };

        Assert.True(result.Success);
        Assert.NotNull(result.DebugMessage);
        Assert.Contains("INSERT INTO", result.DebugMessage);
    }

    /// <summary>
    /// 演示 DbField 主键字段。
    /// </summary>
    [Fact]
    public void Sample_DbField_PrimaryKey()
    {
        var pkField = new DbField
        {
            FieldName = "id",
            DataType = "bigint",
            IsPrimaryKey = true,
            IsNullable = false
        };

        Assert.True(pkField.IsPrimaryKey);
        Assert.False(pkField.IsNullable);
        Assert.Null(pkField.MaxLength);
    }

    /// <summary>
    /// 演示 DbField 可空数值字段。
    /// </summary>
    [Fact]
    public void Sample_DbField_NullableNumeric()
    {
        var field = new DbField
        {
            FieldName = "price",
            DataType = "numeric",
            NumericPrecision = 18,
            NumericScale = 4,
            IsNullable = true
        };

        Assert.Equal(18, field.NumericPrecision);
        Assert.Equal(4, field.NumericScale);
        Assert.True(field.IsNullable);
    }
}
