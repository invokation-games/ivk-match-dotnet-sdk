# IVK Match SDK for C#

Official C# SDK for the [IVK Match](https://docs.ivk.dev) matchmaking service.

## Status

Pre-release. Public API may change without notice until 1.0.

## Installation

Install via NuGet:

```bash
dotnet add package Invokation.Match.Sdk
```

Or via the Package Manager Console:

```powershell
Install-Package Invokation.Match.Sdk
```

## Quick Start

```csharp
using Google.Protobuf.WellKnownTypes;
using Invokation.Match.Sdk;
using Invokation.Match.Sdk.Engines;
using Matchmaker.Core.V1;
using Matchmaker.Engines.BasicSbmm.V1;

// Create the SDK instance
using var sdk = MatchSdk.CreateBuilder()
    .WithBaseUrl("http://localhost:50051")
    .Build();

// Build an engine-specific input and pack it into the ticket
var input = new EngineInput
{
    Players = { new Player { PlayerId = "alice", Mmr = 0.55 } },
};

var ticket = new Ticket
{
    Id = Guid.NewGuid().ToString(),
    QueueId = "ranked_solo",
    CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow),
    EngineInput = BasicSbmm.PackInput(input),
};

// Submit the ticket
var ticketId = await sdk.CreateTicketAsync(ticket);
Console.WriteLine($"Created ticket {ticketId}");

// Later, cancel it if needed
await sdk.CancelTicketAsync("ranked_solo", ticketId);
```

## Features

- **Async-only API** over gRPC (`MatchmakerService`)
- **Automatic retries** with exponential backoff via grpc-dotnet's native `RetryPolicy`
- **Builder pattern** for configuration
- **Engine helpers** for the bundled match engines (basic_sbmm, rule_based, nemesis)
- **Full .NET support** — targets .NET 6, .NET 8, and .NET 10

## Configuration

### Builder Options

```csharp
var sdk = MatchSdk.CreateBuilder()
    .WithBaseUrl("http://localhost:50051") // Required: matchmaker gRPC endpoint
    .WithRetryConfig(RetryConfig.Default)  // Optional: see below
    .WithHttpClient(httpClient)            // Optional: custom HttpClient (advanced)
    .WithLogger(logger)                    // Optional: ILogger for gRPC diagnostics
    .Build();
```

### Retry Configuration

The SDK retries transient failures (`UNAVAILABLE`, `DEADLINE_EXCEEDED`) with exponential backoff. Retries are configured on the gRPC channel via `Grpc.Net.Client.Configuration.MethodConfig.RetryPolicy`.

```csharp
var retryConfig = new RetryConfig
{
    MaxAttempts = 5,        // Total attempts including the first (default: 3)
    InitialBackoffMs = 100, // First retry delay (default: 500ms)
    MaxBackoffMs = 30000,   // Maximum backoff cap (default: 10000ms)
    BackoffMultiplier = 2.0 // Backoff growth factor (default: 2.0)
};

using var sdk = MatchSdk.CreateBuilder()
    .WithBaseUrl("http://localhost:50051")
    .WithRetryConfig(retryConfig)
    .Build();
```

Validation rules:

- `MaxAttempts` must be `>= 1`
- `InitialBackoffMs` must be `>= 0`
- `MaxBackoffMs` must be `>= 0`
- `BackoffMultiplier` must be `>= 1.0`

To disable retries:

```csharp
.WithRetryConfig(RetryConfig.NoRetry)
```

`RetryConfig.NoRetry` performs a single total attempt with no retries.

## API Reference

### Tickets

```csharp
// CreateTicket — submit a new matchmaking ticket; returns the ticket id.
string ticketId = await sdk.CreateTicketAsync(ticket, ct);

// CancelTicket — remove a ticket from matchmaking.
await sdk.CancelTicketAsync(queueId: "ranked_solo", ticketId: ticketId, ct);

// ReactivateTickets — move resolved tickets back to active state.
// Returns the IDs that could not be reactivated (empty on full success).
IReadOnlyList<string> failed = await sdk.ReactivateTicketsAsync(
    queueId: "ranked_solo",
    ticketIds: new[] { ticketId },
    ct);
```

### Backfill

```csharp
// CreateBackfillRequest — register a backfill request for an existing match.
string backfillId = await sdk.CreateBackfillRequestAsync(backfillRequest, ct);

// CancelBackfillRequests — cancel one or more backfill requests in a queue.
await sdk.CancelBackfillRequestsAsync(
    queueId: "ranked_solo",
    backfillIds: new[] { backfillId },
    ct);
```

### Cache (debug / E2E)

```csharp
// GetCacheTickets — query cached tickets with optional filters.
GetCacheTicketsResponse response = await sdk.GetCacheTicketsAsync(
    new GetCacheTicketsRequest { QueueId = "ranked_solo", Limit = 100 },
    ct);

// ClearQueueCache — clear all cached tickets for a queue (E2E helper).
ulong cleared = await sdk.ClearQueueCacheAsync(queueId: "ranked_solo", ct);
```

## Engine Helpers

`Ticket.EngineInput`, `BackfillRequest.EngineInput`, and `Match.EngineOutput` are opaque `bytes` fields. The matchmaker delegates their schema to the configured match engine. The SDK ships static `Pack`/`Unpack` helpers per engine so you can move between strongly-typed protos and the wire bytes.

```csharp
using Invokation.Match.Sdk.Engines;
using Matchmaker.Engines.BasicSbmm.V1;

var input = new EngineInput { Players = { new Player { PlayerId = "p1", Mmr = 0.5 } } };

// Pack for transmission
ByteString packed = BasicSbmm.PackInput(input);

// Unpack on receipt
EngineInput roundtripped = BasicSbmm.UnpackInput(packed);
```

The same shape applies to the other engines:

- `Invokation.Match.Sdk.Engines.BasicSbmm` — `Matchmaker.Engines.BasicSbmm.V1`
- `Invokation.Match.Sdk.Engines.RuleBased` — `Matchmaker.Engines.RuleBased.V1`
- `Invokation.Match.Sdk.Engines.Nemesis` — `Matchmaker.Engines.Nemesis.V1`

Each helper exposes `PackInput` / `UnpackInput` and `PackOutput` / `UnpackOutput`, with overloads accepting `ByteString` or `byte[]` on unpack.

## Error Handling

gRPC errors surface as `Grpc.Core.RpcException` with a `Status` describing the failure. Inspect `ex.StatusCode` to handle specific cases.

```csharp
try
{
    var ticketId = await sdk.CreateTicketAsync(ticket);
}
catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.InvalidArgument)
{
    Console.WriteLine($"Invalid ticket: {ex.Status.Detail}");
}
catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.Unavailable)
{
    // Already retried per RetryConfig before bubbling up.
    Console.WriteLine("Matchmaker unreachable");
}
```

## Development

### Prerequisites

- .NET 6, .NET 8, and .NET 10 SDKs (managed via `mise`)
- Just (task runner)
- buf (proto lint / breaking-change detection)

### Common Tasks

```bash
just build       # build the SDK assembly
just build-all   # build SDK, example, and tests
just test        # run all tests
just test-net6   # run tests on .NET 6 only
just test-net8   # run tests on .NET 8 only
just test-net10  # run tests on .NET 10 only
just pack        # produce a NuGet package under ./nupkg
just run-example # run the example app
just buf-lint    # lint vendored protos
```

The SDK consumes vendored protos from `protos/` and generates C# at build time via `Grpc.Tools`. To refresh them, update the `ivk-protos` commit pinned in this repo and re-vendor.

## Support

- Documentation: [https://docs.ivk.dev](https://docs.ivk.dev)
- Discord: [Community Discord](https://discord.gg/JfNGsunrjX)
