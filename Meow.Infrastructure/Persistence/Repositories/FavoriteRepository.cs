namespace Meow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for user favorites with bidirectional sync
/// </summary>
public class FavoriteRepository : BaseRepository, IFavoriteRepository
{
    private readonly IUserIdentifierProvider? _userIdentifierProvider;

    public FavoriteRepository(
        IDatabasePathProvider pathProvider,
        IUserIdentifierProvider? userIdentifierProvider = null) : base(pathProvider)
    {
        _userIdentifierProvider = userIdentifierProvider;
    }

    public async Task<List<FavoriteCatResponse>> GetUserFavoritesAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            var userId = _userIdentifierProvider?.GetUserIdentifier();

            List<UserFavoriteEntity> favorites;
            if (!string.IsNullOrEmpty(userId))
            {
                favorites = await database.Table<UserFavoriteEntity>()
                    .Where(f => f.UserId == userId && !f.IsPendingDeletion)
                    .OrderByDescending(f => f.AddedAt)
                    .ToListAsync();
            }
            else
            {
                favorites = await database.Table<UserFavoriteEntity>()
                    .Where(f => !f.IsPendingDeletion)
                    .OrderByDescending(f => f.AddedAt)
                    .ToListAsync();
            }

            return favorites.Select(f => f.ToFavoriteCatResponse()).ToList();
        }, new List<FavoriteCatResponse>());
    }

    public async Task<bool> AddFavoriteAsync(Cat cat)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            var userId = _userIdentifierProvider?.GetUserIdentifier();

            var existing = !string.IsNullOrEmpty(userId)
                ? await database.Table<UserFavoriteEntity>()
                    .Where(f => f.ImageId == cat.Id && f.UserId == userId && !f.IsPendingDeletion)
                    .FirstOrDefaultAsync()
                : await database.Table<UserFavoriteEntity>()
                    .Where(f => f.ImageId == cat.Id && !f.IsPendingDeletion)
                    .FirstOrDefaultAsync();

            if (existing != null) return false;

            await database.InsertAsync(UserFavoriteEntity.FromCat(cat, userId));
            return true;
        }, false);
    }

    public async Task<bool> RemoveFavoriteAsync(string imageId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            var userId = _userIdentifierProvider?.GetUserIdentifier();

            var favorite = !string.IsNullOrEmpty(userId)
                ? await database.Table<UserFavoriteEntity>()
                    .Where(f => f.ImageId == imageId && f.UserId == userId && !f.IsPendingDeletion)
                    .FirstOrDefaultAsync()
                : await database.Table<UserFavoriteEntity>()
                    .Where(f => f.ImageId == imageId && !f.IsPendingDeletion)
                    .FirstOrDefaultAsync();

            if (favorite == null) return false;

            if (favorite.IsSynced && !string.IsNullOrEmpty(favorite.FavoriteId))
            {
                favorite.IsPendingDeletion = true;
                await database.UpdateAsync(favorite);
            }
            else
            {
                await database.DeleteAsync(favorite);
            }
            return true;
        }, false);
    }

    public async Task<bool> IsFavoriteAsync(string imageId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            var userId = _userIdentifierProvider?.GetUserIdentifier();

            var favorite = !string.IsNullOrEmpty(userId)
                ? await database.Table<UserFavoriteEntity>()
                    .Where(f => f.ImageId == imageId && f.UserId == userId && !f.IsPendingDeletion)
                    .FirstOrDefaultAsync()
                : await database.Table<UserFavoriteEntity>()
                    .Where(f => f.ImageId == imageId && !f.IsPendingDeletion)
                    .FirstOrDefaultAsync();

            return favorite != null;
        }, false);
    }

    public async Task<bool> SyncFavoritesAsync(ICatApiService catApiService)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            var userId = _userIdentifierProvider?.GetUserIdentifier();

            // Clean up any legacy or orphaned favorites from older installs or different user IDs
            if (!string.IsNullOrEmpty(userId))
            {
                await PurgeOrphanedFavoritesAsync(database, userId);
            }

            // Get server favorites segmented by sub_id
            var serverFavorites = await catApiService.GetFavoritesAsync(userId);
            if (serverFavorites != null)
            {
                await SyncServerFavoritesAsync(database, serverFavorites, userId);
            }

            await PushLocalFavoritesAsync(database, catApiService, userId);
            await HandlePendingDeletionsAsync(database, catApiService, userId);

            return true;
        }, false);
    }

    public async Task<int> GetUnsyncedCountAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            var userId = _userIdentifierProvider?.GetUserIdentifier();

            return !string.IsNullOrEmpty(userId)
                ? await database.Table<UserFavoriteEntity>()
                    .Where(f => f.UserId == userId && (!f.IsSynced || f.IsPendingDeletion))
                    .CountAsync()
                : await database.Table<UserFavoriteEntity>()
                    .Where(f => !f.IsSynced || f.IsPendingDeletion)
                    .CountAsync();
        }, 0);
    }

    public async Task ClearFavoritesAsync()
    {
        await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            var userId = _userIdentifierProvider?.GetUserIdentifier();

            if (!string.IsNullOrEmpty(userId))
            {
                await database.ExecuteAsync("DELETE FROM UserFavorites WHERE UserId = ?", userId);
            }
            else
            {
                await database.DeleteAllAsync<UserFavoriteEntity>();
            }
            return true;
        }, false);
    }

    private async Task PurgeOrphanedFavoritesAsync(SQLiteAsyncConnection database, string currentUserId)
    {
        await database.ExecuteAsync("DELETE FROM UserFavorites WHERE UserId != ? OR UserId IS NULL", currentUserId);
    }

    private async Task SyncServerFavoritesAsync(
        SQLiteAsyncConnection database,
        List<FavoriteCatResponse> serverFavorites,
        string? userId)
    {
        foreach (var serverFav in serverFavorites)
        {
            var local = !string.IsNullOrEmpty(userId)
                ? await database.Table<UserFavoriteEntity>()
                    .Where(f => f.FavoriteId == serverFav.Id && f.UserId == userId)
                    .FirstOrDefaultAsync()
                : await database.Table<UserFavoriteEntity>()
                    .Where(f => f.FavoriteId == serverFav.Id)
                    .FirstOrDefaultAsync();

            if (local == null)
            {
                await database.InsertAsync(UserFavoriteEntity.FromFavoriteCatResponse(serverFav, userId));
            }
            else if (!local.IsPendingDeletion)
            {
                local.IsSynced = true;
                local.LastSyncAttempt = DateTime.UtcNow;
                await database.UpdateAsync(local);
            }
        }
    }

    private async Task PushLocalFavoritesAsync(
        SQLiteAsyncConnection database,
        ICatApiService catApiService,
        string? userId)
    {
        var unsynced = !string.IsNullOrEmpty(userId)
            ? await database.Table<UserFavoriteEntity>()
                .Where(f => f.UserId == userId && !f.IsSynced && !f.IsPendingDeletion)
                .ToListAsync()
            : await database.Table<UserFavoriteEntity>()
                .Where(f => !f.IsSynced && !f.IsPendingDeletion)
                .ToListAsync();

        foreach (var fav in unsynced)
        {
            try
            {
                var response = await catApiService.AddFavoriteAsync(fav.ImageId, userId);
                if (!string.IsNullOrEmpty(response))
                {
                    var parsed = JsonSerializer.Deserialize<Dictionary<string, object>>(response);
                    if (parsed?.ContainsKey("id") == true)
                    {
                        fav.FavoriteId = parsed["id"].ToString();
                        fav.IsSynced = true;
                    }
                }
                fav.LastSyncAttempt = DateTime.UtcNow;
                await database.UpdateAsync(fav);
            }
            catch (Exception ex)
            {
                fav.LastSyncAttempt = DateTime.UtcNow;
                await database.UpdateAsync(fav);
                System.Diagnostics.Debug.WriteLine($"Failed to sync favorite {fav.ImageId}: {ex.Message}");
            }
        }
    }

    private async Task HandlePendingDeletionsAsync(
        SQLiteAsyncConnection database,
        ICatApiService catApiService,
        string? userId)
    {
        var pending = !string.IsNullOrEmpty(userId)
            ? await database.Table<UserFavoriteEntity>()
                .Where(f => f.UserId == userId && f.IsPendingDeletion && f.FavoriteId != null && f.FavoriteId != "")
                .ToListAsync()
            : await database.Table<UserFavoriteEntity>()
                .Where(f => f.IsPendingDeletion && f.FavoriteId != null && f.FavoriteId != "")
                .ToListAsync();

        foreach (var fav in pending)
        {
            try
            {
                if (int.TryParse(fav.FavoriteId, out var favIdInt))
                {
                    var response = await catApiService.DeleteFavoriteAsync(favIdInt);
                    if (!string.IsNullOrEmpty(response))
                    {
                        await database.DeleteAsync(fav);
                        continue;
                    }
                }
                else
                {
                    await database.DeleteAsync(fav);
                    continue;
                }

                fav.LastSyncAttempt = DateTime.UtcNow;
                await database.UpdateAsync(fav);
            }
            catch (Exception ex)
            {
                fav.LastSyncAttempt = DateTime.UtcNow;
                await database.UpdateAsync(fav);
                System.Diagnostics.Debug.WriteLine($"Failed to delete favorite {fav.FavoriteId}: {ex.Message}");
            }
        }
    }
}
