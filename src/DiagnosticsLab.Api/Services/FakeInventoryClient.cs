namespace DiagnosticsLab.Api.Services;

/// <summary>
/// Simulates an external inventory provider used by retry and resilience scenarios.
/// </summary>
public sealed class FakeInventoryClient
{
    /// <summary>
    /// Gets the simulated available stock for the specified SKU.
    /// </summary>
    /// <param name="sku">The SKU used by the scenario.</param>
    /// <param name="cancellationToken">A token that can cancel the simulated dependency call.</param>
    /// <returns>The simulated stock level.</returns>
    /// <exception cref="InventoryUnavailableException">Thrown when the simulated inventory provider is unavailable.</exception>
    public async Task<int> GetAvailableStockAsync(string sku, CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(50), cancellationToken);

        if (string.Equals(sku, "FAIL", StringComparison.OrdinalIgnoreCase))
        {
            throw new InventoryUnavailableException($"Inventory provider is unavailable for SKU '{sku}'.");
        }

        return string.Equals(sku, "EMPTY", StringComparison.OrdinalIgnoreCase) ? 0 : 42;
    }
}

/// <summary>
/// Represents a simulated inventory provider failure.
/// </summary>
/// <param name="message">The exception message.</param>
public sealed class InventoryUnavailableException(string message) : Exception(message);
