namespace DiagnosticsLab.Api.Services;

/// <summary>
/// Simulates an external audit or logging sink that can fail.
/// </summary>
public sealed class FakeAuditSink
{
    /// <summary>
    /// Writes a simulated audit event.
    /// </summary>
    /// <param name="eventName">The audit event name.</param>
    /// <param name="shouldFail">A value indicating whether the simulated sink should fail.</param>
    /// <param name="cancellationToken">A token that can cancel the simulated audit write.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="AuditSinkException">Thrown when the simulated audit sink fails.</exception>
    public async Task WriteAsync(string eventName, bool shouldFail, CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(25), cancellationToken);

        if (shouldFail)
        {
            throw new AuditSinkException($"Audit sink failed while writing '{eventName}'.");
        }
    }
}

/// <summary>
/// Represents a simulated audit sink failure.
/// </summary>
/// <param name="message">The exception message.</param>
public sealed class AuditSinkException(string message) : Exception(message);
