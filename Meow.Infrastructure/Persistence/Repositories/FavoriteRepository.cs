namespace Meow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for user favorites with bidirectional sync
/// </summary>
public class FavoriteRepository : BaseRepository, IFavoriteRepository
{
    public FavoriteRepository(IDatabasePathProvider pathProvider) : base(pathProvider) { }

    public async Task<List<FavoriteCatResponse>> GetUserFavoritesAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            var favorites = await database.Table<UserFavoriteEntity>()
                .Where(f => !f.IsPendingDeletion)
                .OrderByDescending(f => f.AddedAt)
                .ToListAsync();
            return favorites.Select(f => f.ToFavoriteCatResponse()).ToList();
        }, new List<FavoriteCatResponse>());
    }

    public async Task<bool> AddFavoriteAsync(Cat cat)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            var existing = await database.Table<UserFavoriteEntity>()
                .Where(f => f.ImageId == cat.Id && !f.IsPendingDeletion)
                .FirstOrDefaultAsync();

            if (existing != null) return false;

            await database.InsertAsync(UserFavoriteEntity.FromCat(cat));
            return true;
        }, false);
    }

    public async Task<bool> RemoveFavoriteAsync(string imageId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            var favorite = await database.Table<UserFavoriteEntity>()
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
            var favorite = await database.Table<UserFavoriteEntity>()
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

            // Get server favorites
            var serverFavorites = await catApiService.GetFavoritesAsync();
            if (serverFavorites != null)
            {
                await SyncServerFavoritesAsync(database, serverFavorites);
            }

            await PushLocalFavoritesAsync(database, catApiService);
            await HandlePendingDeletionsAsync(database, catApiService);

            return true;
        }, false);
    }

    public async Task<int> GetUnsyncedCountAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            return await database.Table<UserFavoriteEntity>()
                .Where(f => !f.IsSynced || f.IsPendingDeletion)
                .CountAsync();
        }, 0);
    }

    public async Task ClearFavoritesAsync()
    {
        await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            await database.DeleteAllAsync<UserFavoriteEntity>();
            return true;
        }, false);
    }

    private async Task SyncServerFavoritesAsync(SQLiteAsyncConnection database, List<FavoriteCatResponse> serverFavorites)
    {
        foreach (var serverFav in serverFavorites)
        {
            var local = await database.Table<UserFavoriteEntity>()
                .Where(f => f.FavoriteId == serverFav.Id)
                .FirstOrDefaultAsync();

            if (local == null)
            {
                await database.InsertAsync(UserFavoriteEntity.FromFavoriteCatResponse(serverFav));
            }
            else if (!local.IsPendingDeletion)
            {
                local.IsSynced = true;
                local.LastSyncAttempt = DateTime.UtcNow;
                await database.UpdateAsync(local);
            }
        }
    }

    private async Task PushLocalFavoritesAsync(SQLiteAsyncConnection database, ICatApiService catApiService)
    {
        var unsynced = await database.Table<UserFavoriteEntity>()
            .Where(f => !f.IsSynced && !f.IsPendingDeletion)
            .ToListAsync();

        foreach (var fav in unsynced)
        {
            try
            {
                var response = await catApiService.AddFavoriteAsync(fav.ImageId);
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

    private async Task HandlePendingDeletionsAsync(SQLiteAsyncConnection database, ICatApiService catApiService)
    {
        var pending = await database.Table<UserFavoriteEntity>()
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
