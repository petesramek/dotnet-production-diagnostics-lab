namespace DiagnosticsLab.Api.Services;

public sealed class FakeInventoryClient
{
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

public sealed class InventoryUnavailableException(string message) : Exception(message);
