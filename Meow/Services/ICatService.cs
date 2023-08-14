namespace Meow.Services;

public interface ICatService
{
    Task<List<Cat>> GetRandomKitty();

    Task<List<FavoriteCatResponse>> GetFavoriteKittens();
    Task<string> AddFavoriteKitten(string image_id);
    Task<string> DeleteFavoriteKitten(int favourite_id);
    Task<string> RemoveFavoriteKitten(string cat_id);
}
