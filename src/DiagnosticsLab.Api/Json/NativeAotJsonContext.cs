using System.Text.Json.Serialization;
using DiagnosticsLab.Api.Endpoints;

namespace DiagnosticsLab.Api;

/// <summary>
/// Provides source-generated JSON serialization metadata for Native AOT scenarios.
/// </summary>
[JsonSerializable(typeof(NativeAotEndpoints.SimplePayload))]
public partial class NativeAotJsonContext : JsonSerializerContext {
}