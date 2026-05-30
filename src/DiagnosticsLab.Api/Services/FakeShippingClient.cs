namespace DiagnosticsLab.Api.Services;

public sealed class FakeShippingClient
{
    public async Task<decimal> GetRateAsync(string country, CancellationToken cancellationToken)
    {
        var delay = string.Equals(country, "SLOW", StringComparison.OrdinalIgnoreCase)
            ? TimeSpan.FromSeconds(10)
            : TimeSpan.FromMilliseconds(250);

        await Task.Delay(delay, cancellationToken);

        return string.Equals(country, "CZ", StringComparison.OrdinalIgnoreCase) ? 8.50m : 12.00m;
    }
}
