using Invokation.Match.Sdk;
using Xunit;

namespace Invokation.Match.Sdk.Tests;

public class MatchSdkBuilderTests
{
    [Fact]
    public void Build_RequiresBaseUrl()
    {
        var builder = MatchSdk.CreateBuilder();
        Assert.Throws<System.InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Build_AcceptsBaseUrl()
    {
        using var sdk = MatchSdk.CreateBuilder()
            .WithBaseUrl("http://localhost:50051")
            .Build();
        Assert.NotNull(sdk);
    }

    [Fact]
    public void WithRetryConfig_OverridesDefault()
    {
        using var sdk = MatchSdk.CreateBuilder()
            .WithBaseUrl("http://localhost:50051")
            .WithRetryConfig(RetryConfig.NoRetry)
            .Build();
        Assert.NotNull(sdk);
    }
}
