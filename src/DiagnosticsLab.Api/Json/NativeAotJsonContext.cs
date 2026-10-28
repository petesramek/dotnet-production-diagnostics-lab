using System.Text.Json.Serialization;
using DiagnosticsLab.Api.Scenarios.Startup;

namespace DiagnosticsLab.Api;

/// <summary>
/// Provides source-generated JSON serialization metadata for Native AOT scenarios.
/// </summary>
[JsonSerializable(typeof(NativeAotSerializationFailureEndpoints.SimplePayload))]
public partial class NativeAotJsonContext : JsonSerializerContext {
}