using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using Matchmaker.Core.V1;

namespace Invokation.Match.Sdk;

public interface IMatchSdk
{
    Task<string> CreateTicketAsync(
        Ticket ticket,
        CancellationToken ct = default);

    Task CancelTicketAsync(
        string queueId,
        string ticketId,
        CancellationToken ct = default);

    Task<string> CreateBackfillRequestAsync(
        BackfillRequest request,
        CancellationToken ct = default);

    Task CancelBackfillRequestsAsync(
        string queueId,
        IEnumerable<string> backfillIds,
        CancellationToken ct = default);

    Task<IReadOnlyList<string>> ReactivateTicketsAsync(
        string queueId,
        IEnumerable<string> ticketIds,
        CancellationToken ct = default);

    Task<ListPoolTicketsResponse> ListPoolTicketsAsync(
        ListPoolTicketsRequest request,
        CancellationToken ct = default);

    Task<ClearQueuePoolResponse> ClearQueuePoolAsync(
        string queueId,
        CancellationToken ct = default);
}

/// <summary>
/// Async-only client facade over the IVK Match matchmaker gRPC service.
/// Construct via <see cref="CreateBuilder"/>.
/// </summary>
public sealed class MatchSdk : IMatchSdk, IDisposable, IAsyncDisposable
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

    public async Task<string> CreateTicketAsync(
        Ticket ticket,
        CancellationToken ct = default)
    {
        var resp = await _client.CreateTicketAsync(
            new CreateTicketRequest { Ticket = ticket },
            cancellationToken: ct).ConfigureAwait(false);
        return resp.TicketId;
    }

    public async Task CancelTicketAsync(
        string queueId,
        string ticketId,
        CancellationToken ct = default)
    {
        await _client.CancelTicketAsync(
            new CancelTicketRequest { QueueId = queueId, TicketId = ticketId },
            cancellationToken: ct).ConfigureAwait(false);
    }

    public async Task<string> CreateBackfillRequestAsync(
        BackfillRequest request,
        CancellationToken ct = default)
    {
        var resp = await _client.CreateBackfillRequestAsync(
            new CreateBackfillRequestRequest { BackfillRequest = request },
            cancellationToken: ct).ConfigureAwait(false);
        return resp.BackfillId;
    }

    public async Task CancelBackfillRequestsAsync(
        string queueId,
        IEnumerable<string> backfillIds,
        CancellationToken ct = default)
    {
        var req = new CancelBackfillRequestsRequest { QueueId = queueId };
        req.BackfillIds.AddRange(backfillIds);
        await _client.CancelBackfillRequestsAsync(req, cancellationToken: ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<string>> ReactivateTicketsAsync(
        string queueId,
        IEnumerable<string> ticketIds,
        CancellationToken ct = default)
    {
        var req = new ReactivateTicketsRequest { QueueId = queueId };
        req.TicketIds.AddRange(ticketIds);
        var resp = await _client.ReactivateTicketsAsync(req, cancellationToken: ct).ConfigureAwait(false);
        return resp.FailedTicketIds;
    }

    public async Task<ListPoolTicketsResponse> ListPoolTicketsAsync(
        ListPoolTicketsRequest request,
        CancellationToken ct = default)
    {
        return await _client.ListPoolTicketsAsync(request, cancellationToken: ct).ConfigureAwait(false);
    }

    public async Task<ClearQueuePoolResponse> ClearQueuePoolAsync(
        string queueId,
        CancellationToken ct = default)
    {
        return await _client.ClearQueuePoolAsync(
            new ClearQueuePoolRequest { QueueId = queueId },
            cancellationToken: ct).ConfigureAwait(false);
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
                InitialBackoff = TimeSpan.FromMilliseconds(retry.InitialBackoffMs),
                MaxBackoff = TimeSpan.FromMilliseconds(retry.MaxBackoffMs),
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

    public async ValueTask DisposeAsync()
    {
        if (_ownsChannel)
        {
            await _channel.ShutdownAsync().ConfigureAwait(false);
            _channel.Dispose();
        }
    }
}
