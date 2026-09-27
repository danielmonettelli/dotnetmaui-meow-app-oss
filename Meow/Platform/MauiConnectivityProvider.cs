using Meow.Domain.Interfaces;

namespace Meow.Platform;

/// <summary>
/// MAUI platform implementation of IConnectivityProvider.
/// Uses MAUI's Connectivity API - stays in the MAUI project since it's platform-specific.
/// Other platforms (Web, API) would provide their own implementations (SOLID: Dependency Inversion).
/// </summary>
public class MauiConnectivityProvider : IConnectivityProvider, IDisposable
{
    private bool _subscribed;

    public bool IsConnected
    {
        get
        {
            EnsureSubscribed();
            return Connectivity.NetworkAccess == NetworkAccess.Internet;
        }
    }

    public event EventHandler<bool>? ConnectivityChanged;

    public MauiConnectivityProvider()
    {
        // Don't subscribe in constructor — Android Context may not be ready yet.
        // Subscribed lazily on first access.
    }

    private void EnsureSubscribed()
    {
        if (!_subscribed)
        {
            _subscribed = true;
            Connectivity.ConnectivityChanged += OnConnectivityChanged;
        }
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        // Marshal to main thread to avoid CalledFromWrongThreadException on Android
        MainThread.BeginInvokeOnMainThread(() =>
            ConnectivityChanged?.Invoke(this, e.NetworkAccess == NetworkAccess.Internet));
    }

    public void Dispose()
    {
        Connectivity.ConnectivityChanged -= OnConnectivityChanged;
    }
}
