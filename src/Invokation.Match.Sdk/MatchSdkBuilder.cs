using System;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Invokation.Match.Sdk;

/// <summary>
/// Fluent builder for <see cref="MatchSdk"/>. Obtain via <see cref="MatchSdk.CreateBuilder"/>.
///
/// Not thread-safe; configure from a single thread before calling <see cref="Build"/>.
/// </summary>
public sealed class MatchSdkBuilder
{
    private string? _baseUrl;
    private RetryConfig _retry = RetryConfig.Default;
    private ILogger? _logger;
    private System.Net.Http.HttpClient? _httpClient;

    internal MatchSdkBuilder() { }

    public MatchSdkBuilder WithBaseUrl(string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentException("BaseUrl must be a non-empty URL.", nameof(baseUrl));
        _baseUrl = baseUrl;
        return this;
    }

    public MatchSdkBuilder WithRetryConfig(RetryConfig retry)
    {
        ArgumentNullException.ThrowIfNull(retry);
        _retry = retry;
        return this;
    }

    public MatchSdkBuilder WithLogger(ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
        return this;
    }

    public MatchSdkBuilder WithHttpClient(System.Net.Http.HttpClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        _httpClient = client;
        return this;
    }

    public MatchSdk Build()
    {
        if (string.IsNullOrWhiteSpace(_baseUrl))
            throw new InvalidOperationException("BaseUrl is required (e.g., http://localhost:50051).");

        var channelOptions = new GrpcChannelOptions
        {
            LoggerFactory = _logger is null ? NullLoggerFactory.Instance : new LoggerFactoryFromLogger(_logger),
            ServiceConfig = MatchSdk.BuildServiceConfig(_retry),
        };
        if (_httpClient is not null)
            channelOptions.HttpClient = _httpClient;

        var channel = GrpcChannel.ForAddress(_baseUrl, channelOptions);
        return new MatchSdk(channel, ownsChannel: true);
    }
}

internal sealed class LoggerFactoryFromLogger(ILogger logger) : ILoggerFactory
{
    public void AddProvider(ILoggerProvider provider) { }
    public ILogger CreateLogger(string categoryName) => logger;
    public void Dispose() { }
}
