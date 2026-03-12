using Xunit;
using YTStdEntity.Audit;

namespace YTStdEntity.Tests;

public class AuditQueryFilterTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var filter = new AuditQueryFilter();

        Assert.Null(filter.Id);
        Assert.Null(filter.Opt);
        Assert.Null(filter.OperatorId);
        Assert.Null(filter.StartTime);
        Assert.Null(filter.EndTime);
        Assert.Equal(0, filter.PageIndex);
        Assert.Equal(50, filter.PageSize);
    }

    [Fact]
    public void AuditOpt_ValuesAreCorrectChars()
    {
        Assert.Equal((byte)'I', (byte)AuditOpt.Insert);
        Assert.Equal((byte)'U', (byte)AuditOpt.Update);
        Assert.Equal((byte)'D', (byte)AuditOpt.Delete);
    }
}
