namespace Meow.Services;

public interface ICatService
{
    Task<List<Cat>> GetRandomKitty();
    Task<List<Breed>> GetBreeds();
    Task<List<Cat>> GetRandomKittensByBreed(string breed);

    Task<List<FavoriteCatResponse>> GetFavoriteKittens();
    Task<string> AddFavoriteKitten(string image_id);
    Task<string> DeleteFavoriteKitten(int favourite_id);
    Task<string> RemoveFavoriteKitten(string cat_id);
}
