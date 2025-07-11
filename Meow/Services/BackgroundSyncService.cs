namespace Meow.Services;

/// <summary>
/// Background service for handling periodic cache maintenance and sync operations
/// </summary>
public class BackgroundSyncService
{
    #region Private Fields

    private readonly ICacheService _cacheService;
    private Timer _syncTimer;
    private readonly int _syncIntervalMinutes = 15; // Sync every 15 minutes

    #endregion

    #region Constructor

    public BackgroundSyncService(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Starts the background sync service
    /// </summary>
    public void Start()
    {
        // Start timer for periodic sync
        _syncTimer = new Timer(
            PerformBackgroundSync, 
            null, 
            TimeSpan.FromMinutes(1), // Start after 1 minute
            TimeSpan.FromMinutes(_syncIntervalMinutes)); // Repeat every 15 minutes
    }

    /// <summary>
    /// Stops the background sync service
    /// </summary>
    public void Stop()
    {
        _syncTimer?.Dispose();
        _syncTimer = null;
    }

    /// <summary>
    /// Manually triggers a sync operation
    /// </summary>
    public async Task ManualSyncAsync()
    {
        await PerformSyncOperationsAsync();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Timer callback for periodic sync operations
    /// </summary>
    private async void PerformBackgroundSync(object state)
    {
        await PerformSyncOperationsAsync();
    }

    /// <summary>
    /// Performs all sync operations
    /// </summary>
    private async Task PerformSyncOperationsAsync()
    {
        try
        {
            // Only perform sync operations if connected to internet
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                return;

            // Perform cache maintenance
            await _cacheService.PerformCacheMaintenanceAsync();

            // Sync favorites
            await _cacheService.SyncFavoritesAsync();

            // Refresh breeds cache if needed (weekly)
            var stats = await _cacheService.GetCacheStatisticsAsync();
            if (stats.CachedBreedsCount == 0 || 
                (DateTime.UtcNow - stats.LastCacheUpdate).TotalDays > 7)
            {
                await _cacheService.GetBreedsAsync(forceRefresh: true);
            }

            System.Diagnostics.Debug.WriteLine("Background sync completed successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Background sync failed: {ex.Message}");
        }
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        Stop();
    }

    #endregion
}
