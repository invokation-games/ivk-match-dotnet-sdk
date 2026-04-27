using Invokation.Match.Sdk.Engines;
using Matchmaker.Engines.Nemesis.V1;
using Xunit;

namespace Invokation.Match.Sdk.Tests.Engines;

public class NemesisTests
{
    [Fact]
    public void PackInput_RoundTrips()
    {
        var input = new EngineInput
        {
            Players = { new Player { PlayerId = "p1", Mmr = 0.5, Platform = "ps", Latencies = { { "us-east", 30u } } } },
        };
        var rt = Nemesis.UnpackInput(Nemesis.PackInput(input));
        Assert.Single(rt.Players);
        Assert.Equal("p1", rt.Players[0].PlayerId);
    }
}
