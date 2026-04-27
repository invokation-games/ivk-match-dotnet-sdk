using Google.Protobuf;
using Matchmaker.Engines.BasicSbmm.V1;

namespace Invokation.Match.Sdk.Engines;

/// <summary>
/// Helpers for packing and unpacking <see cref="EngineInput"/> and
/// <see cref="EngineOutput"/> for the basic_sbmm engine into the opaque
/// <c>bytes</c> fields on <c>Ticket</c>, <c>BackfillRequest</c>, etc.
/// </summary>
public static class BasicSbmm
{
    public static ByteString PackInput(EngineInput input) => input.ToByteString();
    public static EngineInput UnpackInput(ByteString bytes) => EngineInput.Parser.ParseFrom(bytes);
    public static EngineInput UnpackInput(byte[] bytes) => EngineInput.Parser.ParseFrom(bytes);

    public static ByteString PackOutput(EngineOutput output) => output.ToByteString();
    public static EngineOutput UnpackOutput(ByteString bytes) => EngineOutput.Parser.ParseFrom(bytes);
    public static EngineOutput UnpackOutput(byte[] bytes) => EngineOutput.Parser.ParseFrom(bytes);
}
