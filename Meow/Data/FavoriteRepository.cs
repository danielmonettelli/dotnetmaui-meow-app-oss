namespace Meow.Data;

/// <summary>
/// Repository for managing user favorites with sync capabilities
/// </summary>
public class FavoriteRepository : BaseRepository
{
    /// <summary>
    /// Gets all user favorites (both synced and local)
    /// </summary>
    public async Task<List<FavoriteCatResponse>> GetUserFavoritesAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            
            var userFavorites = await database.Table<UserFavorite>()
                .Where(f => !f.IsPendingDeletion)
                .OrderByDescending(f => f.AddedAt)
                .ToListAsync();

            return userFavorites.Select(f => f.ToFavoriteCatResponse()).ToList();
        }, new List<FavoriteCatResponse>());
    }

    /// <summary>
    /// Adds a favorite locally
    /// </summary>
    public async Task<bool> AddFavoriteAsync(Cat cat)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();

            // Check if already exists
            var existing = await database.Table<UserFavorite>()
                .Where(f => f.ImageId == cat.Id && !f.IsPendingDeletion)
                .FirstOrDefaultAsync();

            if (existing != null)
                return false; // Already exists

            var favorite = UserFavorite.FromCat(cat);
            await database.InsertAsync(favorite);
            
            return true;
        }, false);
    }

    /// <summary>
    /// Removes a favorite locally (marks for deletion if synced)
    /// </summary>
    public async Task<bool> RemoveFavoriteAsync(string imageId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();

            var favorite = await database.Table<UserFavorite>()
                .Where(f => f.ImageId == imageId && !f.IsPendingDeletion)
                .FirstOrDefaultAsync();

            if (favorite == null)
                return false;

            if (favorite.IsSynced && !string.IsNullOrEmpty(favorite.FavoriteId))
            {
                // Mark for deletion instead of removing immediately
                favorite.IsPendingDeletion = true;
                await database.UpdateAsync(favorite);
            }
            else
            {
                // Remove local-only favorite immediately
                await database.DeleteAsync(favorite);
            }
            
            return true;
        }, false);
    }

    /// <summary>
    /// Syncs favorites with the server
    /// </summary>
    public async Task<bool> SyncFavoritesAsync(ICatService catService)
    {
        if (!IsConnected)
            return false;

        return await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();

            // Get server favorites
            var serverFavorites = await catService.GetFavoriteKittens();
            if (serverFavorites != null)
            {
                await SyncServerFavoritesAsync(serverFavorites);
            }

            // Push local favorites to server
            await PushLocalFavoritesToServerAsync(catService);

            // Handle pending deletions
            await HandlePendingDeletionsAsync(catService);

            return true;
        }, false);
    }

    /// <summary>
    /// Gets unsynced favorites count
    /// </summary>
    public async Task<int> GetUnsyncedCountAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            
            var unsyncedCount = await database.Table<UserFavorite>()
                .Where(f => !f.IsSynced || f.IsPendingDeletion)
                .CountAsync();

            return unsyncedCount;
        }, 0);
    }

    /// <summary>
    /// Checks if a cat is favorited
    /// </summary>
    public async Task<bool> IsFavoriteAsync(string imageId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            
            var favorite = await database.Table<UserFavorite>()
                .Where(f => f.ImageId == imageId && !f.IsPendingDeletion)
                .FirstOrDefaultAsync();

            return favorite != null;
        }, false);
    }

    /// <summary>
    /// Syncs server favorites to local database
    /// </summary>
    private async Task SyncServerFavoritesAsync(List<FavoriteCatResponse> serverFavorites)
    {
        var database = await GetDatabaseAsync();

        foreach (var serverFavorite in serverFavorites)
        {
            var localFavorite = await database.Table<UserFavorite>()
                .Where(f => f.FavoriteId == serverFavorite.Id)
                .FirstOrDefaultAsync();

            if (localFavorite == null)
            {
                // Add server favorite to local database
                var newFavorite = UserFavorite.FromFavoriteCatResponse(serverFavorite);
                await database.InsertAsync(newFavorite);
            }
            else if (localFavorite.IsPendingDeletion)
            {
                // Remove from server if marked for deletion locally
                // This will be handled in HandlePendingDeletionsAsync
            }
            else
            {
                // Update local favorite with server data
                localFavorite.IsSynced = true;
                localFavorite.LastSyncAttempt = DateTime.UtcNow;
                await database.UpdateAsync(localFavorite);
            }
        }
    }

    /// <summary>
    /// Pushes local unsynced favorites to server
    /// </summary>
    private async Task PushLocalFavoritesToServerAsync(ICatService catService)
    {
        var database = await GetDatabaseAsync();

        var unsyncedFavorites = await database.Table<UserFavorite>()
            .Where(f => !f.IsSynced && !f.IsPendingDeletion)
            .ToListAsync();

        foreach (var favorite in unsyncedFavorites)
        {
            try
            {
                var response = await catService.AddFavoriteKitten(favorite.ImageId);
                
                if (!string.IsNullOrEmpty(response))
                {
                    // Parse the response to get the favorite ID
                    var favoriteResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(response);
                    if (favoriteResponse.ContainsKey("id"))
                    {
                        favorite.FavoriteId = favoriteResponse["id"].ToString();
                        favorite.IsSynced = true;
                        favorite.LastSyncAttempt = DateTime.UtcNow;
                        await database.UpdateAsync(favorite);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log sync failure
                favorite.LastSyncAttempt = DateTime.UtcNow;
                await database.UpdateAsync(favorite);
                System.Diagnostics.Debug.WriteLine($"Failed to sync favorite {favorite.ImageId}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Handles pending deletions by removing from server
    /// </summary>
    private async Task HandlePendingDeletionsAsync(ICatService catService)
    {
        var database = await GetDatabaseAsync();

        var pendingDeletions = await database.Table<UserFavorite>()
            .Where(f => f.IsPendingDeletion && !string.IsNullOrEmpty(f.FavoriteId))
            .ToListAsync();

        foreach (var favorite in pendingDeletions)
        {
            try
            {
                // Convert string favorite ID to int
                if (int.TryParse(favorite.FavoriteId, out int favoriteIdInt))
                {
                    var response = await catService.DeleteFavoriteKitten(favoriteIdInt);
                    
                    if (!string.IsNullOrEmpty(response))
                    {
                        // Successfully deleted from server, remove from local database
                        await database.DeleteAsync(favorite);
                    }
                }
                else
                {
                    // If ID conversion fails, just remove from local database
                    await database.DeleteAsync(favorite);
                }
            }
            catch (Exception ex)
            {
                // Log deletion failure
                favorite.LastSyncAttempt = DateTime.UtcNow;
                await database.UpdateAsync(favorite);
                System.Diagnostics.Debug.WriteLine($"Failed to delete favorite {favorite.FavoriteId}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Clears all favorites
    /// </summary>
    public async Task ClearFavoritesAsync()
    {
        await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            await database.DeleteAllAsync<UserFavorite>();
            return true;
        }, false);
    }
}
