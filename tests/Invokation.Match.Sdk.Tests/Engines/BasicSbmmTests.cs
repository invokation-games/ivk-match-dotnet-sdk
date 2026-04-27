using Invokation.Match.Sdk.Engines;
using Matchmaker.Engines.BasicSbmm.V1;
using Xunit;

namespace Invokation.Match.Sdk.Tests.Engines;

public class BasicSbmmTests
{
    [Fact]
    public void Pack_RoundTrips()
    {
        var input = new EngineInput
        {
            Players = { new Player { PlayerId = "p1", Mmr = 0.5 } },
        };
        var bytes = BasicSbmm.PackInput(input);
        var roundtrip = BasicSbmm.UnpackInput(bytes);
        Assert.Single(roundtrip.Players);
        Assert.Equal("p1", roundtrip.Players[0].PlayerId);
        Assert.Equal(0.5, roundtrip.Players[0].Mmr);
    }
}
