using Invokation.Match.Sdk;
using Xunit;

namespace Invokation.Match.Sdk.Tests;

public class RetryConfigTests
{
    [Fact]
    public void Default_HasReasonableValues()
    {
        var rc = RetryConfig.Default;
        Assert.Equal(3, rc.MaxAttempts);
        Assert.Equal(500, rc.InitialBackoffMs);
        Assert.Equal(10_000, rc.MaxBackoffMs);
    }

    [Fact]
    public void NoRetry_HasMaxAttempts1()
    {
        Assert.Equal(1, RetryConfig.NoRetry.MaxAttempts);
    }

    [Fact]
    public void Construction_RejectsInvalidValues()
    {
        Assert.Throws<System.ArgumentException>(() => new RetryConfig { MaxAttempts = 0 });
        Assert.Throws<System.ArgumentException>(() => new RetryConfig { InitialBackoffMs = -1 });
    }
}
