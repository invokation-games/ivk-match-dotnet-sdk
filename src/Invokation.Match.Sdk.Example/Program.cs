using Google.Protobuf.WellKnownTypes;
using Invokation.Match.Sdk;
using Invokation.Match.Sdk.Engines;
using Matchmaker.Core.V1;
using Matchmaker.Engines.BasicSbmm.V1;

var baseUrl = System.Environment.GetEnvironmentVariable("MATCH_BASE_URL") ?? "http://localhost:50051";

using var sdk = MatchSdk.CreateBuilder()
    .WithBaseUrl(baseUrl)
    .Build();

var input = new EngineInput
{
    Players = { new Player { PlayerId = "alice", Mmr = 0.55 } },
};

var ticket = new Ticket
{
    Id = System.Guid.NewGuid().ToString(),
    QueueId = "ranked_solo",
    CreatedAt = Timestamp.FromDateTime(System.DateTime.UtcNow),
    EngineInput = BasicSbmm.PackInput(input),
};

var ticketId = await sdk.CreateTicketAsync(ticket);
System.Console.WriteLine($"Created ticket {ticketId}");

await System.Threading.Tasks.Task.Delay(1000);
await sdk.CancelTicketAsync("ranked_solo", ticketId);
System.Console.WriteLine($"Cancelled ticket {ticketId}");
