namespace Meow.Infrastructure.Services;

/// <summary>
/// Background service for periodic cache maintenance and favorites sync.
/// Reacts to connectivity changes to sync when back online.
/// </summary>
public class BackgroundSyncService : IDisposable
{
    private readonly CacheMaintenanceUseCase _cacheMaintenanceUseCase;
    private readonly GetBreedsUseCase _getBreedsUseCase;
    private readonly IConnectivityProvider _connectivity;
    private Timer? _syncTimer;
    private const int SyncIntervalMinutes = 15;

    public BackgroundSyncService(
        CacheMaintenanceUseCase cacheMaintenanceUseCase,
        GetBreedsUseCase getBreedsUseCase,
        IConnectivityProvider connectivity)
    {
        _cacheMaintenanceUseCase = cacheMaintenanceUseCase;
        _getBreedsUseCase = getBreedsUseCase;
        _connectivity = connectivity;

        // Auto-sync when connectivity is restored
        _connectivity.ConnectivityChanged += OnConnectivityChanged;
    }

    public void Start()
    {
        _syncTimer = new Timer(
            async _ => await PerformSyncAsync(),
            null,
            TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(SyncIntervalMinutes));
    }

    public void Stop()
    {
        _syncTimer?.Dispose();
        _syncTimer = null;
    }

    public async Task ManualSyncAsync()
    {
        await PerformSyncAsync();
    }

    private async Task PerformSyncAsync()
    {
        try
        {
            if (!_connectivity.IsConnected)
                return;

            await _cacheMaintenanceUseCase.ExecuteMaintenanceAsync();

            // Refresh breeds if cache is empty or stale
            var stats = await _cacheMaintenanceUseCase.GetStatisticsAsync();
            if (stats.CachedBreedsCount == 0 ||
                (DateTime.UtcNow - stats.LastCacheUpdate).TotalDays > 7)
            {
                await _getBreedsUseCase.ExecuteAsync(forceRefresh: true);
            }

            System.Diagnostics.Debug.WriteLine("Background sync completed successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Background sync failed: {ex.Message}");
        }
    }

    private async void OnConnectivityChanged(object? sender, bool isConnected)
    {
        if (isConnected)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Connectivity restored - triggering sync");
                await PerformSyncAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Auto-sync on reconnect failed: {ex.Message}");
            }
        }
    }

    public void Dispose()
    {
        Stop();
        _connectivity.ConnectivityChanged -= OnConnectivityChanged;
    }
}
