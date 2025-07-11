namespace Meow.Services;

/// <summary>
/// Service for monitoring network connectivity and cache status
/// </summary>
public class ConnectivityService : IDisposable
{
    #region Events

    /// <summary>
    /// Event triggered when connectivity status changes
    /// </summary>
    public event EventHandler<ConnectivityChangedEventArgs> ConnectivityChanged;

    #endregion

    #region Private Fields

    private readonly ICacheService _cacheService;
    private bool _isOnline;

    #endregion

    #region Constructor

    public ConnectivityService(ICacheService cacheService)
    {
        _cacheService = cacheService;
        _isOnline = Connectivity.NetworkAccess == NetworkAccess.Internet;

        // Subscribe to connectivity changes
        Connectivity.ConnectivityChanged += OnConnectivityChanged;
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets the current connectivity status
    /// </summary>
    public bool IsOnline => Connectivity.NetworkAccess == NetworkAccess.Internet;

    /// <summary>
    /// Gets a user-friendly connectivity status message
    /// </summary>
    public string ConnectivityStatus => IsOnline ? "Online" : "Offline";

    #endregion

    #region Public Methods

    /// <summary>
    /// Gets cache statistics for UI display
    /// </summary>
    public async Task<CacheStatistics> GetCacheStatusAsync()
    {
        return await _cacheService.GetCacheStatisticsAsync();
    }

    /// <summary>
    /// Forces a sync when connectivity is available
    /// </summary>
    public async Task<bool> ForceSyncAsync()
    {
        if (!IsOnline)
            return false;

        return await _cacheService.SyncFavoritesAsync();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Handles connectivity changes
    /// </summary>
    private async void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
    {
        var wasOnline = _isOnline;
        _isOnline = e.NetworkAccess == NetworkAccess.Internet;

        // Trigger event
        ConnectivityChanged?.Invoke(this, e);

        // If we just came online, trigger sync
        if (!wasOnline && _isOnline)
        {
            try
            {
                await _cacheService.SyncFavoritesAsync();
                System.Diagnostics.Debug.WriteLine("Auto-sync triggered after connectivity restored");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Auto-sync failed: {ex.Message}");
            }
        }
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        Connectivity.ConnectivityChanged -= OnConnectivityChanged;
    }

    #endregion
}
