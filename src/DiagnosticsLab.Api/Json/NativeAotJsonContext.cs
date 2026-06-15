using System.Text.Json.Serialization;
using DiagnosticsLab.Api.Endpoints;

namespace DiagnosticsLab.Api;

[JsonSerializable(typeof(NativeAotEndpoints.SimplePayload))]
public partial class NativeAotJsonContext : JsonSerializerContext {
}