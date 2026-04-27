using Invokation.Match.Sdk.Engines;
using Matchmaker.Engines.RuleBased.V1;
using Xunit;

namespace Invokation.Match.Sdk.Tests.Engines;

public class RuleBasedTests
{
    [Fact]
    public void PackInput_RoundTrips()
    {
        var input = new EngineInput
        {
            CrossPlatform = true,
            Gamemodes = { "tdm" },
            Players = { new Player { PlayerId = "p1", Mmr = 0.5, Platform = "ps", Latencies = { { "us-east", 30u } } } },
        };
        var bytes = RuleBased.PackInput(input);
        var rt = RuleBased.UnpackInput(bytes);
        Assert.True(rt.CrossPlatform);
        Assert.Single(rt.Gamemodes);
        Assert.Single(rt.Players);
    }

    [Fact]
    public void PackOutput_RoundTrips()
    {
        var output = new EngineOutput { Gamemode = "tdm", Region = "us-east", CrossPlatform = false };
        var rt = RuleBased.UnpackOutput(RuleBased.PackOutput(output));
        Assert.Equal("tdm", rt.Gamemode);
    }
}
