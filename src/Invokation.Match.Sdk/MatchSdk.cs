using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using Matchmaker.Core.V1;

namespace Invokation.Match.Sdk;

/// <summary>
/// Async-only client facade over the IVK Match matchmaker gRPC service.
/// Construct via <see cref="CreateBuilder"/>.
/// </summary>
public sealed class MatchSdk : System.IDisposable
{
    private readonly GrpcChannel _channel;
    private readonly bool _ownsChannel;
    private readonly MatchmakerService.MatchmakerServiceClient _client;

    internal MatchSdk(GrpcChannel channel, bool ownsChannel)
    {
        _channel = channel;
        _ownsChannel = ownsChannel;
        _client = new MatchmakerService.MatchmakerServiceClient(channel);
    }

    public static MatchSdkBuilder CreateBuilder() => new();

    public async System.Threading.Tasks.Task<string> CreateTicketAsync(
        Ticket ticket,
        System.Threading.CancellationToken ct = default)
    {
        var resp = await _client.CreateTicketAsync(
            new CreateTicketRequest { Ticket = ticket },
            cancellationToken: ct);
        return resp.TicketId;
    }

    public async System.Threading.Tasks.Task CancelTicketAsync(
        string queueId,
        string ticketId,
        System.Threading.CancellationToken ct = default)
    {
        await _client.CancelTicketAsync(
            new CancelTicketRequest { QueueId = queueId, TicketId = ticketId },
            cancellationToken: ct);
    }

    public async System.Threading.Tasks.Task<string> CreateBackfillRequestAsync(
        BackfillRequest request,
        System.Threading.CancellationToken ct = default)
    {
        var resp = await _client.CreateBackfillRequestAsync(
            new CreateBackfillRequestRequest { BackfillRequest = request },
            cancellationToken: ct);
        return resp.BackfillId;
    }

    public async System.Threading.Tasks.Task CancelBackfillRequestsAsync(
        string queueId,
        System.Collections.Generic.IEnumerable<string> backfillIds,
        System.Threading.CancellationToken ct = default)
    {
        var req = new CancelBackfillRequestsRequest { QueueId = queueId };
        req.BackfillIds.AddRange(backfillIds);
        await _client.CancelBackfillRequestsAsync(req, cancellationToken: ct);
    }

    public async System.Threading.Tasks.Task<System.Collections.Generic.IReadOnlyList<string>> ReactivateTicketsAsync(
        string queueId,
        System.Collections.Generic.IEnumerable<string> ticketIds,
        System.Threading.CancellationToken ct = default)
    {
        var req = new ReactivateTicketsRequest { QueueId = queueId };
        req.TicketIds.AddRange(ticketIds);
        var resp = await _client.ReactivateTicketsAsync(req, cancellationToken: ct);
        return resp.FailedTicketIds;
    }

    public async System.Threading.Tasks.Task<GetCacheTicketsResponse> GetCacheTicketsAsync(
        GetCacheTicketsRequest request,
        System.Threading.CancellationToken ct = default)
    {
        return await _client.GetCacheTicketsAsync(request, cancellationToken: ct);
    }

    public async System.Threading.Tasks.Task<ulong> ClearQueueCacheAsync(
        string queueId,
        System.Threading.CancellationToken ct = default)
    {
        var resp = await _client.ClearQueueCacheAsync(
            new ClearQueueCacheRequest { QueueId = queueId },
            cancellationToken: ct);
        return resp.TicketsCleared;
    }

    internal static ServiceConfig BuildServiceConfig(RetryConfig retry)
    {
        var sc = new ServiceConfig();
        sc.MethodConfigs.Add(new MethodConfig
        {
            Names = { MethodName.Default },
            RetryPolicy = new RetryPolicy
            {
                MaxAttempts = retry.MaxAttempts,
                InitialBackoff = System.TimeSpan.FromMilliseconds(retry.InitialBackoffMs),
                MaxBackoff = System.TimeSpan.FromMilliseconds(retry.MaxBackoffMs),
                BackoffMultiplier = retry.BackoffMultiplier,
                RetryableStatusCodes = { Grpc.Core.StatusCode.Unavailable, Grpc.Core.StatusCode.DeadlineExceeded },
            },
        });
        return sc;
    }

    public void Dispose()
    {
        if (_ownsChannel) _channel.Dispose();
    }
}
