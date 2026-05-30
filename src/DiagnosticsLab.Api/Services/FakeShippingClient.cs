namespace DiagnosticsLab.Api.Services;

/// <summary>
/// Simulates an external shipping provider used by dependency reliability scenarios.
/// </summary>
public sealed class FakeShippingClient
{
    /// <summary>
    /// Gets a simulated shipping rate for the specified country.
    /// </summary>
    /// <param name="country">The country code used by the scenario.</param>
    /// <param name="cancellationToken">A token that can cancel the simulated dependency call.</param>
    /// <returns>The simulated shipping rate.</returns>
    public async Task<decimal> GetRateAsync(string country, CancellationToken cancellationToken)
    {
        var delay = string.Equals(country, "SLOW", StringComparison.OrdinalIgnoreCase)
            ? TimeSpan.FromSeconds(10)
            : TimeSpan.FromMilliseconds(250);

        await Task.Delay(delay, cancellationToken);

        return string.Equals(country, "CZ", StringComparison.OrdinalIgnoreCase) ? 8.50m : 12.00m;
    }
}
