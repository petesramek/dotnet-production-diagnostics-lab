namespace DiagnosticsLab.Api.Services;

/// <summary>
/// Simulates startup initialization work that may fail.
/// </summary>
public sealed class FakeStartupDependency
{
    /// <summary>
    /// Simulates initialization work for a startup dependency.
    /// </summary>
    /// <param name="shouldFail">A value indicating whether initialization should fail.</param>
    /// <param name="cancellationToken">A token that can cancel the simulated startup work.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="StartupDependencyException">Thrown when the simulated startup dependency fails.</exception>
    public async Task InitializeAsync(bool shouldFail, CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(50), cancellationToken);

        if (shouldFail)
        {
            throw new StartupDependencyException("Startup dependency initialization failed.");
        }
    }
}

/// <summary>
/// Represents a simulated startup dependency failure.
/// </summary>
/// <param name="message">The exception message.</param>
public sealed class StartupDependencyException(string message) : Exception(message);
