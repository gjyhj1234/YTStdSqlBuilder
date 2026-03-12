using Xunit;
using YTStdAdo;

namespace YTStdAdo.Tests;

/// <summary>返回结构体测试</summary>
public class ResultTypesTests
{
    [Fact]
    public void DbInsResult_DefaultValues()
    {
        var result = new DbInsResult();
        Assert.False(result.Success);
        Assert.Equal(0, result.Id);
        Assert.Null(result.ErrorMessage);
        Assert.Null(result.DebugMessage);
    }

    [Fact]
    public void DbInsResult_InitProperties()
    {
        var result = new DbInsResult
        {
            Success = true,
            Id = 42,
            ErrorMessage = "err",
            DebugMessage = "dbg"
        };
        Assert.True(result.Success);
        Assert.Equal(42, result.Id);
        Assert.Equal("err", result.ErrorMessage);
        Assert.Equal("dbg", result.DebugMessage);
    }

    [Fact]
    public void DbUdqResult_DefaultValues()
    {
        var result = new DbUdqResult();
        Assert.False(result.Success);
        Assert.Equal(0, result.RowsAffected);
        Assert.Null(result.ErrorMessage);
        Assert.Null(result.DebugMessage);
    }

    [Fact]
    public void DbUdqResult_InitProperties()
    {
        var result = new DbUdqResult
        {
            Success = true,
            RowsAffected = 5,
            ErrorMessage = "err",
            DebugMessage = "dbg"
        };
        Assert.True(result.Success);
        Assert.Equal(5, result.RowsAffected);
        Assert.Equal("err", result.ErrorMessage);
        Assert.Equal("dbg", result.DebugMessage);
    }

    [Fact]
    public void DbScalarResult_DefaultValues()
    {
        var result = new DbScalarResult<int>();
        Assert.False(result.Success);
        Assert.Equal(0, result.Value);
        Assert.Null(result.ErrorMessage);
        Assert.Null(result.DebugMessage);
    }

    [Fact]
    public void DbScalarResult_InitProperties()
    {
        var result = new DbScalarResult<string>
        {
            Success = true,
            Value = "hello",
            ErrorMessage = null,
            DebugMessage = "sql debug"
        };
        Assert.True(result.Success);
        Assert.Equal("hello", result.Value);
        Assert.Null(result.ErrorMessage);
        Assert.Equal("sql debug", result.DebugMessage);
    }

    [Fact]
    public void DDLStatus_HasExpectedValues()
    {
        Assert.Equal(0, (int)DDLStatus.Success);
        Assert.Equal(1, (int)DDLStatus.Failed);
        Assert.Equal(2, (int)DDLStatus.Existed);
    }

    [Fact]
    public void DbField_DefaultValues()
    {
        var field = new DbField();
        Assert.Equal("", field.FieldName);
        Assert.Equal("", field.DataType);
        Assert.Null(field.MaxLength);
        Assert.Null(field.NumericPrecision);
        Assert.Null(field.NumericScale);
        Assert.False(field.IsNullable);
        Assert.False(field.IsPrimaryKey);
    }

    [Fact]
    public void DbField_InitProperties()
    {
        var field = new DbField
        {
            FieldName = "name",
            DataType = "varchar",
            MaxLength = 100,
            NumericPrecision = null,
            NumericScale = null,
            IsNullable = true,
            IsPrimaryKey = false
        };
        Assert.Equal("name", field.FieldName);
        Assert.Equal("varchar", field.DataType);
        Assert.Equal(100, field.MaxLength);
        Assert.True(field.IsNullable);
        Assert.False(field.IsPrimaryKey);
    }

    [Fact]
    public void DbIndex_DefaultValues()
    {
        var index = new DbIndex();
        Assert.Equal("", index.IndexName);
        Assert.Equal("", index.TableName);
        Assert.False(index.IsUnique);
        Assert.Empty(index.Columns);
    }
}
