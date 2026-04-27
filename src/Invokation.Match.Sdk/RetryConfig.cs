namespace Invokation.Match.Sdk;

/// <summary>
/// Retry configuration for the gRPC client. Wraps the values that get
/// translated into a <c>Grpc.Net.Client.Configuration.RetryPolicy</c> by
/// <see cref="MatchSdk.BuildServiceConfig(RetryConfig)"/>.
/// </summary>
/// <remarks>
/// Implemented as a regular sealed class (not a <c>record</c>) so each
/// property can validate during its <c>init</c> accessor. With a record's
/// primary constructor, validation in the constructor body runs before
/// object-initializer assignments, which would let bogus values through.
/// </remarks>
public sealed class RetryConfig
{
    private readonly int _maxAttempts = 3;
    private readonly int _initialBackoffMs = 500;
    private readonly int _maxBackoffMs = 10_000;
    private readonly double _backoffMultiplier = 2.0;

    public int MaxAttempts
    {
        get => _maxAttempts;
        init
        {
            if (value < 1) throw new System.ArgumentException("MaxAttempts must be >= 1");
            _maxAttempts = value;
        }
    }

    public int InitialBackoffMs
    {
        get => _initialBackoffMs;
        init
        {
            if (value < 0) throw new System.ArgumentException("InitialBackoffMs must be >= 0");
            _initialBackoffMs = value;
        }
    }

    public int MaxBackoffMs
    {
        get => _maxBackoffMs;
        init
        {
            if (value < 0) throw new System.ArgumentException("MaxBackoffMs must be >= 0");
            _maxBackoffMs = value;
        }
    }

    public double BackoffMultiplier
    {
        get => _backoffMultiplier;
        init
        {
            if (value < 1.0) throw new System.ArgumentException("BackoffMultiplier must be >= 1.0");
            _backoffMultiplier = value;
        }
    }

    public static RetryConfig Default => new();
    public static RetryConfig NoRetry => new() { MaxAttempts = 1 };
}
