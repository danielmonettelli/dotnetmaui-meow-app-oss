namespace Meow.Domain.Interfaces;

/// <summary>
/// Interface for connectivity monitoring.
/// Abstracts platform-specific connectivity checks (SOLID: Dependency Inversion).
/// </summary>
public interface IConnectivityProvider
{
    /// <summary>
    /// Gets whether the device currently has internet access
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Event raised when connectivity changes
    /// </summary>
    event EventHandler<bool> ConnectivityChanged;
}
