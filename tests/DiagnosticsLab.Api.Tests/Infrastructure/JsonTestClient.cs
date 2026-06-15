using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace DiagnosticsLab.Api.Tests.Infrastructure;

/// <summary>
/// Provides small JSON helpers used by endpoint behavior tests.
/// </summary>
internal static class JsonTestClient
{
    /// <summary>
    /// Sends a GET request and parses the JSON array response.
    /// </summary>
    /// <param name="client">The HTTP client.</param>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="expectedStatusCode">The expected HTTP status code.</param>
    /// <returns>The cloned JSON array root element.</returns>
    public static async Task<JsonElement> GetJsonArrayAsync(
        HttpClient client,
        string requestUri,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
    {
        using var document = await GetJsonDocumentAsync(client, requestUri, expectedStatusCode);
        return document.RootElement.Clone();
    }

    /// <summary>
    /// Sends a GET request and parses the JSON response.
    /// </summary>
    /// <param name="client">The HTTP client.</param>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="expectedStatusCode">The expected HTTP status code.</param>
    /// <returns>The parsed JSON document.</returns>
    public static async Task<JsonDocument> GetJsonDocumentAsync(
        HttpClient client,
        string requestUri,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
    {
        var response = await client.GetAsync(requestUri);
        response.StatusCode.Should().Be(expectedStatusCode);

        var stream = await response.Content.ReadAsStreamAsync();
        return await JsonDocument.ParseAsync(stream);
    }

    /// <summary>
    /// Sends a JSON POST request and parses the successful JSON response.
    /// </summary>
    /// <param name="client">The HTTP client.</param>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="value">The value to send as JSON.</param>
    /// <param name="expectedStatusCode">The expected HTTP status code.</param>
    /// <returns>The parsed JSON document.</returns>
    public static async Task<JsonDocument> PostJsonDocumentAsync(
        HttpClient client,
        string requestUri,
        object value,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
    {
        var response = await client.PostAsJsonAsync(requestUri, value);
        response.StatusCode.Should().Be(expectedStatusCode);

        var stream = await response.Content.ReadAsStreamAsync();
        return await JsonDocument.ParseAsync(stream);
    }

    /// <summary>
    /// Sends a POST request and parses the successful JSON response.
    /// </summary>
    /// <param name="client">The HTTP client.</param>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="content">The HTTP content.</param>
    /// <param name="expectedStatusCode">The expected HTTP status code.</param>
    /// <returns>The parsed JSON document.</returns>
    public static async Task<JsonDocument> PostJsonDocumentAsync(
        HttpClient client,
        string requestUri,
        HttpContent content,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
    {
        var response = await client.PostAsync(requestUri, content);
        response.StatusCode.Should().Be(expectedStatusCode);

        var stream = await response.Content.ReadAsStreamAsync();
        return await JsonDocument.ParseAsync(stream);
    }
}
