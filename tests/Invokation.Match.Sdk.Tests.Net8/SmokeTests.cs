using Google.Protobuf;
using Invokation.Match.Sdk;
using Invokation.Match.Sdk.Engines;
using Matchmaker.Engines.BasicSbmm.V1;

namespace Invokation.Match.Sdk.Tests.Net8;

/// <summary>
/// Smoke tests verifying the SDK runs on .NET 8.
/// </summary>
public class SmokeTests
{
    [Fact]
    public void CanCreateSdk()
    {
        using var sdk = MatchSdk.CreateBuilder()
            .WithBaseUrl("http://localhost:50051")
            .Build();

        Assert.NotNull(sdk);
    }

    [Fact]
    public void CanConfigureRetry()
    {
        var config = new RetryConfig
        {
            MaxAttempts = 5,
            InitialBackoffMs = 100,
            MaxBackoffMs = 5000,
        };

        using var sdk = MatchSdk.CreateBuilder()
            .WithBaseUrl("http://localhost:50051")
            .WithRetryConfig(config)
            .Build();

        Assert.NotNull(sdk);
    }

    [Fact]
    public void CanPackAndUnpackBasicSbmmInput()
    {
        var input = new EngineInput
        {
            Players = { new Player { PlayerId = "alice", Mmr = 0.55 } },
        };

        ByteString bytes = BasicSbmm.PackInput(input);
        var roundtrip = BasicSbmm.UnpackInput(bytes);

        Assert.Single(roundtrip.Players);
        Assert.Equal("alice", roundtrip.Players[0].PlayerId);
        Assert.Equal(0.55, roundtrip.Players[0].Mmr);
    }

    [Fact]
    public void RetryConfigDefaultIsValid()
    {
        var rc = RetryConfig.Default;
        Assert.Equal(3, rc.MaxAttempts);
        Assert.True(rc.InitialBackoffMs > 0);
        Assert.True(rc.MaxBackoffMs > rc.InitialBackoffMs);
    }
}
